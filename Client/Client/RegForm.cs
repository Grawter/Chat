using System;
using System.Windows.Forms;

namespace Client
{
    public partial class RegForm : Form
    {
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
            if (string.IsNullOrWhiteSpace(textBox1.Text)|| string.IsNullOrWhiteSpace(textBox2.Text)|| string.IsNullOrWhiteSpace(textBox3.Text) || string.IsNullOrWhiteSpace(textBox4.Text))
            {
                label5.Text = "Заполните пустые поля!";
                return;
            }

            if (!textBox3.Text.Equals(textBox4.Text))
            {
                label5.Text = "Пароли не совпадают!";
                return;
            }

            ChatForm Chat = new ChatForm();
            Chat.Owner = this;

            Chat.UserName = textBox1.Text;
            Chat.Email = textBox2.Text;
            Chat.Password = textBox3.Text;

            Chat.Show();
            Chat.Handshake(false);
            this.Hide();
        }

    }
}