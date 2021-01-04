using System.Collections.Generic;
using GoRogue.MapGeneration;
using SadRogue.Primitives.GridViews;

namespace TheSadRogue.Integration.MapGenerationSteps
{
    /// <summary>
    /// A very simple cellular automaton capable of generating cave systems
    /// </summary>
    public class CaveGenerationStep : GenerationStep
    {
        private ISettableGridView<bool> _map = new ArrayView<bool>(4,4);
        protected override IEnumerator<object?> OnPerform(GenerationContext context)
        {
            _map = context.GetFirstOrNew<ISettableGridView<bool>>(()=> new ArrayView<bool>(context.Width, context.Height));
            var proposedMap = new ArrayView<bool>(context.Width, context.Height);
            for (int i = 0; i < _map.Width; i++)
            {
                for (int j = 0; j < _map.Height; j++)
                {
                    proposedMap[i, j] = ShouldPlaceWall(i, j);
                }
            }
            _map.ApplyOverlay(proposedMap);
            yield return null;
        }
        
        public bool ShouldPlaceWall(int x, int y)
        {
            int numWalls = GetAdjacentWalls(x, y, 1, 1);
            if (_map[x, y])
                return numWalls >= 4;

            return numWalls >= 5;
        }
        public int GetAdjacentWalls(int x, int y, int scopeX, int scopeY)
        {
            int startX = x - scopeX;
            int startY = y - scopeY;
            int endX = x + scopeX;
            int endY = y + scopeY;

            int iX = startX;
            int iY = startY;

            int wallCounter = 0;

            for (iY = startY; iY <= endY; iY++)
            {
                for (iX = startX; iX <= endX; iX++)
                {
                    if (!(iX == x && iY == y))
                    {
                        if (IsWall(iX, iY))
                        {
                            wallCounter += 1;
                        }
                    }
                }
            }

            return wallCounter;
        }
        public bool IsWall(int x, int y) => IsOutOfBounds(x, y) || _map[x, y];
        public bool IsOutOfBounds(int x, int y) => x < 0 || y < 0 || x >= _map.Width || y >= _map.Height;
    }
}