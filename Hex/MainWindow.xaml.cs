using System.Windows;
using HexLibrary;

namespace Hex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        internal static MainWindow Main;
        internal BoardDrawing _boardDrawing;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Main = this;
            _boardDrawing = new BoardDrawing();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _boardDrawing?.Redraw();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            _boardDrawing.ClearBoard();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _boardDrawing.Undo();
        }
    }
}
