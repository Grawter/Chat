using System;
using System.Windows;
using System.Security.Cryptography;
using Client.Helpers;
using Client.Helpers.KeySet;
using Client.Interfaces;

namespace Client
{
    public partial class KeySetWindow : Window
    {
        IShowInfo showInfo = new ShowInfo();
        public string UserName
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

        public KeySetWindow()
        {
            InitializeComponent();
            this.Loaded += KeySetWindow_Loaded;
        }

        private void KeySetWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TBlock1.Text = username;
            TBlock2.Text = username;

            if (current_simmetric_key != null)
                MaskedTB1.Value = ByteConvStr.ByteToStr(current_simmetric_key);

            if (current_asimmetric_key.Item1.Modulus != null)
                TB1.Text = Convert.ToBase64String(current_asimmetric_key.Item1.Modulus);

            if (usernamersa.Modulus != null)
                TB2.Text = Convert.ToBase64String(usernamersa.Modulus);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Aes aes = Aes.Create();

            MaskedTB1.Value = ByteConvStr.ByteToStr(aes.Key);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string[] str_mass = MaskedTB1.Text.Split(".");

            try
            {
                byte[] key_mass = ByteConvStr.StrToByte(str_mass);
                (Owner as ChatWindow).SetAES(username, key_mass);

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

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            (Owner as ChatWindow).DelKey(UserName, true);
            MaskedTB1.Text = "";
            showInfo.ShowMessage("Ключ удалён");
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(MaskedTB1.Text);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            RSA RSA = RSA.Create();

            RSA.KeySize = CB1.SelectedIndex switch
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

            TB1.Text = Convert.ToBase64String(current_asimmetric_key.Item1.Modulus);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            bool send = false;

            if (CurrentAsimmetricKey.Item1.Modulus != null && CurrentAsimmetricKey.Item2.Modulus != null)
            {
                (Owner as ChatWindow).SetMyRSA(UserName, ref current_asimmetric_key);
                send = true;
            }

            if (!string.IsNullOrWhiteSpace(TB2.Text))
            {
                (Owner as ChatWindow).SetOtherRSA(UserName, TB2.Text);
                send = true;
            }

            if (send)
                showInfo.ShowMessage("Ключ установлен");
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            (Owner as ChatWindow).DelKey(UserName, false);
            TB1.Text = "";
            TB2.Text = "";
            showInfo.ShowMessage("Ключ удалён");
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            if (current_asimmetric_key.Item1.Modulus != null && current_asimmetric_key.Item2.Modulus != null)
                (Owner as ChatWindow).SendRSA(username, TB1.Text);

            Owner.Show();
            this.Close();
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TB1.Text);
        }
    }
}