using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Client.Helpers
{
    public class DataValidation
    {
        public static int isValid(string name, string email, string password)
        {
            if (name.Length < 6)
                return 1;

            if (!MailAddress.TryCreate(email, out var mailAddress))
                return 2;

            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9])\S{6,16}$";

            if (!Regex.IsMatch(password, pattern))
                return 3;           

            return 0;
        }
    }
}