using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using JsonClasses;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FTP_Personal
{
    public class Connector
    {
        public delegate void RecieveMessage(Connector sender, JObject inp);
        public event RecieveMessage msreceived;
        public delegate void RecieveUsers(Connector sender, JObject inp);
        public event RecieveUsers usreceived;
        public delegate void RecieveHistory(Connector sender, JObject inp);
        public event RecieveHistory hsreceived;
        public delegate void RecieveRoot(Connector sender, JObject inp);
        public event RecieveRoot rtreceived;
        public delegate void RecieveFile(Connector sender);
        public event RecieveFile flreceived;
        public delegate void Recieve(Connector sender, JObject inp);
        public event Recieve received;
        public delegate void ConnectionLost(Connector sender, Exception e);
        public event ConnectionLost conLost;
        int conLostCount = 0;
        static Connector con;
        private Socket sock;
        IPAddress ip;
        private Connector()
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ip = new IPAddress(IPAddress.Parse(File.ReadAllText("ip")).GetAddressBytes());
        }

        public static Connector GetConnector()
        {
            if (con == null)
            {
                con = new Connector();
            }
            return con;
        }


        public void Connect(int port = 25565)
        {
            if (!sock.Connected)
            {
                sock.Connect(ip, port);
                Thread thr = new Thread(() =>
                {
                    try
                    {
                        while (sock.Connected)
                        {
                            var str = ReceiveMessage();
                            var t = JObject.Parse(str);
                            if (t["type"].ToString() == "!Message")
                            {
                                msreceived?.Invoke(this, t);
                                continue;
                            }
                            if (t["type"].ToString() == "!Refresh")
                            {
                                usreceived?.Invoke(this, t);
                                continue;
                            }
                            if (t["type"].ToString() == "!History")
                            {
                                hsreceived?.Invoke(this, t);
                                continue;
                            }
                            if (t["type"].ToString() == "!Root")
                            {
                                rtreceived?.Invoke(this, t);
                                continue;
                            }
                            if (t["type"].ToString() == "!File")
                            {
                                flreceived?.Invoke(this);
                                continue;
                            }
                            received?.Invoke(this, t);
                        }
                    }
                    catch (Exception e)
                    {


                        conLostCount++;
                        Task.Delay(5000).Wait();
                        if (conLostCount < 4)
                        {

                            conLost?.Invoke(this, e);
                            if (port == 25565)
                            {
                                Connect(25566);
                            }
                            else
                            {
                                Connect();
                            }

                        }
                        else
                        {
                            Exception f = new Exception("ConnectionLostRestart", e);

                            conLost?.Invoke(this, f);
                        }

                    }
                });
                thr.Start();
            }
        }


        public void SendTo(string str, string nick)
        {
            var com = new Input() { type = "!Message", text = str, nick = nick };
            var smess = com.ObJsStr();
            SendMessage(smess);
        }

        public void SendMessage(string str)
        {
            var packet = Encoding.UTF8.GetBytes(str);
            int size = packet.Length;
            byte[] sz = BitConverter.GetBytes(size);
            Send(sock, sz, sz.Length, 0, 100000);
            Send(sock, packet, packet.Length, 0, 100000);
        }

        public void Send(Socket socket, byte[] buffer, int size, int offset, int timeout)
        {
            int startTickCount = Environment.TickCount;
            int sent = 0;  // how many bytes is already sent
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new Exception("Timeout.");
                try
                {
                    sent += socket.Send(buffer, offset + sent, size - sent, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably full, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw ex;  // any serious error occurr
                }
            } while (sent < size);
        }

        public void Receive(Socket socket, byte[] buffer, int offset, int size, int timeout)
        {
            int startTickCount = Environment.TickCount;
            int received = 0;  // how many bytes is already received
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new Exception("Timeout.");
                try
                {
                    received += socket.Receive(buffer, offset + received, size - received, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably empty, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw ex;  // any serious error occurr
                }
            } while (received < size);
        }
        public string ReceiveMessage()
        {
            byte[] size = new byte[4];
            Receive(sock, size, 0, size.Length, 100000);
            int sz = BitConverter.ToInt32(size, 0);
            byte[] buffer = new byte[sz];
            Receive(sock, buffer, 0, buffer.Length, 100000);
            var str = Encoding.UTF8.GetString(buffer);
            return str;
        }
        public byte[] ReceiveFile()
        {
            byte[] size = new byte[4];
            Receive(sock, size, 0, size.Length, 100000);
            int sz = BitConverter.ToInt32(size, 0);
            byte[] buffer = new byte[sz];
            Receive(sock, buffer, 0, buffer.Length, 100000);

            return buffer;
        }
    }
}
