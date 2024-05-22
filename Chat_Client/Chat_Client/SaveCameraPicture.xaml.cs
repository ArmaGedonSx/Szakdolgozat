using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Chat_Client
{
    /// <summary>
    /// Interaction logic for SaveCameraPicture.xaml
    /// </summary>
    public partial class SaveCameraPicture : Window
    {
        private BitmapImage _currentImage;

        public SaveCameraPicture(BitmapImage currentImage)
        {
            InitializeComponent();
            _currentImage = currentImage;
            imageCapture.Source = _currentImage;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a default file name with the current timestamp
                string defaultFileName = $"captured_image_{DateTime.Now:yyyyMMddHHmmss}.png";

                // Display a dialog for saving the image
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.Filter = "JPEG images (*.jpg)|*.jpg|PNG images (*.png)|*.png|BMP images (*.bmp)|*.bmp|All files (*.*)|*.*";
                dialog.Title = "Save Image";

                // Set the default file name with the current timestamp
                dialog.FileName = defaultFileName;

                if (dialog.ShowDialog() == true)
                {
                    // Get the selected file path
                    string filePath = dialog.FileName;

                    // Save the image to the selected file
                    SaveImageToFile(_currentImage, filePath);

                    // Show a success message to the user
                    MessageBox.Show("The image has been successfully saved!", "Image Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                // Show an error message if there's an exception during image saving
                MessageBox.Show("An error occurred while saving the image: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void SaveImageToFile(BitmapImage image, string filePath)
        {
            // Save the image to the file path provided
            // First, convert the BitmapImage object to a Bitmap object
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        bool FullScreen_Button_True = false;
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




    }
}
