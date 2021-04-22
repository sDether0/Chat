using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Server
{
    class ConListener
    {
        public delegate void ListenHandler(object sender, EventArgs e, Socket sock);
        public event ListenHandler listenSucces;

        public void Listen(Socket sock)
        {

            sock.Listen();
            new Thread(() =>
            {
                while (true)
                {
                    var sock2 = sock.Accept();
                    if (sock2.Connected)
                    {
                        if (listenSucces != null)
                        {
                            EventArgs e = new EventArgs();
                            listenSucces(this, e, sock2);
                        }
                    }
                }
            }).Start();

        }

        public string ReceiveMessage(Socket sock)
        {
            byte[] size = new byte[4];
            Receive(sock, size, 0, size.Length, 100000);
            int sz = BitConverter.ToInt32(size);
            byte[] buffer = new byte[sz];
            Receive(sock, buffer, 0, buffer.Length, 100000);
            var str = Encoding.UTF8.GetString(buffer);
            return str;
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
        public void SendMessage(Socket sock, string str)
        {
            var packet = Encoding.UTF8.GetBytes(str);
            int size = packet.Length;
            byte[] sz = BitConverter.GetBytes(size);
            Send(sock, sz, sz.Length, 0, 100000);
            Send(sock, packet, packet.Length, 0, 10000);
            
        }

        public void SendFile(Socket sock, byte[] file)
        {
            int size = file.Length;
            byte[] sz = BitConverter.GetBytes(size);
            var str = BitConverter.ToInt32(sz);
            Send(sock, sz, 4, 0, 100000);
            Send(sock, file, size, 0, 100000);

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

    }
}
