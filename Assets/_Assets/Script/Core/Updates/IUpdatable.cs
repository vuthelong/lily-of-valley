namespace LilyOfValley.Core.Updates
{
    public interface IUpdatable
    {
        void UpdateManually(float deltaTime);
    }

    public interface IFixedUpdatable
    {
        void FixedUpdateManually(float fixedDeltaTime);
    }
}
