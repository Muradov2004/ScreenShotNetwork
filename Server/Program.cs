﻿using System.Drawing;
using System.Drawing.Imaging;
using System.Management;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Versioning;
using System.Text;





var server = new Socket(
    AddressFamily.InterNetwork,
    SocketType.Dgram,
    ProtocolType.Udp
    );

IPAddress.TryParse("127.0.0.1", out var ip);
var listenerEP = new IPEndPoint(ip, 27001);

server.Bind(listenerEP);

var encryptedMessage = new byte[ushort.MaxValue];

EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

while (true)
{
    var len = server.ReceiveFrom(encryptedMessage, ref endPoint);
    var message = Encoding.Default.GetString(encryptedMessage, 0, len);
    if (message.Contains("capture screenshot"))
    {
        server.SendTo(CaptureScreenShot(), endPoint);
    }
}

byte[] CaptureScreenShot()
{
    try
    {

        ManagementObjectSearcher mydisplayResolution = new ManagementObjectSearcher("SELECT CurrentHorizontalResolution, CurrentVerticalResolution FROM Win32_VideoController");

        int height = 0, width = 0;

        foreach (ManagementObject record in mydisplayResolution.Get())
        {
            height = Convert.ToInt32(record["CurrentHorizontalResolution"]);
            width = Convert.ToInt32(record["CurrentVerticalResolution"]);
        }

        using (Bitmap bitmap = new Bitmap(height, width))
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            }
            return BitmapToByteArray(bitmap);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        var bytes = Encoding.Default.GetBytes("Error capturing the screenshot: " + ex.Message);
        return bytes;
    }
}
byte[] BitmapToByteArray(Bitmap bitmap)
{
    using (MemoryStream ms = new MemoryStream())
    {
        bitmap.Save(ms, ImageFormat.Jpeg);
        return ms.ToArray();
    }
}