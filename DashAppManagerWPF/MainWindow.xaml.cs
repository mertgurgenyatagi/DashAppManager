using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DashAppManagerWPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Button? lastClickedButton = null;
    
    // Cache page instances for better performance
    private readonly Dictionary<string, UserControl> _pageCache = new();
    
    // Cache commonly used elements
    private ContentControl? _pageContentHost;
    private Canvas? _dotsCanvas;
    
    // Cache for button template elements to avoid repeated FindName calls
    private readonly Dictionary<Button, (Border border, Image image)> _buttonElementCache = new();
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Cache frequently accessed elements
        _pageContentHost = (ContentControl)FindName("PageContentHost");
        _dotsCanvas = (Canvas)FindName("DotsCanvas");
        
        Loaded += MainWindow_Loaded;
        MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
        RegisterNavEvents();
    }

    private void RegisterNavEvents()
    {
        var navCanvas = (Canvas)FindName("NavCanvas");
        if (navCanvas != null)
        {
            foreach (var child in navCanvas.Children)
            {
                if (child is Button btn)
                {
                    double top = Canvas.GetTop(btn);
                    if (top == 188) btn.Click += (s, e) => { UpdateLastClickedButton(btn); ShowPage("dashboard"); };
                    else if (top == 237) btn.Click += (s, e) => { UpdateLastClickedButton(btn); ShowPage("user"); };
                    else if (top == 286) btn.Click += (s, e) => { UpdateLastClickedButton(btn); ShowPage("notifications"); };
                    else if (top == 335) btn.Click += (s, e) => { UpdateLastClickedButton(btn); ShowPage("calendar"); };
                    else if (top == 384) btn.Click += (s, e) => { UpdateLastClickedButton(btn); ShowPage("config"); };
                    else if (top == 433) btn.Click += (s, e) => { UpdateLastClickedButton(btn); ShowPage("ai"); };
                    else if (top == 482) btn.Click += (s, e) => { UpdateLastClickedButton(btn); ShowPage("settings"); };
                }
            }
        }
    }

    private void UpdateLastClickedButton(Button clickedButton)
    {
        // Reset previous button to normal state
        if (lastClickedButton != null)
        {
            SetButtonActiveState(lastClickedButton, false);
        }
        
        // Set new button to active state
        SetButtonActiveState(clickedButton, true);
        lastClickedButton = clickedButton;
    }

    private void ShowPage(string page)
    {
        if (_pageContentHost == null) return;
        
        // Use cached page instances instead of creating new ones
        if (!_pageCache.ContainsKey(page))
        {
            _pageCache[page] = page switch
            {
                "dashboard" => new DashboardPage(),
                "user" => new UserPage(),
                "notifications" => new NotificationsPage(),
                "calendar" => new CalendarPage(),
                "config" => new ConfigPage(),
                "ai" => new AIPage(),
                "settings" => new SettingsPage(),
                _ => new DashboardPage()
            };
        }
        
        // Skip animation if same page
        if (_pageContentHost.Content == _pageCache[page]) return;
        
        // Simplified fade transition
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(100)); // Reduced from 150ms
        fadeOut.Completed += (s, e) =>
        {
            _pageContentHost.Content = _pageCache[page];
            
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(100)); // Reduced from 150ms
            _pageContentHost.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        };
        
        _pageContentHost.BeginAnimation(UIElement.OpacityProperty, fadeOut);
    }
    private void SetButtonActiveState(Button btn, bool isActive)
    {
        // Use cached elements to avoid repeated template searches
        if (!_buttonElementCache.ContainsKey(btn))
        {
            var template = btn.Template;
            if (template != null)
            {
                var border = template.FindName("HoverBorder", btn) as Border;
                var image = template.FindName("DashboardIcon", btn) as Image ??
                           template.FindName("UserIcon", btn) as Image ??
                           template.FindName("NotificationsIcon", btn) as Image ??
                           template.FindName("CalendarIcon", btn) as Image ??
                           template.FindName("ConfigIcon", btn) as Image ??
                           template.FindName("AIIcon", btn) as Image ??
                           template.FindName("SettingsIcon", btn) as Image;
                
                _buttonElementCache[btn] = (border!, image!);
            }
        }
        
        if (_buttonElementCache.TryGetValue(btn, out var elements))
        {
            var (border, image) = elements;
            
            if (border != null)
            {
                // Use direct color assignment instead of creating new brushes
                border.Background = isActive ? 
                    new SolidColorBrush(Color.FromArgb(0xFF, 0x35, 0x34, 0x54)) : 
                    new SolidColorBrush(Color.FromArgb(0x00, 0x35, 0x34, 0x54));
            }
            
            if (image != null)
            {
                string iconName = image.Name?.Replace("Icon", "").ToLower() ?? "";
                if (!string.IsNullOrEmpty(iconName))
                {
                    // Use simpler URI construction
                    string iconPath = isActive ? 
                        $"pack://application:,,,/assets/icons/{iconName}_blue.png" :
                        $"pack://application:,,,/assets/icons/{iconName}.png";
                    
                    image.Source = new BitmapImage(new Uri(iconPath));
                }
            }
        }
    }

    private void AIButton_Click(object sender, RoutedEventArgs e)
    {
        var pageContentHost = (ContentControl)FindName("PageContentHost");
        if (pageContentHost != null)
        {
            pageContentHost.Content = new AIPage();
        }
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
        // Optimize dot grid generation - use larger spacing for fewer elements
        CreateOptimizedDotGrid();
    }

    private void CreateOptimizedDotGrid()
    {
        if (_dotsCanvas == null) return;
        
        // Optimized parameters - fewer dots for better performance
        double dotRadius = 1.0;
        double spacing = 12; // Doubled spacing = 4x fewer dots
        double gridWidth = 577;
        double gridHeight = 190;
        Color dotColor = (Color)ColorConverter.ConvertFromString("#34354b");

        int cols = (int)(gridWidth / spacing);
        int rows = (int)(gridHeight / spacing);

        // Clear existing and create new dots
        _dotsCanvas.Children.Clear();
        
        // Use single brush instance for all dots
        var dotBrush = new SolidColorBrush(dotColor);
        dotBrush.Freeze(); // Freeze for better performance
        
        for (int y = 0; y < rows; y++)
        {
            double opacity = 1.0;
            if (y * spacing > 125)
            {
                double fadeLength = gridHeight - 125;
                opacity = 1.0 - ((y * spacing - 125) / fadeLength);
                if (opacity < 0) opacity = 0;
            }
            
            for (int x = 0; x < cols; x++)
            {
                // Use Rectangle instead of Ellipse for better performance
                var dot = new Rectangle
                {
                    Width = dotRadius * 2,
                    Height = dotRadius * 2,
                    Fill = dotBrush,
                    Opacity = opacity,
                    RadiusX = dotRadius,
                    RadiusY = dotRadius
                };
                Canvas.SetLeft(dot, x * spacing);
                Canvas.SetTop(dot, y * spacing);
                _dotsCanvas.Children.Add(dot);
            }
        }
    }
}