using System.ComponentModel;
using System.Windows;
using Client.Helpers;
using Client.Interfaces;

namespace Client
{
    public partial class RegWindow : Window
    {
        IShowInfo showInfo = new ShowInfo();
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
            string UserName = TB1.Text, Email = TB2.Text, Password = PB1.Password;

            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(PB2.Password))
            {
                WarningTBlock.Text = "Заполните пустые поля!";
                return;
            }

            if (!Password.Equals(PB2.Password))
            {
                WarningTBlock.Text = "Пароли не совпадают!";
                return;
            }

            switch (DataValidation.IsValid(UserName, Email, Password))
            {
                case 1:
                    WarningTBlock.Text = "Логин меньше 6 символов";
                    return;

                case 2:
                    WarningTBlock.Text = "Email не соответствует формату";
                    return;

                case 3:
                    WarningTBlock.Text = "Пароли не соответствует формату";
                    showInfo.ShowMessage("Пароль должен содержать не менее 6 символов, включая минимум один формата [a-z],[A-Z],[0-9] и спец. символ");
                    return;
            }

            if (Chat == null)
            {
                Chat = new ChatWindow() { Owner = this };
            }

            Chat.UserName = UserName;
            Chat.Email = Email;
            Chat.Password = Password;

            Chat.Show();

            if (!attempt)
                Chat.Handshake(false);
            else
                Chat.Registration();

            this.Hide();

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