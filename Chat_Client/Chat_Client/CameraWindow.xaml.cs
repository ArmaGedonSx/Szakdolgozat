#region Using
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using AForge.Video;
using AForge.Video.DirectShow;
using Color = System.Drawing.Color;
using System.Reflection;
using System.IO;

#endregion

namespace Chat_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class CameraWindow : Window, INotifyPropertyChanged
    {
        #region Public properties

        public ObservableCollection<FilterInfo> VideoDevices { get; set; }

        public FilterInfo CurrentDevice
        {
            get { return _currentDevice; }
            set { _currentDevice = value; this.OnPropertyChanged("CurrentDevice"); }
        }
        private FilterInfo _currentDevice;

        /// <summary>
        /// Displays the original image taken from the camera
        /// </summary>
        public bool Original
        {
            get { return _original; }
            set { _original = value; this.OnPropertyChanged("Original"); }
        }
        private bool _original;

        /// <summary>
        /// Displays the image taken from the camera with a grayscale filter applied
        /// </summary>
        public bool Grayscaled
        {
            get { return _grayscale; }
            set { _grayscale = value; this.OnPropertyChanged("Grayscaled"); }
        }
        private bool _grayscale;

        /// <summary>
        /// Displays the image taken from the camera with a threshold filter applied
        /// </summary>
        public bool Thresholded
        {
            get { return _thresholded; }
            set { _thresholded = value; this.OnPropertyChanged("Thresholded"); }
        }
        private bool _thresholded;

        /// <summary>
        /// Threshold of the thresholding filter
        /// </summary>
        public int Threshold
        {
            get { return _threshold; }
            set { _threshold = value; this.OnPropertyChanged("Threshold"); }
        }
        private int _threshold;

        /// <summary>
        /// Color picker: red channel
        /// </summary>
        public int Red
        {
            get { return _red; }
            set { _red = value; this.OnPropertyChanged("Red"); }
        }
        private int _red;

        /// <summary>
        /// Color picker: blue channel
        /// </summary>
        public int Blue
        {
            get { return _blue; }
            set { _blue = value; this.OnPropertyChanged("Blue"); }
        }
        private int _blue;

        /// <summary>
        /// Color picker: green channel
        /// </summary>
        public int Green
        {
            get { return _green; }
            set { _green = value; this.OnPropertyChanged("Green"); }
        }
        private int _green;

        /// <summary>
        /// True if the user hit the color picking button and is choosing a color
        /// </summary>
        public bool PickingColor
        {
            get { return _pickingColor; }
            set { _pickingColor = value; this.OnPropertyChanged("PickingColor"); }
        }
        private bool _pickingColor;

        /// <summary>
        /// Displays the image with a Euclidean color filter applied
        /// </summary>
        public bool ColorFiltered
        {
            get { return _colorFiltered; }
            set { _colorFiltered = value; this.OnPropertyChanged("ColorFiltered"); }
        }
        private bool _colorFiltered;

        /// <summary>
        /// Displays the image inverted color filter applied
        /// </summary>
        public bool Inverted
        {
            get { return _inverted; }
            set { _inverted = value; this.OnPropertyChanged("Inverted"); }
        }
        private bool _inverted;

        /// <summary>
        /// Radius of the euclidean color filter
        /// </summary>
        public short Radius
        {
            get { return _radius; }
            set { _radius = value; this.OnPropertyChanged("Radius"); }
        }
        private short _radius;


        #endregion

        #region Private fields

        /// <summary>
        /// The camera which we use to acquire bitmaps
        /// </summary>
        private IVideoSource _videoSource;

        #endregion

        #region Constructor

        public CameraWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            GetVideoDevices();
            Threshold = 127;
            Radius = 60;
            Original = true;
            this.Closing += CameraWindow_Closing;
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// On closing the application stops the camera if active and restore the cursor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CameraWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Cursor = Cursors.Arrow;
            StopCamera();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            StartCamera();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }

        /// <summary>
        /// Frame received callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void video_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            try
            {
                BitmapImage bi;
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    // Here we choose what image to display
                    if (ColorFiltered)
                    {
                        new EuclideanColorFiltering(new AForge.Imaging.RGB((byte)Red, (byte)Green, (byte)Blue), Radius).ApplyInPlace(bitmap);
                    }
                    if (Grayscaled)
                    {
                        using (var grayscaledBitmap = Grayscale.CommonAlgorithms.BT709.Apply(bitmap))
                        {
                            bi = grayscaledBitmap.ToBitmapImage();
                        }
                    }
                    else if (Thresholded)
                    {
                        using (var grayscaledBitmap = Grayscale.CommonAlgorithms.BT709.Apply(bitmap))
                        using (var thresholdedBitmap = new Threshold(Threshold).Apply(grayscaledBitmap))
                        {
                            if (Inverted)
                            {
                                new Invert().ApplyInPlace(thresholdedBitmap);
                            }
                            bi = thresholdedBitmap.ToBitmapImage();
                        }
                    }
                    else // original
                    {
                        var corners = FindCorners(bitmap);
                        if (corners.Any())
                        {
                            PaintCorners(corners, bitmap);
                        }
                        bi = bitmap.ToBitmapImage();
                    }
                }
                bi.Freeze(); // avoid cross thread operations and prevents leaks
                Dispatcher.BeginInvoke(new ThreadStart(delegate { videoPlayer.Source = bi; }));
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopCamera();
            }
        }


        /// <summary>
        /// Handles the mouse enter event when a user is picking a color, to display a special cursor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void videoPlayer_MouseEnter(object sender, MouseEventArgs e)
        {
            if (PickingColor)
            {
                var cursor = ((FrameworkElement)App.Current.Resources["CursorPicker"]).Cursor;
                Cursor = cursor;
            }
        }

        /// <summary>
        /// Restores the cursor when user is leaving the image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void videoPlayer_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Handles the click when the user picks a color from the image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void videoPlayer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (PickingColor)
            {
                var clickPoint = e.GetPosition(videoPlayer);
                var image = videoPlayer.Source as BitmapImage;
                // color finding algorithm taken from:
                // http://stackoverflow.com/questions/1176910/finding-specific-pixel-colors-of-a-bitmapimage
                int stride = image.PixelWidth * 4;
                int size = image.PixelHeight * stride;
                byte[] pixels = new byte[size];
                image.CopyPixels(pixels, stride, 0);
                int index = ((int)clickPoint.Y) * stride + 4 * ((int)clickPoint.X);
                Blue = pixels[index];
                Green = pixels[index + 1];
                Red = pixels[index + 2];
                PickingColor = false;
                Cursor = Cursors.Arrow;
            }
        }

        #endregion

        /// <summary>
        /// We process the image applying all the filters, then we filter the blobs and we find the border or the biggest one
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private List<IntPoint> FindCorners(Bitmap bitmap)
        {
            List<IntPoint> corners = new List<IntPoint>();
            using (var clone = bitmap.Clone() as Bitmap)
            {
                new EuclideanColorFiltering(new AForge.Imaging.RGB((byte)Red, (byte)Green, (byte)Blue), Radius).ApplyInPlace(clone);
                using (var grayscaledBitmap = Grayscale.CommonAlgorithms.BT709.Apply(clone))
                {
                    new Threshold(Threshold).ApplyInPlace(grayscaledBitmap);
                    if (Inverted)
                    {
                        new Invert().ApplyInPlace(grayscaledBitmap);
                    }
                    BlobCounter blobCounter = new BlobCounter();
                    blobCounter.FilterBlobs = true;
                    blobCounter.MinWidth = 50;
                    blobCounter.MinHeight = 50;
                    blobCounter.ObjectsOrder = ObjectsOrder.Size;
                    blobCounter.ProcessImage(grayscaledBitmap);
                    Blob[] blobs = blobCounter.GetObjectsInformation();
                    // create convex hull searching algorithm
                    GrahamConvexHull hullFinder = new GrahamConvexHull();
                    for (int i = 0, n = blobs.Length; i < n; i++)
                    {
                        List<IntPoint> leftPoints, rightPoints;
                        List<IntPoint> edgePoints = new List<IntPoint>();
                        // get blob's edge points
                        blobCounter.GetBlobsLeftAndRightEdges(blobs[i], out leftPoints, out rightPoints);
                        edgePoints.AddRange(leftPoints);
                        edgePoints.AddRange(rightPoints);
                        // blob's convex hull
                        corners = hullFinder.FindHull(edgePoints);
                    }
                }
            }
            return corners;
        }

        /// <summary>
        /// Given a list of points, draws blue lines that connects the points on a given bitmap
        /// </summary>
        /// <param name="corners"></param>
        /// <param name="bitmap"></param>
        void PaintCorners(List<IntPoint> corners, Bitmap bitmap)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            using (System.Drawing.Pen bluePen = new System.Drawing.Pen(Color.Blue, 2))
            {
                g.DrawPolygon(bluePen, ToPointsArray(corners));
            }
        }

        void GetVideoDevices()
        {
            VideoDevices = new ObservableCollection<FilterInfo>();
            foreach (FilterInfo filterInfo in new FilterInfoCollection(FilterCategory.VideoInputDevice))
            {
                VideoDevices.Add(filterInfo);
            }
            if (VideoDevices.Any())
            {
                CurrentDevice = VideoDevices[0];
            }
            else
            {
                MessageBox.Show("No video sources found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        bool cameraStarted = false;
        void StartCamera()
        {
            if (CurrentDevice != null)
            {
                cameraStarted = true;
                _videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                _videoSource.NewFrame += video_NewFrame;
                _videoSource.Start();
            }
        }

        void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= new NewFrameEventHandler(video_NewFrame);
            }
        }

        void Camera_Picture_Capture(object sender, EventArgs e)
        {
            try
            {
                // Check if the camera is started
                if (cameraStarted == false)
                {
                    MessageBox.Show("The camera is not started. Please start the camera before capturing.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Capture an image from the camera feed
                BitmapImage currentImage = CreateCameraImage();

                // Create a new SaveCameraPicture window and pass the captured image to it
                SaveCameraPicture cameraPictureWindow = new SaveCameraPicture(currentImage);
                cameraPictureWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while capturing and saving the image: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        BitmapImage CreateCameraImage()
        {
            // Capture and return the image from the camera feed
            // This is where you create the camera image
            // You can use the currently displayed image or the current state of the camera
            // Here's an example of how to use the current source image of the videoPlayer:
            return videoPlayer.Source as BitmapImage;
        }


        // Conver list of AForge.NET's points to array of .NET points
        private System.Drawing.Point[] ToPointsArray(List<IntPoint> points)
        {
            System.Drawing.Point[] array = new System.Drawing.Point[points.Count];

            for (int i = 0, n = points.Count; i < n; i++)
            {
                array[i] = new System.Drawing.Point(points[i].X, points[i].Y);
            }

            return array;
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }



        #endregion


        #region Style 
        bool FullScreen_Button_True = false;

        void OnWindowClosing(object sender, CancelEventArgs e)
        {
            CameraWindow_Closing(sender, e);
        }
        BitmapImage LoadBitmapFromResource(string Path)
        {
            Path = @"Images/" + Path;
            Assembly assembly = Assembly.GetCallingAssembly();
            if (Path[0] == '/')
            {
                Path = Path.Substring(1);
            }
            return new BitmapImage(new Uri(@"pack://application:,,,/" + assembly.GetName().Name + ";component/" + Path, UriKind.Absolute));
        }


        void Cut_Down_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        void Mouse_Enter_Cut_Down_Button(object sender, RoutedEventArgs e)
        {
            Cut_Down_Button_Image1.Source = LoadBitmapFromResource("Cut_Down_Button2.png");
        }

        void Mouse_Leave_Cut_Down_Button(object sender, RoutedEventArgs e)
        {
            Cut_Down_Button_Image1.Source = LoadBitmapFromResource("Cut_Down_Button1.png");
        }

        void Move_Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch
            {

            }
        }

        void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

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
        }

        void FullScreen_Button_Click(object sender, RoutedEventArgs e)
        {
            if (FullScreen_Button_True == false)
            {
                WindowState = WindowState.Maximized;
                FullScreen_Button_True = true;
            }
            else
            {
                WindowState = WindowState.Normal;
                FullScreen_Button_True = false;
            }
        }


        #endregion
    }


}

