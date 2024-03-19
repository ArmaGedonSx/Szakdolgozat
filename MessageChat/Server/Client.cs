using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketChat
{
    // A Client osztály implementálja a IDisposable és INotifyPropertyChanged interfészeket
    public class Client : IDisposable, INotifyPropertyChanged
    {
        // A kliens azonosítója
        private int id;

        // Az objektum felszabadítását jelző flag
        private bool isDisposed;

        // A kliens felhasználóneve
        private string username;

        // Az INotifyPropertyChanged interfész PropertyChanged eseménye
        public event PropertyChangedEventHandler PropertyChanged;

        // A kliens socket példánya
        public Socket Socket { get; set; }

        // A kliens üzenetkezelő szálának példánya
        public Thread Thread { get; set; }

        // A kliens azonosítóját lekérdező és beállító tulajdonság
        public int ID
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
                this.NotifyPropertyChanged("ID");
            }
        }

        // A kliens felhasználónevét lekérdező és beállító tulajdonság
        public string Username
        {
            get
            {
                return this.username;
            }
            set
            {
                this.username = value;
                this.NotifyPropertyChanged("Username");
            }
        }

        // A Client osztály konstruktora
        public Client()
        {
            this.isDisposed = false;
        }

        // A socket kapcsolat állapotát ellenőrző statikus metódus
        public static bool IsSocketConnected(Socket s)
        {
            if (!s.Connected)
            {
                return false;
            }

            if (s.Available == 0)
            {
                if (s.Poll(1000, SelectMode.SelectRead))
                {
                    return false;
                }
            }

            return true;
        }

        // Az objektum felszabadítását végző metódus
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                if (this.Socket != null)
                {
                    // Socket lezárása és felszabadítása
                    this.Socket.Shutdown(SocketShutdown.Both);
                    this.Socket.Dispose();
                    this.Socket = null;
                }
                if (this.Thread != null)
                {
                    // Üzenetkezelő szál nullázása
                    this.Thread = null;
                }

                this.isDisposed = true;
            }
        }

        // A socket kapcsolat állapotát ellenőrző metódus
        public bool IsSocketConnected()
        {
            return IsSocketConnected(this.Socket);
        }

        // Üzenet küldését végző metódus
        public void SendMessage(string message)
        {
            try
            {
                this.Socket.Send(Encoding.Unicode.GetBytes(message));
            }
            catch (Exception)
            {
                // Hiba esetén elkapjuk, de nem dobunk kivételt
            }
        }

        // PropertyChanged eseményt kiváltó metódus
        private void NotifyPropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
