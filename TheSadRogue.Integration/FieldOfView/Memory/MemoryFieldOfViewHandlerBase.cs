using System;
using JetBrains.Annotations;

namespace SadRogue.Integration.FieldOfView.Memory
{
    /// <summary>
    /// Field of view handler that implements the concept of "memory", wherein the player sees previously
    /// seen tiles as they were when they were last in view until they come into view again.
    /// </summary>
    /// <remarks>
    /// All terrain objects in the map this component is added to must be of type <see cref="MemoryAwareRogueLikeCell"/>.
    /// If they are not, it will result in an exception being thrown at run-time.
    /// </remarks>
    [PublicAPI]
    public abstract class MemoryFieldOfViewHandlerBase : FieldOfViewHandlerBase
    {
        /// <summary>
        /// Creates a new handler.
        /// </summary>
        /// <param name="startingState">The starting value for <see cref="FieldOfViewHandlerBase.CurrentState"/>.</param>
        protected MemoryFieldOfViewHandlerBase(State startingState = State.Enabled)
            : base(startingState)
        {
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
        /// Makes terrain visible and sets its appearance to its regular value, and ensures that it will
        /// remain as such as long as it is visible.
        /// </summary>
        /// <param name="terrain">Terrain to modify.</param>
        protected override void UpdateTerrainSeen(RogueLikeCell terrain)
        {
            // Invalid use of MemoryFieldOfViewHandlerBase
            if (!(terrain is MemoryAwareRogueLikeCell awareTerrain))
                throw new InvalidOperationException(
                    $"{nameof(MemoryFieldOfViewHandlerBase)} must only be used on a  map that contains {nameof(MemoryAwareRogueLikeCell)} instances as its terrain.");

            // If the appearances don't match currently, synchronize them
            if (!awareTerrain.LastSeenAppearance.Matches(awareTerrain.TrueAppearance))
            {
                awareTerrain.LastSeenAppearance.CopyAppearanceFrom(awareTerrain.TrueAppearance);
                awareTerrain.LastSeenAppearance.IsVisible = awareTerrain.TrueAppearance.IsVisible;
            }


            // Set up an event handler that will keep the actual appearance in sync with the
            // true one, so long as the tile remains visible to the player.  Because this could be
            // called twice in certain state changes, we make sure we do not add the handler twice.
            //
            // Removing the event even if it does not exist does not throw exception and appears to be the
            // most performant way to account for duplicates.
            awareTerrain.TrueAppearance.IsDirtySet -= On_VisibleTileTrueAppearanceIsDirtySet;
            awareTerrain.TrueAppearance.IsDirtySet += On_VisibleTileTrueAppearanceIsDirtySet;
        }

        /// <summary>
        /// Makes terrain invisible if it is not explored.  Changes the appearance by calling
        /// <see cref="ApplyMemoryAppearance"/> if it is explored.  The terrain will continue to
        /// appear as set by that function (even if its true appearance changes) until the player
        /// sees it again.
        /// </summary>
        /// <param name="terrain">Terrain to modify.</param>
        protected override void UpdateTerrainUnseen(RogueLikeCell terrain)
        {
            // Invalid use of MemoryFieldOfViewHandlerBase
            if (!(terrain is MemoryAwareRogueLikeCell awareTerrain))
                throw new InvalidOperationException(
                    $"{nameof(MemoryFieldOfViewHandlerBase)} must only be used on a  map that contains {nameof(MemoryAwareRogueLikeCell)} instances as its terrain.");

            // If the event handler for synchronizing the appearance with true appearance is added, remove it
            // which will cause the appearance to remain as last-seen if it changes
            awareTerrain.TrueAppearance.IsDirtySet -= On_VisibleTileTrueAppearanceIsDirtySet;

            // If the unseen terrain is explored (eg. it was in FOV previously), apply the visual change
            // as appropriate
            if (Parent!.PlayerExplored[terrain.Position])
                ApplyMemoryAppearance(awareTerrain);
            else // If the unseen tile isn't explored, it's invisible
                awareTerrain.LastSeenAppearance.IsVisible = false;
        }

        /// <summary>
        /// Called when each cell goes outside of FOV, indicating that it can no longer be seen
        /// and any view of it is reliant on memory.  Should apply the "memory" appearance
        /// to the cell's <see cref="MemoryAwareRogueLikeCell.LastSeenAppearance"/>.
        /// </summary>
        /// <param name="terrain">Terrain to apply memory view to.</param>
        protected abstract void ApplyMemoryAppearance(MemoryAwareRogueLikeCell terrain);

        private void On_VisibleTileTrueAppearanceIsDirtySet(object? sender, EventArgs e)
        {
            // Sender will not be null because of event invariants.  Cast is safe since we
            // control what this handler is added to and it is checked first
            var awareTerrain = (MemoryAwareRogueLikeCell)((TerrainAppearance)sender!).Terrain;

            // If appearances are synchronized, there is nothing to do
            if (awareTerrain.LastSeenAppearance.Matches(awareTerrain.TrueAppearance))
                return;

            // Otherwise, synchronize them
            awareTerrain.LastSeenAppearance.CopyAppearanceFrom(awareTerrain.TrueAppearance);
            awareTerrain.LastSeenAppearance.IsVisible = awareTerrain.TrueAppearance.IsVisible;
        }
    }
}
