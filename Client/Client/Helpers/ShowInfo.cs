using System.Windows.Forms;
using Client.Interfaces;

namespace Client.Helpers
{
    public class ShowInfo : IShowInfo
    {
        public object ShowMessage(string message, int type = 1)
        {
            return type switch
            {
                1 => MessageBox.Show(message, "Уведомление", MessageBoxButtons.OK, MessageBoxIcon.Information),
                2 => MessageBox.Show(message, "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning),
                3 => MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error),
                4 => MessageBox.Show(message, "Решение", MessageBoxButtons.YesNo, MessageBoxIcon.Question),
                _ => MessageBox.Show(message),
            };
        }
    }
}