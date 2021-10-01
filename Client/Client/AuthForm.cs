using System;
using System.Windows.Forms;

namespace Client
{
    public partial class AuthForm : Form
    {
        bool attempt = false;
        RegForm Reg;
        ChatForm Chat;

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
            if (Reg == null)
            {
                Reg = new RegForm();
                Reg.Owner = this;
            }        

            Reg.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                label3.Text = "Заполните пустые поля!";
                return;
            }

            if(Chat == null)
            {
                Chat = new ChatForm();
                Chat.Owner = this;
            }

            Chat.UserName = textBox1.Text;
            Chat.Password = textBox2.Text;
            
            Chat.Show();
            
            if (!attempt)
                Chat.Handshake(true);
            else
                Chat.Login();

            this.Hide();

            attempt = true;
        }
    }
}