using System;
using LilyOfValley.Core.Updates;
using LilyOfValley.Units.Data;
using UnityEngine;

namespace LilyOfValley.Units
{
    [DisallowMultipleComponent]
    public class Character : MonoBehaviour, IUpdatable, IFixedUpdatable
    {
        #region Field

        private const int FirstUid = 1;

        private static int _nextUid = FirstUid;

        [SerializeField] private CharacterData data;

        [SerializeField, Min(1)] private int startLevel = 1;

        [SerializeField] private bool buildOnAwake = true;

        private CharacterBehaviour[] _behaviours;
        private ICharacterTick[] _tickables;
        private ICharacterFixedTick[] _fixedTickables;
        private int _tickableCount;
        private int _fixedTickableCount;

        public event Action<Character> Built;

        public event Action<Character> Released;

        #endregion

        #region Property

        public CharacterModel Model { get; private set; }

        public CharacterData Data => this.data;

        public bool IsBuilt => Model != null;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            EnsureBehaviourCache();

            if (!this.buildOnAwake) return;

            Build(this.startLevel);
        }

        protected virtual void OnEnable()
        {
            UpdateManager.Register(this);
            FixedUpdateManager.Register(this);
        }

        protected virtual void OnDisable()
        {
            UpdateManager.Unregister(this);
            FixedUpdateManager.Unregister(this);
        }

        protected virtual void OnDestroy() => Release();

        #endregion

        #region Ticking

        public virtual void UpdateManually(float deltaTime)
        {
            if (Model == null) return;

            for (var i = 0; i < this._tickableCount; i++)
            {
                this._tickables[i].Tick(deltaTime);
            }
        }

        public virtual void FixedUpdateManually(float fixedDeltaTime)
        {
            if (Model == null) return;

            for (var i = 0; i < this._fixedTickableCount; i++)
            {
                this._fixedTickables[i].FixedTick(fixedDeltaTime);
            }
        }

        #endregion

        #region Model Binding

        public void Build(int level)
        {
            if (this.data == null)
            {
                Debug.LogError($"{nameof(Character)}: no {nameof(CharacterData)} assigned on '{name}'.", this);
                return;
            }

            Build(new CharacterModel(this.data, level, Character._nextUid++));
        }

        public void Build(CharacterModel model)
        {
            if (model == null)
            {
                Debug.LogError($"{nameof(Character)}: cannot build '{name}' from a null model.", this);
                return;
            }

            Release();
            EnsureBehaviourCache();

            Model = model;
            AttachBehaviours();
            Built?.Invoke(this);
        }

        public void Release()
        {
            if (Model == null) return;

            for (var i = 0; i < this._behaviours.Length; i++)
            {
                this._behaviours[i].Detach();
            }

            this._tickableCount = 0;
            this._fixedTickableCount = 0;
            Array.Clear(this._tickables, 0, this._tickables.Length);
            Array.Clear(this._fixedTickables, 0, this._fixedTickables.Length);

            Model.Dispose();
            Model = null;
            Released?.Invoke(this);
        }

        #endregion

        #region Behaviour Handling

        public T GetBehaviour<T>() where T : class
        {
            if (this._behaviours == null) return null;

            for (var i = 0; i < this._behaviours.Length; i++)
            {
                if (this._behaviours[i] is T match) return match;
            }

            return null;
        }

        private void EnsureBehaviourCache()
        {
            if (this._behaviours != null) return;

            this._behaviours = GetComponentsInChildren<CharacterBehaviour>(true);
            this._tickables = new ICharacterTick[this._behaviours.Length];
            this._fixedTickables = new ICharacterFixedTick[this._behaviours.Length];
        }

        private void AttachBehaviours()
        {
            this._tickableCount = 0;
            this._fixedTickableCount = 0;

            for (var i = 0; i < this._behaviours.Length; i++)
            {
                var behaviour = this._behaviours[i];
                behaviour.Attach(this);

                if (behaviour is ICharacterTick tickable)
                {
                    this._tickables[this._tickableCount] = tickable;
                    this._tickableCount++;
                }

                if (behaviour is not ICharacterFixedTick fixedTickable) continue;

                this._fixedTickables[this._fixedTickableCount] = fixedTickable;
                this._fixedTickableCount++;
            }
        }

        #endregion
    }
}
