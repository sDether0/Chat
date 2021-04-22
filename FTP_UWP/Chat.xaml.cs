using ConnectorUWP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using JsonClassesUWP;
using Windows.UI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FTP_UWP
{
    static class AddToList
    {
        public static void AddExc<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }
        public static void AddExc(this List<User> list, User item)
        {
            if (!list.Any(x => x.name == item.name))
            {
                list.Add(item);
            }
        }
    }
    class User
    {
        public string name;
        public bool haveNewMes;
        public User(string name, bool mes = false)
        {
            this.name = name;
            haveNewMes = mes;
        }
    }

    class TreeViewItemB : TreeViewItem
    {
        public bool file;
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Chat : Page
    {
        private Connector cn;
        private string savepath;
        List<User> users = new List<User>();
        bool restart = false;
        string myname;
        string pass;
        string curnick;
        Dictionary<string, string> chats = new Dictionary<string, string>();

        public Chat()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(610, 580);
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(610,580));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            myname = ((string)e.Parameter).Split("&")[0];
            pass = ((string)e.Parameter).Split("&")[1];
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            cn = Connector.GetConnector();
            EventBind();
        }
        private void EventBind()
        {
            cn.received += Cn_received;
            cn.usreceived += Cn_usreceived;
            cn.rtreceived += Cn_rtreceived;
            cn.flreceived += Cn_flreceived;
            cn.msreceived += Cn_msreceived;
            cn.conLost += Cn_conLost;
            cn.hsreceived += Cn_hsreceived;
        }

        private void Cn_hsreceived(Connector sender, JObject inp)
        {
            var hs = inp.ToObject<History>();
            hs.users.ForEach(x =>
            {
                if (chats.ContainsKey(x.nick))
                {
                    var todecr = x.text.Replace("\r", "").Split("\n");
                    var key = myname + File.ReadAllText("key") + x.nick;
                    var rkey = x.nick + File.ReadAllText("key") + myname;
                    var res = String.Join(Environment.NewLine, todecr.ToList().Select(z =>
                    {
                        if (!string.IsNullOrEmpty(z))
                        {
                            if (Crypto.Decrypt(z, key) != null)
                            {
                                return Crypto.Decrypt(z, key);
                            }
                            else
                            {
                                return Crypto.Decrypt(z, rkey);
                            }
                        }
                        return "";
                    }));

                    chats[x.nick] = res;
                }
                else
                {
                    var todecr = x.text.Replace("\r", "").Split("\n");
                    var key = myname + File.ReadAllText("key") + x.nick;
                    var rkey = x.nick + File.ReadAllText("key") + myname;
                    var res = String.Join(Environment.NewLine, todecr.ToList().Select(z =>
                    {
                        if (!string.IsNullOrEmpty(z))
                        {
                            if (Crypto.Decrypt(z, key) != null)
                            {
                                return Crypto.Decrypt(z, key);
                            }
                            else
                            {
                                return Crypto.Decrypt(z, rkey);
                            }
                        }
                        return "";
                    }));
                    chats.Add(x.nick, res);
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => ListUsers.Items.Add(x));
                }
            });
        }

        private void Cn_conLost(Connector sender, Exception e)
        {
            new ToastContentBuilder().AddHeader("error", "Con lost", "").AddText(e.Message).Show(toast => toast.ExpirationTime = DateTime.Now.AddMinutes(2));
            new Thread(() =>
            {
                while (!Connector.Connected())
                {
                    Task.Delay(145).Wait();
                }
                var inp = new Input() { type = "!Login", login = myname, pass = pass };
                var smess = inp.ObJsStr();
                cn.SendMessage(smess);
            }).Start();

        }

        private void Cn_msreceived(Connector sender, JObject inp)
        {
            var ms = inp.ToObject<Message>();
            var key = myname + File.ReadAllText("key") + ms.nick;
            var mes = Crypto.Decrypt(ms.text, key);
            new ToastContentBuilder().AddHeader("mes", ms.nick, "").AddText(mes).Show(toast => toast.ExpirationTime = DateTime.Now.AddMinutes(2));
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                if (chats.ContainsKey(ms.nick))
                {

                    chats[ms.nick] += Environment.NewLine + mes;
                    if (ListUsers.Items.Any(x => ((x as ListBoxItem).Content as TextBlock).Text == ms.nick))
                    {
                        if (curnick == ms.nick)
                        {
                            ChatBox.Text += Environment.NewLine + mes;
                        }
                        else
                        {
                            ((TextBlock)(ListUsers.Items.First(x => ((x as ListBoxItem).Content as TextBlock).Text == ms.nick) as ListBoxItem).Content as TextBlock).Foreground = new SolidColorBrush(Color.FromArgb(255, 40, 150, 30));
                        }
                    }
                    else
                    {
                        ListUsers.Items.Add(new ListBoxItem() { Content = new TextBlock() { Text = ms.nick } });
                        ((TextBlock)(ListUsers.Items.First(x => ((x as ListBoxItem).Content as TextBlock).Text == ms.nick) as ListBoxItem).Content as TextBlock).Foreground = new SolidColorBrush(Color.FromArgb(255, 40, 150, 30));
                    }
                }
                else
                {
                    chats.Add(ms.nick, mes);
                    ListUsers.Items.Add(new ListBoxItem() { Content = new TextBlock() { Text = ms.nick } });
                    ((TextBlock)(ListUsers.Items.First(x => ((x as ListBoxItem).Content as TextBlock).Text == ms.nick) as ListBoxItem).Content as TextBlock).Foreground = new SolidColorBrush(Color.FromArgb(255, 40, 150, 30));
                }
            });
        }

        private void Cn_flreceived(Connector sender)
        {

        }

        private void Cn_rtreceived(Connector sender, JObject inp)
        {
            /*
            var root = inp.ToObject<Root>();
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                root.paths.ForEach(x =>
                {
                    var t = x.path.Split("\\");
                    FileTree.RootNodes.Add(new TreeViewNode() { Content = "root" });
                    var node = FileTree.RootNodes.First();
                    for (int i = 0; i < t.Length; i++)
                    {
                        if (node.Children.ToList().Any(l => l.Content.ToString() == t[i]))
                        {
                            node = node.Children.ToList().Find(l => l.Content.ToString() == t[i]);

                        }
                        else
                        {
                            if (i == t.Length - 1 && x.file)
                            {
                                node.Content = new TreeViewItemB() { Content = t[i], file = true };
                            }
                            else
                            {
                                node.Children.Add(new TreeViewNode() { Content = t[i] });
                                node = node.Children.ToList().Find(l => l.Content.ToString() == t[i]);
                            }
                        }
                    }
                });
            });*/
        }

        private void Cn_usreceived(Connector sender, JObject inp)
        {
            var inpt = inp.ToObject<Refresh>();
            inpt.users.ForEach(x =>
            {
                if (!chats.ContainsKey(x.nick))
                {
                    chats.Add(x.nick, "");
                }
                users.AddExc(new User(x.nick));
            });
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => users.ForEach(x => ListUsers.Items.Add(new ListBoxItem() { Content = new TextBlock() { Text = x.name } })));

        }

        private void Cn_received(Connector sender, JObject inp)
        {

        }

        private void ListUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curnick = ((ListUsers.SelectedItem as ListBoxItem).Content as TextBlock).Text;
            if (chats.ContainsKey(curnick))
            {
                ChatBox.Text = chats[curnick];
                ((ListUsers.SelectedItem as ListBoxItem).Content as TextBlock).Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        public void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(message.Text))
            {
                var key = curnick + File.ReadAllText("key") + myname;
                string mssg = Crypto.Encrypt(message.Text, key);
                cn.SendTo(mssg, curnick);
                chats[curnick] += Environment.NewLine + message.Text;
                ChatBox.Text += Environment.NewLine + message.Text;
                message.Text = "";
            }
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SendMessage();
            }
        }
    }
}
