using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using JsonClasses;
namespace Server
{
    public interface IReceiver
    {
        ISendible ReceiveMessage(Socket sock);

    }
}
