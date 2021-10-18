using System.Windows;

namespace Client
{
    public partial class AuthWindow : Window
    {
        bool attempt = false;
        ChatWindow Chat;
        public string Status
        {
            set { WarningTBlock.Text = value; }
        }

        public AuthWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RegWindow Reg = new RegWindow();
            Reg.Owner = this;
            Reg.Show();

            this.Hide();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TB1.Text) || string.IsNullOrWhiteSpace(PB1.Password))
            {
                WarningTBlock.Text = "Заполните пустые поля!";
                return;
            }

            if (Chat == null)
            {
                Chat = new ChatWindow() { Owner = this };
            }

            Chat.Show();

            if (!attempt)
                Chat.Handshake(true, TB1.Text, PB1.Password);
            else
                Chat.Login(TB1.Text, PB1.Password);

            this.Hide();

            PB1.Clear();

            attempt = true;
        }

        private void OnPasswordChange(object sender, RoutedEventArgs e)
        {
            if (PB1.Password.Length > 0)
                WaterMark1TB.Visibility = Visibility.Collapsed;
            else
                WaterMark1TB.Visibility = Visibility.Visible;
        }
    }

}