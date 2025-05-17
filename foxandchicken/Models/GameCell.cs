namespace foxandchicken.Models
{
    public class GameCell
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public CellType Type { get; set; }
        public bool IsTargetSquare { get; set; }

        public GameCell() { }

        public GameCell(int row, int col, CellType type, bool isTarget = false)
        {
            Row = row;
            Column = col;
            Type = type;
            IsTargetSquare = isTarget;
        }
    }
}