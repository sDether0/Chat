using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonClasses;

namespace Server
{
    class Command : Attribute
    {
        public SecondType Trigger { get; private set; }

        public Command(SecondType type)
        {
            Trigger = type;
        }

    }
}
