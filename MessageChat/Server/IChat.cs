using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace SocketChat
{
    // Az IChat interfész, amely meghatározza a SocketChat alkalmazás chat funkcionalitását
    public interface IChat
    {
        // A socket kapcsolatot reprezentáló tulajdonság
        Socket Socket { get; set; }

        // A chat üzenetkezelő szálát reprezentáló tulajdonság
        Thread Thread { get; set; }

        // A WPF dispatcher példányát reprezentáló tulajdonság
        Dispatcher Dispatcher { get; set; }

        // A chat aktív állapotát jelző tulajdonság
        bool IsActive { get; set; }

        // A szerver IP címét reprezentáló tulajdonság
        IPAddress IPAddress { get; set; }

        // A szerver portját reprezentáló tulajdonság
        ushort Port { get; set; }

        // A szerver címét és portját tartalmazó IPEndPoint
        IPEndPoint IPEndPoint { get; }

        // A chat kliens azonosítóit számláló tulajdonság
        int ClientIdCounter { get; set; }

        // A saját felhasználónevét reprezentáló tulajdonság
        string SourceUsername { get; set; }

        // A csatlakozott kliensek listáját tartalmazó BindingList
        BindingList<Client> ClientList { get; set; }

        // A chat üzeneteit tartalmazó BindingList
        BindingList<string> ChatList { get; set; }

        // Az IsActiveChanged eseményt kiváltó eseménykezelő
        EventHandler IsActiveChanged { get; set; }

        // A kapcsolat indítását végző metódus
        void StartConnection();

        // A kapcsolat leállítását végző metódus
        void StopConnection();

        // Az IsActiveChanged esemény kiváltását végző metódus
        void OnIsActiveChanged(EventArgs e);
    }
}
