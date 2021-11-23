using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LizardTCP
{
    internal class Proxy
    {
        public class TcpForwarderSlim
        {
            private readonly Socket _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            public async Task Start(IPEndPoint local, IPEndPoint remote)
            {
                _mainSocket.Bind(local);
                _mainSocket.Listen(10);

                while (true)
                {
                    var source = _mainSocket.Accept();
                    var destination = new TcpForwarderSlim();
                    var state = new State(source, destination._mainSocket);
                    destination.Connect(remote, source);
                    if (source.Connected == false)
                    {
                        Console.WriteLine("source.Connected was false!!!");
                    }
                    source.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                }
            }

            private void Connect(EndPoint remoteEndpoint, Socket destination)
            {
                var state = new State(_mainSocket, destination);
                _mainSocket.Connect(remoteEndpoint);
                _mainSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnDataReceive, state);
            }

            private static void OnDataReceive(IAsyncResult result)
            {
                var state = (State)result.AsyncState;
                try
                {
                    var bytesRead = state.SourceSocket.EndReceive(result);
                    if (bytesRead > 0)
                    {
                        Console.WriteLine("SocketBalancer got data! IP: " + state.DestinationSocket.LocalEndPoint.ToString());
                        var str = Encoding.Default.GetString(state.Buffer);
                        //Get or create a new buffer for the state object.
                        /*  */
                        byte[] bytes3 = Encoding.Default.GetBytes("HTTP/1.1 200 OK");
                        byte[] bytes = Encoding.Default.GetBytes("\nx-lizardtcp: LizardTCP/Alfa");
                        byte[] bytes2 = Encoding.Default.GetBytes("\nx-lizardtcp-node: rulocal1");
                        bool isHttp = false;
                        for (int t = 0; t < 15; t++)
                        {
                            if (state.Buffer[t] == bytes3[t])
                            {
                                isHttp = true;
                            }
                            else
                                isHttp = false;
                        }

                        byte[] responseBuff = new byte[] { };
                        if (isHttp)
                        {
                            Console.WriteLine("Found HTTP Answer");
                            byte[] smallPortion = state.Buffer.Skip(15).Take((state.Buffer.Length - 15)).ToArray();
                            //responseBuff = new byte[bytes3.Length + bytes.Length + bytes2.Length + smallPortion.Length];
                            responseBuff = new byte[0];
                            Array.Reverse(bytes, 0, bytes.Length);
                            Array.Reverse(bytes2, 0, bytes2.Length);
                            Array.Reverse(bytes3, 0, bytes3.Length);
                            responseBuff = AddByteAToArray(smallPortion, bytes);
                            responseBuff = AddByteAToArray(responseBuff, bytes2);
                            responseBuff = AddByteAToArray(responseBuff, bytes3);
                            Console.WriteLine(Encoding.Default.GetString(responseBuff));
                            //responseBuff = state.Buffer;
                        }

                        //Start an asyncronous send.
                        IAsyncResult sendAr = null;
                        if (isHttp)
                        {
                            sendAr = state.DestinationSocket.BeginSend(responseBuff, 0,
                                                     bytesRead + bytes2.Length + bytes2.Length + 2, SocketFlags.None, null, null);
                        }
                        else
                        {
                            sendAr = state.DestinationSocket.BeginSend(state.Buffer, 0, bytesRead, SocketFlags.None, null, null);
                        }

                        var oldBuffer = state.ReplaceBuffer();

                        state.SourceSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);

                        //Wait for the send to finish.
                        state.DestinationSocket.EndSend(sendAr);

                        //Return byte[] to the pool.
                        state.AddBufferToPool(oldBuffer);
                    }
                }
                catch
                {
                    state.DestinationSocket.Close();
                    state.SourceSocket.Close();
                }
            }

            public static byte[] AddByteAToArray(byte[] bArray, byte[] newByte)
            {
                byte[] ss = AddByteToArray(bArray, newByte[0]);
                for (int i = 1; i < newByte.Length; ++i)
                {
                    ss = AddByteToArray(ss, newByte[i]);
                }

                return ss;
            }

            public static byte[] AddByteToArray(byte[] bArray, byte newByte)
            {
                byte[] newArray = new byte[bArray.Length + 1];
                bArray.CopyTo(newArray, 1);
                newArray[0] = newByte;
                return newArray;
            }

            public class State
            {
                private readonly ConcurrentBag<byte[]> _bufferPool = new ConcurrentBag<byte[]>();
                private readonly int _bufferSize;
                public Socket SourceSocket { get; private set; }
                public Socket DestinationSocket { get; private set; }
                public byte[] Buffer { get; private set; }

                public State(Socket source, Socket destination)
                {
                    SourceSocket = source;
                    DestinationSocket = destination;
                    _bufferSize = Math.Min(SourceSocket.ReceiveBufferSize, DestinationSocket.SendBufferSize);
                    Buffer = new byte[_bufferSize];
                }

                /// <summary>
                /// Replaces the buffer in the state object.
                /// </summary>
                /// <returns>The previous buffer.</returns>
                public byte[] ReplaceBuffer()
                {
                    byte[] newBuffer;
                    if (!_bufferPool.TryTake(out newBuffer))
                    {
                        newBuffer = new byte[_bufferSize];
                    }
                    var oldBuffer = Buffer;
                    Buffer = newBuffer;
                    return oldBuffer;
                }

                public void AddBufferToPool(byte[] buffer)
                {
                    _bufferPool.Add(buffer);
                }
            }
        }

        public class UdpForwarderSlim
        {
            public static IPEndPoint m_listenEp = null;
            public static EndPoint m_connectedClientEp = null;
            public static IPEndPoint m_sendEp = null;
            public static Socket m_UdpListenSocket = null;
            public static Socket m_UdpSendSocket = null;

            public static async Task UdpForwarder()
            {
                // Creates Listener UDP Server
                m_listenEp = new IPEndPoint(IPAddress.Any, 27014);
                m_UdpListenSocket = new Socket(m_listenEp.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                m_UdpListenSocket.Bind(m_listenEp);

                //Connect to zone IP EndPoint
                m_sendEp = new System.Net.IPEndPoint(IPAddress.Parse("94.103.84.90"), 1339);
                m_connectedClientEp = new System.Net.IPEndPoint(IPAddress.Any, 1339);

                byte[] data = new byte[1024];

                while (true)
                {
                    if (m_UdpListenSocket.Available > 0)
                    {
                        int size = m_UdpListenSocket.ReceiveFrom(data, ref m_connectedClientEp); //client to listener

                        if (m_UdpSendSocket == null)
                        {
                            // Connect to UDP Game Server.
                            m_UdpSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        }

                        m_UdpSendSocket.SendTo(data, size, SocketFlags.None, m_sendEp); //listener to server.
                    }

                    if (m_UdpSendSocket != null && m_UdpSendSocket.Available > 0)
                    {
                        int size = m_UdpSendSocket.Receive(data); //server to client.

                        m_UdpListenSocket.SendTo(data, size, SocketFlags.None, m_connectedClientEp); //listner
                    }
                }
            }
        }
    }
}