using System;
using System.Net.Sockets;

namespace VideoChatApplication.Net
{
    internal class Server
    {
        TcpClient _client;

        public Server()
        {
            _client = new TcpClient();
        }

        public void ConnectToServer() {
            if (!_client.Connected) {
                _client.Connect("127.0.0.1", 7891);
            }
        }
            
    }
}
