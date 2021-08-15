using JetBrains.Annotations;
using SadConsole;
using SadRogue.Primitives;

namespace SadRogue.Integration
{
    /// <summary>
    /// An object representing the appearance of a piece of terrain.
    /// </summary>
    /// <remarks>
    /// Effectively, this is just a ColoredGlyph from SadConsole that is aware of the GameObject it is
    /// representing the appearance for.  This is useful because the object is aware of its position.
    /// </remarks>
    [PublicAPI]
    public class TerrainAppearance : ColoredGlyph
    {
        /// <summary>
        /// The terrain object that this appearance represents.
        /// </summary>
        public readonly RogueLikeCell Terrain;

        /// <summary>
        /// Creates a cell with a white foreground, black background, glyph 0, and no mirror effect.
        /// </summary>
        /// <param name="terrain">The terrain object this appearance represents.</param>
        public TerrainAppearance(RogueLikeCell terrain)
        {
            Terrain = terrain;
        }

        /// <summary>
        /// Creates a cell with the specified foreground, black background, glyph 0, and no mirror effect.
        /// </summary>
        /// <param name="terrain">The terrain object this appearance represents.</param>
        /// <param name="foreground">Foreground color.</param>
        public TerrainAppearance(RogueLikeCell terrain, Color foreground)
            : base(foreground)
        {
            Terrain = terrain;
        }

        /// <summary>
        /// Creates a cell with the specified foreground, specified background, glyph 0, and no mirror effect.
        /// </summary>
        /// <param name="terrain">The terrain object this appearance represents.</param>
        /// <param name="foreground">Foreground color.</param>
        /// <param name="background">Background color.</param>
        public TerrainAppearance(RogueLikeCell terrain, Color foreground, Color background)
            : base(foreground, background)
        {
            Terrain = terrain;
        }

        /// <summary>
        /// Creates a cell with the specified foreground, background, and glyph, with no mirror effect.
        /// </summary>
        /// <param name="terrain">The terrain object this appearance represents.</param>
        /// <param name="foreground">Foreground color.</param>
        /// <param name="background">Background color.</param>
        /// <param name="glyph">The glyph index.</param>
        public TerrainAppearance(RogueLikeCell terrain, Color foreground, Color background, int glyph)
            : base(foreground, background, glyph)
        {
            Terrain = terrain;
        }

        /// <summary>
        /// Creates a cell with the specified foreground, background, glyph, and mirror effect.
        /// </summary>
        /// <param name="terrain">The terrain object this appearance represents.</param>
        /// <param name="foreground">Foreground color.</param>
        /// <param name="background">Background color.</param>
        /// <param name="glyph">The glyph index.</param>
        /// <param name="mirror">The mirror effect.</param>
        public TerrainAppearance(RogueLikeCell terrain, Color foreground, Color background, int glyph, Mirror mirror)
            : base(foreground, background, glyph, mirror)
        {
            Terrain = terrain;
        }

        /// <summary>
        /// Creates a cell with the specified foreground, background, glyph, mirror, and visibility.
        /// </summary>
        /// <param name="terrain">The terrain object this appearance represents.</param>
        /// <param name="foreground">Foreground color.</param>
        /// <param name="background">Background color.</param>
        /// <param name="glyph">The glyph index.</param>
        /// <param name="mirror">The mirror effect.</param>
        /// <param name="isVisible">The visibility of the glyph.</param>
        public TerrainAppearance(RogueLikeCell terrain, Color foreground, Color background, int glyph, Mirror mirror, bool isVisible)
            : base(foreground, background, glyph, mirror, isVisible)
        {
            Terrain = terrain;
        }

        /// <summary>
        /// Creates a cell with the specified foreground, background, glyph, and mirror effect.
        /// </summary>
        /// <param name="terrain">The terrain object this appearance represents.</param>
        /// <param name="foreground">Foreground color.</param>
        /// <param name="background">Background color.</param>
        /// <param name="glyph">The glyph index.</param>
        /// <param name="mirror">The mirror effect.</param>
        /// <param name="isVisible">The visibility of the glyph.</param>
        /// <param name="decorators">Decorators for the cell.</param>
        public TerrainAppearance(RogueLikeCell terrain, Color foreground, Color background, int glyph, Mirror mirror, bool isVisible, CellDecorator[] decorators)
            : base(foreground, background, glyph, mirror, isVisible, decorators)
        {
            Terrain = terrain;
        }

        /// <summary>
        /// Creates a cell with the specified appearance.
        /// </summary>
        /// <param name="terrain">The terrain object this appearance represents.</param>
        /// <param name="appearance">The appearance to copy from.</param>
        public TerrainAppearance(RogueLikeCell terrain, ColoredGlyph appearance)
        {
            Terrain = terrain;
            CopyAppearanceFrom(appearance);
        }
    }
}
