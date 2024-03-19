using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SocketChat
{
    // ClientSelector osztály, amely INotifyPropertyChanged interfészt implementál
    public class ClientSelector : INotifyPropertyChanged
    {
        // A kiválasztott üzenetcserélő típusa (szerver vagy kliens)
        private bool isServer;

        // Az üzenetcserélő interfész, amely lehet szerver vagy kliens típusú
        private IChat chatInterface;

        // A kiválasztott felhasználói név
        private Client selectedUsername;

        // A cél felhasználó neve
        private string targetUsername;

        // Az üzenet tartalma
        private string messageContent;

        // Kezdeti beállítások és parancsok létrehozása
        public ClientSelector()
        {
            this.IsServer = true;

            // Az üzenetcserélő indítására és leállítására szolgáló parancsok
            this.StartConnectionCMD = new RelayCommand(this.StartConnection);
            this.SendMessageCMD = new RelayCommand(() => this.SendMessage(this.TargetUsername, this.MessageContent));
        }

        // Az INotifyPropertyChanged interfész PropertyChanged eseménye
        public event PropertyChangedEventHandler PropertyChanged;

        // Az üzenetcserélő indítására szolgáló parancs
        public ICommand StartConnectionCMD { get; }

        // Az üzenet küldésére szolgáló parancs
        public ICommand SendMessageCMD { get; }

        // Az üzenetlista, amely tartalmazza a beszélgetés adatait
        public BindingList<String> ChatList
        {
            get
            {
                return this.chatInterface.ChatList;
            }
            private set
            {
                this.chatInterface.ChatList = value;
                this.NotifyPropertyChanged("ChatList");
            }
        }

        // A csatlakozott emberek listája
        public BindingList<Client> ClientList
        {
            get
            {
                return this.chatInterface.ClientList;
            }
            private set
            {
                this.chatInterface.ClientList = value;
                this.NotifyPropertyChanged("ClientList");
            }
        }

        // Az ablak címe, attól függően, hogy a szerver vagy kliens mód van kiválasztva
        public string WindowTitle
        {
            get
            {
                return IsServer ? "Server" : "Client";
            }
        }

        // Az ablak ikonja, attól függően, hogy a szerver vagy kliens mód van kiválasztva
        public string WindowIcon
        {
            get
            {
                return IsServer ? "Server.ico" : "Client.ico";
            }
        }

        // A kiválasztott üzenetküldő típusa (szerver vagy kliens)
        public bool IsServer
        {
            get
            {
                return this.isServer;
            }
            set
            {
                this.isServer = value;
                this.NotifyPropertyChanged("IsServer");
                this.NotifyPropertyChanged("WindowTitle");
                this.NotifyPropertyChanged("WindowIcon");
                this.SelectClient();
            }
        }

        // Az üzenetküldő állapotát jelző tulajdonság
        public bool IsActive
        {
            get
            {
                return this.chatInterface.IsActive;
            }
            private set
            {
                this.chatInterface.IsActive = value;
                this.NotifyPropertyChanged("IsActive");
            }
        }

        // Az aktív ügyfelek számát jelző tulajdonság
        public int ActiveClients
        {
            get
            {
                return this.chatInterface.ClientList.Count;
            }
        }

        // Az IP cím tulajdonsága, amelyre a szerver vagy kliens csatlakozik
        public string IpAddress
        {
            get
            {
                return this.chatInterface.IPAddress.ToString();
            }
            set
            {
                if (this.IsActive)
                {
                    throw new Exception("Can't change this property when the server is active");
                }

                this.chatInterface.IPAddress = IPAddress.Parse(value);
                this.NotifyPropertyChanged("IpAddress");
            }
        }

        // A port tulajdonsága, amelyen a szerver vagy kliens csatlakozik
        public ushort Port
        {
            get
            {
                return this.chatInterface.Port;
            }
            set
            {
                if (this.IsActive)
                {
                    throw new Exception("Can't change this property when the server is active");
                }

                this.chatInterface.Port = value;
                this.NotifyPropertyChanged("Port");
            }
        }

        // A forrás felhasználói név tulajdonsága
        public string SourceUsername
        {
            get
            {
                return this.chatInterface.SourceUsername;
            }
            set
            {
                this.chatInterface.SourceUsername = value;
                if (this.IsActive)
                {
                    this.chatInterface.ClientList[0].Username = value;
                }
                this.NotifyPropertyChanged("SourceUsername");
            }
        }

        // A cél felhasználói név tulajdonsága
        public string TargetUsername
        {
            get
            {
                return this.targetUsername;
            }
            set
            {
                this.targetUsername = value;
                this.NotifyPropertyChanged("TargetUsername");
            }
        }

        // A kiválasztott felhasználói név tulajdonsága
        public Client SelectedUsername
        {
            get
            {
                return this.selectedUsername;
            }
            set
            {
                this.selectedUsername = value;

                if (this.selectedUsername == null)
                {
                    return;
                }

                if (this.selectedUsername is Client)
                {
                    this.TargetUsername = this.selectedUsername.Username.ToString();
                }

                this.NotifyPropertyChanged("SelectedUsername");
            }
        }

        // Az üzenet tartalmának tulajdonsága
        public string MessageContent
        {
            get
            {
                return this.messageContent;
            }
            set
            {
                this.messageContent = value;
                this.NotifyPropertyChanged("MessageContent");
            }
        }

        // Az üzenetküldő típusának kiválasztása (szerver vagy kliens)
        private void SelectClient()
        {
            if (this.IsServer)
            {
                this.chatInterface = new ChatServer();
                this.SourceUsername = this.chatInterface.SourceUsername;
            }
            else
            {
                this.chatInterface = new ChatClient();
                this.SourceUsername = this.chatInterface.SourceUsername;
            }

            this.IpAddress = "127.0.0.1";
            this.Port = 80;

            this.ClientList = new BindingList<Client>();
            this.ChatList = new BindingList<string>();

            // Eseménykezelő hozzáadása az üzenetküldő állapotváltozásához
            this.chatInterface.IsActiveChanged = new EventHandler(this.IsActiveBool);

            // Eseménykezelő hozzáadása az ügyfél lista változásához
            this.chatInterface.ClientList.ListChanged += (_sender, _e) =>
            {
                this.NotifyPropertyChanged("ActiveClients");
            };
        }

        // Az üzenetküldő indítása vagy leállítása
        public void StartConnection()
        {
            if (this.IsServer)
            {
                if (!this.IsActive)
                {
                    try
                    {
                        this.chatInterface.StartConnection();
                    }
                    catch (SocketException ex)
                    {
                        if (ex.ErrorCode == 10048)
                        {
                            MessageBox.Show("Port " + Port + " is currently in use.");
                        }
                        else
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
                else
                {
                    this.chatInterface.StopConnection();
                }
            }
            else
            {
                if (!this.IsActive)
                {
                    try
                    {
                        this.chatInterface.StartConnection();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    this.chatInterface.StopConnection();
                }
            }
        }

        // Üzenet küldése
        public void SendMessage(string targetUsername, string messageContent)
        {
            string sourceMessage = this.chatInterface.SourceUsername + ": " + messageContent;

            bool isSent = false;

            if (targetUsername == this.SourceUsername)
            {
                isSent = true;
            }
            else
            {
                // Szerver küldése
                if (this.IsServer)
                {
                    Client client = this.chatInterface.ClientList.FirstOrDefault(i => i.Username == this.TargetUsername);
                    if (client != null)
                    {
                        client.SendMessage(sourceMessage);
                        isSent = true;
                    }
                    else
                    {
                        sourceMessage = targetUsername + ": " + "is not an active client.";
                        isSent = false;
                    }
                }
                // Kliens küldése
                else
                {
                    string message = string.Format("/msgto {0}:{1}", targetUsername, messageContent);
                    this.chatInterface.Socket.Send(Encoding.Unicode.GetBytes(message));

                    isSent = true;
                }
            }

            this.chatInterface.ChatList.Add(sourceMessage);
        }

        // Az üzenetküldő állapotának változásakor hívódik meg
        public void IsActiveBool(object sender, EventArgs e)
        {
            this.NotifyPropertyChanged("IsActive");
        }

        // PropertyChanged esemény kiváltása
        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
