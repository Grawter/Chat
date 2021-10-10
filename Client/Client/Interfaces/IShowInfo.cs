

namespace Client.Interfaces
{
    interface IShowInfo
    {
        object ShowMessage(string message, int type = 1);   // Показ сообщения
    }
}