﻿using NAudio.Wave;
using System.Net;
using System.Windows.Threading;

namespace Chat_Client
{
    public static class Library
    {
        public const string Version = "2.0";

        public delegate void Error_asyncDelegate(string title, string description, AlertType type);

        public enum AlertType
        {
            Error,
            Notification
        }

        ///<summary>Sampling frequency </summary>
        public const int Rate = 48000; //8000
        ///<summary> Sample width </summary>
        public const byte Sample = 16; //16
        ///<summary> 1 channel - mono </summary>
        public const byte Channels = 1; //1
        ///<summary> Delay </summary>
        public const byte Latency = 50; //50

        public static WaveFormat SoundFormat = new WaveFormat(Rate, Sample, Channels);

        public static void Error(string title, string description, AlertType type = AlertType.Error)
        {
            new ErrorWindow(title, description, type).ShowDialog();
            //MessageBox.Show(title + Environment.NewLine + description);
        }
    }
}
