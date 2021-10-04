using System;
using System.Windows.Forms;
using System.Security.Cryptography;
using Server.Interfaces;
using Client.Helpers;
using Client.Helpers.KeySet;

namespace Client
{
    public partial class KeySetForm : Form
    {
        IShowInfo showInfo = new ShowInfo();
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
                maskedTextBox1.Text = ByteConvStr.ByteToStr(current_key);                  
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Aes aes = Aes.Create();

            maskedTextBox1.Text = ByteConvStr.ByteToStr(aes.Key);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string[] str_mass = maskedTextBox1.Text.Split(".");

            try
            {
                byte[] key_mass = ByteConvStr.StrToByte(str_mass);
                (Owner as ChatForm).Mass_k = key_mass;

                Owner.Show();
                this.Close();
            }
            catch (OverflowException)
            {
                showInfo.ShowMessage("Указывайте цифры от 0 до 255");
            }
            catch (FormatException)
            {
                showInfo.ShowMessage("Ключ не указан, либо указан не полностью", 2);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            (Owner as ChatForm).DelKey(Username);
            maskedTextBox1.Text = "";
            showInfo.ShowMessage("Ключ отключён");         
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(maskedTextBox1.Text);
        }
    }
}