using System.Windows;
using static Chat_Client.Library;

namespace Chat_Client
{
    public partial class ErrorWindow : Window
    {
        public ErrorWindow(string title, string description, AlertType type)
        {
            InitializeComponent();
            ErrorTitle.Content = title;

            switch (type)
            {
                case AlertType.Error: ErrorDescription.Content = "Reason: " + description; break;
                case AlertType.Notification: ErrorDescription.Content = "Description: " + description; break;
            }
        }

        void OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}