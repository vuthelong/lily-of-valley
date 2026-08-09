using UnityEngine;

namespace LilyOfValley.Core
{
    public static class CursorService
    {
        #region Property

        public static bool IsLocked { get; private set; }

        #endregion

        #region Method

        public static void SetLocked(bool isLocked)
        {
            IsLocked = isLocked;
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLocked;
        }

        #endregion
    }
}
