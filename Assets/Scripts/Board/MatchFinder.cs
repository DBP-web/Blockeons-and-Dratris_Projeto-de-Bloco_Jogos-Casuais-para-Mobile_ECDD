using System.Collections.Generic;
using BlockeonsDratris.Blocks;

namespace BlockeonsDratris.Board
{
    public class MatchGroup
    {
        public List<BlockBase> blocks = new List<BlockBase>();
        public int Size => blocks.Count;
    }

    public class MatchFinder
    {
        private BlockBase[,] grid;
        private int columns;
        private int rows;

        public MatchFinder(int cols, int r)
        {
            columns = cols;
            rows = r;
        }

        public List<MatchGroup> FindAllMatches(BlockBase[,] currentGrid)
        {
            grid = currentGrid;
            var visited = new bool[columns, rows];
            var matches = new List<MatchGroup>();

            for (int c = 0; c < columns; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    if (visited[c, r] || grid[c, r] == null) continue;

                    var group = new MatchGroup();
                    FloodFill(c, r, GetBlockKey(grid[c, r]), visited, group);

                    if (group.Size >= 3)
                    {
                        matches.Add(group);
                    }
                }
            }

            return matches;
        }

        private void FloodFill(int c, int r, string key, bool[,] visited, MatchGroup group)
        {
            if (c < 0 || c >= columns || r < 0 || r >= rows) return;
            if (visited[c, r]) return;
            if (grid[c, r] == null) return;
            if (GetBlockKey(grid[c, r]) != key) return;

            visited[c, r] = true;
            group.blocks.Add(grid[c, r]);

            FloodFill(c + 1, r, key, visited, group);
            FloodFill(c - 1, r, key, visited, group);
            FloodFill(c, r + 1, key, visited, group);
            FloodFill(c, r - 1, key, visited, group);
        }

        // Antes: private string GetBlockKey(BlockBase block)
        // Agora: público e estático, para poder ser chamado de fora (BoardManager),
        // isso foi necessario para resolver o problema dos blocos estarem sumindo
        // com um inicio movimento no inicio do jogo.  
        public static string GetBlockKey(BlockBase block)
        {
            switch (block)
            {
                case OffensiveBlock ob:
                    return "Offensive_" + ob.offensiveType;
                case SupportBlock sb:
                    return "Support_" + sb.supportType;
                case ShieldBlock:
                    return "Shield";
                default:
                    return "Unknown";
            }
        }
    }
}