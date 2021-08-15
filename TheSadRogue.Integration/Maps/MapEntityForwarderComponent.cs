using System;
using System.Collections.Generic;
using GoRogue.GameFramework;
using GoRogue.SpatialMaps;
using JetBrains.Annotations;
using SadConsole;
using SadConsole.Components;
using SadConsole.Entities;
using SadConsole.Input;

namespace SadRogue.Integration.Maps
{
    /// <summary>
    /// Component used by <see cref="RogueLikeMap"/> to process updates and keyboard/mouse events across its entities
    /// as necessary.
    /// </summary>
    [PublicAPI]
    public class MapEntityForwarderComponent : IComponent
    {
        /// <inheritdoc />
        public uint SortOrder { get; set; }

        bool IComponent.IsUpdate => true;
        bool IComponent.IsRender => false;
        bool IComponent.IsMouse => true;
        bool IComponent.IsKeyboard => true;

        // Cached list of map entities for quick processing
        private readonly List<Entity> _mapEntities = new List<Entity>();

        /// <inheritdoc />
        public void Update(IScreenObject host, TimeSpan delta)
        {
            foreach (var entity in _mapEntities.ToArray())
                entity.Update(delta);
        }

        /// <inheritdoc />
        public void Render(IScreenObject host, TimeSpan delta) { }

        /// <inheritdoc />
        public void ProcessMouse(IScreenObject host, MouseScreenObjectState state, out bool handled)
        {
            handled = false;
            foreach (var entity in _mapEntities.ToArray())
                handled = entity.ProcessMouse(state);
        }

        /// <inheritdoc />
        public void ProcessKeyboard(IScreenObject host, Keyboard keyboard, out bool handled)
        {
            handled = false;
            foreach (var entity in _mapEntities.ToArray())
                handled = entity.ProcessKeyboard(keyboard);
        }

        /// <inheritdoc />
        public void OnAdded(IScreenObject host)
        {
            if (!(host is RogueLikeMap map))
                throw new InvalidOperationException(
                    $"{nameof(MapEntityForwarderComponent)} components may only be attached to objects of type ${nameof(RogueLikeMap)}.");

            map.Entities.ItemAdded += EntitiesOnItemAdded;
            map.Entities.ItemRemoved += EntitiesOnItemRemoved;

            foreach (var entity in map.Entities.Items)
                _mapEntities.Add((Entity)entity);
        }

        /// <inheritdoc />
        public void OnRemoved(IScreenObject host)
        {
            var map = (RogueLikeMap)host;

            map.Entities.ItemAdded -= EntitiesOnItemAdded;
            map.Entities.ItemRemoved -= EntitiesOnItemRemoved;

            _mapEntities.Clear();
        }

        private void EntitiesOnItemAdded(object? sender, ItemEventArgs<IGameObject> e)
        {
            // Cast is guaranteed to succeed due to invariants enforced by RogueLikeMap
            _mapEntities.Add((Entity)e.Item);
        }

        private void EntitiesOnItemRemoved(object? sender, ItemEventArgs<IGameObject> e)
        {
            // Cast is guaranteed to succeed due to invariants enforced by RogueLikeMap
            _mapEntities.Remove((Entity)e.Item);
        }
    }
}
