﻿using System.Linq;
using GoRogue.MapGeneration;
using GoRogue.MapGeneration.Steps;
using SadConsole;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using TheSadRogue.Integration;
using TheSadRogue.Integration.Components;
using TheSadRogue.Integration.MapGenerationSteps;
#pragma warning disable 8618

namespace ExampleGame
{
    /// <summary>
    /// A tiny game to give examples of how to use GoRogue
    /// </summary>
    class Program
    {
        public const int Width = 80;
        public const int Height = 25;
        private const int MapWidth = 80;
        private const int MapHeight = 25;
        public static RogueLikeMap Map;
        public static RogueLikeEntity PlayerCharacter;
        public static ScreenSurface MapWindow;
        static void Main(/*string[] args*/)
        {
            Game.Create(Width, Height);
            Game.Instance.OnStart = Init;
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        /// <summary>
        /// Runs before the game starts
        /// </summary>
        private static void Init()
        {
            Map = GenerateMap();
            var cells = new ArrayView<ColoredGlyph>(MapWidth, MapHeight);
            cells.ApplyOverlay(Map.TerrainView);

            MapWindow = new ScreenSurface(MapWidth, MapHeight, cells);
            MapWindow.SadComponents.Add(Map.EntityRenderer);
            
            PlayerCharacter = GeneratePlayerCharacter();
            Map.AddEntity(PlayerCharacter);
            GameHost.Instance.Screen.Children.Add(MapWindow);
        }
        
        private static RogueLikeMap GenerateMap()
        {
            var generator = new Generator(MapWidth, MapHeight)
                .AddStep(new CompositeGenerationStep(MapWidth, MapHeight))
                .Generate();
            
            var generatedMap = generator.Context.GetFirst<ISettableGridView<bool>>();

            RogueLikeMap map = new RogueLikeMap(MapWidth, MapHeight, 4, Distance.Euclidean);
            int floorGlyph = '_';
            int wallGlyph = '#';
            
            for (int i = 0; i < map.Width; i++)
            {
                for (int j = 0; j < map.Height; j++)
                {
                    int glyph = generatedMap[(i, j)] ? floorGlyph : wallGlyph;
                    bool walkable = generatedMap[(i, j)];
                    map.SetTerrain(new RogueLikeEntity((i, j), glyph, walkable, walkable, 0));
                }
            }

            return map;
        }

        private static RogueLikeEntity GeneratePlayerCharacter()
        {
            var position = Map.WalkabilityView.Positions()
                .Where(p => Map.WalkabilityView[p])
                .OrderBy(p => p.X)
                .ThenBy(p => p.Y)
                .First();
                
            var player = new RogueLikeEntity(position,1, false, layer: 1);
            var motionControl = new PlayerControlsComponent();
            player.AddComponent(motionControl);
            player.IsFocused = true;
            return player;
        }
    }
}