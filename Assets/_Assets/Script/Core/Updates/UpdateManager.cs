namespace LilyOfValley.Core.Updates
{
    public static class UpdateManager
    {
        #region Field

        private const int InitialCapacity = 256;

        private static readonly TickRegistry<IUpdatable> Registry =
            new(static (updatable, deltaTime) => updatable.UpdateManually(deltaTime), InitialCapacity);

        #endregion

        #region Property

        public static int Count => UpdateManager.Registry.Count;

        public static int ChunkSize
        {
            get => UpdateManager.Registry.ChunkSize;
            set => UpdateManager.Registry.ChunkSize = value;
        }

        #endregion

        #region Method

        public static bool Register(IUpdatable updatable) => UpdateManager.Registry.Register(updatable);

        public static bool Unregister(IUpdatable updatable) => UpdateManager.Registry.Unregister(updatable);

        public static void Tick(float deltaTime) => UpdateManager.Registry.Tick(deltaTime);

        public static void Clear() => UpdateManager.Registry.Clear();

        #endregion
    }
}
