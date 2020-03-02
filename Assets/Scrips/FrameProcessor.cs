using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class FrameProcessor
{
  private string ipAddress;
  private const int JOINTS_3D = 26;
  private const int JOINTS_2D = 26;
  private Socket sender;

public FrameProcessor(string ipAddressString) {
    IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
    IPAddress ipAddress = IPAddress.Parse(ipAddressString);
    IPEndPoint remoteEP = new IPEndPoint(ipAddress, 2020);

    sender = new Socket(
      ipAddress.AddressFamily,
      SocketType.Stream,
      ProtocolType.Tcp
    );

    try {
      sender.Connect(remoteEP);

      Debug.Log($"Socket connected to {sender.RemoteEndPoint.ToString()}");
    } catch (Exception ex) {
      Debug.Log($"Error: {ex.ToString()}");
    }
}
  public Tuple<float[], float[]> getHandPoints3dAnd2d(byte[] img) {
    int size = img.Length;
    byte[] msg = new byte[4 + img.Length];

    msg[3] = (byte)(size >> 24);
    msg[2] = (byte)(size >> 16);
    msg[1] = (byte)(size >> 8);
    msg[0] = (byte)size;
    Buffer.BlockCopy(img, 0, msg, 4, img.Length);

    byte[] bytes = new byte[4*(3*JOINTS_3D + 2*JOINTS_2D)];

    int bytesSent = sender.Send(msg);
    int bytesRec = sender.Receive(bytes);

    float[] points3d = new float[3*JOINTS_3D];
    float[] points2d = new float[2*JOINTS_2D];

    const int JOINTS_3D_SIZE = 4*3*JOINTS_3D;
    const int JOINTS_2D_SIZE = 4*2*JOINTS_2D;

    Buffer.BlockCopy(bytes, 0, points3d, 0, JOINTS_3D_SIZE);
    Buffer.BlockCopy(bytes, JOINTS_3D_SIZE, points2d, 0, JOINTS_2D_SIZE);

    return new Tuple<float[], float[]>(points3d, points2d);
  }

  ~FrameProcessor() {
    sender.Shutdown(SocketShutdown.Both);
    sender.Close();
  }
}
