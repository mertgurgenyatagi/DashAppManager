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

namespace DashAppManagerWPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Button? lastClickedButton = null;
    
    public MainWindow()
    {
        InitializeComponent();
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
        var pageContentHost = (ContentControl)FindName("PageContentHost");
        if (pageContentHost == null) return;
        
        // Fade out current content, then fade in new content
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
        fadeOut.Completed += (s, e) =>
        {
            // Set new content
            switch (page)
            {
                case "dashboard":
                    pageContentHost.Content = new DashboardPage();
                    break;
                case "user":
                    pageContentHost.Content = new UserPage();
                    break;
                case "notifications":
                    pageContentHost.Content = new NotificationsPage();
                    break;
                case "calendar":
                    pageContentHost.Content = new CalendarPage();
                    break;
                case "config":
                    pageContentHost.Content = new ConfigPage();
                    break;
                case "ai":
                    pageContentHost.Content = new AIPage();
                    break;
                case "settings":
                    pageContentHost.Content = new SettingsPage();
                    break;
            }
            
            // Fade in new content
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
            pageContentHost.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        };
        
        pageContentHost.BeginAnimation(UIElement.OpacityProperty, fadeOut);
    }
    private void SetButtonActiveState(Button btn, bool isActive)
    {
        // Find the HoverBorder and Icon within the button's template
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
            
            if (border != null)
            {
                border.Background = new SolidColorBrush(isActive ? 
                    Color.FromArgb(0xFF, 0x35, 0x34, 0x54) : 
                    Color.FromArgb(0x00, 0x35, 0x34, 0x54));
            }
            
            if (image != null)
            {
                // Get the icon name from the image name
                string iconName = image.Name?.Replace("Icon", "").ToLower() ?? "";
                if (!string.IsNullOrEmpty(iconName))
                {
                    if (isActive)
                    {
                        // Set to blue version of icon
                        image.Source = new BitmapImage(new Uri($"pack://application:,,,/assets/icons/{iconName}_blue.png"));
                    }
                    else
                    {
                        // Set to white version of icon
                        image.Source = new BitmapImage(new Uri($"pack://application:,,,/assets/icons/{iconName}.png"));
                    }
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
        // Dot grid parameters
        double dotRadius = 1.0; // 2x2 px
        double spacing = 6;
        double gridWidth = 577;
        double gridHeight = 190;
        Color dotColor = (Color)ColorConverter.ConvertFromString("#34354b");

        int cols = (int)(gridWidth / spacing);
        int rows = (int)(gridHeight / spacing);

        var dotsCanvas = (Canvas)FindName("DotsCanvas");
        if (dotsCanvas != null)
        {
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
                    dotsCanvas.Children.Add(dot);
                }
            }
        }
    }
}