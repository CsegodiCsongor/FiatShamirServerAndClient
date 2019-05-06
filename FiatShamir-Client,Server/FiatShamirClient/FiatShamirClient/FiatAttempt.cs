using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Numerics;
using Newtonsoft.Json;

namespace FiatShamirClient
{
    public class FiatAttempt
    {
        private static Random rnd = new Random();

        public int Attempts;
        public int CurrentAttempt;
        public string DestClientName;
        public string SourceClientName;
        public NetworkStream stream;

        public BigInteger n;
        public BigInteger s;
        public BigInteger v;

        public BigInteger r;
        public BigInteger x;

        public BigInteger y;

        public byte e;

        public FiatAttempt(PrivateMessage pm, int Attempts, NetworkStream stream,BigInteger n)
        {
            this.SourceClientName = pm.dUser;
            this.DestClientName = pm.sUser;
            this.Attempts = Attempts;
            this.stream = stream;
            CurrentAttempt = 1;
            this.n = n;

            HandleWitness(pm);
        }

        public FiatAttempt(string SourceClientName, string DestClientName, NetworkStream stream, BigInteger n, BigInteger s, BigInteger v)
        {
            this.SourceClientName = SourceClientName;
            this.DestClientName = DestClientName;
            this.stream = stream;

            this.n = n;
            this.s = s;
            this.v = v;

            Commitment();
        }

        public void HandleChallenge(PrivateMessage message)
        {
            this.e = byte.Parse(message.text);
            if (e == 0)
            {
                y = r;
            }
            else
            {
                y = (r * s) % n;
            }

            PrivateMessage pm = new PrivateMessage();
            pm.dUser = DestClientName;
            pm.sUser = SourceClientName;
            pm.text = y.ToString();
            pm.mType = "Response";

            SendPM(pm);
        }

        public void HandleWitness(PrivateMessage message)
        {
            e = (byte)rnd.Next(0, 2);
            string s = ASCIIEncoding.ASCII.GetString(ASCIIEncoding.ASCII.GetBytes(message.text.Split(' ')[0]));
            this.x = BigInteger.Parse(s);
            //s = ASCIIEncoding.ASCII.GetString(ASCIIEncoding.ASCII.GetBytes(message.text.Split(' ')[1]));
            //this.v = BigInteger.Parse(s);

            PrivateMessage pm = new PrivateMessage();
            pm.dUser = DestClientName;
            pm.sUser = SourceClientName;
            pm.text = e.ToString();
            pm.mType = "Challenge";

            SendPM(pm);
        }

        public void HandleResopnse(PrivateMessage message)
        {
            string s = ASCIIEncoding.ASCII.GetString(ASCIIEncoding.ASCII.GetBytes(message.text));
            y = BigInteger.Parse(s);

            PrivateMessage pm = new PrivateMessage();
            pm.dUser = message.sUser;
            pm.sUser = message.dUser;

            if ((y*y)%n==(x*BigInteger.Pow(v,e))%n)
            {
                if (CurrentAttempt >= Attempts)
                {
                    pm.mType = "Finished";
                }
                else
                {
                    pm.mType = "Correct";
                    CurrentAttempt++;
                }
            }
            else
            {
                pm.mType = "Incorrect";
            }

            SendPM(pm);

        }

        public void HandleResopnse()
        {
            PrivateMessage pm = new PrivateMessage();
            pm.dUser = DestClientName;
            pm.sUser = SourceClientName;

            if ((y * y) % n == (x * BigInteger.Pow(v, e)) % n)
            {
                if (CurrentAttempt >= Attempts)
                {
                    pm.mType = "Finished";
                }
                else
                {
                    pm.mType = "Correct";
                    CurrentAttempt++;
                }
            }
            else
            {
                pm.mType = "Incorrect";
            }

            SendPM(pm);

        }

        public void Commitment()
        {
            r = BigHelper.GetRandomBigInt(1, n - 1);
            x = BigInteger.ModPow(r, 2, n);

            PrivateMessage pm = new PrivateMessage();
            pm.dUser = DestClientName;
            pm.sUser = SourceClientName;
            pm.text = x.ToString();
            pm.mType = "Witness";

            SendPM(pm);
        }

        private void SendPM(PrivateMessage message)
        {
            Envelope en = new Envelope("PM", message);
            string obj = JsonConvert.SerializeObject(en);
            byte[] messageToSend = ASCIIEncoding.ASCII.GetBytes(obj);

            stream.Write(messageToSend, 0, messageToSend.Length);
        }
    }
}
