using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace FiatShamirClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static TcpClient client;
        static NetworkStream stream;
        static BigInteger v;
        static BigInteger n;
        static BigInteger s;
        static string name;
        static List<FiatAttempt> incomingAttempts;

        static FiatAttempt outgoingAttempt;

        public MainWindow()
        {
            do
            {
                try
                {
                    InputIP getIp = new InputIP();
                    getIp.ShowDialog();
                    if (getIp.close == true)
                    {
                        System.Environment.Exit(0);
                    }
                    client = new TcpClient(getIp.ip, 5432);
                    stream = client.GetStream();
                    name = getIp.name;
                    break;
                }
                catch { }
            } while (true);

            Task.Run(() =>
            {
                Message t = new Message();
                t.user = name;
                Envelope en = new Envelope("Message", t);
                string mes = JsonConvert.SerializeObject(en);
                byte[] messageToSend = ASCIIEncoding.ASCII.GetBytes(mes);
                stream.Write(messageToSend, 0, messageToSend.Length);

                while (true)
                {
                    byte[] messageRecieve = new byte[client.ReceiveBufferSize];
                    int read = stream.Read(messageRecieve, 0, client.ReceiveBufferSize);
                    string obj = Encoding.ASCII.GetString(messageRecieve, 0, read);
                    Dispatcher.Invoke(() => recieve(obj));
                }
            });

            InitializeComponent();
            NameBox.Text = name;
            incomingAttempts = new List<FiatAttempt>();
        }

        public void recieve(string obj)
        {
            try
            {
                while (true)
                {
                    int index = obj.IndexOf("}}") + 1;
                    string objTemp = obj.Substring(0, index + 1);
                    Envelope unknown = JsonConvert.DeserializeObject<Envelope>(objTemp);

                    if (unknown.type == "Message")
                    {
                        Message m = JsonConvert.DeserializeObject<Message>(unknown.obj.ToString());
                        string s = m.user + " : " + m.text + "\n";
                        RecieveBox.Text += s;
                    }
                    else if (unknown.type == "PublicN")
                    {
                        Message m = JsonConvert.DeserializeObject<Message>(unknown.obj.ToString());
                        string sm = m.user + " sent public N : " + m.text;
                        RecieveBox.Text += sm+"\n";
                        byte[] bytes = Encoding.ASCII.GetBytes(m.text);
                        string aux = ASCIIEncoding.ASCII.GetString(bytes);
                        n = BigInteger.Parse(aux);
                        BigHelper bh = new BigHelper();
                        s = bh.GenerateRandomE(n);
                        v = BigInteger.ModPow(s, 2, n);

                        Message t = new Message();
                        t.text = v.ToString();
                        Envelope en = new Envelope("Message", t);
                        string mes = JsonConvert.SerializeObject(en);
                        byte[] messageToSend = ASCIIEncoding.ASCII.GetBytes(mes);
                        stream.Write(messageToSend, 0, messageToSend.Length);
                    }
                    else if (unknown.type == "PM")
                    {
                        PrivateMessage pm = JsonConvert.DeserializeObject<PrivateMessage>(unknown.obj.ToString());

                        if (pm.mType == "Witness" && !Contains(pm.sUser))
                        {
                            incomingAttempts.Add(new FiatAttempt(pm, 3, stream,n));
                        }
                        else
                        {
                            for (int i = 0; i < incomingAttempts.Count;i++)
                            {
                                if(incomingAttempts[i].DestClientName==pm.sUser)
                                {
                                    if(pm.mType == "Response")
                                    {
                                        string s = ASCIIEncoding.ASCII.GetString(ASCIIEncoding.ASCII.GetBytes(pm.text));
                                        incomingAttempts[i].y = BigInteger.Parse(s);

                                        PrivateMessage pmAux = new PrivateMessage();
                                        pmAux.dUser = incomingAttempts[i].DestClientName;
                                        pmAux.sUser = incomingAttempts[i].SourceClientName;

                                        Envelope en = new Envelope("VRequest", pmAux);
                                        string objAux = JsonConvert.SerializeObject(en);
                                        byte[] messageToSend = ASCIIEncoding.ASCII.GetBytes(objAux);

                                        stream.Write(messageToSend, 0, messageToSend.Length);
                                    }
                                    if(pm.mType=="Witness")
                                    {
                                        incomingAttempts[i].HandleWitness(pm);
                                    }
                                }
                            }
                        }
                        
                        if(pm.mType== "Challenge")
                        {
                            outgoingAttempt.HandleChallenge(pm);
                        }
                        if(pm.mType== "Finished")
                        {
                            MessageBox.Show("YOU MADE IT");
                        }
                        if(pm.mType== "Correct")
                        {
                            outgoingAttempt.Commitment();
                        }
                        if(pm.mType== "Incorrect")
                        {
                            MessageBox.Show("WE FAILED");
                        }
                        //MessageBox.Show("got a pm: sUser=" + pm.sUser + "\n" + "dUser: " + pm.dUser + "\n" + "type " + pm.mType +"\n"+ "message "+ pm.text);
                    }

                    else if(unknown.type == "VRequest")
                    {
                        Message m = JsonConvert.DeserializeObject<Message>(unknown.obj.ToString());

                        for (int i=0;i<incomingAttempts.Count;i++)
                        {
                            if(m.user==incomingAttempts[i].DestClientName)
                            {
                                byte[] bytesAux = ASCIIEncoding.ASCII.GetBytes(m.text);
                                string aux = ASCIIEncoding.ASCII.GetString(bytesAux);
                                incomingAttempts[i].v= BigInteger.Parse(aux);
                                incomingAttempts[i].HandleResopnse();
                            }
                        }
                    }

                    break;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (NameBox.Text.Length > 3 && SendBox.Text != "")
            {
                Message m = new Message();
                m.text = SendBox.Text;
                m.user = NameBox.Text;
                SendBox.Text = "";
                Envelope en = new Envelope("Message", m);
                string obj = JsonConvert.SerializeObject(en);
                byte[] messageToSend = ASCIIEncoding.ASCII.GetBytes(obj);
                stream.Write(messageToSend, 0, messageToSend.Length);
            }
            else
            {
                MessageBox.Show("Something to show");
            }
        }

        private void PmSendButton_Click(object sender, RoutedEventArgs e)
        {
            //if (NameBox.Text.Length > 3 && SendBox.Text != "" && PMNameBox.Text.Length > 3)
            //{
            //    PrivateMessage pm = new PrivateMessage();
            //    pm.dUser = PMNameBox.Text;
            //    pm.sUser = name;
            //    pm.text = SendBox.Text;
            //    pm.mType = "TEST";
            //    SendBox.Text = "";
            //    Envelope en = new Envelope("PM", pm);
            //    string obj = JsonConvert.SerializeObject(en);
            //    byte[] messageToSend = ASCIIEncoding.ASCII.GetBytes(obj);

            //    outgoingAttempt = new FiatAttempt(pm.sUser, pm.dUser, stream, n, s, v);

            //    stream.Write(messageToSend, 0, messageToSend.Length);
            //}
            PrivateMessage pm = new PrivateMessage();
            pm.dUser = PMNameBox.Text;
            pm.sUser = name;
            outgoingAttempt = new FiatAttempt(pm.sUser, pm.dUser, stream, n, s, v);

        }

        public bool Contains(string name)
        {
            for (int i = 0; i < incomingAttempts.Count; i++)
            {
                if (incomingAttempts[i].DestClientName == name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
