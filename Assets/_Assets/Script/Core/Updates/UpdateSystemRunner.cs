using UnityEngine;

namespace LilyOfValley.Core.Updates
{
    [DefaultExecutionOrder(UpdateSystemRunner.RunnerExecutionOrder)]
    public sealed class UpdateSystemRunner : MonoBehaviour
    {
        #region Field

        public const int RunnerExecutionOrder = -1000;

        private const string RunnerName = "[Update System]";

        private const float PausedTimeScale = 0f;

        private static UpdateSystemRunner _instance;

        #endregion

        #region Property

        public static bool SkipUpdateWhenPaused { get; set; } = true;

        public static bool IsRunning => UpdateSystemRunner._instance != null;

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            if (SkipUpdateWhenPaused && Time.timeScale <= PausedTimeScale) return;

            UpdateManager.Tick(Time.deltaTime);
        }

        private void FixedUpdate() => FixedUpdateManager.Tick(Time.fixedDeltaTime);

        private void OnDestroy()
        {
            if (UpdateSystemRunner._instance != this) return;

            UpdateSystemRunner._instance = null;
        }

        #endregion

        #region Method

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            UpdateManager.Clear();
            FixedUpdateManager.Clear();

            if (UpdateSystemRunner._instance != null) return;

            var host = new GameObject(RunnerName);
            DontDestroyOnLoad(host);
            UpdateSystemRunner._instance = host.AddComponent<UpdateSystemRunner>();
        }

        #endregion
    }
}
