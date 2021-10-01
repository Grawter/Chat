using System;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Client
{
    public partial class KeySetForm : Form
    {
        public string Username
        {
            get { return username; }
            set { username = value; }
        }
        private string username;

        public byte[] Current_key
        {
            get { return current_key; }
            set { current_key = value; }
        }
        private byte[] current_key;

        public KeySetForm()
        {
            InitializeComponent();         
        }

        private void KeySetForm_Load(object sender, EventArgs e)
        {
            label2.Text = username;

            if (current_key != null)
            {
                string key_str = "";
                foreach (var item in current_key)
                {
                    if (item < 10)
                        key_str += "00" + item;
                    else if (item >= 10 && item < 100)
                        key_str += "0" + item;
                    else
                        key_str += item;
                }

                maskedTextBox1.Text = key_str;
            }       
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Owner.Show();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Aes aes = Aes.Create();

            string key_str = "";
            foreach (var item in aes.Key)
            {
                if (item < 10)
                    key_str += "00" + item;
                else if (item >= 10 && item < 100)
                    key_str += "0" + item;
                else
                    key_str += item;
            }

            maskedTextBox1.Text = key_str;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string[] mass = maskedTextBox1.Text.Split(".");
            byte[] key_mass = new byte[mass.Length];

            try
            {
                for (int i = 0; i < mass.Length; i++)
                {
                    key_mass[i] = byte.Parse(mass[i]);
                }

                (Owner as ChatForm).Mass_k = key_mass;

                Owner.Show();
                this.Close();
            }
            catch (OverflowException)
            {
                MessageBox.Show("Указывайте цифры от 0 до 255");
            }
            catch (FormatException)
            {
                MessageBox.Show("Ключ не указан, либо указан не полностью");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            (Owner as ChatForm).DelKey(Username);
            maskedTextBox1.Text = "";
            MessageBox.Show("Ключ отключён");         
        }
      
    }
}