using System;
using System.Windows.Forms;
using System.Security.Cryptography;
using Client.Interfaces;
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

        public byte[] CurrentSimmetricKey
        {
            get { return current_simmetric_key; }
            set { current_simmetric_key = value; }
        }
        private byte[] current_simmetric_key;

        public (RSAParameters, RSAParameters) CurrentAsimmetricKey
        {
            get { return current_asimmetric_key; }
            set { current_asimmetric_key = value; }
        }
        private (RSAParameters, RSAParameters) current_asimmetric_key;

        public RSAParameters UsernameRSA
        {
            get { return usernamersa; }
            set { usernamersa = value; }
        }
        private RSAParameters usernamersa;

        public KeySetForm()
        {
            InitializeComponent();
        }

        private void KeySetForm_Load(object sender, EventArgs e)
        {
            groupBox1.Enabled = false;
            groupBox2.Enabled = false;

            label2.Text = username;

            if (current_simmetric_key != null)
                maskedTextBox1.Text = ByteConvStr.ByteToStr(current_simmetric_key);

            if(current_asimmetric_key.Item1.Modulus != null)
                textBox1.Text = Convert.ToBase64String(current_asimmetric_key.Item1.Modulus);

            if (usernamersa.Modulus != null)
                textBox2.Text = Convert.ToBase64String(usernamersa.Modulus);

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
                (Owner as ChatForm).SetAES(username, key_mass);

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
            (Owner as ChatForm).DelKey(Username, true);
            maskedTextBox1.Text = "";
            showInfo.ShowMessage("Ключ удалён");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(maskedTextBox1.Text);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                groupBox1.Enabled = true;
                groupBox2.Enabled = false;
            }
            else if (comboBox1.SelectedIndex == 1)
            {
                groupBox1.Enabled = false;
                groupBox2.Enabled = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            RSA RSA = RSA.Create();

            RSA.KeySize = comboBox2.SelectedIndex switch
            {
                0 => 2048,
                1 => 3072,
                2 => 4096,
                3 => 5120,
                4 => 6016,
                _ => 2048,
            };

            current_asimmetric_key.Item1 = RSA.ExportParameters(false);
            current_asimmetric_key.Item2 = RSA.ExportParameters(true);

            textBox1.Text = Convert.ToBase64String(current_asimmetric_key.Item1.Modulus);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            (Owner as ChatForm).SetRSA(username, current_asimmetric_key);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            (Owner as ChatForm).DelKey(Username, false);
            textBox1.Text = "";
            textBox2.Text = "";
            showInfo.ShowMessage("Ключ удалён");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (current_asimmetric_key.Item1.Modulus != null && current_asimmetric_key.Item2.Modulus != null)
                (Owner as ChatForm).SendRSA(username, textBox1.Text);

            Owner.Show();
            this.Close();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
        }
    }
}