using foxandchicken.GameLogic;
using foxandchicken.Models;
using foxandchicken.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace foxandchicken
{
    
    public struct CellCoordinates
    {
        public int Row { get; set; }
        public int Column { get; set; }
    }

    public partial class MainWindow : Window
    {
        private FoxesAndChickensGame _game;
        private GameCell _selectedCell;
        private Button[,] _cellButtons;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGameBoard();
            StartNewGame();
        }

        private void InitializeGameBoard()
        {
            _cellButtons = new Button[GameConstants.BoardRows, GameConstants.BoardColumns];
            GameBoard.Rows = GameConstants.BoardRows;
            GameBoard.Columns = GameConstants.BoardColumns;

            CreateCellButtons();
        }

        private void CreateCellButtons()
        {
            for (int row = 0; row < GameConstants.BoardRows; row++)
            {
                for (int col = 0; col < GameConstants.BoardColumns; col++)
                {
                    if (ShouldSkipCell(row, col))
                    {
                        AddInvisiblePlaceholder();
                        continue;
                    }

                    CreateGameCellButton(row, col);
                }
            }
        }

        private bool ShouldSkipCell(int row, int col)
        {
            return (row == 0 || row == 1 || row == 5 || row == 6) &&
                   (col < 2 || col > 4);
        }

        private void AddInvisiblePlaceholder()
        {
            GameBoard.Children.Add(new Border { Width = 0, Height = 0 });
        }

        private void CreateGameCellButton(int row, int col)
        {
            var button = new Button
            {
                Width = GameConstants.CellSize,
                Height = GameConstants.CellSize,
                Margin = new Thickness(2),
                Tag = new CellCoordinates { Row = row, Column = col }
            };
            button.Click += CellButton_Click;

            _cellButtons[row, col] = button;
            GameBoard.Children.Add(button);
        }

        private void StartNewGame()
        {
            _game = new FoxesAndChickensGame();
            _selectedCell = null;
            UpdateGameBoard();
        }

        private void UpdateGameBoard()
        {
            UpdateAllCells();
            UpdateStatusText();
        }

        private void UpdateAllCells()
        {
            for (int row = 0; row < GameConstants.BoardRows; row++)
            {
                for (int col = 0; col < GameConstants.BoardColumns; col++)
                {
                    UpdateCell(row, col);
                }
            }
        }

        private void UpdateCell(int row, int col)
        {
            var button = _cellButtons[row, col];
            if (button == null) return;

            var cell = _game.Board.GetCell(row, col);
            if (cell != null)
            {
                UpdateButtonAppearance(button, cell);
                button.Visibility = Visibility.Visible;
            }
            else
            {
                button.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateButtonAppearance(Button button, GameCell cell)
        {
            button.Content = GetCellContent(cell.Type);
            button.Background = GetCellBackground(cell.Type);
        }

        private string GetCellContent(CellType type)
        {
            switch (type)
            {
                case CellType.Fox:
                    return "Л";
                case CellType.Chicken:
                    return "К";
                default:
                    return "";
            }
        }

        private Brush GetCellBackground(CellType type)
        {
            switch (type)
            {
                case CellType.Fox:
                    return Brushes.Orange;
                case CellType.Chicken:
                    return Brushes.LightGray;
                default:
                    return Brushes.White;
            }
        }

        private void UpdateStatusText()
        {
            StatusTextBlock.Text = GetGameStatusText();
            ScoreTextBlock.Text = $"Съедено кур: {_game.ChickensEaten}";
        }

        private string GetGameStatusText()
        {
            if (_game.GameOver)
                return $"Игра окончена. Победили: {_game.Winner}";

            return _game.IsFoxesTurn ? "Ход лис" : "Ход кур";
        }

        private void CellButton_Click(object sender, RoutedEventArgs e)
        {
            if (_game.GameOver) return;

            var button = (Button)sender;
            var coords = (CellCoordinates)button.Tag;
            int row = coords.Row;
            int col = coords.Column;
            var cell = _game.Board.GetCell(row, col);

            if (cell == null) return;

            if (_game.IsFoxesTurn) return;

            ProcessChickenMove(cell, row, col);
        }

        private void ProcessChickenMove(GameCell cell, int row, int col)
        {
            if (cell.Type == CellType.Chicken)
            {
                SelectChicken(cell);
            }
            else if (_selectedCell != null && cell.Type == CellType.Empty)
            {
                TryMoveChicken(row, col);
            }
        }

        private void SelectChicken(GameCell cell)
        {
            _selectedCell = cell;
            HighlightValidMoves();
        }

        private void TryMoveChicken(int row, int col)
        {
            if (_game.MoveChicken(_selectedCell.Row, _selectedCell.Column, row, col))
            {
                _selectedCell = null;
                UpdateGameBoard();

                if (!_game.GameOver)
                {
                    Dispatcher.BeginInvoke(
                        DispatcherPriority.Background,
                        new System.Action(() =>
                        {
                            _game.MakeFoxMove();
                            UpdateGameBoard();
                        }));
                }
            }
        }

        private void HighlightValidMoves()
        {
            ClearHighlights();

            if (_selectedCell == null) return;

            for (int row = 0; row < GameConstants.BoardRows; row++)
            {
                for (int col = 0; col < GameConstants.BoardColumns; col++)
                {
                    HighlightValidCell(row, col);
                }
            }
        }

        private void HighlightValidCell(int row, int col)
        {
            var button = _cellButtons[row, col];
            if (button == null || button.Visibility != Visibility.Visible) return;

            if (_game.IsValidChickenMove(_selectedCell.Row, _selectedCell.Column, row, col))
            {
                button.Background = Brushes.LightBlue;
            }
        }

        private void ClearHighlights()
        {
            for (int row = 0; row < GameConstants.BoardRows; row++)
            {
                for (int col = 0; col < GameConstants.BoardColumns; col++)
                {
                    ResetCellHighlight(row, col);
                }
            }
        }

        private void ResetCellHighlight(int row, int col)
        {
            var button = _cellButtons[row, col];
            if (button == null || button.Visibility != Visibility.Visible) return;

            var cell = _game.Board.GetCell(row, col);
            if (cell != null)
            {
                button.Background = GetCellBackground(cell.Type);
            }
        }

        private void NewGameBtn_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }
    }
}