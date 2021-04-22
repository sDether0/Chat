using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Command : System.Attribute
    {
        public string Trigger { get; private set; }

        public Command(string trigger)
        {
            Trigger = trigger;
        }

    }
}
