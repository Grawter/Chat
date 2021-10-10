using System;
using Client.Helpers;
using Client.Interfaces;
using System.Windows.Forms;

namespace Client
{
    public partial class RegForm : Form
    {
        IShowInfo showInfo = new ShowInfo();
        bool attempt = false;
        ChatForm Chat;

        public string Status
        {
            set { label5.Text = value; }
        }

        public RegForm()
        {
            InitializeComponent();
        }

        private void RegForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Owner.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string UserName = textBox1.Text, Email = textBox2.Text, Password = textBox3.Text;

            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(textBox4.Text))
            {
                label5.Text = "Заполните пустые поля!";
                return;
            }

            if (!Password.Equals(textBox4.Text))
            {
                label5.Text = "Пароли не совпадают!";
                return;
            }

            switch (DataValidation.IsValid(UserName, Email, Password))
            {
                case 1:
                    label5.Text = "Логин меньше 6 символов";
                    return;

                case 2:
                    label5.Text = "Email не соответствует формату";
                    return;

                case 3:
                    label5.Text = "Пароли не соответствует формату";
                    showInfo.ShowMessage("Пароль должен содержать не менее 6 символов, включая минимум один формата [a-z],[A-Z],[0-9] и спец. символ");
                    return;
            }

            if (Chat == null)
            {
                Chat = new ChatForm() { Owner = this };
            }

            Chat.UserName = UserName;
            Chat.Email = Email;
            Chat.Password = Password;

            Chat.Show();

            if(!attempt)
                Chat.Handshake(false);
            else
                Chat.Registration();

            this.Hide();

            attempt = true;
        }

    }
}