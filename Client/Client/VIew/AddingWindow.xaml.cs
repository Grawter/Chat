using System.Windows;
using Client.ViewModel;

namespace Client
{
    public partial class AddingWindow : Window
    {
        public AddingWindow()
        {
            InitializeComponent();
            this.Closing += AddingWindow_Closing;
        }

        private void AddingWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) // При закрытии окна
        {
            (DataContext as AddingViewModel).CloseCommand.Execute(null);
        }
    }
}