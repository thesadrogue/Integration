using System;
using GoRogue.Components;
using GoRogue.GameFramework;
using JetBrains.Annotations;
using SadConsole;
using SadRogue.Primitives;

namespace SadRogue.Integration
{
    /// <summary>
    /// A GoRogue GameObject with a ColoredGlyph, designed to represent terrain on a map.
    /// </summary>
    /// <remarks>
    /// This class creates GameObjects that reside on terrain layer 0 in GoRogue's map.
    /// When they are added to a map, they automatically render on any screens displaying
    /// that map as applicable, according to their <see cref="Appearance"/>.
    /// </remarks>
    [PublicAPI]
    public class RogueLikeCell : GameObject
    {
        /// <summary>
        /// The ColoredGlyph of this cell
        /// </summary>
        public TerrainAppearance Appearance { get; }

        /// <summary>
        /// Creates a new RogueLikeCell
        /// </summary>
        /// <param name="position">Where the Cell is located</param>
        /// <param name="foreground">The Foreground color of the Cell</param>
        /// <param name="background">The Foreground color of the Cell</param>
        /// <param name="glyph">The symbol of the Cell</param>
        /// <param name="layer">The map layer to which this Cell is added</param>
        /// <param name="walkable">Whether the Cell is considered in Collision Detection</param>
        /// <param name="transparent">Whether the Cell is considered in Field-of-View algorithms</param>
        /// <param name="idGenerator">The function which produces the unique ID for this Cell</param>
        /// <param name="customComponentContainer">Accepts a custom collection</param>
        public RogueLikeCell(Point position, Color foreground, Color background, int glyph, int layer,
            bool walkable = true, bool transparent = true, Func<uint>? idGenerator = null,
            IComponentCollection? customComponentContainer = null)
            : base(position, layer, walkable, transparent, idGenerator, customComponentContainer)
        {
            Appearance = new TerrainAppearance(this, foreground, background, glyph);
        }
        /// <summary>
        /// Creates a new RogueLikeCell
        /// </summary>
        /// <param name="position">Where the Cell is located</param>
        /// <param name="appearance">The Foreground color of the Cell</param>
        /// <param name="layer">The map layer to which this Cell is added</param>
        /// <param name="walkable">Whether the Cell is considered in Collision Detection</param>
        /// <param name="transparent">Whether the Cell is considered in Field-of-View algorithms</param>
        /// <param name="idGenerator">The function which produces the unique ID for this Cell</param>
        /// <param name="customComponentContainer">Accepts a custom collection</param>
        public RogueLikeCell(Point position, ColoredGlyph appearance, int layer, bool walkable = true, bool transparent = true,
            Func<uint>? idGenerator = null, IComponentCollection? customComponentContainer = null)
            : base(position, layer, walkable, transparent, idGenerator, customComponentContainer)
        {
            Appearance = new TerrainAppearance(this, appearance);
        }
    }
}
