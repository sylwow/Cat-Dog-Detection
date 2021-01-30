using AForge.Video;
using AForge.Video.DirectShow;
using Camera_RecordML.Model;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConsumeModel model = new ConsumeModel();
        private int counter = 0;
        private int treshold = 10;
        VideoCaptureDevice LocalWebCam;
        public FilterInfoCollection LoaclWebCamsCollection;

        void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                System.Drawing.Image img = (Bitmap)eventArgs.Frame.Clone();

                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                bi.Freeze();
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    frameHolder.Source = bi;
                }));
                if (counter++ > treshold)
                {
                    counter = 0;
                    Dispatcher.BeginInvoke(new ThreadStart(delegate
                    {
                        UpdateCaptureSnapshotManifast(bi);
                    }));
                }
            }
            catch (Exception ex)
            {
            }
        }


        public void UpdateCaptureSnapshotManifast(BitmapImage image)
        {
            try
            {
                var path = $"./{counter}.jpg";
                Save(image, path);
                ModelInput sampleData = new ModelInput()
                {
                    ImageSource = path
                };

                // Make a single prediction on the sample data and print results
                var predictionResult = model.Predict(sampleData);

                if (predictionResult.Prediction == "Dog")
                {
                    classification.Text = "Pies";
                    classification.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    classification.Text = "Kot";
                    classification.Foreground = System.Windows.Media.Brushes.Red;
                }
                result.Source = image;
                File.Delete(path);
            }

            catch { }

        }
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoaclWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            LocalWebCam = new VideoCaptureDevice(LoaclWebCamsCollection[0].MonikerString);
            LocalWebCam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);

            LocalWebCam.Start();
        }
        public void Save(BitmapImage image, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
    }

}
