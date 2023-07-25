using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing.Imaging;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Net.Sockets;
using System.Net;

namespace ScreenShotNetwork;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    Socket client = new Socket(
    AddressFamily.InterNetwork,
    SocketType.Dgram,
    ProtocolType.Udp
    );
    IPAddress ip = IPAddress.Parse("127.0.0.1");

    IPEndPoint connectEP;

    EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
    public MainWindow()
    {
        InitializeComponent();
        connectEP = new IPEndPoint(ip, 27001);
        DataContext = this;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {

        var sendingBytes = Encoding.Default.GetBytes("capture screenshot");

        client.SendTo(sendingBytes, connectEP);

        Task.Run(() =>
        {
            List<byte> receivedBytes = new List<byte>();
            int bytesReceived = 0;
            bool isEndOfMessage = false;

            while (!isEndOfMessage)
            {
                byte[] buffer = new byte[500];

                try
                {
                    bytesReceived = client.ReceiveFrom(buffer, ref endPoint);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Error receiving data: {ex.Message}");
                    break;
                }

                if (bytesReceived > 0)
                {
                    receivedBytes.AddRange(buffer.Take(bytesReceived));

                    if (bytesReceived < 500)
                        isEndOfMessage = true;
                }
            }

            Bitmap bitmap = ByteArrayToBitmap(receivedBytes.ToArray());
            //BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
            //    bitmap.GetHbitmap(),
            //    IntPtr.Zero,
            //    Int32Rect.Empty,
            //    BitmapSizeOptions.FromEmptyOptions()
            //);
            Dispatcher.Invoke(() =>
            {
                BitmapImage bitmapImage = ByteArrayToImageSource(receivedBytes.ToArray());
                ImageBox.Source = bitmapImage;
            });
        });

    }

    private BitmapImage ByteArrayToImageSource(byte[] byteArray)
    {
        try
        {
            MemoryStream ms = new MemoryStream(byteArray);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = ms;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return image;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error converting Bitmap to BitmapImage: {ex.Message}");
            return null!;
        }
    }


    private BitmapImage BitmapToImageSource(Bitmap bitmap)
    {
        using (MemoryStream memory = new MemoryStream())
        {
            try
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error converting Bitmap to BitmapImage: {ex.Message}");
                return null!;
            }
        }
    }

    private Bitmap ByteArrayToBitmap(byte[] byteArray)
    {
        using (MemoryStream ms = new MemoryStream(byteArray))
        {
            return new Bitmap(ms);
        }
    }
}
