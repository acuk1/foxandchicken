using foxandchicken.Models;
using foxandchicken.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace foxandchicken.GameLogic
{
    public class GameBoard
    {
        public List<GameCell> Cells { get; private set; }

        public GameBoard()
        {
            InitializeBoard();
        }

        public void InitializeBoard()
        {
            Cells = new List<GameCell>();
            InitializeRow(0, hasAllCells: false);
            InitializeRow(1, hasAllCells: false);
            InitializeRow(2, hasAllCells: true);
            InitializeRow(3, hasAllCells: true);
            InitializeRow(4, hasAllCells: true);
            InitializeRow(5, hasAllCells: false);
            InitializeRow(6, hasAllCells: false);
        }

        private void InitializeRow(int row, bool hasAllCells)
        {
            int startCol = hasAllCells ? 0 : 2;
            int endCol = hasAllCells ? GameConstants.BoardColumns : 5;

            for (int col = startCol; col < endCol; col++)
            {
                CellType type = GetInitialCellType(row, col);
                Cells.Add(new GameCell(row, col, type));
            }
        }

        private CellType GetInitialCellType(int row, int col)
        {
            if (row == 2 && (col == 2 || col == 4)) return CellType.Fox;
            if (row >= 3 && row <= 6) return CellType.Chicken;
            return CellType.Empty;
        }

        public GameCell GetCell(int row, int col)
        {
            return Cells.FirstOrDefault(c => c.Row == row && c.Column == col);
        }
    }
}