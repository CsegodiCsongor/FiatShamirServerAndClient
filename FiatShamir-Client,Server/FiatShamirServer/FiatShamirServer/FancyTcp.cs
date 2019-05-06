using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace FiatShamirServer
{
    class FancyTcp
    {
        public TcpClient client;
        public string name;
        public BigInteger V;
    }
}
