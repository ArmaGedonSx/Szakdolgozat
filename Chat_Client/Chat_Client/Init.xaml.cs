using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Chat_Client
{
    public partial class Init : Window
    {
        public delegate void InvokeDelegate(string text);
        public delegate void Loading();
        Loading LoadForm;
        Thread AnimationThread;

        ///<summary> Initialization </summary>
        public Init()
        {
            InitializeComponent();
            Config.Load();
            Chat.SetIP(Config.CurrentConfig.ServerIP);
        }

        ///<summary> Beginning </summary>
        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadForm += Success;

            try
            {
                new Thread(Check).Start();
                AnimationThread = new Thread(Animation);
                AnimationThread.Start();
            }
            catch (Exception x)
            {
                Library.Error("Initialization error!", x.Message);
                Environment.Exit(0);
            }
        }

        ///<summary> Animation of points </summary>
        public void Animation()
        {
            while (true)
            {
                SetText_async("Loading.");
                Thread.Sleep(500);
                SetText_async("Loading..");
                Thread.Sleep(500);
                SetText_async("Loading...");
                Thread.Sleep(500);
            }
        }

        ///<summary> Assign text to status </summary>
        void SetText(string text)
        {
            this.text.Content = text;
        }

        ///<summary> Assign text to status </summary>
        void SetText_async(string text)
        {
            Dispatcher.BeginInvoke(new InvokeDelegate(SetText), text);
        }

        ///<summary> Checking for updates </summary>
        void Check()
        {
            string Result = Chat.GetServerVersion();
            if (Result == "ERROR")
            {
                AnimationThread.Abort();
                SetText_async("Chat unavailable");
            }
            else
            {
                if (Result == Library.Version) Dispatcher.Invoke(LoadForm);
                else Dispatcher.Invoke(LoadForm);
                //else Dispatcher.Invoke(new Library.Error_asyncDelegate(Library.Error), "Update error", "Update function is temporarily unavailable.", Library.AlertType.Notification);
            }
        }

        ///<summary> Loading the authorization window </summary>
        void Success()
        {
            new Auth().Show();
            Hide();
        }

        ///<summary> When closing 2 </summary>
        void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        ///<summary> Drag windows </summary>
        void _MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch
            {

            }
        }
    }
}
