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
    private ContentControl? _pageContentHost1;
    private ContentControl? _pageContentHost2;
    private Canvas? _dotsCanvas;
    private bool _useHost1 = true; // Toggle between the two hosts
    private string? _currentPage = null; // Track the currently displayed page
    
    // Cache for button template elements to avoid repeated FindName calls
    private readonly Dictionary<Button, (Border border, Image image)> _buttonElementCache = new();
    
    // Static cached brushes for better performance
    private static readonly SolidColorBrush ActiveBrush = new(Color.FromArgb(0xFF, 0x35, 0x34, 0x54));
    private static readonly SolidColorBrush InactiveBrush = new(Color.FromArgb(0x00, 0x35, 0x34, 0x54));
    
    // Cache for preloaded images
    private static readonly Dictionary<string, BitmapImage> _imageCache = new();
    
    static MainWindow()
    {
        // Freeze brushes for performance
        ActiveBrush.Freeze();
        InactiveBrush.Freeze();
        
        // Preload commonly used images
        PreloadImages();
    }
    
    private static void PreloadImages()
    {
        var icons = new[] { "dashboard", "user", "notifications", "calendar", "config", "ai", "settings" };
        foreach (var icon in icons)
        {
            try
            {
                var normalUri = new Uri($"pack://application:,,,/assets/icons/{icon}.png");
                var blueUri = new Uri($"pack://application:,,,/assets/icons/{icon}_blue.png");
                
                var normalImage = new BitmapImage(normalUri);
                var blueImage = new BitmapImage(blueUri);
                
                normalImage.Freeze();
                blueImage.Freeze();
                
                _imageCache[icon] = normalImage;
                _imageCache[$"{icon}_blue"] = blueImage;
            }
            catch
            {
                // Ignore missing images
            }
        }
    }
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Cache frequently accessed elements
        _pageContentHost1 = (ContentControl)FindName("PageContentHost1");
        _pageContentHost2 = (ContentControl)FindName("PageContentHost2");
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
        if (_pageContentHost1 == null || _pageContentHost2 == null) return;
        
        // Control dots grid visibility - only show on dashboard page
        if (_dotsCanvas != null)
        {
            _dotsCanvas.Visibility = page == "dashboard" ? Visibility.Visible : Visibility.Collapsed;
        }
        
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
        
        var newPage = _pageCache[page];
        
        // Skip animation if same page is already showing
        if (_currentPage == page) return;
        
        // Determine which host to use for the new page and which has the current page
        var newHost = _useHost1 ? _pageContentHost1 : _pageContentHost2;
        var oldHost = _useHost1 ? _pageContentHost2 : _pageContentHost1;
        
        // Disable hit testing on the old host immediately to prevent ghost button issues
        oldHost.IsHitTestVisible = false;
        
        // Set the new page in the new host (initially transparent)
        newHost.Content = newPage;
        newHost.Opacity = 0;
        newHost.IsHitTestVisible = true; // Ensure new host can receive hits
        
        // Create optimized overlapping animations with faster duration
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150)); // Reduced from 200ms
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));  // Reduced from 200ms
        
        // Use QuadraticEase for smoother performance
        var easing = new QuadraticEase { EasingMode = EasingMode.EaseOut };
        fadeOut.EasingFunction = easing;
        fadeIn.EasingFunction = easing;
        
        // Start both animations simultaneously for true overlap
        oldHost.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        newHost.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        
        // After transition, clear the old host content and reset its state
        fadeOut.Completed += (s, e) =>
        {
            oldHost.Content = null;
            oldHost.Opacity = 1; // Reset for next use
            oldHost.IsHitTestVisible = true; // Re-enable for future use
        };
        
        // Update current page tracking
        _currentPage = page;
        
        // Toggle which host to use next time
        _useHost1 = !_useHost1;
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
                // Use cached static brushes
                border.Background = isActive ? ActiveBrush : InactiveBrush;
            }
            
            if (image != null)
            {
                string iconName = image.Name?.Replace("Icon", "").ToLower() ?? "";
                if (!string.IsNullOrEmpty(iconName))
                {
                    // Use cached images
                    string imageKey = isActive ? $"{iconName}_blue" : iconName;
                    if (_imageCache.TryGetValue(imageKey, out var cachedImage))
                    {
                        image.Source = cachedImage;
                    }
                }
            }
        }
    }

    private void AIButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPage("ai");
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
        
        // Initialize with dashboard page
        ShowPage("dashboard");
    }

    private void CreateOptimizedDotGrid()
    {
        if (_dotsCanvas == null) return;
        
        // More aggressive optimization - even fewer dots and less rendering complexity
        double dotRadius = 0.8; // Smaller dots
        double spacing = 16; // Larger spacing = even fewer dots
        double gridWidth = 577;
        double gridHeight = 140; // Reduced height
        Color dotColor = (Color)ColorConverter.ConvertFromString("#34354b");

        int cols = (int)(gridWidth / spacing);
        int rows = (int)(gridHeight / spacing);

        // Clear existing and create new dots
        _dotsCanvas.Children.Clear();
        
        // Use single frozen brush instance for all dots
        var dotBrush = new SolidColorBrush(dotColor);
        dotBrush.Freeze();
        
        // Create dots in batches to reduce UI thread blocking
        var dots = new List<Rectangle>(cols * rows);
        
        for (int y = 0; y < rows; y++)
        {
            double opacity = 1.0;
            if (y * spacing > 90) // Start fade earlier
            {
                double fadeLength = gridHeight - 90;
                opacity = 1.0 - ((y * spacing - 90) / fadeLength);
                if (opacity < 0.1) continue; // Skip nearly invisible dots
            }
            
            for (int x = 0; x < cols; x++)
            {
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
                dots.Add(dot);
            }
        }
        
        // Add all dots at once
        foreach (var dot in dots)
        {
            _dotsCanvas.Children.Add(dot);
        }
    }
}