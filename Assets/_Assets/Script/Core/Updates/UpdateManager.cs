namespace LilyOfValley.Core.Updates
{
    public static class UpdateManager
    {
        #region Field

        private const int GameplayCapacity = 256;

        private const int PersistentCapacity = 32;

        private static readonly TickRegistry<IUpdatable> GameplayRegistry = new(Invoke, GameplayCapacity);

        private static readonly TickRegistry<IUpdatable> PersistentRegistry = new(Invoke, PersistentCapacity);

        #endregion

        #region Property

        public static int Count => UpdateManager.GameplayRegistry.Count + UpdateManager.PersistentRegistry.Count;

        public static int GameplayCount => UpdateManager.GameplayRegistry.Count;

        public static int PersistentCount => UpdateManager.PersistentRegistry.Count;

        public static int GameplayChunkSize
        {
            get => UpdateManager.GameplayRegistry.ChunkSize;
            set => UpdateManager.GameplayRegistry.ChunkSize = value;
        }

        #endregion

        #region Registration

        public static bool Register(IUpdatable updatable, UpdateChannel channel = UpdateChannel.Gameplay) =>
            Resolve(channel).Register(updatable);

        public static bool Unregister(IUpdatable updatable)
        {
            var leftGameplay = UpdateManager.GameplayRegistry.Unregister(updatable);
            var leftPersistent = UpdateManager.PersistentRegistry.Unregister(updatable);

            return leftGameplay || leftPersistent;
        }

        public static void Clear()
        {
            UpdateManager.GameplayRegistry.Clear();
            UpdateManager.PersistentRegistry.Clear();
        }

        #endregion

        #region Method

        public static void Tick(float deltaTime, float unscaledDeltaTime)
        {
            if (!TimeService.IsPaused) UpdateManager.GameplayRegistry.Tick(deltaTime);

            UpdateManager.PersistentRegistry.Tick(unscaledDeltaTime);
        }

        private static TickRegistry<IUpdatable> Resolve(UpdateChannel channel) =>
            channel == UpdateChannel.Persistent ? UpdateManager.PersistentRegistry : UpdateManager.GameplayRegistry;

        private static void Invoke(IUpdatable updatable, float deltaTime) => updatable.UpdateManually(deltaTime);

        #endregion
    }
}
