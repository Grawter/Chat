using System;
using System.Windows.Forms;

namespace Client
{
    public partial class AuthForm : Form
    {
        public string Status
        {
            set { label3.Text = value; }
        }

        public AuthForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RegForm Reg = new RegForm();
            Reg.Owner = this;
            Reg.Show();

            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "" || textBox1.Text == "")
            {
                label3.Text = "Заполните пустые поля!";
                return;
            }

            ChatForm Chat = new ChatForm();
            Chat.Owner = this;

            Chat.UserName = textBox1.Text;
            Chat.Password = textBox2.Text;

            Chat.Show();
            Chat.Login();
            this.Hide();
        }
    }
}