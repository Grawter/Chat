using System.Net.Mail;
using System.Text.RegularExpressions;
using Client.Interfaces;

namespace Client.Helpers
{
    public class DataValidation
    {       
        public static string IsValid(string name, string email, string password, string confpassword)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confpassword))
                return "Заполните пустые поля!";

            if (name.Length < 6)
                return "Логин меньше 6 символов";

            if (!MailAddress.TryCreate(email, out var mailAddress))
                return "Email не соответствует формату";

            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9])\S{6,16}$";

            if (!Regex.IsMatch(password, pattern))
            {
                IShowInfo showInfo = new ShowInfo();
                showInfo.ShowMessage("Пароль должен содержать не менее 6 символов, включая минимум один формата [a-z],[A-Z],[0-9] и спец. символ");
                return "Пароль не соответствует формату";
            }

            if (!password.Equals(confpassword))
                return "Пароли не совпадают!";

            return null;
        }
    }
}