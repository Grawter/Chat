using System;
using Server.Interfaces;

namespace Server.Helpers
{
    public class ShowInfo : IShowInfo
    {
        public void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}