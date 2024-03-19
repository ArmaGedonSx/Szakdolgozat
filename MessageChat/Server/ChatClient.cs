using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SocketChat
{
    // A ChatClient osztály implementálja az IChat interfészt, és felelős a kliens oldali üzenetküldésért és kapcsolatkezelésért
    public class ChatClient : IChat
    {
        // Az üzenetkezelő szál
        private bool isActive;

        // A kliens socket példánya
        public Socket Socket { get; set; }

        // A kliens üzenetkezelő szála
        public Thread Thread { get; set; }

        // A WPF Dispatcher példánya
        public Dispatcher Dispatcher { get; set; }

        // A kliens aktivitását jelző tulajdonság
        public bool IsActive
        {
            get
            {
                return this.isActive;
            }
            set
            {
                this.isActive = value;
                this.OnIsActiveChanged(EventArgs.Empty);
            }
        }

        // A kliens IP címe
        public IPAddress IPAddress { get; set; }

        // A kliens portja
        public ushort Port { get; set; }

        // Az IPEndPoint példánya a kliens kapcsolódási pontjához
        public IPEndPoint IPEndPoint { get { return new IPEndPoint(this.IPAddress, this.Port); } }

        // A kliens azonosítóját számontartó számláló
        public int ClientIdCounter { get; set; }

        // A kliens felhasználóneve
        public string SourceUsername { get; set; }

        // A kliens által ismert más kliensek listája
        public BindingList<Client> ClientList { get; set; }

        // A kliens által ismert üzenetek listája
        public BindingList<string> ChatList { get; set; }

        // Az IsActiveChanged eseményt kezelő eseménykezelő
        public EventHandler IsActiveChanged { get; set; }

        // A ChatClient osztály konstruktora
        public ChatClient()
        {
            // A WPF Dispatcher inicializálása
            this.Dispatcher = Dispatcher.CurrentDispatcher;

            // A kliens által ismert más kliensek listájának inicializálása
            this.ClientList = new BindingList<Client>();

            // A kliens által ismert üzenetek listájának inicializálása
            this.ChatList = new BindingList<string>();

            // A kliens azonosítóját számontartó számláló inicializálása
            this.ClientIdCounter = 0;

            // A kliens felhasználónevének generálása
            this.SourceUsername = "Client" + new Random().Next(0, 99).ToString();
        }

        // A kliens kapcsolódását indító metódus
        public void StartConnection()
        {
            // Ha a kliens már aktív, nincs teendő
            if (this.IsActive)
            {
                return;
            }

            // A kliens socket példányosítása és kapcsolódás az IPEndPoint-hez
            this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.Socket.Connect(this.IPEndPoint);

            // Felhasználónév beállítása
            this.SetUsername(this.SourceUsername);

            // Az üzenetkezelő szál indítása
            this.Thread = new Thread(() => this.ReceiveMessages());
            this.Thread.Start();

            // Aktivitás állapotának beállítása
            this.IsActive = true;
        }

        // Felhasználónév beállítása
        private void SetUsername(string newUsername)
        {
            string cmd = string.Format("/setname {0}", newUsername);
            this.Socket.Send(Encoding.Unicode.GetBytes(cmd));
        }

        // Üzenetek fogadásáért felelős metódus
        public void ReceiveMessages()
        {
            while (true)
            {
                byte[] inf = new byte[1024];

                try
                {
                    // Ha a socket nincs csatlakoztatva, leáll a fogadás
                    if (!IsSocketConnected(this.Socket))
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.StopConnection();
                        }));
                        return;
                    }

                    // Üzenet fogadása és feldolgozása
                    int x = this.Socket.Receive(inf);
                    if (x > 0)
                    {
                        string strMessage = Encoding.Unicode.GetString(inf).Trim('\0');

                        // Új felhasználó hozzáadása a kliens listához
                        if (strMessage.Substring(0, 8) == "/setname")
                        {
                            string newUsername = strMessage.Replace("/setname ", "").Trim('\0');
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.ClientList.Add(new Client() { ID = ClientIdCounter, Username = newUsername });
                            }));
                        }
                        // Régi felhasználó eltávolítása a kliens listából
                        else if (strMessage.Substring(0, 8) == "/delname")
                        {
                            string oldUsername = strMessage.Replace("/delname ", "").Trim('\0');
                            Client oldUser = this.ClientList.First(item => item.Username == oldUsername);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.ClientList.Remove(oldUser);
                            }));
                        }
                        // Egyéb üzenetek hozzáadása az üzenetek listájához
                        else
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.ChatList.Add(strMessage);
                            }));
                        }
                    }
                }
                catch (SocketException ex)
                {
                    // Hiba kezelése és leállás
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.StopConnection();
                        Debug.WriteLineIf(ex.ErrorCode != 10004, $"*EXCEPTION* {ex.ErrorCode}: {ex.Message}");
                        if (ex.ErrorCode != 10004)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }));
                    return;
                }
            }
        }

        // Az üzenetek fogadásáért felelős metódus
        public static bool IsSocketConnected(Socket socket)
        {
            if (!socket.Connected)
            {
                return false;
            }

            if (socket.Available == 0)
            {
                if (socket.Poll(1000, SelectMode.SelectRead))
                {
                    return false;
                }
            }

            return true;
        }

        // Kapcsolat leállítását végző metódus
        public void StopConnection()
        {
            if (!this.IsActive)
            {
                return;
            }

            if (this.Socket != null && this.Thread != null)
            {
                // A szál leállítása és a socket bezárása
                this.Socket.Shutdown(SocketShutdown.Both);
                this.Socket.Dispose();
                this.Socket = null;
                this.Thread = null;
            }

            // Üzenet- és klienslisták törlése, aktivitás állapotának visszaállítása
            this.ChatList.Clear();
            this.ClientList.Clear();
            this.IsActive = false;
        }

        // Az IsActiveChanged esemény kiváltását végző metódus
        public void OnIsActiveChanged(EventArgs e)
        {
            this.IsActiveChanged?.Invoke(this, e);
        }
    }
}
