using System;
using System.Linq;
using JetBrains.Annotations;
using SadConsole;
using SadRogue.Primitives;

namespace SadRogue.Integration.FieldOfView
{
    /// <summary>
    /// Basic field of view handler that makes entities outside of FOV invisible, and makes
    /// terrain outside of FOV invisible if it is not explored, and tinted a darker color if they are
    /// explored.
    /// </summary>
    /// <remarks>
    /// This implementation is fairly basic, and does not directly implement more complex
    /// concepts such as memory.
    ///
    /// An example of player "memory" is implemented in <see cref="Memory.MemoryFieldOfViewHandlerBase"/>,
    /// but for cases like this, or cases where you want to handle FOV differently, feel free to
    /// create your own implementation of <see cref="FieldOfViewHandlerBase"/>.  This one
    /// may at least serve as an example, even if it does not fit your use case.
    /// </remarks>
    [PublicAPI]
    public class BasicFieldOfViewHandler : FieldOfViewHandlerBase
    {
        /// <summary>
        /// Decorator being used to tint terrain that is outside of FOV but has been explored.
        /// </summary>
        public CellDecorator ExploredDecorator { get; }

        /// <summary>
        /// Creates a new handler component.
        /// </summary>
        /// <param name="exploredColorTint">
        /// Color to use for tinting.  Should generally be partially transparent.
        /// Defaults to (0.05f, 0.05f, 0.05f, 0.75f).
        /// </param>
        /// <param name="tintGlyph">
        /// Glyph to use for tinting squares; should generally be a fully solid glyph.
        /// </param>
        /// <param name="startingState">The starting value for <see cref="FieldOfViewHandlerBase.CurrentState"/>.</param>
        public BasicFieldOfViewHandler(Color? exploredColorTint = null, int tintGlyph = 219, State startingState = State.Enabled)
            : base(startingState)
        {
            ExploredDecorator = new CellDecorator(
                exploredColorTint ?? new Color(0.05f, 0.05f, 0.05f, 0.75f),
                tintGlyph,
                Mirror.None);
        }

        /// <summary>
        /// Makes entity visible.
        /// </summary>
        /// <param name="entity">Entity to modify.</param>
        protected override void UpdateEntitySeen(RogueLikeEntity entity) => entity.IsVisible = true;

        /// <summary>
        /// Makes entity invisible.
        /// </summary>
        /// <param name="entity">Entity to modify.</param>
        protected override void UpdateEntityUnseen(RogueLikeEntity entity) => entity.IsVisible = false;

        /// <summary>
        /// Makes terrain visible and sets its foreground color to its regular value.
        /// </summary>
        /// <param name="terrain">Terrain to modify.</param>
        protected override void UpdateTerrainSeen(RogueLikeCell terrain)
        {
            terrain.Appearance.IsVisible = true;
            if (terrain.Appearance.Decorators.Contains(ExploredDecorator))
            {
                // If there is only 1 decorator, it must be ours so we can replace
                // the array with a static blank one
                terrain.Appearance.Decorators = terrain.Appearance.Decorators.Length == 1 ? Array.Empty<CellDecorator>() : terrain.Appearance.Decorators.Where(i => i != ExploredDecorator).ToArray();
            }
        }

        /// <summary>
        /// Makes terrain invisible if it is not explored.  Makes terrain visible but tints it using
        /// <see cref="ExploredDecorator"/> if it is explored.
        /// </summary>
        /// <param name="terrain">Terrain to modify.</param>
        protected override void UpdateTerrainUnseen(RogueLikeCell terrain)
        {
            // Parent can't be null because of invariants enforced by structure for when
            // this function is called
            var parent = Parent!;

            // If the unseen terrain is outside of FOV, apply the decorator to tint the square appropriately.
            if (parent.PlayerExplored[terrain.Position])
                terrain.Appearance.Decorators = terrain.Appearance.Decorators.Append(ExploredDecorator).ToArray();
            else // If the unseen tile isn't explored, it's invisible
                terrain.Appearance.IsVisible = false;
        }
    }
}
