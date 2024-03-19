using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SocketChat
{
    // A ChatServer osztály implementálja az IChat interfészt, és felelős a szerver oldali üzenetküldésért és kapcsolatkezelésért
    public class ChatServer : IChat
    {
        // Az üzenetkezelő szál
        private bool isActive;

        // A szerver socket példánya
        public Socket Socket { get; set; }

        // A szerver üzenetkezelő szála
        public Thread Thread { get; set; }

        // A WPF Dispatcher példánya
        public Dispatcher Dispatcher { get; set; }

        // A szerver aktivitását jelző tulajdonság
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

        // A szerver IP címe
        public IPAddress IPAddress { get; set; }

        // A szerver portja
        public ushort Port { get; set; }

        // Az IPEndPoint példánya a szerver kapcsolódási pontjához
        public IPEndPoint IPEndPoint { get { return new IPEndPoint(this.IPAddress, this.Port); } }

        // A kliens azonosítóját számontartó számláló
        public int ClientIdCounter { get; set; }

        // A szerver felhasználóneve
        public string SourceUsername { get; set; }

        // A szerver által ismert más kliensek listája
        public BindingList<Client> ClientList { get; set; }

        // A szerver által ismert üzenetek listája
        public BindingList<string> ChatList { get; set; }

        // Az IsActiveChanged eseményt kezelő eseménykezelő
        public EventHandler IsActiveChanged { get; set; }

        // A ChatServer osztály konstruktora
        public ChatServer()
        {
            // A WPF Dispatcher inicializálása
            this.Dispatcher = Dispatcher.CurrentDispatcher;

            // A szerver által ismert más kliensek listájának inicializálása
            this.ClientList = new BindingList<Client>();

            // A szerver által ismert üzenetek listájának inicializálása
            this.ChatList = new BindingList<string>();

            // A kliens azonosítóját számontartó számláló inicializálása
            this.ClientIdCounter = 0;

            // A szerver felhasználónevének inicializálása
            this.SourceUsername = "Server";
        }

        // A szerver kapcsolódását indító metódus
        public void StartConnection()
        {
            // Ha a szerver már aktív, nincs teendő
            if (this.IsActive)
            {
                return;
            }

            // A szerver socket példányosítása és kapcsolódás az IPEndPoint-hoz
            this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.Socket.Bind(this.IPEndPoint);
            this.Socket.Listen(1);

            // A szerver üzenetkezelő szálának indítása
            this.Thread = new Thread(new ThreadStart(this.WaitForConnections));
            this.Thread.Start();

            // Az aktív kliensek listájának frissítése
            this.ClientList.Add(new Client() { ID = ClientIdCounter, Username = SourceUsername });
            this.IsActive = true;
        }

        // Kliensek bejövő kapcsolatait váró metódus
        private void WaitForConnections()
        {
            while (true)
            {
                if (this.Socket == null)
                {
                    throw new Exception();
                }

                // Új kliens bejövő kapcsolatának fogadása
                Client client = new Client();
                client.ID = this.ClientIdCounter;
                client.Username = "NewUser"; // Ideiglenes felhasználónév
                try
                {
                    client.Socket = this.Socket.Accept();
                    client.Thread = new Thread(() => this.ProcessMessages(client));

                    // Az új kliens hozzáadása az aktív kliensek listájához
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.ClientList.Add(client);
                    }), null);

                    // Az új kliens üzenetkezelő szálának indítása
                    client.Thread.Start();
                }
                catch (SocketException ex)
                {
                    // Hiba kezelése
                    Debug.WriteLineIf(ex.ErrorCode != 10004, $"*EXCEPTION* {ex.ErrorCode}: {ex.Message}");
                    if (ex.ErrorCode != 10004)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        // Hibaüzeneteket megjelenítő metódus
        public void PrintError(Exception ex)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"\nMessage ---\n{ex.Message}");
            sb.AppendLine($"\nHelpLink ---\n{ex.HelpLink}");
            sb.AppendLine($"\nSource ---\n{ex.Source}");
            sb.AppendLine($"\nStackTrace ---\n{ex.StackTrace}");
            sb.AppendLine($"\nTargetSite ---\n{ex.TargetSite}");

            MessageBox.Show(sb.ToString());
        }

        // Kliens üzeneteit feldolgozó metódus
        private void ProcessMessages(Client client)
        {
            while (true)
            {
                try
                {
                    // Ha a kliens kapcsolata megszakadt, leáll a feldolgozás
                    if (!client.IsSocketConnected())
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.RemoveActiveUser(client);
                            this.ClientList.Remove(client);
                            client.Dispose();
                        }), null);

                        return;
                    }

                    byte[] inf = new byte[1024];
                    int x = client.Socket.Receive(inf);
                    if (x > 0)
                    {
                        string strMessage = Encoding.Unicode.GetString(inf);

                        // Parancsok ellenőrzése és végrehajtása
                        if (strMessage.Substring(0, 8) == "/setname")
                        {
                            string newUsername = strMessage.Replace("/setname ", "").Trim('\0');

                            // Kliens felhasználónevét beállítjuk
                            client.Username = newUsername;

                            // Az aktív kliensek felé elküldjük az új és meglévő felhasználók listáját
                            this.UpdateClientsActiveUsers(client, strMessage);
                        }
                        else if (strMessage.Substring(0, 6) == "/msgto")
                        {
                            string data = strMessage.Replace("/msgto ", "").Trim('\0');
                            string targetUsername = data.Substring(0, data.IndexOf(':'));
                            string message = data.Substring(data.IndexOf(':') + 1);

                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                string targetMessage = client.Username + ": " + message;

                                // Ha a címzett a szerver, az üzenetet továbbítjuk a kliensek felé
                                if (targetUsername == this.SourceUsername)
                                {
                                    this.ChatList.Add(targetMessage);
                                    this.ForwardClientMessage(client, targetUsername, targetMessage);
                                }
                                // Különben az üzenet a címzetthez továbbítása
                                else
                                {
                                    // A kapott üzenet megjelenítése a chat ablakban
                                    this.ChatList.Add(targetMessage);

                                    // Az üzenet továbbítása a címzett kliensnek
                                    this.ForwardClientMessage(client, targetUsername, targetMessage);
                                }
                            }), null);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    // Hiba kezelése
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.RemoveActiveUser(client);
                        this.ClientList.Remove(client);
                        client.Dispose();

                        Debug.WriteLine($"*EXCEPTION* {ex.ErrorCode}: {ex.Message}");
                        if (ex.ErrorCode == 10054)
                        {
                            MessageBox.Show("Client has disconnected");
                        }
                        if (ex.ErrorCode != 10054 && ex.ErrorCode != 10004)
                        {
                            MessageBox.Show(ex.Message, ex.ErrorCode.ToString());
                        }
                    }), null);
                    return;
                }
            }
        }

        // Az aktív kliensek listájának frissítését végző metódus
        private void UpdateClientsActiveUsers(Client client, string strMessage)
        {
            foreach (Client user in this.ClientList)
            {
                // Minden meglévő felhasználó nevét elküldjük az utolsó csatlakozott kliensnek
                if (client == this.ClientList.Last())
                {
                    string allUsers = $"/setname {user.Username}";
                    client.SendMessage(allUsers);

                    // Az üzenetküldés aszinkron, ezért kis várakozás
                    Thread.Sleep(50);
                }

                // Az utolsó csatlakozott felhasználó nevét elküldjük minden meglévő felhasználónak
                if (client != user)
                {
                    this.ForwardClientMessage(client, user.Username, strMessage);
                }
            }
        }

        // Az aktív kliens törlését végző metódus
        private void RemoveActiveUser(Client client)
        {
            string delMessage = string.Format("/delname {0}", client.Username);

            // Az aktív kliens törlésének jelzése minden meglévő kliensnek
            foreach (Client user in this.ClientList)
            {
                if (client != user)
                {
                    this.ForwardClientMessage(client, user.Username, delMessage);
                }
            }
        }

        // Kliens üzenetének továbbítását végző metódus
        private void ForwardClientMessage(Client client, string targetUsername, string targetMessage)
        {
            // Ha a címzett a szerver, az üzenet továbbítása minden kliensnek
            if (targetUsername == this.SourceUsername)
            {
                foreach (Client user in this.ClientList)
                {
                    if (client != user)
                    {
                        user.SendMessage(targetMessage);
                    }
                }
            }
            // Különben az üzenet továbbítása a címzetthez
            else
            {
                // A címzett kliens kikeresése
                Client user = this.ClientList.FirstOrDefault(item => item.Username == targetUsername);

                // Ha a címzett létezik
                if (user != null)
                {
                    // Az üzenet elküldése a címzett kliensnek
                    user.SendMessage(targetMessage);
                }
                // Ha a címzett felhasználó nem létezik
                else
                {
                    // Hibaüzenet küldése a küldő kliensnek
                    string errorMessage = "Error! Username not found, unable to deliver your message";
                    client.SendMessage(errorMessage);

                    // Hibaüzenet megjelenítése a chat ablakban
                    this.ChatList.Add("**Server**: Error! Username not found, unable to deliver your message");
                }
            }
        }

        // Kapcsolat leállítását végző metódus
        public void StopConnection()
        {
            // Ha a szerver nem aktív, nincs teendő
            if (!this.IsActive)
            {
                return;
            }

            // Az összes aktív kliens eltávolítása
            while (this.ClientList.Count != 0)
            {
                Client c = this.ClientList[0];

                this.ClientList.Remove(c);
                c.Dispose();
            }

            // A szerver socket lezárása és felszabadítása
           
            this.Socket.Close();

            // Close helyett esetleg valami disconnect vagy valami
            this.Socket.Dispose();

            // Az üzenet- és klienslisták törlése, aktivitás állapotának visszaállítása
            this.ChatList.Clear();
            this.IsActive = false;
        }

        // Az IsActiveChanged esemény kiváltását végző metódus
        public void OnIsActiveChanged(EventArgs e)
        {
            this.IsActiveChanged?.Invoke(this, e);
        }
    }
}
