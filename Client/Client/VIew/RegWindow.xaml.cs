using System.ComponentModel;
using System.Windows;
using Client.Helpers;

namespace Client
{
    public partial class RegWindow : Window
    {
        bool attempt = false;
        ChatWindow Chat;

        public string Status
        {
            set { WarningTBlock.Text = value; }
        }

        public RegWindow()
        {
            InitializeComponent();
            this.Closing += RegWindow_Closing;
        }

        private void RegWindow_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Owner.Show();
            this.Hide();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string UserName = TB1.Text, Email = TB2.Text;

            string Error = DataValidation.IsValid(UserName, Email, PB1.Password, PB2.Password);
            if (!string.IsNullOrEmpty(Error))
            {
                Status = Error;
                return;
            }

            if (Chat == null)
            {
                Chat = new ChatWindow() { Owner = this };
            }

            Chat.Show();

            if (!attempt)
                Chat.Handshake(false, UserName, PB1.Password, Email);
            else
                Chat.Registration(UserName, Email, PB1.Password);

            this.Hide();

            PB1.Clear();
            PB2.Clear();

            attempt = true;
        }

        private void OnPasswordChange(object sender, RoutedEventArgs e)
        {
            if (PB1.Password.Length > 0)
                WaterMark1TB.Visibility = Visibility.Collapsed;
            else
                WaterMark1TB.Visibility = Visibility.Visible;

        }

        private void OnPasswordChangeConf(object sender, RoutedEventArgs e)
        {
            if (PB2.Password.Length > 0)
                WaterMark2TB.Visibility = Visibility.Collapsed;
            else
                WaterMark2TB.Visibility = Visibility.Visible;

        }
    }
}