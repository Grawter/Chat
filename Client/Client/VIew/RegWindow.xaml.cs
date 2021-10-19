using System.Windows;
using System.Windows.Controls;
using Client.ViewModel;

namespace Client
{
    public partial class RegWindow : Window
    {
        public RegWindow()
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
                ((RegViewModel)this.DataContext).Password = ((PasswordBox)sender).Password;

        }

        private void OnPasswordChangeConf(object sender, RoutedEventArgs e)
        {
            if (PB2.Password.Length > 0)
                WaterMark2TB.Visibility = Visibility.Collapsed;
            else
                WaterMark2TB.Visibility = Visibility.Visible;

            if (this.DataContext != null)
                ((RegViewModel)this.DataContext).ConfPassword = ((PasswordBox)sender).Password;

        }
    }
}