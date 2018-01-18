using System.Windows;

namespace Hex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static MainWindow Main;
        internal Board Board;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Main = this;
            Board = new Board();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Board?.Resize();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Board.Clear();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Board.Undo();
        }
    }
}
