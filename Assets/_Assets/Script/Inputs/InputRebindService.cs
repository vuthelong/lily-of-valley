using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LilyOfValley.Inputs
{
    public static class InputRebindService
    {
        #region Fields
        private const string OverridesPrefsKey = "lov.input.binding-overrides";

        private static readonly List<InputActionMap> PreviouslyEnabledMaps = new();

        public static event Action<InputAction> BindingChanged;
        #endregion

        #region Public Methods
        public static void LoadOverrides(InputActionAsset asset)
        {
            if (asset == null) return;

            var json = PlayerPrefs.GetString(OverridesPrefsKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return;

            asset.LoadBindingOverridesFromJson(json);
        }

        public static void SaveOverrides(InputActionAsset asset)
        {
            if (asset == null) return;

            PlayerPrefs.SetString(OverridesPrefsKey, asset.SaveBindingOverridesAsJson());
            PlayerPrefs.Save();
        }

        public static void ResetAllOverrides(InputActionAsset asset)
        {
            if (asset == null) return;

            asset.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey(OverridesPrefsKey);
            PlayerPrefs.Save();
            BindingChanged?.Invoke(null);
        }

        public static void ResetBinding(InputAction action, int bindingIndex)
        {
            if (!IsValidTarget(action, bindingIndex)) return;

            action.RemoveBindingOverride(bindingIndex);
            SaveOverrides(action.actionMap.asset);
            BindingChanged?.Invoke(action);
        }

        public static string GetDisplayString(InputAction action, int bindingIndex)
        {
            if (!IsValidTarget(action, bindingIndex)) return string.Empty;

            return action.GetBindingDisplayString(bindingIndex);
        }

        public static InputActionRebindingExtensions.RebindingOperation StartRebind(
            InputAction action,
            int bindingIndex,
            Action<bool> onFinished)
        {
            if (!IsValidTarget(action, bindingIndex))
            {
                onFinished?.Invoke(false);
                return null;
            }

            if (action.bindings[bindingIndex].isComposite)
            {
                Debug.LogError($"{nameof(InputRebindService)}: binding {bindingIndex} of '{action.name}' is a composite head; rebind its parts instead.");
                onFinished?.Invoke(false);
                return null;
            }

            var asset = action.actionMap.asset;
            CaptureEnabledMaps(asset);
            asset.Disable();

            var operation = action.PerformInteractiveRebinding(bindingIndex)
                                  .WithControlsExcluding("<Pointer>/position")
                                  .WithControlsExcluding("<Pointer>/delta")
                                  .WithControlsExcluding("<Mouse>/scroll")
                                  .WithCancelingThrough("<Keyboard>/escape");

            operation.OnCancel(op => Finish(op, action, bindingIndex, false, onFinished));
            operation.OnComplete(op => Finish(op, action, bindingIndex, true, onFinished));
            operation.Start();
            return operation;
        }
        #endregion

        #region Private Methods
        private static void Finish(
            InputActionRebindingExtensions.RebindingOperation operation,
            InputAction action,
            int bindingIndex,
            bool completed,
            Action<bool> onFinished)
        {
            var succeeded = completed;
            if (completed && HasDuplicate(action, bindingIndex))
            {
                Debug.LogWarning($"{nameof(InputRebindService)}: '{action.bindings[bindingIndex].effectivePath}' is already bound in map '{action.actionMap.name}'; change reverted.");
                action.RemoveBindingOverride(bindingIndex);
                succeeded = false;
            }

            operation.Dispose();
            RestoreEnabledMaps();

            if (succeeded)
            {
                SaveOverrides(action.actionMap.asset);
                BindingChanged?.Invoke(action);
            }

            onFinished?.Invoke(succeeded);
        }

        private static bool HasDuplicate(InputAction action, int bindingIndex)
        {
            var newBinding = action.bindings[bindingIndex];
            var map = action.actionMap;

            for (var i = 0; i < map.bindings.Count; i++)
            {
                var binding = map.bindings[i];
                if (binding.id == newBinding.id) continue;
                if (binding.isComposite) continue;
                if (binding.effectivePath != newBinding.effectivePath) continue;

                return true;
            }

            return false;
        }

        private static void CaptureEnabledMaps(InputActionAsset asset)
        {
            PreviouslyEnabledMaps.Clear();

            for (var i = 0; i < asset.actionMaps.Count; i++)
            {
                var map = asset.actionMaps[i];
                if (map.enabled) PreviouslyEnabledMaps.Add(map);
            }
        }

        private static void RestoreEnabledMaps()
        {
            for (var i = 0; i < PreviouslyEnabledMaps.Count; i++) PreviouslyEnabledMaps[i].Enable();

            PreviouslyEnabledMaps.Clear();
        }

        private static bool IsValidTarget(InputAction action, int bindingIndex)
        {
            if (action == null || action.actionMap == null || action.actionMap.asset == null)
            {
                Debug.LogError($"{nameof(InputRebindService)}: action must belong to an {nameof(InputActionAsset)}.");
                return false;
            }

            if (bindingIndex >= 0 && bindingIndex < action.bindings.Count) return true;

            Debug.LogError($"{nameof(InputRebindService)}: binding index {bindingIndex} is out of range for '{action.name}'.");
            return false;
        }
        #endregion
    }
}
