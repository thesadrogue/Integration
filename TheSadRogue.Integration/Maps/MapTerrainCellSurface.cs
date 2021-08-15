using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SadConsole;
using SadConsole.Effects;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace SadRogue.Integration.Maps
{
    /// <summary>
    /// A CellSurface that renders the terrain layer of a map.
    /// </summary>
    [PublicAPI]
    public class MapTerrainCellSurface : GridViewBase<ColoredGlyph>, ICellSurface
    {
        private bool _isDirty = true;
        private readonly BoundedRectangle _viewArea;
        private Color _defaultBackground;
        private Color _defaultForeground;
        private readonly RogueLikeMap _map;

        #region Properties/Indexers
        /// <inheritdoc />
        public override int Height => _viewArea.BoundingBox.Height;

        /// <inheritdoc />
        public override int Width => _viewArea.BoundingBox.Width;

        /// <inheritdoc />
        public override ColoredGlyph this[Point pos] => _map.TerrainView[pos];

        /// <inheritdoc />
        public Rectangle Area => _viewArea.BoundingBox;

        /// <inheritdoc />
        public int TimesShiftedDown { get; set; }

        /// <inheritdoc />
        public int TimesShiftedRight { get; set; }

        /// <inheritdoc />
        public int TimesShiftedLeft { get; set; }

        /// <inheritdoc />
        public int TimesShiftedUp { get; set; }

        /// <inheritdoc />
        public bool UsePrintProcessor { get; set; }

        /// <inheritdoc />
        public EffectsManager Effects { get; }

        /// <inheritdoc />
        public Color DefaultBackground
        {
            get => _defaultForeground;
            set { _defaultForeground = value; IsDirty = true; }
        }
        /// <inheritdoc />
        public Color DefaultForeground
        {
            get => _defaultBackground;
            set { _defaultBackground = value; IsDirty = true; }
        }

        /// <inheritdoc />
        public int DefaultGlyph { get; set; }

        /// <inheritdoc />
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    IsDirtyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc />
        public bool IsScrollable => Height != _viewArea.Area.Height || Width != _viewArea.Area.Width;

        /// <inheritdoc />
        public Rectangle View
        {
            get => _viewArea.Area;
            set
            {
                _viewArea.SetArea(value);
                IsDirty = true;
            }
        }

        /// <inheritdoc />
        public int ViewHeight
        {
            get => _viewArea.Area.Height;
            set => _viewArea.SetArea(_viewArea.Area.WithHeight(value));
        }

        /// <inheritdoc />
        public Point ViewPosition
        {
            get => _viewArea.Area.Position;
            set
            {
                _viewArea.SetArea(_viewArea.Area.WithPosition(value));
                IsDirty = true;
            }
        }

        /// <inheritdoc />
        public int ViewWidth
        {
            get => _viewArea.Area.Width;
            set => _viewArea.SetArea(_viewArea.Area.WithWidth(value));
        }

        /// <inheritdoc />
        public event EventHandler? IsDirtyChanged;
        #endregion

        #region Initialization
        /// <summary>
        /// Create a new MapTerrainCellSurface
        /// </summary>
        /// <param name="map">The map which we are rendering</param>
        /// <param name="viewWidth">The height of the view (screen size)</param>
        /// <param name="viewHeight">The Width of the view (screen size)</param>
        public MapTerrainCellSurface(RogueLikeMap map, int viewWidth, int viewHeight)
        {
            _map = map;
            Effects = new EffectsManager(this);

            _viewArea = new BoundedRectangle((0, 0, viewWidth, viewHeight),
                (0, 0, map.Width, map.Height));
        }
        #endregion

        // Disabled nullability check because the issue is due to SadConsole not annotating nullability
        /// <inheritdoc />
#pragma warning disable 8613
        public IEnumerator<ColoredGlyph?> GetEnumerator()
#pragma warning restore 8613
        {
            foreach (var pos in this.Positions())
                yield return this[pos];
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
