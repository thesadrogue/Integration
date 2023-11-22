﻿using SadRogue.Integration;
using SadRogue.Integration.Keybindings;

namespace ExampleGame
{
    internal class Player : RogueLikeEntity
    {
        /// <summary>
        /// The sight radius of the player.
        /// </summary>
        public int FOVRadius { get; private set; }

        public Player(Point position, int fovRadius = 10)
            : base(position, 1, false)
        {
            // Set FOV radius we will use for calculating FOV
            FOVRadius = fovRadius;

            // Set hook so that FOV is recalculated when the player moves
            PositionChanged += OnPositionChanged;

            // Add component for controlling player movement via keyboard
            var motionControl = new CustomKeybindingsComponent();
            motionControl.SetMotions(KeybindingsComponent.ArrowMotions);
            AllComponents.Add(motionControl);
        }

        /// <summary>
        /// Calculate FOV if a player is part of a map.
        /// </summary>
        public void CalculateFOV()
        {
            CurrentMap?.PlayerFOV.Calculate(Position, FOVRadius, CurrentMap.DistanceMeasurement);
        }

        // If the player is added to a map, update the player FOV when the player moves
        private void OnPositionChanged(object? sender, ValueChangedEventArgs<Point> e)
        {
            CalculateFOV();
        }
    }
}
