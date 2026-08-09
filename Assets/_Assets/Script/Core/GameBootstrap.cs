using LilyOfValley.Inputs;
using UnityEngine;

namespace LilyOfValley.Core
{
    [DefaultExecutionOrder(BootstrapExecutionOrder)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        #region Field

        private const int BootstrapExecutionOrder = -100;

        [SerializeField] private InputReader inputReader;

        [SerializeField] private bool lockCursorOnStart = true;

        [SerializeField] private bool keepAliveBetweenScenes;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (this.inputReader == null)
            {
                Debug.LogError($"{nameof(GameBootstrap)}: {nameof(this.inputReader)} is not assigned.", this);
                enabled = false;
                return;
            }

            this.inputReader.Initialize();

            if (this.keepAliveBetweenScenes) DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            this.inputReader.EnableGameplay();
            this.inputReader.EnableUI();
        }

        private void Start() => CursorService.SetLocked(this.lockCursorOnStart);

        private void OnDisable()
        {
            if (this.inputReader == null) return;

            this.inputReader.DisableAll();
        }

        #endregion
    }
}
