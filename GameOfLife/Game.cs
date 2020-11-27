using System;
using System.Text;
using System.Threading;
using static System.Console;

namespace GameOfLife
{
    public class Game
    {
        private readonly int _cols;
        private readonly int _rows;
        private readonly int _leftPadding;
        private readonly int _topPadding;
        private int[,] _grid;
        private int[,] _previousGrid;
        private bool _isGridChanged;
        private int _iterations;
        private readonly string TITLE = "The Game of Life";
        private readonly StringBuilder _sb;

        public Game(int rows, int cols )
        {

            SetWindowSize(50, 50);

            _cols = cols;
            _rows = rows;
            _leftPadding = (WindowWidth - _cols) / 2;
            _topPadding = (WindowHeight - _rows) / 2;

            _grid = new int[_rows, _cols];

            _sb = new StringBuilder();

            Initialize();
            Run();
        }

        private void Initialize()
        {
            var random = new Random();
            for (int i = 0; i < _rows; ++i)
            {
                for (int j = 0; j < _cols; ++j)
                {
                    _grid[i, j] = random.Next(0, 2);
                }
            }

            _isGridChanged = true;

        }


        private void Run()
        {

            for (var j = 0; j < (_topPadding - 1) / 2; ++j)
                Write('\n');

            WriteLine(TITLE);

            for (var j = 0; j < (_topPadding - 1) / 2; ++j)
                Write('\n');

            while (_isGridChanged)
            {
                Draw();
                Thread.Sleep(200);

            }

            WriteLine("\nGame Over!!!");
            WriteLine($"\nYour world survived for {_iterations} generations!");
        }



        private void Draw()
        {
            _sb.Length = 0;

            for (var i = 0; i < _rows; ++i)
            {
                for(var j = 0; j < _leftPadding; ++j)
                    _sb.Append(' ');

                for (var j = 0; j < _rows; ++j)
                {
                   _sb.Append(_grid[i, j] != 0 ? 'x' : ' ');
                }

                _sb.Append('\n');
            }

            ++_iterations;
            _sb.Append($"\nYour world survived for {_iterations} generations!");

            SetCursorPosition(0,_topPadding);
            WriteLine(_sb.ToString());
            _previousGrid = _grid;
            _isGridChanged = false;
            _grid = GetNextGrid(_previousGrid, _rows, _cols);

        }

        private int[,] GetNextGrid(int[,] grid, int rows, int cols)
        {
            var nextGrid = new int[rows, cols];
            for (int row = 0; row < rows; ++row)
            {
                for (int col = 0; col < cols; ++col)
                {
                    var neighborSum = GetNeighborsSum(grid, row, col);

                    if (neighborSum <= 0) continue;

                    _isGridChanged = true;
                    nextGrid[row, col] = neighborSum switch
                    {
                        2 => grid[row, col],
                        3 => 1,
                        _ => 0
                    };
                }
            }

            return nextGrid;
        }

        private int GetNeighborsSum(int[,] grid, int row, int col)
        {
            var neighborsSum = 0;

            for (var i = -1; i <= 1; ++i)
            {
                for (var j = -1; j <= 1; ++j)
                {
                    var modRow = (i + row + _rows) % _rows;
                    var modCol = (j + col + _cols) % _cols;

                    neighborsSum += grid[modRow, modCol];
                }
            }
            return neighborsSum - grid[row, col];
        }
    }
}