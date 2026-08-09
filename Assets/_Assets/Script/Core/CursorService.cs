using UnityEngine;

namespace LilyOfValley.Core
{
    public static class CursorService
    {
        #region Properties
        public static bool IsLocked { get; private set; }
        #endregion

        #region Public Methods
        public static void SetLocked(bool locked)
        {
            IsLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
        #endregion
    }
}
