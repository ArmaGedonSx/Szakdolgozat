using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chat_Server
{
    public partial class ServerWindow : Window
    {
        public const string Version = "1.0";
        public readonly static string n = Environment.NewLine;

        Chat chat;
        BitmapImage Button_Background_1;
        BitmapImage Button_Background_2;

        delegate void AddLogDelegate(string sender, string text);
        delegate void ShowUsersDelegate();

        public ServerWindow()
        {
            InitializeComponent();

        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UserInfo.Content = "";
            ServersInfo.Content = "";

            ServerTitle.Content = "Server" + Version;
            Button_Background_1 = LoadBitmapFromResource("Button_Background_1");
            Button_Background_2 = LoadBitmapFromResource("Button_Background_2");
            Tabs.Background = new SolidColorBrush(Colors.Transparent);
            Tabs.BorderBrush = new SolidColorBrush(Colors.Transparent);
            foreach (TabItem tab in Tabs.Items)
            {
                tab.Visibility = Visibility.Collapsed;
            }
            AddLog("SERVER", "Window loaded.");
            chat = new Chat(this);
        }

        public void AddLog_delegate(string sender, string text)
        {
            Dispatcher.Invoke(new AddLogDelegate(AddLog), sender, text);
        }

        public void AddLog(string sender, string text)
        {
            if (Log.Text != "") Log.Text += $"{Environment.NewLine}[{sender}]: {text}";
            else Log.Text += $"[{sender}]: {text}";
            Log.ScrollToEnd();
        }

        void ServerWindow_Closing(object sender, CancelEventArgs e)
        {
            chat.Dispose();
            Environment.Exit(0);
        }

        void Window_Closed(object sender, EventArgs e)
        {

        }

        void Cut_Down_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        /*
        void Mouse_Enter_Close_Button(object sender, RoutedEventArgs e)
        {
            Close_Button_Image1.Source = LoadBitmapFromResource("Close_Button2.png");
        }

        void Mouse_Leave_Close_Button(object sender, RoutedEventArgs e)
        {
            Close_Button_Image1.Source = LoadBitmapFromResource("Close_Button1.png");
        }

        void Mouse_Enter_FullScreen_Button(object sender, RoutedEventArgs e)
        {
            FullScreen_Button_Image1.Source = LoadBitmapFromResource("Fullscreen_Button2.png");
        }

        void Mouse_Leave_FullScreen_Button(object sender, RoutedEventArgs e)
        {
            FullScreen_Button_Image1.Source = LoadBitmapFromResource("FullScreen_Button1.png");
        }*/

        void Mouse_Enter_Cut_Down_Button(object sender, RoutedEventArgs e)
        {
            Cut_Down_Button_Image1.Source = LoadBitmapFromResource("Cut_Down_Button2.png");
        }

        void Mouse_Leave_Cut_Down_Button(object sender, RoutedEventArgs e)
        {
            Cut_Down_Button_Image1.Source = LoadBitmapFromResource("Cut_Down_Button1.png");
        }

        #region Style
        public static BitmapImage LoadBitmapFromResource(string Path)
        {
            Path = @"Resources/" + Path + ".png";
            Assembly assembly = Assembly.GetCallingAssembly();
            if (Path[0] == '/')
            {
                Path = Path.Substring(1);
            }
            return new BitmapImage(new Uri(@"pack://application:,,,/" + assembly.GetName().Name + ";component/" + Path, UriKind.Absolute));
        }

        void Home_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Tabs.SelectedIndex = 0;
        }

        void Home_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Home.Source = Button_Background_2;
        }

        void Home_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Home.Source = Button_Background_1;
        }

        void Statistics_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Tabs.SelectedIndex = 1;
        }

        void Statistics_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Statistics.Source = Button_Background_2;
        }

        void Statistics_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Statistics.Source = Button_Background_1;
        }

        void UsersTab_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Tabs.SelectedIndex = 2;
        }

        void UsersTab_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            UsersTab.Source = Button_Background_2;
        }

        void UsersTab_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            UsersTab.Source = Button_Background_1;
        }

        void ServersTab_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Tabs.SelectedIndex = 3;
        }

        void ServersTab_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ServersTab.Source = Button_Background_2;
        }

        void ServersTab_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ServersTab.Source = Button_Background_1;
        }

        void Exit_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Exit.Source = Button_Background_1;
        }

        void Exit_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Exit.Source = Button_Background_2;
        }

        void Exit_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        void Reload_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Reload.Source = Button_Background_2;
        }

        void Reload_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Reload.Source = Button_Background_1;
        }

        void Reload_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process currentProcess = Process.GetCurrentProcess();
            try
            {
                currentProcess.WaitForExit(1000);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("RESTART ERROR! " + ex.Message);
            }
            Process.Start(currentProcess.ProcessName, "");
            Environment.Exit(0);
        }
        #endregion

        void ShowUsers()
        {
            UsersList.Items.Clear();
            foreach (User user in Database.Users.Values) UsersList.Items.Add(user.Info.Nickname);
        }

        public void ShowUsers_async()
        {
            Dispatcher.Invoke(new ShowUsersDelegate(ShowUsers));
        }

        void ShowServers()
        {
            ServersList.Items.Clear();
            foreach (Server server in Database.Servers.Values) ServersList.Items.Add(server.Info.Title);
        }

        public void ShowServers_async()
        {
            Dispatcher.Invoke(new ShowUsersDelegate(ShowServers));
        }
        void UsersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            User user = Database.GetUser(UsersList.SelectedIndex);
            if (user != null)
            {
                UserInfo.Content = $"Nickname: {user.Info.Nickname}{n}Login: {user.Info.Login}{n}Password: {user.Info.Password}{n}Mail: {user.Info.Email}{n}Recovery code: {user.Info.RecoveryCode}{n}Current server: {(user.CurrentServer == null ? "No" : user.CurrentServer.Info.Title)}{n}IP: {user.CommandsIpAddress}";
            }
        }

        void Send_Click(object sender, RoutedEventArgs e)
        {
            string text = Command.Text;
            string message = "";

            if (text != "")
            {
                if (text.Contains("notification: "))
                {
                    string msg = text.Substring(text.IndexOf(":") + 2);
                    foreach (Server server in Database.Servers.Values)
                    {
                        foreach (User user in server.Users)
                        {
                            Chat.SendCommand(ServerCommands.TextMessage, user.CommandsIpAddress, $"[SERVER]: {msg}{Environment.NewLine}");
                        }
                    }

                    message = $"Notification sent ({msg})";
                }
                else if (text.Contains("create_server: "))
                {
                    string msg = text.Substring(text.IndexOf(":") + 2);
                    string[] msg_mas = msg.Split(' ');
                    try
                    {
                        Database.CreateServer(msg_mas[0], msg_mas[1], int.Parse(msg_mas[2]));
                        message = $"Server created ({msg_mas[0]})";
                    }
                    catch (Exception ex)
                    {
                        message = ex.Message;
                    }


                }
                else message = "Unknown command.";
                AddLog("SERVER", message); Command.Text = "";
            }
        }

        void ServersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Server server = Database.GetServer(ServersList.SelectedIndex);
            if (server != null)
            {
                string serverUsers = n;
                for (int i = 0; i < server.Users.Count; i++)
                {
                    if (i == server.Users.Count - 1) serverUsers += server.GetUser(i).Info.Nickname;
                    else serverUsers += $"{server.GetUser(i).Info.Nickname},{n}";
                }
                ServersInfo.Content = $"Title: {server.Info.Title}{n}SID: {server.Info.SID}{n}Password: {server.Info.Password}{n}Maximum number of users: {server.Info.MaxUsersCount}{n}Users on the server: {serverUsers}";
            }
        }

        void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }


    }
}
