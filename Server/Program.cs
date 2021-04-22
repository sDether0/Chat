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
using static Server.Program;
using System.Data;
using JsonClasses;

namespace Server
{
    
    class UsSocket
    {
        public string Name;
        public List<Socket> sock = new List<Socket>();

        public UsSocket(string name, Socket sck = null)
        {
            Name = name;
            if (sck != null)
            {
                sock.Add(sck);
            }
        }
    }
    
    
    class Commands
    {
        [Command("!Message")]
        public static void Message(Socket sock, Input inp)
        {
            if (connectedSockets.Any(x => x.sock.Contains(sock)))
            {
                DebugLog.WriteLine(inp.type + " " + connectedSockets.Find(x => x.sock.Contains(sock)).Name);
                connectedSockets.Find(x => x.Name == inp.nick).sock.ForEach(r =>
                {
                    var mess = new Message(inp.text,connectedSockets.Find(f=>f.sock.Contains(sock)).Name);
                    var smess = mess.ObJsStr();
                    //var mess = "{type: '!Message', text: '" + text + "', nick: '" + connectedSockets.Find(t => t.sock.Contains(sock)).Name + "'}";
                    cl.SendMessage(r, smess);
                });
                DebugLog.WriteLine("Start saving");
                MessageHistory.SaveMessage(connectedSockets.Find(x => x.sock.Contains(sock)).Name, inp.nick, inp.text);
                DebugLog.WriteLine("Saving success");
            }
            else
            {
                cl.SendMessage(sock, "{ \"type\":\"!NeedLogin\"}");
            }
        }

        [Command("!GetHistory")]
        public static void GetHistory(Socket sock, Input inp)
        {
            if(connectedSockets.Any(x => x.sock.Contains(sock)))
            {
                DebugLog.WriteLine(inp.type + " " + connectedSockets.Find(x => x.sock.Contains(sock)).Name);
                var list = MessageHistory.GetMessageHistory(connectedSockets.Find(x => x.sock.Contains(sock)).Name);
                var mess = new History();
                for(int i = 0; i < list.GetLength(0); i++)
                {
                    if (connectedSockets.Any(x => x.Name == list[i, 1]))
                    {
                        var t = string.Join('\n',list[i, 0].Split("\n").TakeLast(15));
                        mess.Add(list[i,1],t);
                    }
                }
                var smess = mess.ObJsStr();
                cl.SendMessage(sock, smess);
            }
            else
            {
                cl.SendMessage(sock, "{ \"type\":\"!NeedLogin\"}");
            }
        }

        [Command("!Refresh")]
        public static void Refresh(Socket sock, Input inp)
        {
            if (connectedSockets.Any(x => x.sock.Contains(sock)))
            {
                DebugLog.WriteLine(inp.type + " " + connectedSockets.Find(x => x.sock.Contains(sock)).Name);
                var sender = connectedSockets.Find(x => x.sock.Contains(sock)).Name;
                var mess = new Refresh();
                connectedSockets.ForEach(x =>
                {
                    if (x.Name != sender)
                    {
                        mess.Add(x.Name);
                    }
                });
                var smess = mess.ObJsStr();
                cl.SendMessage(sock, smess);
            }
            else
            {
                cl.SendMessage(sock, "{ \"type\":\"!NeedLogin\"}");
            }
        }

        [Command("!GetRoot")]
        public static void GetRoot(Socket sock, Input inp)
        {
            if (connectedSockets.Any(x => x.sock.Contains(sock)))
            {
                DebugLog.WriteLine(inp.type + " " + connectedSockets.Find(x => x.sock.Contains(sock)).Name);
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
                var smess = mess.ObJsStr();
                cl.SendMessage(sock, smess);
            }
            else
            {
                cl.SendMessage(sock, "{ \"type\":\"!NeedLogin\"}");
            }
        }

        [Command("!GetFile")]
        public static void GetFile(Socket sock, Input inp)
        {
            if (connectedSockets.Any(x => x.sock.Contains(sock)))
            {
                DebugLog.WriteLine(inp.type + " " + connectedSockets.Find(x => x.sock.Contains(sock)).Name);
                cl.SendMessage(sock, "{\"type\":\"!File\"");
                DebugLog.WriteLine(inp.file);
                long s = File.OpenRead(inp.file).Length;
                var file = File.ReadAllBytes(inp.file);
                cl.SendFile(sock, file);
            }
            else
            {
                cl.SendMessage(sock, "{ \"type\":\"!NeedLogin\"}");
            }
        }

        [Command("!Login")]
        public static void Login(Socket sock, Input inp)
        {
            DebugLog.WriteLine(inp.type);
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
                    if (!connectedSockets.Any(x => x.sock.Contains(sock)) && connectedSockets.Any(x=>x.Name==name))
                    {
                        connectedSockets.Find(x => x.Name == name).sock.Add(sock);
                    }

                    cl.SendMessage(sock, "{\"type\":\"!Successlog\"}");
                    DebugLog.WriteLine("Sended");
                    return;
                }
            }
            cl.SendMessage(sock, "{\"type\":\"!Deniedlog\"}");

        }

        [Command("!Signin")]
        public static void Signin(Socket sock, Input inp)
        {
            DebugLog.WriteLine(inp.type);
            var name = inp.login;
            var pass = inp.pass;
            DebugLog.WriteLine("Try db");
            var dt = Mysql.fill("Select * from users where Username = @p1", new[] { name });
            DebugLog.WriteLine("Db success");
            if (dt.Rows.Count > 0)
            {
                Console.WriteLine("Denied");
                cl.SendMessage(sock, "{\"type\":\"!Deniedsign\"}");
            }
            else
            {
                DebugLog.WriteLine("Inserting");
                Mysql.com("Insert into users (Username, Password) values (@p1, @p2)", new[] { name, pass });
                Console.WriteLine("Success");
                connectedSockets.Add(new UsSocket(name, sock));
                cl.SendMessage(sock, "{\"type\":\"!Successsign\"}");
            }
        }
    }
    class Program
    {
        
        public static List<UsSocket> connectedSockets = new List<UsSocket>();
        public static ConListener cl = new ConListener();

        static void Main(string[] args)
        {
            string command = "select Username from users";
            var dt = Mysql.fill(command);
            foreach (DataRow dr in dt.Rows)
            {
                connectedSockets.Add(new UsSocket(dr[0].ToString()));
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

        public static MethodInfo GetCommand(string str)
        {
            return typeof(Commands).GetMethods().Where(x => x.GetCustomAttributes(false).OfType<Command>().Count() > 0)
                .Where(y => y.GetCustomAttributes(false).OfType<Command>().First().Trigger == str).First();
        }

        private static void Cl_listenSucces(object sender, EventArgs e, Socket sock)
        {
            Console.WriteLine("Connected ");

            new Thread(() =>
            {
                try
                {

                    while (sock.Connected)
                    {
                        var str = cl.ReceiveMessage(sock);
                        //Console.WriteLine(str);
                        var job = JObject.Parse(str);
                        Input inp = job.ToObject<Input>();

                        GetCommand(inp.type).Invoke(null, new object[] { sock, inp });
                    }
                }
                catch
                {
                    if (!sock.Connected)
                    {
                        Console.WriteLine("Disconnected" + sock.RemoteEndPoint.Serialize().ToString());
                        connectedSockets.Find(x => x.sock.Contains(sock)).sock.Remove(sock); 
                        
                    }
                }
            }).Start();
        }
    }
}
