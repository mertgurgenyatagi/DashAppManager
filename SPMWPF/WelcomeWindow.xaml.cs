using System.Windows;

namespace SPMWPF;

public partial class WelcomeWindow : Window
{
    public WelcomeWindow()
    {
        InitializeComponent();
        OkBtn.Click += OkBtn_Click;
        DontShowAgainBtn.Click += DontShowAgainBtn_Click;
    }

    private void OkBtn_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = true;
        this.Close();
    }

    private void DontShowAgainBtn_Click(object sender, RoutedEventArgs e)
    {
        SettingsStore.SetDontShowWelcome(true);
        this.DialogResult = true;
        this.Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
        this.Close();
    }
}
