using Client.Helpers;
using Client.Interfaces;
using System.Windows;

namespace Client
{
    public partial class AddingWindow : Window
    {
        IShowInfo showInfo = new ShowInfo();
        public AddingWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TB1.Text))
            {
                (Owner as ChatWindow).Friend = TB1.Text;
                Owner.Show();
                this.Close();
            }
            else
                showInfo.ShowMessage("Имя не указано", 2);
        }
    }
}
