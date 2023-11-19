using Server;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ChatApp;
class Program
{

    static void Server(string name)
    {
        try
        {
            UdpClient udpClient;
            IPEndPoint remoteEndPoint;

            udpClient = new UdpClient(12345);
            remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine("UDP Клиент ожидает сообщений...");

            while (true)
            {
                byte[] receiveBytes = udpClient.Receive(ref remoteEndPoint);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        byte[] data = br.ReadBytes(receiveBytes.Length);
                        string receivedData = Encoding.ASCII.GetString(receiveBytes);
                        var message = Message.FromJson(receivedData);

                        Console.WriteLine("Получение подтверждено");

                        Console.WriteLine($"Получено сообщение от {message.FromName} ({message.Date}):");
                        Console.WriteLine(message.Text);
                    }
                }

                Console.Write("Введите ответ и нажмите Enter: ");
                string replyMessage = Console.ReadLine();

                var replyMessageJson = new Message() { Date = DateTime.Now, FromName = name, Text = replyMessage }.ToJson();

                byte[] replyBytes = Encoding.ASCII.GetBytes(replyMessageJson);

                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(replyBytes);
                    }

                    byte[] data = stream.ToArray();

                    udpClient.Send(data, data.Length, remoteEndPoint);
                    Console.WriteLine("Ответ отправлен.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при обработке сообщения: " + ex.Message);
        }
    }


    static void Client(string name, string ip)
    {
        UdpClient udpClient;
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), 12345);

        udpClient = new UdpClient();
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), 12345);

        while (true)
        {
            try
            {
                
                Console.WriteLine("UDP Клиент ожидает ввода сообщения");

                Console.Write("Введите сообщение и нажмите Enter: ");
                string message = Console.ReadLine();

                var messageJson = new Message() { Date = DateTime.Now, FromName = name, Text = message }.ToJson();

                byte[] replyBytes = Encoding.ASCII.GetBytes(messageJson);

                using (MemoryStream ms = new MemoryStream())
                {
                    using(BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write(replyBytes);
                    }
                    byte[] data = ms.ToArray();

                    Console.WriteLine("Отправка подтверждена");

                    udpClient.Send(data, data.Length, remoteEndPoint);

                    Console.WriteLine("Сообщение отправлено.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при обработке сообщения: " + ex.Message);
            }

            byte[] receiveBytes = udpClient.Receive(ref remoteEndPoint);

            using(MemoryStream ms = new MemoryStream())
            {
                using(BinaryReader br = new BinaryReader(ms))
                {
                    byte[] data = br.ReadBytes(receiveBytes.Length);
                    string receivedData = Encoding.ASCII.GetString(receiveBytes);
                    var messageReceived = Message.FromJson(receivedData);

                    Console.WriteLine("Получение подтверждено");

                    Console.WriteLine($"Получено сообщение от {messageReceived.FromName} ({messageReceived.Date}):");
                    Console.WriteLine(messageReceived.Text);
                }
            }
        }
    }


    static void Main(string[] args)
    {
        if (args.Length == 1)
        {
            Console.WriteLine(string.Join(" ", args));
            Server(args[0]);
        }
        else
        if (args.Length == 2)
        {
            Client(args[0], args[1]);
        }
        else
        {
            Console.WriteLine("Для запуска сервера введите ник-нейм как параметр запуска приложения");
            Console.WriteLine("Для запуска клиента введите ник-нейм и IP сервера как параметры запуска приложения");
        }

    }
}
