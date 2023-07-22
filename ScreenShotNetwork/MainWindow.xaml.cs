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
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {

        var sendingBytes = Encoding.Default.GetBytes("capture screenshot");

        client.SendTo(sendingBytes, connectEP);

        var recievingBytes = new byte[ushort.MaxValue];
        var len = client.ReceiveFrom(recievingBytes, ref endPoint);
    }

    private Bitmap ByteArrayToBitmap(byte[] byteArray)
    {
        using (MemoryStream ms = new MemoryStream(byteArray))
        {
            return new Bitmap(ms);
        }
    }
}
