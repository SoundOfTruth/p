
using System.IO;
using System.Threading;
using System.Windows;
using System.Text;
using System.Net.Sockets;
using Microsoft.Win32;
using System.Net;

namespace SendSignal
{
    class Program
    {

        static void Main(string[] args)
        {
            IPEndPoint userAddress = new IPEndPoint(IPAddress.Any, 9898);
            IPEndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10001);
            UdpClient udpClient = new UdpClient(userAddress);
            udpClient.Connect(serverAddress);
            byte signal;
            while (true)
            {
                Console.WriteLine("Введите номер сигнала в пределе 48-53");
                string inputValue = Console.ReadLine();
                try
                {
                    signal = (byte)Int32.Parse(inputValue);
                }
                catch (Exception)
                {
                    signal = 53;
                }
                byte[] data = new byte[33];
                for (int i = 0; i < 32; i++)
                {
                    data[i] = 0;
                }
                data[32] = signal;
                Console.WriteLine($"Отправка стоп сигнала {signal}");
                udpClient.Send(data, data.Length);
            }
        }
    }
}
