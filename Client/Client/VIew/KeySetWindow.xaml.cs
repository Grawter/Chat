using System.Windows;
using Client.ViewModel;

namespace Client
{
    public partial class KeySetWindow : Window
    {
        public KeySetWindow()
        {
            InitializeComponent();
            this.Closing += KeySetWindow_Closing;
        }

        private void KeySetWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) // При закрытии окна
        {
            (DataContext as KeySetViewModel).CloseCommand.Execute(null);
        }

    }
}