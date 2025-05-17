using foxandchicken.Models;
using foxandchicken.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace foxandchicken.GameLogic
{
    public class FoxesAndChickensGame
    {
        private readonly Random _random = new Random();

        public GameBoard Board { get; }
        public bool IsFoxesTurn { get; private set; }
        public int ChickensEaten { get; private set; }
        public bool GameOver { get; private set; }
        public string Winner { get; private set; }

        public FoxesAndChickensGame()
        {
            Board = new GameBoard();
            ResetGame();
        }

        public void ResetGame()
        {
            Board.InitializeBoard();
            IsFoxesTurn = false;
            ChickensEaten = 0;
            GameOver = false;
            Winner = null;
        }

        public bool MoveChicken(int fromRow, int fromCol, int toRow, int toCol)
        {
            if (!CanMoveChicken(fromRow, fromCol, toRow, toCol))
                return false;

            PerformMove(fromRow, fromCol, toRow, toCol);
            CheckWinConditions();

            if (!GameOver)
            {
                IsFoxesTurn = true;
            }

            return true;
        }

        private bool CanMoveChicken(int fromRow, int fromCol, int toRow, int toCol)
        {
            return !GameOver &&
                   !IsFoxesTurn &&
                   IsValidChickenMove(fromRow, fromCol, toRow, toCol);
        }

        private void PerformMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            Board.GetCell(fromRow, fromCol).Type = CellType.Empty;
            Board.GetCell(toRow, toCol).Type = CellType.Chicken;
        }

        public bool IsValidChickenMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            return IsOneStepMove(fromRow, fromCol, toRow, toCol) &&
                   IsForwardOrSideMove(fromRow, toRow) &&
                   IsTargetEmpty(toRow, toCol) &&
                   IsChickenSelected(fromRow, fromCol);
        }

        private bool IsOneStepMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            return Math.Abs(toRow - fromRow) + Math.Abs(toCol - fromCol) == 1;
        }

        private bool IsForwardOrSideMove(int fromRow, int toRow)
        {
            return toRow <= fromRow; 
        }

        private bool IsTargetEmpty(int row, int col)
        {
            return Board.GetCell(row, col)?.Type == CellType.Empty;
        }

        private bool IsChickenSelected(int row, int col)
        {
            return Board.GetCell(row, col)?.Type == CellType.Chicken;
        }

        public void MakeFoxMove()
        {
            if (GameOver || !IsFoxesTurn) return;

            var bestMove = FindBestFoxMove();

            if (bestMove != null)
            {
                ExecuteFoxMove(bestMove);
                CheckWinConditions();

                if (!GameOver)
                {
                    IsFoxesTurn = false;
                }
            }
        }

        private FoxMove FindBestFoxMove()
        {
            var possibleMoves = GetAllPossibleFoxMoves();

            if (possibleMoves.Count == 0)
                return null;

            return SelectBestMove(possibleMoves);
        }

        private List<FoxMove> GetAllPossibleFoxMoves()
        {
            var possibleMoves = new List<FoxMove>();

            foreach (var foxCell in Board.Cells.Where(c => c.Type == CellType.Fox))
            {
                possibleMoves.AddRange(GetMovesForFox(foxCell.Row, foxCell.Column));
            }

            return possibleMoves;
        }

        private List<FoxMove> GetMovesForFox(int foxRow, int foxCol)
        {
            var moves = new List<FoxMove>();
            int[] directions = { -1, 1 }; 

            foreach (int dRow in directions)
            {
                CheckDirection(foxRow, foxCol, dRow, 0, moves);
                CheckDirection(foxRow, foxCol, 0, dRow, moves);
            }

            return moves;
        }

        private void CheckDirection(int foxRow, int foxCol, int dRow, int dCol, List<FoxMove> moves)
        {
            if (CanEatInDirection(foxRow, foxCol, dRow, dCol, out var eatenChickens))
            {
                moves.Add(CreateFoxMove(foxRow, foxCol, dRow, dCol, eatenChickens));
            }
            else if (CanMoveToEmptyCell(foxRow, foxCol, dRow, dCol))
            {
                moves.Add(CreateSimpleMove(foxRow, foxCol, dRow, dCol));
            }
        }

        private bool CanEatInDirection(int foxRow, int foxCol, int dRow, int dCol, out List<(int, int)> eatenChickens)
        {
            eatenChickens = new List<(int, int)>();
            int currentRow = foxRow;
            int currentCol = foxCol;

            while (true)
            {
                int nextRow = currentRow + dRow;
                int nextCol = currentCol + dCol;
                int jumpRow = nextRow + dRow;
                int jumpCol = nextCol + dCol;

                if (!IsInBounds(nextRow, nextCol) || !IsInBounds(jumpRow, jumpCol))
                    break;

                if (IsChicken(nextRow, nextCol) && IsEmpty(jumpRow, jumpCol))
                {
                    eatenChickens.Add((nextRow, nextCol));
                    currentRow = jumpRow;
                    currentCol = jumpCol;
                }
                else
                {
                    break;
                }
            }

            return eatenChickens.Count > 0;
        }

        private FoxMove CreateFoxMove(int foxRow, int foxCol, int dRow, int dCol, List<(int, int)> eatenChickens)
        {
            int finalRow = foxRow;
            int finalCol = foxCol;

            foreach (var _ in eatenChickens)
            {
                finalRow += 2 * dRow;
                finalCol += 2 * dCol;
            }

            return new FoxMove
            {
                FromRow = foxRow,
                FromCol = foxCol,
                ToRow = finalRow,
                ToCol = finalCol,
                EatenChickens = eatenChickens
            };
        }

        private bool CanMoveToEmptyCell(int foxRow, int foxCol, int dRow, int dCol)
        {
            int newRow = foxRow + dRow;
            int newCol = foxCol + dCol;

            return IsInBounds(newRow, newCol) && IsEmpty(newRow, newCol);
        }

        private FoxMove CreateSimpleMove(int foxRow, int foxCol, int dRow, int dCol)
        {
            return new FoxMove
            {
                FromRow = foxRow,
                FromCol = foxCol,
                ToRow = foxRow + dRow,
                ToCol = foxCol + dCol,
                EatenChickens = new List<(int, int)>()
            };
        }

        private FoxMove SelectBestMove(List<FoxMove> moves)
        {
            int maxEaten = moves.Max(m => m.EatenChickens.Count);
            var bestMoves = moves.Where(m => m.EatenChickens.Count == maxEaten).ToList();

            return bestMoves[_random.Next(bestMoves.Count)];
        }

        private void ExecuteFoxMove(FoxMove move)
        {
            Board.GetCell(move.FromRow, move.FromCol).Type = CellType.Empty;
            Board.GetCell(move.ToRow, move.ToCol).Type = CellType.Fox;

            foreach (var (row, col) in move.EatenChickens)
            {
                Board.GetCell(row, col).Type = CellType.Empty;
                ChickensEaten++;
            }
        }

        private void CheckWinConditions()
        {
            CheckFoxWinCondition();

            if (!GameOver)
            {
                CheckChickenWinCondition();
            }
        }

        private void CheckFoxWinCondition()
        {
            if (ChickensEaten >= GameConstants.ChickensToEatForWin)
            {
                GameOver = true;
                Winner = "Лисы";
            }
        }

        private void CheckChickenWinCondition()
        {
            int chickensInTop = Board.Cells
                .Count(c => c.Row < 3 && c.Type == CellType.Chicken);

            if (chickensInTop >= GameConstants.TargetChickensForWin)
            {
                GameOver = true;
                Winner = "Куры";
            }
        }

        private bool IsInBounds(int row, int col)
        {
            return row >= 0 && row < GameConstants.BoardRows &&
                   col >= 0 && col < GameConstants.BoardColumns;
        }

        private bool IsChicken(int row, int col)
        {
            return Board.GetCell(row, col)?.Type == CellType.Chicken;
        }

        private bool IsEmpty(int row, int col)
        {
            return Board.GetCell(row, col)?.Type == CellType.Empty;
        }
    }
}