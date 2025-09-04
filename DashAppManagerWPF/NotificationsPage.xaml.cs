using System.Windows.Controls;
using System.Windows;

namespace DashAppManagerWPF
{
    public partial class NotificationsPage : UserControl
    {
        public NotificationsPage()
        {
            InitializeComponent();
        }

        private void MarkAllAsRead_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mark All as Read clicked!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Type_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Type filter clicked!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Sort_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sort filter clicked!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
