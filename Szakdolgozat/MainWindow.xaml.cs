using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.ComponentModel;
using System.CodeDom;
using System.Windows.Media;

namespace Szakdolgozat
{

    public partial class MainWindow : Window
    {
        private TcpClient client;
        public StreamReader STR;
        public StreamWriter STW;
        public string receive;
        public string TextToSend;

        BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        BackgroundWorker backgroundWorker2 = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();

            IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress address in localIP)
            {
                if(address.AddressFamily == AddressFamily.InterNetwork) { 
                    ServerIpTextBox.Text = address.ToString();
                }
            }
        }

        private void StartButton_Click(object sender, EventArgs e) {
            TcpListener listener = new TcpListener(IPAddress.Any, int.Parse(ServerPortTextBox.Text));
            listener.Start();
            client = listener.AcceptTcpClient();
            STR = new StreamReader(client.GetStream()); 
            STW = new StreamWriter(client.GetStream());
            STW.AutoFlush = true;
            backgroundWorker1.RunWorkerAsync();
            backgroundWorker2.WorkerSupportsCancellation = true;
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            client = new TcpClient();
            IPEndPoint IpEnd = new IPEndPoint(IPAddress.Parse(ClientIpTextBox.Text), int.Parse(ClientPortTextBox.Text));
            //client.Connect(IpEnd);


            try
            {
                ChatScreenTextBox.AppendText("Connect to Server" + "\n");
                STW = new StreamWriter(client.GetStream());
                STR = new StreamReader(client.GetStream());
                STW.AutoFlush = true;
                backgroundWorker1.RunWorkerAsync();
                backgroundWorker2.WorkerSupportsCancellation = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while(client.Connected)
            {
                try {
                    receive = STR.ReadLine();
                    this.ChatScreenTextBox.Dispatcher.BeginInvoke(new Action(delegate () { ChatScreenTextBox.AppendText("You: " + receive + "\n"); }));
                    receive = "";
                } catch (Exception ex){
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            if (client.Connected)
            {
                STW.WriteLine(TextToSend);
                this.ChatScreenTextBox.Dispatcher.BeginInvoke(new Action(delegate() { ChatScreenTextBox.AppendText("Me: " + TextToSend + "\n"); }));
            }else
            {
                MessageBox.Show("Sending Failed");
            }

            backgroundWorker2.CancelAsync();
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            if(MessageTextBox.Text != "")
            {
                TextToSend = MessageTextBox.Text;
                backgroundWorker2.RunWorkerAsync();
            }
            MessageTextBox.Text = "";
        }




    }
}
