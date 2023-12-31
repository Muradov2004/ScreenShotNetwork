﻿using System;
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
using Microsoft.Win32;

namespace ScreenShotNetwork;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    List<byte> imageBytes = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private void ScreenShot_Click(object sender, RoutedEventArgs e)
    {
        List<byte> receivedBytes = new List<byte>();

        var client = new Socket(
                                    AddressFamily.InterNetwork,
                                    SocketType.Dgram,
                                    ProtocolType.Udp
                                );
        var ip = IPAddress.Parse("127.0.0.1");

        var connectEP = new IPEndPoint(ip, 27001);

        EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

        var sendingBytes = Encoding.Default.GetBytes("capture screenshot");

        client.SendTo(sendingBytes, connectEP);

        Task.Run(() =>
        {
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

            Dispatcher.Invoke(() =>
            {
                BitmapImage bitmapImage = ByteArrayToImageSource(receivedBytes.ToArray());
                ImageBox.Source = bitmapImage;
                imageBytes.Clear();
                imageBytes.AddRange(receivedBytes);
            });
        });
        SaveBtn.IsEnabled = true;
    }

    private BitmapImage ByteArrayToImageSource(byte[] byteArray)
    {
        using (MemoryStream ms = new MemoryStream(byteArray))
        {
            try
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error converting Bytes to BitmapImage: {ex.Message}");
                return null!;
            }
        }
    }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog dialog = new SaveFileDialog();
        dialog.FileName = "image";
        dialog.Filter = "PNG Files(*.png) | *.png|JPEG Files(*.jpeg) | *.jpeg";

        if (dialog.ShowDialog() == true)
            File.WriteAllBytes(dialog.FileName, imageBytes.ToArray());

        SaveBtn.IsEnabled = false;
        ImageBox.Source = null;
    }
}
