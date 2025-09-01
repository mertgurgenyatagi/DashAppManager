using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DashAppManagerWPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
    }

    private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Only allow drag if mouse is within top 40px of window
        var position = e.GetPosition(this);
    if (position.Y <= 70)
        {
            DragMove();
        }
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Dot grid parameters
        double dotRadius = 1.0; // 2x2 px
        double spacing = 6;
        double gridWidth = 577;
        double gridHeight = 190;
        Color dotColor = (Color)ColorConverter.ConvertFromString("#34354b");

        int cols = (int)(gridWidth / spacing);
        int rows = (int)(gridHeight / spacing);

        for (int y = 0; y < rows; y++)
        {
            double opacity = 1.0;
            // Fade starts at 125px from the top
            if (y * spacing > 125)
            {
                double fadeLength = gridHeight - 125;
                opacity = 1.0 - ((y * spacing - 125) / fadeLength);
                if (opacity < 0) opacity = 0;
            }
            for (int x = 0; x < cols; x++)
            {
                Ellipse dot = new Ellipse
                {
                    Width = dotRadius * 2,
                    Height = dotRadius * 2,
                    Fill = new SolidColorBrush(dotColor),
                    Opacity = opacity
                };
                Canvas.SetLeft(dot, x * spacing);
                Canvas.SetTop(dot, y * spacing);
                DotsCanvas.Children.Add(dot);
            }
        }
    }
}