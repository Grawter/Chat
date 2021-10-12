using System.Windows;
using Client.Interfaces;

namespace Client.Helpers
{
    public class ShowInfo : IShowInfo
    {
        public object ShowMessage(string message, int type = 1)
        {
            return type switch
            {
                1 => MessageBox.Show(message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information),
                2 => MessageBox.Show(message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning),
                3 => MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error),
                4 => MessageBox.Show(message, "Решение", MessageBoxButton.YesNo, MessageBoxImage.Question),
                _ => MessageBox.Show(message),
            };
        }
    }
}