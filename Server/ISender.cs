using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using JsonClasses;

namespace Server
{
    interface ISender
    {
        void SendMessage(Socket sock, ISendible mess);
        void SendFile(Socket sock, byte[] file);
    }
}
