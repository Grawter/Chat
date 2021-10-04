using System;
using Client.Helpers;
using Server.Interfaces;
using System.Windows.Forms;

namespace Client
{
    public partial class AddingForm : Form
    {
        IShowInfo showInfo = new ShowInfo();
        public AddingForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                (Owner as ChatForm).Friend = textBox1.Text;
                Owner.Show();
                this.Close();
            }     
            else
                showInfo.ShowMessage("Имя не указано", 2);
        }
    }
}