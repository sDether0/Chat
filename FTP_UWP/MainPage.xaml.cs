using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using ConnectorUWP;
using JsonClassesUWP;
using System.Net.Sockets;
using System.Net;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.ViewManagement;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FTP_UWP
{
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Connector cn;
        
        string myname = "";

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(610, 580);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
            //Frame.Navigate(typeof(Chat), myname);
            var log = login.Text;
            myname = log;
            var pas = pass.Password;
            var inp = new Input() { type = "!Login", login = log, pass = pas };
            var smess = inp.ObJsStr();
            cn.SendMessage(smess);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            //sp = new System.Media.SoundPlayer();
            //FileStream fs = new FileStream("message.wav", FileMode.Open);
            //sp.Stream = fs;
            
            //IPAddress ip = new IPAddress(IPAddress.Parse("127.0.0.1").GetAddressBytes());
            //Scale = new System.Numerics.Vector3(0.5f);
            cn = Connector.GetConnector();
            EventBind();
            cn.Connect(25566);
        }

        private void EventBind()
        {
            cn.received += Cn_received;
            //cn.usreceived += Cn_usreceived;
            //cn.rtreceived += Cn_rtreceived;
            //cn.flreceived += Cn_flreceived;
            //cn.msreceived += Cn_msreceived;
            cn.conLost += Cn_conLost;
            //cn.hsreceived += Cn_hsreceived;
        }

        private void Cn_conLost(Connector sender, Exception e)
        {
            Debug.WriteLine(e.Message);
        }

        private void Cn_received(Connector sender, Newtonsoft.Json.Linq.JObject inp)
        {
            var str = inp["type"].ToString();
            if (str == "!Successlog")
            {                
                var refr = new Input()
                {
                    type = "!Refresh"
                };
                var hist = new Input()
                {
                    type = "!GetHistory"
                };
                var root = new Input()
                {
                    type = "!GetRoot"
                };
                var rf = refr.ObJsStr();
                var hs = hist.ObJsStr();
                var rt = root.ObJsStr();
                cn.SendMessage(rf);
                cn.SendMessage(hs);
                cn.SendMessage(rt);
                cn.received -= Cn_received;
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => Frame.Navigate(typeof(Chat), myname+"&"+pass.Password));
                return;
            }
            if (str == "!Deniedlog")
            {
                return;
            }
            if (str == "!Successsign")
            {
                return;
            }
            if (str == "!Deniedsign")
            {
                return;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var log = login.Text;
            var pas = pass.Password;
            var inp = new Input() { type = "!Signin", login = log, pass = pas };
            var smess = inp.ObJsStr();
            cn.SendMessage(smess);
        }
    }
}
