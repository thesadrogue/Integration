using System;
using GoRogue.Components;
using GoRogue.GameFramework;
using GoRogue.Random;
using JetBrains.Annotations;
using SadConsole;
using SadConsole.Components;
using SadConsole.Entities;
using SadRogue.Primitives;

namespace SadRogue.Integration
{
    /// <summary>
    /// A GoRogue GameObject that also inherits from SadConsole's Entity, designed to
    /// represent and render non-terrain objects in a GoRogue map.
    /// </summary>
    /// <remarks>
    /// This class creates objects that reside on the entity (non-0) layers of a GoRogue
    /// Map.  When they are added to a map, they automatically render on any screens displaying
    /// that map as applicable.
    /// </remarks>
    [PublicAPI]
    public partial class RogueLikeEntity : Entity, IGameObject
    {
        /// <summary>
        /// Each and every component on this entity.
        /// </summary>
        /// <remarks>
        /// Confused about which collection to add a component to?
        /// Add it here.  Any SadConsole components added to this collection are automatically
        /// tracked by SadConsole, and all components added here will appear in GoRogue's
        /// component collection as well.
        /// </remarks>
        public IComponentCollection AllComponents => GoRogueComponents;

        #region Initialization

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="position">Position the entity will start at.</param>
        /// <param name="glyph">The entity's glyph when displayed.</param>
        /// <param name="walkable">Whether or not the entity can be walked through.</param>
        /// <param name="transparent">Whether or not the entity is transparent for the purposes of FOV.</param>
        /// <param name="layer">The layer on which the entity resides. Must NOT be 0, because layer 0 is reserved for terrain.</param>
        /// <param name="idGenerator">
        /// The function used to generate and return an unsigned integer to use assign to the <see cref="ID" /> field.
        /// Most of the time, you will not need to specify this as the default implementation will be sufficient.
        /// </param>
        /// <param name="customComponentCollection">
        /// A custom component collection to use for objects.  If not specified, a <see cref="ComponentCollection"/> is
        /// used.  Typically you will not need to specify this, as a ComponentCollection is sufficient for nearly all
        /// use cases.
        /// </param>
        public RogueLikeEntity(Point position, int glyph, bool walkable = true, bool transparent = true, int layer = 1,
            Func<uint>? idGenerator = null, IComponentCollection? customComponentCollection = null)
            : base(Color.White, Color.Transparent, glyph, CheckLayer(layer))
        {
            GoRogueInitialize(position, walkable, transparent, idGenerator, customComponentCollection);
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="position">Position the entity will start at.</param>
        /// <param name="foreground">The foreground for the entity's glyph when displayed.</param>
        /// <param name="glyph">The entity's glyph when displayed.</param>
        /// <param name="walkable">Whether or not the entity can be walked through.</param>
        /// <param name="transparent">Whether or not the entity is transparent for the purposes of FOV.</param>
        /// <param name="layer">The layer on which the entity resides. Must NOT be 0, because layer 0 is reserved for terrain.</param>
        /// <param name="idGenerator">
        /// The function used to generate and return an unsigned integer to use assign to the <see cref="ID" /> field.
        /// Most of the time, you will not need to specify this as the default implementation will be sufficient.
        /// </param>
        /// <param name="customComponentCollection">
        /// A custom component collection to use for objects.  If not specified, a <see cref="ComponentCollection"/> is
        /// used.  Typically you will not need to specify this, as a ComponentCollection is sufficient for nearly all
        /// use cases.
        /// </param>
        public RogueLikeEntity(Point position, Color foreground, int glyph, bool walkable = true,
            bool transparent = true, int layer = 1, Func<uint>? idGenerator = null,
            IComponentCollection? customComponentCollection = null)
            : base(foreground, Color.Transparent, glyph, CheckLayer(layer))
        {
            GoRogueInitialize(position, walkable, transparent, idGenerator, customComponentCollection);
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="position">Position the entity will start at.</param>
        /// <param name="foreground">The foreground for the entity's glyph when displayed.</param>
        /// <param name="background">The background for the entity's glyph when displayed.</param>
        /// <param name="glyph">The entity's glyph when displayed.</param>
        /// <param name="walkable">Whether or not the entity can be walked through.</param>
        /// <param name="transparent">Whether or not the entity is transparent for the purposes of FOV.</param>
        /// <param name="layer">The layer on which the entity resides. Must NOT be 0, because layer 0 is reserved for terrain.</param>
        /// <param name="idGenerator">
        /// The function used to generate and return an unsigned integer to use assign to the <see cref="ID" /> field.
        /// Most of the time, you will not need to specify this as the default implementation will be sufficient.
        /// </param>
        /// <param name="customComponentCollection">
        /// A custom component collection to use for objects.  If not specified, a <see cref="ComponentCollection"/> is
        /// used.  Typically you will not need to specify this, as a ComponentCollection is sufficient for nearly all
        /// use cases.
        /// </param>
        public RogueLikeEntity(Point position, Color foreground, Color background, int glyph, bool walkable = true, bool transparent = true, int layer = 1, Func<uint>? idGenerator = null, IComponentCollection? customComponentCollection = null)
            : base(foreground, background, glyph, CheckLayer(layer))
        {
            GoRogueInitialize(position, walkable, transparent, idGenerator, customComponentCollection);
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="position">Position the entity will start at.</param>
        /// <param name="appearance">The appearance of the entity when displayed.  A copy will be made of this value.</param>
        /// <param name="walkable">Whether or not the entity can be walked through.</param>
        /// <param name="transparent">Whether or not the entity is transparent for the purposes of FOV.</param>
        /// <param name="layer">The layer on which the entity resides. Must NOT be 0, because layer 0 is reserved for terrain.</param>
        /// <param name="idGenerator">
        /// The function used to generate and return an unsigned integer to use assign to the <see cref="ID" /> field.
        /// Most of the time, you will not need to specify this as the default implementation will be sufficient.
        /// </param>
        /// <param name="customComponentCollection">
        /// A custom component collection to use for objects.  If not specified, a <see cref="ComponentCollection"/> is
        /// used.  Typically you will not need to specify this, as a ComponentCollection is sufficient for nearly all
        /// use cases.
        /// </param>
        public RogueLikeEntity(Point position, ColoredGlyph appearance, bool walkable = true, bool transparent = true, int layer = 1, Func<uint>? idGenerator = null, IComponentCollection? customComponentCollection = null)
            : base(appearance, CheckLayer(layer))
        {
            GoRogueInitialize(position, walkable, transparent, idGenerator, customComponentCollection);
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="position">Position the entity will start at.</param>
        /// <param name="appearance">The appearance of the entity when displayed.  No copy will be made of this value; the given instance will be used directly.</param>
        /// <param name="walkable">Whether or not the entity can be walked through.</param>
        /// <param name="transparent">Whether or not the entity is transparent for the purposes of FOV.</param>
        /// <param name="layer">The layer on which the entity resides. Must NOT be 0, because layer 0 is reserved for terrain.</param>
        /// <param name="idGenerator">
        /// The function used to generate and return an unsigned integer to use assign to the <see cref="ID" /> field.
        /// Most of the time, you will not need to specify this as the default implementation will be sufficient.
        /// </param>
        /// <param name="customComponentCollection">
        /// A custom component collection to use for objects.  If not specified, a <see cref="ComponentCollection"/> is
        /// used.  Typically you will not need to specify this, as a ComponentCollection is sufficient for nearly all
        /// use cases.
        /// </param>
        public RogueLikeEntity(Point position, ref ColoredGlyph appearance, bool walkable = true, bool transparent = true, int layer = 1, Func<uint>? idGenerator = null, IComponentCollection? customComponentCollection = null)
            : base(ref appearance, CheckLayer(layer))
        {
            GoRogueInitialize(position, walkable, transparent, idGenerator, customComponentCollection);
        }


        private void GoRogueInitialize(Point position, bool walkable = true, bool transparent = true,
            Func<uint>? idGenerator = null, IComponentCollection? customComponentContainer = null)
        {
            idGenerator ??= GlobalRandom.DefaultRNG.NextUInt;

            Position = position;
            PositionChanged += Position_Changed;

            IsWalkable = walkable;
            IsTransparent = transparent;

            ID = idGenerator();
            GoRogueComponents = customComponentContainer ?? new ComponentCollection();
            GoRogueComponents.ParentForAddedComponents = this;
            AllComponents.ComponentAdded += On_GoRogueComponentAdded;
            AllComponents.ComponentRemoved += On_GoRogueComponentRemoved;
        }

        private static int CheckLayer(int layer) => layer != 0 ? layer : throw new ArgumentException($"{nameof(RogueLikeEntity)} objects may not reside on the terrain layer.", nameof(layer));
        #endregion

        #region Synchronization Handlers

        private void On_GoRogueComponentAdded(object? s, ComponentChangedEventArgs e)
        {
            if (e.Component is IComponent sadComponent)
                SadComponents.Add(sadComponent);
        }

        private void On_GoRogueComponentRemoved(object? s, ComponentChangedEventArgs e)
        {
            if (e.Component is IComponent sadComponent)
                SadComponents.Remove(sadComponent);
        }

        private void Position_Changed(object? sender, ValueChangedEventArgs<Point> e)
            => Moved?.Invoke(sender, new GameObjectPropertyChanged<Point>(this, e.OldValue, e.NewValue));

        #endregion
    }
}
