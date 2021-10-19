using System.Windows;
using Client.ViewModel;

namespace Client
{
    public partial class ChatWindow : Window
    {
        public ChatWindow()
        {
            InitializeComponent();
            this.Closing += ChatWindow_Closing;
        }
       
        private void ChatWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) // При закрытии окна
        {
            (DataContext as ChatViewModel).CloseCommand.Execute(null);
        }
        
    }
}