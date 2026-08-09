using UnityEngine;

namespace LilyOfValley.Units
{
    public abstract class CharacterBehaviour : MonoBehaviour
    {
        #region Property

        public Character Owner { get; private set; }

        public CharacterModel Model { get; private set; }

        public bool IsAttached => Owner != null && Model != null;

        #endregion

        #region Attachment

        public void Attach(Character owner)
        {
            if (owner == null || owner.Model == null) return;

            Owner = owner;
            Model = owner.Model;
            OnAttached();
        }

        public void Detach()
        {
            if (!IsAttached) return;

            OnDetached();
            Owner = null;
            Model = null;
        }

        #endregion

        #region Attachment Hook

        protected virtual void OnAttached() { }

        protected virtual void OnDetached() { }

        #endregion
    }
}
