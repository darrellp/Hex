using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Math;

namespace Hex
{
	class BoardDrawing
	{
        #region Private variables
        private const double StoneShrinkage = 0.85;
		private readonly Board _board;
		private readonly double _minx;
		private readonly double _hexWidth;
		private readonly double _hexHeight;
		private Transform _xfm;
		private double _cellWidth;
		private Geometry _cellPrototype;
		private readonly Canvas _boardCanvas;
		private readonly Path[,] _cells;
		private readonly Shape[,] _stones;
		private int Size { get; }
	    private readonly Label _lblLocation;
	    private int _cellEnterCount;
        #endregion

        #region Constructor
        internal BoardDrawing(Board board)
		{
			_board = board;
			Size = _board.Size;
			// Assumes side lengths of 1 and origin at board center
			_minx = -(3.0 * Size - 1) / 2;
			_hexWidth = -2 * _minx;
			_hexHeight = 2 * HexCell.S3D2 * Size;
			_boardCanvas = MainWindow.Main.CvsBoard;
		    _lblLocation = MainWindow.Main.LblLocation;
			_cells = new Path[Size, Size];
			_stones = new Shape[Size, Size];
			Redraw();
		}
        #endregion

        #region Drawing Setup
        internal void ClearBoard()
		{
			for (var iRow = 0; iRow < Size; iRow++)
			{
				for (var iCol = 0; iCol < Size; iCol++)
				{
					DrawStone(new GridLocation(iRow, iCol), Player.Unoccupied);
				}
			}
		}

		internal void Redraw()
		{
			SetupTransform();
			_boardCanvas.Children.Clear();
			for (var iRow = 0; iRow < Size; iRow++)
			{
				for (var iCol = 0; iCol < Size; iCol++)
				{
					_stones[iRow, iCol] = null;
					DrawCell(iRow, iCol);
					DrawStone(new GridLocation(iRow, iCol), _board.Players[iRow, iCol]);
				}
			}
			var topCrd = GetCenter(Size - 1, 0).Y;
			var leftCrd = GetCenter(0, 0).X;
			var bottomCrd = GetCenter(0, Size - 1).Y;
			var rightCrd = GetCenter(Size - 1, Size - 1).X;
			DrawStone(new Point(leftCrd, topCrd), Player.Black);
			DrawStone(new Point(rightCrd, bottomCrd), Player.Black);
			DrawStone(new Point(leftCrd, bottomCrd), Player.White);
			DrawStone(new Point(rightCrd, topCrd), Player.White);
		}

	    private void SetupTransform()
	    {
	        var xfm = new TransformGroup();
	        var scaleX = _boardCanvas.ActualWidth / _hexWidth;
	        var scaleY = _boardCanvas.ActualHeight / _hexHeight;
	        _cellWidth = Min(scaleX, scaleY);
	        xfm.Children.Add(new ScaleTransform(_cellWidth, _cellWidth));
	        xfm.Children.Add(new TranslateTransform(_boardCanvas.ActualWidth / 2, _boardCanvas.ActualHeight / 2));
	        _cellPrototype = HexCell.Cell(new Vector(0, 0), _cellWidth);
	        _xfm = xfm;
	    }
        #endregion

        #region Geometry
        private Point GetCenter(int row, int col)
		{
			var x = (3.0 * (row + col) + 2) / 2 + _minx;
			var y = (col - row) * HexCell.S3D2;
			return _xfm.Transform(new Point(x, y));
		}

		private Point GetCenter(GridLocation location)
		{
			return GetCenter(location.Row, location.Column);
		}
        #endregion

        #region Drawing
        private void DrawCell(int row, int col)
		{
			var brush = new SolidColorBrush { Color = Colors.Black };
			var fill = new SolidColorBrush { Color = Colors.BlanchedAlmond };
			var path = new Path
			{
				Data = _cellPrototype,
				Visibility = Visibility.Visible,
				Tag = new GridLocation(row, col),
				Stroke = brush,
				Fill = fill
			};
			path.MouseEnter += EnterCell;
		    path.MouseLeave += LeaveCell;
			path.MouseDown += ClickCell;
			var position = GetCenter(row, col);
			Canvas.SetTop(path, position.Y);
			Canvas.SetLeft(path, position.X);
			_boardCanvas.Children.Add(path);
			_cells[row, col] = path;
		}

	    public void DrawStone(GridLocation location, Player player)
		{
			if (player == Player.Unoccupied)
			{
				var rmvStone = _stones[location.Row, location.Column];
				if (rmvStone == null)
				{
					return;
				}
				_boardCanvas.Children.Remove(rmvStone);
				_stones[location.Row, location.Column] = null;
				return;
			}

			if (_stones[location.Row, location.Column] != null)
			{
				throw new ArgumentException("Placing stone on occupied cell");
			}

			var stone = DrawStone(GetCenter(location), player);
			_stones[location.Row, location.Column] = stone;
		}

	    private Shape DrawStone(Point placement, Player player)
		{
			Brush fill = new SolidColorBrush(player == Player.White ? Colors.White : Colors.Black);
			Brush stroke = new SolidColorBrush(Colors.Black);
			var stoneDiameter = HexCell.S3D2 * 2.0 * StoneShrinkage * _cellWidth;
			Shape stone = new Ellipse
			{
				Fill = fill,
				Stroke = stroke,
				IsHitTestVisible = false,
				Visibility = Visibility.Visible,
				Width = stoneDiameter,
				Height = stoneDiameter,
			};
			Canvas.SetTop(stone, placement.Y - stoneDiameter / 2);
			Canvas.SetLeft(stone, placement.X - stoneDiameter / 2);
			_boardCanvas.Children.Add(stone);

			return stone;
		}
        #endregion

        #region Event handlers
        private void ClickCell(object sender, MouseButtonEventArgs e)
	    {
	        var cell = (Path)sender;
	        var gridLocation = (GridLocation)cell.Tag;
	        _board.Clicked(gridLocation);
	    }

	    private void EnterCell(object sender, MouseEventArgs e)
	    {
	        var cell = (Path)sender;
	        var gridLocation = (GridLocation)cell.Tag;
	        _lblLocation.Content = gridLocation;
	        _cellEnterCount++;
	    }

	    private void LeaveCell(object sender, MouseEventArgs e)
	    {
	        if (--_cellEnterCount == 0)
	        {
	            _lblLocation.Content = String.Empty;
	        }
	    }
        #endregion
    }
}
