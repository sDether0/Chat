﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Toasties;
using JsonClasses;
using Message = JsonClasses.Message;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FTP_Personal;
using Microsoft.Toolkit.Uwp.Notifications;


namespace Interface
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
    public partial class Form1 : Form
    {
        private Connector cn;
        private string savepath;
        List<User> users = new List<User>();
        bool restart = false;
        bool cmbx = true;
        Dictionary<string, string> chats = new Dictionary<string, string>();
        System.Media.SoundPlayer sp;
        string myname = "";



        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var prcs = Process.GetProcessesByName("Interface");
            foreach (Process prc in prcs)
            {
                var temp = Process.GetCurrentProcess();
                var b = prc.Id != temp.Id;
                if (b)
                {
                    prc.Kill();
                }
            }
            sp = new System.Media.SoundPlayer();
            //FileStream fs = new FileStream("message.wav", FileMode.Open);
            //sp.Stream = fs;
            restart = false;
            Size = new Size(0, 0);
            MinimumSize = new Size(280, 280);
            //IPAddress ip = new IPAddress(IPAddress.Parse("127.0.0.1").GetAddressBytes());
            textBox2.ReadOnly = true;
            textBox2.ScrollBars = ScrollBars.Vertical;
            cn = Connector.GetConnector();
            EventBind();
            cn.Connect();
            Icon ic = new Icon("icon.ico");
            treeView1.ImageList = new ImageList();
            treeView1.ImageList.Images.Add(ic);
            //IPAddress ip = new IPAddress(IPAddress.Parse("127.0.0.1").GetAddressBytes());
            //tabControl1.SelectedTab = tabPage2;

        }

        private void EventBind()
        {
            textBox3.KeyDown += TextBox3_KeyDown;
            cn.received += Cn_received;
            cn.usreceived += Cn_usreceived;
            cn.rtreceived += Cn_rtreceived;
            cn.flreceived += Cn_flreceived;
            cn.msreceived += Cn_msreceived;
            cn.conLost += Cn_conLost;
            cn.hsreceived += Cn_hsreceived;
        }

        private void Cn_msreceived(Connector sender, JObject inp)
        {
            var inpt = inp.ToObject<Message>();
            Invoke(new Action(() =>
            {
                //sp.Play();
                if (chats.ContainsKey(inpt.nick))
                {
                    var key = myname + File.ReadAllText("key") + inpt.nick;
                    string mess = Crypto.Decrypt(inpt.text, key);
                    var tst = new ToastContentBuilder().AddHeader("Mess", inpt.nick, "").AddText(mess);
                    
                    if (mess != null)
                    {
                        chats[inpt.nick] += Environment.NewLine + mess;
                        if (comboBox1.Text == inpt.nick)
                        {
                            textBox2.Text = chats[inpt.nick];
                        }
                        else
                        {
                            users.Find(x => x.name == inpt.nick).haveNewMes = true;
                            ComboBoxUpdate();
                        }
                    }
                }
                else
                {
                    chats.Add(inpt.nick, "");
                    users.AddExc(new User(inpt.nick, true));
                    ComboBoxUpdate();
                    var key = myname + File.ReadAllText("key") + inpt.nick;
                    string mess = Crypto.Decrypt(inpt.text, key);
                    chats[inpt.nick] += Environment.NewLine + mess;
                    if (comboBox1.Text == inpt.nick)
                    {
                        textBox2.Text = chats[inpt.nick];
                    }
                }

            }));
        }

        private void TextBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                MessageSend();
            }
        }

        private void Cn_hsreceived(Connector sender, JObject inp)
        {
            var inpt = inp.ToObject<History>();
            inpt.users.ForEach(x =>
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
                    Invoke(new Action(() =>
                    {
                        comboBox1.Items.Add(x.nick);
                    }));
                }
            });

        }

        private void Cn_conLost(Connector sender, Exception e)
        {
            MessageBox.Show(e.Message);
            if (e.Message != "ConnectionLostRestart")
            {
                var t = new Toastie(this, ToastieWrapper.Placement.BottomRight, Toastie.ToastieIcon.Error, "Error", "Connection Lost", true, 1000, 120, 250);
                t.Show();
            }
            else
            {
                var t = new Toastie(this, ToastieWrapper.Placement.BottomRight, Toastie.ToastieIcon.Error, "Error", "Application Restarting", true, 1000, 120, 250);
                t.Show();
                restart = true;
                Task.Delay(2000).ContinueWith(delegate { Application.Restart(); });
            }
        }


        private void Cn_flreceived(Connector sender)
        {

            var file = cn.ReceiveFile();
            File.WriteAllBytes(savepath, file);

        }

        private void Cn_rtreceived(Connector sender, JObject inp)
        {
            var inpt = inp.ToObject<Root>();
            Invoke(new Action(() =>
            {
                treeView1.Nodes.Clear();
            }));

            inpt.paths.ForEach(x =>
            {
                var all = x.path.Split("\\");
                var ft = treeView1.Nodes;
                for (int i = 0; i < all.Length; i++)
                {
                    if (ft.ContainsKey(all[i]))
                    {
                        ft = ft.Find(all[i], false).First().Nodes;
                    }
                    else
                    {
                        Invoke(new Action(() =>
                        {
                            ft.Add(all[i], all[i], 0);
                        }));
                        ft = ft.Find(all[i], false).First().Nodes;
                    }
                }
            });
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


            ComboBoxUpdate();
            if (comboBox1.Items.Count > 0)
            {
                Invoke(new Action(() => { comboBox1.SelectedIndex = 0; }));
            }
            var com = new Input() { type = "!GetHistory" };
            var smess = com.ObJsStr();
            cn.SendMessage(smess);
        }
        public void ComboBoxUpdate()
        {
            string cur="";
            Invoke(new Action(() =>
            {
                cur = comboBox1.Text;
                comboBox1.Items.Clear();
            }));
            users.ForEach(x =>
            {
                Invoke(new Action(() =>
                {
                    if (!x.haveNewMes)
                    {
                        var cbi = new ComboBoxItem(x.name, x.name, Color.Black);
                        comboBox1.Items.Add(cbi);
                    }
                    else
                    {
                        var cbi = new ComboBoxItem(x.name, x.name, Color.DarkGreen);
                        comboBox1.Items.Add(cbi);
                    }
                }));
            });
            Invoke(new Action(() =>
            {
                comboBox1.SelectedIndex = comboBox1.FindString(cur);
            }));
        }

        private void Cn_received(Connector sender, JObject inp)
        {
            var str = inp["type"].ToString();
            Invoke(new Action(() =>
            {
                if (str == "!Successlog")
                {
                    tabControl1.SelectedIndex = 1;
                    MinimumSize = new Size(580, 380);
                    //Size = new Size(540, 380);
                }
                if (str == "!Deniedlog")
                {
                    label3.Text = "Wrong credential";
                }
                if (str == "!Successsign")
                {
                    MessageBox.Show("Succesfully registered", "Success");
                }
                if (str == "!Deniedsign")
                {
                    MessageBox.Show("Username already used");
                }
            }));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageSend();
        }

        private void MessageSend()
        {
            if (!string.IsNullOrWhiteSpace(comboBox1.Text) && !string.IsNullOrWhiteSpace(textBox3.Text))
            {
                var key = comboBox1.Text + File.ReadAllText("key") + myname;
                string message = Crypto.Encrypt(textBox3.Text, key);
                cn.SendTo(message, comboBox1.Text);
                chats[comboBox1.Text] += Environment.NewLine + textBox3.Text;
                textBox3.Text = "";
                textBox2.Text = chats[comboBox1.Text];
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Debug.WriteLine("Mudila "+restart);

            if (!restart)
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbx)
            {
                cmbx = false;
                var name = comboBox1.SelectedItem.ToString();
                users.Find(x => x.name == name).haveNewMes = false;
                ComboBoxUpdate();
                comboBox1.SelectedIndex = comboBox1.FindString(name);
                textBox2.Text = chats[comboBox1.Text];
                cmbx = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
                var com = new Input() { type = "!Refresh" };
                var smess = com.ObJsStr();
                cn.SendMessage(smess);
                button3.Enabled = false;
                Task.Delay(5000).ContinueWith(delegate
                {
                    Invoke(new Action(() => { button3.Enabled = true; }));
                });
                
            
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            
                var com = new Input() { type = "!GetRoot" };
                var smess = com.ObJsStr();
                cn.SendMessage(smess);
                button4.Enabled = false;
                Task.Delay(5000).ContinueWith(delegate
                {
                    Invoke(new Action(() => { button4.Enabled = true; }));
                });
            
        }

        private void button5_Click(object sender, EventArgs e)
        {

            Regex rgx = new Regex(@"^([a-zA-Z0-1а-яА-Я]+\.[a-zA-Z0-1а-яА-Я]+)$");
            if (rgx.IsMatch(treeView1.SelectedNode.Text))
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.RootFolder = Environment.SpecialFolder.Desktop;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    var com = new Input() { type = "!GetFile",file = treeView1.SelectedNode.FullPath };
                    var smess = com.ObJsStr();
                    cn.SendMessage(smess);
                    Task.Delay(1200);
                    var name = treeView1.SelectedNode.Text;
                    savepath = fbd.SelectedPath + "\\" + name;
                }
            }
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            var com = new Input() { type = "!Login",login=textBox4.Text,pass=textBox5.Text };
            var smess = com.ObJsStr();
            cn.SendMessage(smess);
            myname = textBox4.Text;
        }


        private void button7_Click(object sender, EventArgs e)
        {
            var com = new Input() { type = "!Signin", login = textBox4.Text, pass = textBox5.Text };
            var smess = com.ObJsStr();
            cn.SendMessage(smess);

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
