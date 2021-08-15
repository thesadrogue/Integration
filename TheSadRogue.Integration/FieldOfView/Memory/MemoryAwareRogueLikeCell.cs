using System;
using GoRogue.Components;
using JetBrains.Annotations;
using SadConsole;
using SadRogue.Primitives;

namespace SadRogue.Integration.FieldOfView.Memory
{
    /// <summary>
    /// A roguelike cell that is aware of the concept of player "memory", allowing it to be displayed
    /// as the player last saw it when it is not directly visible.  Designed to be used with a
    /// <see cref="MemoryFieldOfViewHandlerBase"/>.
    /// </summary>
    /// <remarks>
    /// For this cell, its <see cref="RogueLikeCell.Appearance"/> represents how it currently appears,
    /// which may or may not be its actual appearance.  Its <see cref="TrueAppearance"/> records its
    /// actual intended appearance.
    ///
    /// It must be used with a <see cref="MemoryFieldOfViewHandlerBase"/>, which is a map component that will
    /// modify the appearances of the cells as appropriate for the current status.
    /// </remarks>
    [PublicAPI]
    public class MemoryAwareRogueLikeCell : RogueLikeCell
    {
        /// <summary>
        /// The cell's true appearance.
        /// </summary>
        public TerrainAppearance TrueAppearance { get; }

        /// <summary>
        /// The appearance as last seen by the player.
        /// </summary>
        public TerrainAppearance LastSeenAppearance => Appearance;

        /// <summary>
        /// Creates a new terrain object.
        /// </summary>
        /// <param name="position">Where the cell is located</param>
        /// <param name="foreground">The true foreground color of the Cell</param>
        /// <param name="background">The true background color of the Cell</param>
        /// <param name="glyph">The true glyph of the cell</param>
        /// <param name="layer">The map layer to which this cell is added</param>
        /// <param name="walkable">Whether the cell is considered walkable in collision detection</param>
        /// <param name="transparent">Whether the cell is considered transparent in field-of-view algorithms</param>
        /// <param name="idGenerator">The function which produces the unique ID for this cell</param>
        /// <param name="customComponentContainer">Accepts a custom collection</param>
        public MemoryAwareRogueLikeCell(Point position, Color foreground, Color background, int glyph, int layer,
            bool walkable = true, bool transparent = true, Func<uint>? idGenerator = null,
            IComponentCollection? customComponentContainer = null)
            : base(position, foreground, background, glyph, layer, walkable, transparent, idGenerator, customComponentContainer)
        {
            // Given appearance was assigned to Appearance, so copy it to TrueAppearance
            TrueAppearance = new TerrainAppearance(Appearance.Terrain, Appearance);

            // Since the tile hasn't been seen, make it invisible.
            LastSeenAppearance.IsVisible = false;
        }

        /// <summary>
        /// Creates a new terrain object.
        /// </summary>
        /// <param name="position">Where the cell is located</param>
        /// <param name="appearance">The true appearance of the cell.</param>
        /// <param name="layer">The map layer to which this cell is added</param>
        /// <param name="walkable">Whether the cell is considered walkable in collision detection</param>
        /// <param name="transparent">Whether the cell is considered transparent in field-of-view algorithms</param>
        /// <param name="idGenerator">The function which produces the unique ID for this cell</param>
        /// <param name="customComponentContainer">Accepts a custom collection</param>
        public MemoryAwareRogueLikeCell(Point position, ColoredGlyph appearance, int layer, bool walkable = true, bool transparent = true,
            Func<uint>? idGenerator = null, IComponentCollection? customComponentContainer = null)
            : base(position, appearance, layer, walkable, transparent, idGenerator, customComponentContainer)
        {
            // Given appearance was assigned to Appearance, so copy it to TrueAppearance
            TrueAppearance = new TerrainAppearance(Appearance.Terrain, Appearance);

            // Since the tile hasn't been seen, make it invisible.
            LastSeenAppearance.IsVisible = false;
        }
    }
}
