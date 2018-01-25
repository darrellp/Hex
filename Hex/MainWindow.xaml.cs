using System.Windows;

namespace Hex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        internal static MainWindow Main;
        internal BoardDrawing BoardDrawing;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Main = this;
            BoardDrawing = new BoardDrawing();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BoardDrawing?.Redraw();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            BoardDrawing.ClearBoard();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            BoardDrawing.Undo();
        }
    }
}
