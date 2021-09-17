using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress address = IPAddress.Parse(Server.Host);

            Server.ServerSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Server.ServerSocket.Bind(new IPEndPoint(address, Server.Port)); // установка точки для прослушивания входящих подключений (связывает объект Socket с локальной конечной точкой)
            Server.ServerSocket.Listen(100); // запуск прослушивания ( В качестве параметра он принимает количество входящих подключений, которые могут быть поставлены в очередь сокета.)

            Console.WriteLine($"Сервер доступен по адресу {Server.Host}:{Server.Port}");
            Console.WriteLine("Ожидание подключений...");

            while (Server.Work)
            {
                Socket handle = Server.ServerSocket.Accept(); // создает новый объект Socket для обработки входящего подключения (Если очередь запросов пуста, то метод Accept
                                                              // блокирует вызывающий поток до появления нового подключения.)
                Console.WriteLine($"Новое подключение: {handle.RemoteEndPoint}"); // возвращает айпишник подключения
                new UsersFunc(handle);

            }

            Console.WriteLine("Сервер закрывается...");

        }
    }
}