using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Logger.Writer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using JsonClasses;

namespace Server
{
    class UsSocket
    {
        public string Name;
        public string Nick;
        public List<Socket> sock = new List<Socket>();

        public UsSocket(string name, string nick, Socket sck = null)
        {
            Name = name;
            Nick = nick;
            if (sck != null)
            {
                sock.Add(sck);
            }
        }
    }
    
    
    class Commands
    {
        [Command(SecondType.Message)]
        public static void Message(Socket sock, Input inp)
        {
            if (Program.connectedSockets.Any(x => x.sock.Contains(sock)))
            {
                DebugLog.WriteLine(inp.sInputType.ToString() + " " + Program.connectedSockets.Find(x => x.sock.Contains(sock))?.Name);
                Program.connectedSockets.Find(x => x.Name == inp.nick)?.sock.ForEach(r =>
                {
                    var mess = new Message(inp.text, Program.connectedSockets.Find(f=>f.sock.Contains(sock))?.Name);
                    Program.cl.SendMessage(r, mess);
                });
                DebugLog.WriteLine("Start saving");
                MessageHistory.SaveMessage(Program.connectedSockets.Find(x => x.sock.Contains(sock))?.Name, inp.nick, inp.text);
                DebugLog.WriteLine("Saving success");
            }
            else
            {
                Program.cl.SendMessage(sock, Program.provider.NeedLogin);
            }
        }

        [Command(SecondType.GetHistory)]
        public static void GetHistory(Socket sock, Input inp)
        {
            if (Program.connectedSockets.Any(x => x.sock.Contains(sock)))
            {
                DebugLog.WriteLine(inp.sInputType.ToString() + " " + Program.connectedSockets.Find(x => x.sock.Contains(sock)).Name);
                var list = MessageHistory.GetMessageHistory(Program.connectedSockets.Find(x => x.sock.Contains(sock)).Name);
                var mess = new History();
                mess.messages = list;
                Program.cl.SendMessage(sock, mess);
            }
            else
            {
                Program.cl.SendMessage(sock, Program.provider.NeedLogin);
            }
        }

        [Command(SecondType.Refresh)]
        public static void Refresh(Socket sock, Input inp)
        {
            if (Program.connectedSockets.Any(x => x.sock.Contains(sock)))
            {
                DebugLog.WriteLine(inp.sInputType.ToString() + " " + Program.connectedSockets.Find(x => x.sock.Contains(sock)).Name);
                var sender = Program.connectedSockets.Find(x => x.sock.Contains(sock)).Name;
                var mess = new Refresh();
                Program.connectedSockets.ForEach(x =>
                {
                    if (x.Name != sender)
                    {
                        mess.Add(x.Nick, x.Name);
                    }
                });
                Program.cl.SendMessage(sock, mess);
            }
            else
            {
                Program.cl.SendMessage(sock, Program.provider.NeedLogin);
            }
        }

        [Command(SecondType.GetRoot)]
        public static void GetRoot(Socket sock, Input inp)
        {
            if (Program.connectedSockets.Any(x => x.sock.Contains(sock)))
            {
                DebugLog.WriteLine(inp.sInputType.ToString() + " " + Program.connectedSockets.Find(x => x.sock.Contains(sock)).Name);
                var mess = new Root();
                var files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "*",
                    SearchOption.AllDirectories);
                files.ToList().ForEach(x =>
                {
                    if (File.Exists(x.ToString()))
                    {
                        mess.paths.Add(new SPath(x,true)); 
                    }
                    if (Directory.Exists(x.ToString()))
                    {
                        mess.paths.Add(new SPath(x, false));
                    }
                });
                Program.cl.SendMessage(sock, mess);
            }
            else
            {
                Program.cl.SendMessage(sock, Program.provider.NeedLogin);
            }
        }

        [Command(SecondType.GetFile)]
        public static void GetFile(Socket sock, Input inp)
        {
            if (Program.connectedSockets.Any(x => x.sock.Contains(sock)))
            {
                DebugLog.WriteLine(inp.sInputType.ToString() + " " + Program.connectedSockets.Find(x => x.sock.Contains(sock)).Name);
                Program.cl.SendMessage(sock, Program.provider.File);
                DebugLog.WriteLine(inp.file);
                long s = File.OpenRead(inp.file).Length;
                var file = File.ReadAllBytes(inp.file);
                Program.cl.SendFile(sock, file);
            }
            else
            {
                Program.cl.SendMessage(sock, Program.provider.NeedLogin);
            }
        }

        [Command(SecondType.Login)]
        public static void Login(Socket sock, Input inp)
        {
            DebugLog.WriteLine(inp.sInputType.ToString());
            var name = inp.login;
            var pass = inp.pass;
            var dt = Mysql.fill("Select * from ftp_chat.users where Username = @p1 and Password = @p2", new[] { name, pass });
            DebugLog.WriteLine("Success DB");
            if (dt.Rows.Count > 0)
            {
                DebugLog.WriteLine(">=1 row");
                DebugLog.WriteLine(dt.Rows[0][0].ToString() + dt.Rows[0][1].ToString() + dt.Rows[0][2].ToString());
                if (dt.Rows?[0][1].ToString() == name && dt.Rows?[0][2].ToString() == pass)
                {
                    Console.WriteLine($"{name} success login!");
                    if (!Program.connectedSockets.Any(x => x.sock.Contains(sock)) && Program.connectedSockets.Any(x=>x.Name==name))
                    {
                        Program.connectedSockets.Find(x => x.Name == name).sock.Add(sock);
                    }

                    Program.cl.SendMessage(sock, Program.provider.SuccessLogin);
                    DebugLog.WriteLine("Sended");
                    return;
                }
            }
            Program.cl.SendMessage(sock, Program.provider.DeniedLogin);

        }

        [Command(SecondType.SignIn)]
        public static void Signin(Socket sock, Input inp)
        {
            DebugLog.WriteLine(inp.sInputType.ToString());
            Console.WriteLine(inp.sInputType.ToString());
            var nick = inp.nick;
            Console.WriteLine(nick);
            var name = inp.login;
            var pass = inp.pass;
            DebugLog.WriteLine("Try db");
            var dt = Mysql.fill("Select * from users where Username = @p1", new[] { name });
            DebugLog.WriteLine("Db success");
            Console.WriteLine("Db success");
            if (dt.Rows.Count > 0)
            {
                Console.WriteLine("Denied");
                Program.cl.SendMessage(sock, Program.provider.DeniedSignin);
            }
            else
            {
                DebugLog.WriteLine("Inserting");
                Mysql.com("Insert into users (Username, Password) values (@p1, @p2)", new[] { name, pass });
                Console.WriteLine("Success");
                Program.connectedSockets.Add(new UsSocket(name,nick, sock));
                Program.cl.SendMessage(sock, Program.provider.SuccessSignin);
            }
        }
    }
    class Program
    {
        public static IInfoProvider provider = new ServerInputProvider();
        public static List<UsSocket> connectedSockets = new List<UsSocket>();
        public static ConListener cl = new ConListener();

        static void Main(string[] args)
        {
            string command = "select Username, Nickname from users";
            var dt = Mysql.fill(command);
            foreach (DataRow dr in dt.Rows)
            {
                connectedSockets.Add(new UsSocket(dr[0].ToString(),  dr[1].ToString()));
            }
            IPAddress ip = IPAddress.Parse("0.0.0.0");
            Socket sock = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket sock2 = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint EP = new IPEndPoint(ip, 25565);
            IPEndPoint EP2 = new IPEndPoint(ip, 25566);
            sock.Bind(EP);
            sock2.Bind(EP2);
            cl.listenSucces += Cl_listenSucces;
            cl.Listen(sock);
            cl.Listen(sock2);
            new Thread(() => {
                while (true)
                {
                    Task.Delay(25000).Wait();
                    connectedSockets.ForEach(x =>
                    {
                        x.sock.ForEach(y =>
                        {
                            if (!y.Connected)
                            {
                                Console.WriteLine("Disconnected" + y.RemoteEndPoint.Serialize().ToString());
                                x.sock.Remove(y);
                            }
                        });
                    });
                }
            }).Start();
            while (true)
            {
                Task.Delay(5000).Wait();
            }
        }

        public static MethodInfo GetCommand(SecondType type)
        {
            return typeof(Commands).GetMethods().Where(x => x.GetCustomAttributes(false).OfType<Command>().Count() > 0)
                .Where(y => y.GetCustomAttributes(false).OfType<Command>().First().Trigger == type).First();
        }

        private static void Cl_listenSucces(object sender, EventArgs e, Socket sock)
        {
            Console.WriteLine("Connected "+sock.RemoteEndPoint.Serialize());

            new Task(() =>
            {
                try
                {
                    while (sock.Connected)
                    {
                        var res = cl.ReceiveMessage(sock);
                        Input inp = (Input)res;
                        Console.WriteLine(inp.fInputType);
                        GetCommand(inp.sInputType).Invoke(null, new object[] { sock, inp });
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (!sock.Connected)
                    {
                        Console.WriteLine("Disconnected" + sock.RemoteEndPoint.Serialize().ToString());
                        connectedSockets.Find(x => x.sock.Contains(sock)).sock.Remove(sock); 
                        
                    }

                    throw;
                }
            }).Start();
        }
    }
}
