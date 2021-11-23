using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PacketTester
{
    internal class TcpServerClass
    {
        public class ClientObject
        {
            public TcpClient client;
            private static TcpListener listener;

            public ClientObject(TcpClient tcpClient)
            {
                client = tcpClient;
            }

            public void Process()
            {
                NetworkStream stream = null;
                try
                {
                    stream = client.GetStream();
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    while (true)
                    {
                        // получаем сообщение
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        do
                        {
                            bytes = stream.Read(data, 0, data.Length);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (stream.DataAvailable);

                        string message = builder.ToString();

                        Console.WriteLine(message);
                        // отправляем обратно сообщение в верхнем регистре
                        message = message.Substring(message.IndexOf(':') + 1).Trim().ToUpper();
                        data = Encoding.Unicode.GetBytes(message);
                        stream.Write(data, 0, data.Length);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                    if (client != null)
                        client.Close();
                }
            }

            public static void ServerInstance(string addr, int port)
            {
                try
                {
                    listener = new TcpListener(IPAddress.Parse(addr), port);
                    listener.Start();
                    Console.WriteLine("Ожидание подключений...");

                    while (true)
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        ClientObject clientObject = new ClientObject(client);

                        // создаем новый поток для обслуживания нового клиента
                        Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                        clientThread.Start();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (listener != null)
                        listener.Stop();
                }
            }
        }
    }

    internal class TcpClientClass
    {
        public static void ClientInstance(string addr, int port)
        {
            Console.Write("Введите свое имя:");
            string userName = Console.ReadLine();
            TcpClient client = null;
            int PacketCountSent = 0;
            int PacketCountRecv = 0;
            DateTime dt = DateTime.Now;
            DateTime dtc = dt + TimeSpan.FromMinutes(1);
            DateTime Previous = DateTime.Now;
            int PPacketCountSent = 0;
            int PPacketCountRecv = 0;
            try
            {
                client = new TcpClient(addr, port);

                NetworkStream stream = client.GetStream();

                while (true)
                {
                    Console.Write(userName + ": ");
                    // ввод сообщения
                    string message = Console.ReadLine();
                    message = String.Format("{0}: {1}", userName, message);
                    // преобразуем сообщение в массив байтов
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    byte[] dataRecv = new byte[64];

                    while (dt < dtc)
                    {
                        // отправка сообщения
                        stream.Write(data, 0, data.Length);
                        PacketCountSent++;
                        // получаем ответ
                        //dataRecv
                        // буфер для получаемых данных
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        do
                        {
                            bytes = stream.Read(dataRecv, 0, dataRecv.Length);
                            builder.Append(Encoding.Unicode.GetString(dataRecv, 0, bytes));
                        }
                        while (stream.DataAvailable);

                        message = builder.ToString();
                        PacketCountRecv++;
                        //Console.WriteLine("Сервер: {0}", message);
                        //Thread.Sleep(5);
                        if (PacketCountSent != 0 && PacketCountSent % 1000 == 0)
                        {
                            Console.WriteLine($"{DateTime.Now} | Speedtest stats: Sent: {PacketCountSent} Recv: {PacketCountRecv} | SpeedSent: {PacketCountSent - PPacketCountSent} / {(DateTime.Now - Previous).TotalMilliseconds }ms | {(PacketCountSent - PPacketCountSent) / (DateTime.Now - Previous).TotalMilliseconds } S/MS | SpeedRecv: {PacketCountRecv - PPacketCountRecv} / {(DateTime.Now - Previous).TotalMilliseconds}ms | {(PacketCountRecv - PPacketCountRecv) / (DateTime.Now - Previous).TotalMilliseconds } S/MS");
                        }
                    }

                    Console.WriteLine($"Speedtest end. Sent: {PacketCountSent} Recv: {PacketCountRecv}");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
        }
    }
}