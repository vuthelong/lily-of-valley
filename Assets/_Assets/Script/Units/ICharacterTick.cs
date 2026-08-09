namespace LilyOfValley.Units
{
    /// <summary>
    /// Implemented by behaviours that need per-frame work. <see cref="Character"/> drives them from
    /// a single Update, so behaviours never pay for their own Unity message.
    /// </summary>
    public interface ICharacterTick
    {
        void Tick(float deltaTime);
    }
}
