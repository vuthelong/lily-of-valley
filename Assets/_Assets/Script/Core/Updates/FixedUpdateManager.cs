namespace LilyOfValley.Core.Updates
{
    public static class FixedUpdateManager
    {
        #region Field

        private const int InitialCapacity = 128;

        private static readonly TickRegistry<IFixedUpdatable> Registry =
            new(static (updatable, fixedDeltaTime) => updatable.FixedUpdateManually(fixedDeltaTime), InitialCapacity);

        #endregion

        #region Property

        public static int Count => FixedUpdateManager.Registry.Count;

        #endregion

        #region Method

        public static bool Register(IFixedUpdatable updatable) => FixedUpdateManager.Registry.Register(updatable);

        public static bool Unregister(IFixedUpdatable updatable) => FixedUpdateManager.Registry.Unregister(updatable);

        public static void Tick(float fixedDeltaTime) => FixedUpdateManager.Registry.Tick(fixedDeltaTime);

        public static void Clear() => FixedUpdateManager.Registry.Clear();

        #endregion
    }
}
