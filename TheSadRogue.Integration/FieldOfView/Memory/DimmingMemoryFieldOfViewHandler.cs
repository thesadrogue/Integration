using JetBrains.Annotations;

namespace SadRogue.Integration.FieldOfView.Memory
{
    /// <summary>
    /// Handler implementing <see cref="MemoryFieldOfViewHandlerBase"/> by dimming the foreground
    /// and background colors of terrain when it is out of FOV and the view is reliant on the player's
    /// memory.
    /// </summary>
    [PublicAPI]
    public class DimmingMemoryFieldOfViewHandler : MemoryFieldOfViewHandlerBase
    {
        /// <summary>
        /// A factor to multiply terrain's colors by to dim them.
        /// </summary>
        public readonly float DimmingFactor;

        /// <summary>
        /// Creates a new handler with the given dimming factor.
        /// </summary>
        /// <param name="dimmingFactor">A factor to multiply terrain's colors by to dim them.</param>
        /// <param name="startingState">The starting value for <see cref="FieldOfViewHandlerBase.CurrentState"/>.</param>
        public DimmingMemoryFieldOfViewHandler(float dimmingFactor, State startingState = State.Enabled)
            : base(startingState)
        {
            DimmingFactor = dimmingFactor;
        }

        /// <inheritdoc/>
        protected override void ApplyMemoryAppearance(MemoryAwareRogueLikeCell terrain)
        {
            // The last-seen is kept up to date, so all we have to do is modify it accordingly
            terrain.LastSeenAppearance.Foreground *= DimmingFactor;
            terrain.LastSeenAppearance.Background *= DimmingFactor;
        }
    }
}
