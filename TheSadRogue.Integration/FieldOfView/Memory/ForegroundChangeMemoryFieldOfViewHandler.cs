using JetBrains.Annotations;
using SadRogue.Primitives;

namespace SadRogue.Integration.FieldOfView.Memory
{
    /// <summary>
    /// Handler implementing <see cref="MemoryFieldOfViewHandlerBase"/> by changing the foreground
    /// of terrain to the specified value when it is out of FOV and the view is reliant on the player's
    /// memory.
    /// </summary>
    [PublicAPI]
    public class ForegroundChangeMemoryFieldOfViewHandler : MemoryFieldOfViewHandlerBase
    {
        /// <summary>
        /// Foreground color to set for tiles that are outside of FOV.
        /// </summary>
        public readonly Color MemoryColor;

        /// <summary>
        /// Creates a new handler with the given dimming factor.
        /// </summary>
        /// <param name="memoryColor">Foreground color to set for tiles as they move out of FOV.</param>
        /// <param name="startingState">The starting value for <see cref="FieldOfViewHandlerBase.CurrentState"/>.</param>
        public ForegroundChangeMemoryFieldOfViewHandler(Color memoryColor, State startingState = State.Enabled)
            : base(startingState)
        {
            MemoryColor = memoryColor;
        }

        /// <inheritdoc/>
        protected override void ApplyMemoryAppearance(MemoryAwareRogueLikeCell terrain)
            => terrain.LastSeenAppearance.Foreground = MemoryColor;
    }
}
