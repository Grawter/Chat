using System;
using System.Windows.Forms;

namespace Client
{
    public partial class AddingForm : Form
    {
        public AddingForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Owner.Show();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                (Owner as ChatForm).Friend = textBox1.Text;
                Owner.Show();
                this.Close();
            }     
            else
                MessageBox.Show("Имя не указано");
        }
    }
}