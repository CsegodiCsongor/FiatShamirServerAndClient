﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiatShamirServer
{
    class Envelope
    {
        public string type;
        public object obj;

        public Envelope(string type, object obj)
        {
            this.type = type;
            this.obj = obj;
        }
    }
}
