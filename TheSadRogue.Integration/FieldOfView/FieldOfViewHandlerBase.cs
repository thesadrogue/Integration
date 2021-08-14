using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.Components.ParentAware;
using GoRogue.GameFramework;
using GoRogue.SpatialMaps;
using SadRogue.Integration.Maps;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace SadRogue.Integration.FieldOfView
{
    /// <summary>
    /// A map component that controls visibility of map objects based on the player's FOV.  Create a subclass and implement
    /// abstract methods to specify what changes to make for each fov-related event.
    /// </summary>
    public abstract class FieldOfViewHandlerBase : ParentAwareComponentBase<RogueLikeMap>
    {
        /// <summary>
        /// Possible states for the <see cref="FieldOfViewHandlerBase"/> to be in.
        /// </summary>
        public enum State
        {
            /// <summary>
            /// Enabled state -- FieldOfViewHandlerBase will actively set things as seen/unseen when appropriate.
            /// </summary>
            Enabled,
            /// <summary>
            /// Disabled state.  All items in the map will be set as seen, and the FieldOfViewHandlerBase
            /// will not set visibility of any items as FOV changes or as items are added/removed.
            /// </summary>
            DisabledResetVisibility,
            /// <summary>
            /// Disabled state.  No changes to the current visibility of terrain/entities will be made, and the
            /// FieldOfViewHandlerBase will not set visibility of any items as FOV changes or as items are added/removed.
            /// </summary>
            DisabledNoResetVisibility
        }

        /// <summary>
        /// Whether or not the handler is actively setting things to seen/unseen as appropriate.
        /// </summary>
        public bool IsEnabled => CurrentState == State.Enabled;

        private State _currentState;

        /// <summary>
        /// The current state of the handler.  See <see cref="State"/> documentation for details
        /// on each possible value.
        /// </summary>
        /// <remarks>
        /// If the component has been added to a map, setting this value will set all values in
        /// the map according to the new state.
        ///
        /// When the component is added to a map, the visibility of all values in that map will
        /// be set according to this value.
        /// </remarks>
        public State CurrentState
        {
            get => _currentState;

            set
            {
                // Nothing to do if the old value is the same as the new
                if (value == _currentState) return;

                // Otherwise, set the state value, and apply it to the map if
                // there is one.
                _currentState = value;
                ApplyStateToMap(_currentState);
            }
        }

        private HashSet<Point> _newlyUnseenSinceLastReset;

        /// <summary>
        /// Creates a handler component that will manage visibility of objects for the map it is added to.
        /// </summary>
        /// <param name="startingState">The starting value for <see cref="CurrentState"/>.</param>
        protected FieldOfViewHandlerBase(State startingState = State.Enabled)
        {
            // Can't have more than one of these on a map; they would interfere with each other.
            Added += IncompatibleWith<FieldOfViewHandlerBase>;

            // When we're added to a map, we need to update that map's state as applicable.
            // When we're removed, we should put the map state back as if there is no
            // visibility control at all so we don't leave visibility different than we
            // found it
            Added += OnAdded;
            Removed += OnRemoved;

            // Record the starting state so the map can be updated when the handler is added.
            _currentState = startingState;

            // Initialize empty list to use to track unseen across append operations
            _newlyUnseenSinceLastReset = new HashSet<Point>();
        }

        private void OnAdded(object? sender, EventArgs e)
        {
            // Parent cannot be null because this event is only fired when the object is added
            var parent = Parent!;

            // Sync up FOV when relevant events happen
            parent.ObjectAdded += Parent_ObjectAdded;
            parent.ObjectMoved += Parent_ObjectMoved;
            parent.ObjectRemoved += Parent_ObjectRemoved;
            parent.PlayerFOV.Recalculated += Parent_PlayerFOVRecalculated;

            // We also need to set a flag every time reset happens, so that we know what all we need to actually check
            // each time the current FOV is updated.
            parent.PlayerFOV.VisibilityReset += Parent_VisibilityReset;

            // Set the state of the new map to match the handler's current state
            ApplyStateToMap(_currentState);

            _newlyUnseenSinceLastReset.Clear();
        }

        private void OnRemoved(object? sender, EventArgs e)
        {
            // Parent cannot be null because this event is only fired when the object is added
            var parent = Parent!;

            // Remove all event handlers to prevent dangling references
            parent.ObjectAdded -= Parent_ObjectAdded;
            parent.ObjectMoved -= Parent_ObjectMoved;
            parent.ObjectRemoved -= Parent_ObjectRemoved;
            parent.PlayerFOV.Recalculated -= Parent_PlayerFOVRecalculated;
            parent.PlayerFOV.VisibilityReset -= Parent_VisibilityReset;

            // Simply revert to everything being seen if it is not already this way
            if (_currentState != State.DisabledResetVisibility)
                ApplyStateToMap(State.DisabledResetVisibility);

            _newlyUnseenSinceLastReset.Clear();
        }

        private void Parent_VisibilityReset(object? sender, EventArgs e)
        {
            _newlyUnseenSinceLastReset.Clear();
        }

        private void ApplyStateToMap(State state)
        {
            // If the component has not been added to a map, there's no actual state updates
            // to do, so just exit
            if (Parent == null)
                return;

            switch (state)
            {
                // Call functions to set each terrain and entity appropriately.  Since we are applying a new
                // state, we have no way to know what tiles we need to apply (since we have no assumption of current
                // map state), so we must call it on all locations/entities.
                case State.Enabled:
                    foreach (var pos in Parent.Positions())
                    {
                        var terrain = Parent.GetTerrainAt<RogueLikeCell>(pos);
                        if (terrain == null) continue;

                        if (Parent.PlayerFOV.BooleanResultView[pos])
                            UpdateTerrainSeen(terrain);
                        else
                            UpdateTerrainUnseen(terrain);
                    }

                    foreach (var entity in Parent.Entities.Items.Cast<RogueLikeEntity>())
                    {
                        if (Parent.PlayerFOV.BooleanResultView[entity.Position])
                            UpdateEntitySeen(entity);
                        else
                            UpdateEntityUnseen(entity);
                    }

                    break;

                // This state disables future modification but doesn't change current state,
                // so there's nothing to do.
                case State.DisabledNoResetVisibility:
                    break;

                // Effectively set all map objects as seen.  Similarly to enabled state,
                // we must call the appropriate function for all objects since this function
                // can make no guarantees about current map state.
                case State.DisabledResetVisibility:
                    foreach (var pos in Parent.Positions())
                    {
                        var terrain = Parent.GetTerrainAt<RogueLikeCell>(pos);
                        if (terrain != null)
                            UpdateTerrainSeen(terrain);
                    }

                    foreach (var entity in Parent.Entities.Items.Cast<RogueLikeEntity>())
                        UpdateEntitySeen(entity);

                    break;
            }
        }

        /// <summary>
        /// Implement to make appropriate changes to a terrain tile that is now inside FOV.
        /// </summary>
        /// <param name="terrain">Terrain tile to modify.</param>
        protected abstract void UpdateTerrainSeen(RogueLikeCell terrain);

        /// <summary>
        /// Implement to make appropriate changes to a terrain tile that is now outside FOV.
        /// </summary>
        /// <param name="terrain">Terrain tile to modify.</param>
        protected abstract void UpdateTerrainUnseen(RogueLikeCell terrain);

        /// <summary>
        /// Implement to make appropriate changes to an entity that is now inside FOV.
        /// </summary>
        /// <param name="entity">Entity to modify.</param>
        protected abstract void UpdateEntitySeen(RogueLikeEntity entity);

        /// <summary>
        /// Implement to make appropriate changes to an entity that is now outside FOV.
        /// </summary>
        /// <param name="entity">Entity to modify.</param>
        protected abstract void UpdateEntityUnseen(RogueLikeEntity entity);

        // When an object is added, set its visibility according to the handler's current
        // state.
        private void Parent_ObjectAdded(object? sender, ItemEventArgs<IGameObject> e)
        {
            if (!IsEnabled) return;

            // The map cannot be null since this event is responding to a Map event.
            var parent = Parent!;

            switch (e.Item)
            {
                case RogueLikeCell terrain:
                    if (parent.PlayerFOV.BooleanResultView[terrain.Position])
                        UpdateTerrainSeen(terrain);
                    else
                        UpdateTerrainUnseen(terrain);
                    break;

                case RogueLikeEntity entity:
                    if (parent.PlayerFOV.BooleanResultView[entity.Position])
                        UpdateEntitySeen(entity);
                    else
                        UpdateEntityUnseen(entity);
                    break;
            }
        }

        // When entities are moved, their visibility needs to be updated in accordance with
        // the current FOV.
        //
        // Only entities (not terrain) can move, so this function may safely cast to the entity type.
        private void Parent_ObjectMoved(object? sender, ItemMovedEventArgs<IGameObject> e)
        {
            if (!IsEnabled) return;

            // The map cannot be null since this event is responding to a Map event.
            var parent = Parent!;

            var entity = (RogueLikeEntity)e.Item;
            if (parent.PlayerFOV.BooleanResultView[e.NewPosition])
                UpdateEntitySeen(entity);
            else
                UpdateEntityUnseen(entity);
        }

        private void Parent_ObjectRemoved(object? sender, ItemEventArgs<IGameObject> e)
        {
            // Because this is exclusively undoing changes that were already done,
            // we explicitly still do work even if the component is disabled.  This ensures
            // that objects are restored when removed from the map if they were at any point
            // changed by this component.

            switch (e.Item)
            {
                case RogueLikeCell terrain:
                    UpdateTerrainSeen(terrain);
                    break;
                case RogueLikeEntity entity:
                    UpdateEntitySeen(entity);
                    break;
            }
        }

        private void Parent_PlayerFOVRecalculated(object? sender, EventArgs e)
        {
            if (!IsEnabled) return;

            // The map cannot be null since this event is responding to an event
            // from a Map member.
            var parent = Parent!;

            // Update values for any position that has just been exposed
            foreach (var position in parent.PlayerFOV.NewlySeen)
            {
                var terrain = parent.GetTerrainAt<RogueLikeCell>(position);
                if (terrain != null)
                    UpdateTerrainSeen(terrain);

                foreach (var entity in parent.GetEntitiesAt<RogueLikeEntity>(position))
                    UpdateEntitySeen(entity);
            }


            // Update values for any position that has just moved out of FOV if such tiles exist; otherwise, simply
            // account for any changes since last iteration
            if (_newlyUnseenSinceLastReset.Count == 0)
            {
                foreach (var position in parent.PlayerFOV.NewlyUnseen)
                {
                    if (_newlyUnseenSinceLastReset.Contains(position))
                        continue;

                    var terrain = parent.GetTerrainAt<RogueLikeCell>(position);
                    if (terrain != null)
                        UpdateTerrainUnseen(terrain);

                    foreach (var entity in parent.GetEntitiesAt<RogueLikeEntity>(position))
                        UpdateEntityUnseen(entity);

                    _newlyUnseenSinceLastReset.Add(position);
                }
            }
            // This recalculated event must be the result of an append; so NewlyUnseen cannot have grown; however
            // it may have shrunk.  Therefore, we must set any cells that were removed from NewlyUnseen to their
            // visible state.
            else
            {
                var currentNewlyUnseen = new HashSet<Point>(parent.PlayerFOV.NewlyUnseen);
                foreach (var position in _newlyUnseenSinceLastReset)
                {
                    if (currentNewlyUnseen.Contains(position))
                        continue;

                    var terrain = parent.GetTerrainAt<RogueLikeCell>(position);
                    if (terrain != null)
                        UpdateTerrainSeen(terrain);

                    foreach (var entity in parent.GetEntitiesAt<RogueLikeEntity>(position))
                        UpdateEntitySeen(entity);
                }
            }


        }
    }
}
