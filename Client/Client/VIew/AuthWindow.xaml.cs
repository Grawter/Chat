using System.Windows;
using System.Windows.Controls;
using Client.ViewModel;

namespace Client
{
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
        }

        private void OnPasswordChange(object sender, RoutedEventArgs e)
        {
            if (PB1.Password.Length > 0)
                WaterMark1TB.Visibility = Visibility.Collapsed;
            else
                WaterMark1TB.Visibility = Visibility.Visible;

            if (this.DataContext != null)
                ((AuthViewModel)this.DataContext).Password = ((PasswordBox)sender).Password;

        }
    }
}