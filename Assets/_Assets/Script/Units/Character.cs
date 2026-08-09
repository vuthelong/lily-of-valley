using System;
using LilyOfValley.Units.Data;
using UnityEngine;

namespace LilyOfValley.Units
{
    /// <summary>
    /// Scene presence of a character: owns its <see cref="CharacterModel"/> and its
    /// <see cref="CharacterBehaviour"/> components, and drives their tick from one Update.
    /// </summary>
    [DisallowMultipleComponent]
    public class Character : MonoBehaviour
    {
        #region Fields
        private static int _nextUid = 1;

        public event Action<Character> Built;
        public event Action<Character> Released;

        [SerializeField] private CharacterData data;
        [SerializeField, Min(1)] private int startLevel = 1;
        [SerializeField] private bool buildOnAwake = true;

        private CharacterBehaviour[] _behaviours;
        private ICharacterTick[] _tickables;
        private int _tickableCount;
        #endregion

        #region Properties
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

        protected virtual void Update()
        {
            if (Model == null) return;

            var deltaTime = Time.deltaTime;
            for (var i = 0; i < this._tickableCount; i++) this._tickables[i].Tick(deltaTime);
        }

        protected virtual void OnDestroy() => Release();
        #endregion

        #region Public Methods
        public void Build(int level)
        {
            if (this.data == null)
            {
                Debug.LogError($"{nameof(Character)}: no {nameof(CharacterData)} assigned on '{name}'.", this);
                return;
            }

            Build(new CharacterModel(this.data, level, _nextUid++));
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

            for (var i = 0; i < this._behaviours.Length; i++) this._behaviours[i].Detach();

            this._tickableCount = 0;
            Array.Clear(this._tickables, 0, this._tickables.Length);

            Model.Dispose();
            Model = null;
            Released?.Invoke(this);
        }

        public T GetBehaviour<T>() where T : class
        {
            if (this._behaviours == null) return null;

            for (var i = 0; i < this._behaviours.Length; i++)
            {
                if (this._behaviours[i] is T match) return match;
            }

            return null;
        }
        #endregion

        #region Private Methods
        private void EnsureBehaviourCache()
        {
            if (this._behaviours != null) return;

            this._behaviours = GetComponentsInChildren<CharacterBehaviour>(true);
            this._tickables = new ICharacterTick[this._behaviours.Length];
        }

        private void AttachBehaviours()
        {
            this._tickableCount = 0;

            for (var i = 0; i < this._behaviours.Length; i++)
            {
                var behaviour = this._behaviours[i];
                behaviour.Attach(this);

                if (behaviour is not ICharacterTick tickable) continue;

                this._tickables[this._tickableCount] = tickable;
                this._tickableCount++;
            }
        }
        #endregion
    }
}
