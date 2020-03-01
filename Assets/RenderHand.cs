using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderHand : MonoBehaviour
{
  public GameObject background;
  private TextReader reader;
  private List<GameObject> lastSpheres = new List<GameObject>();
  private Socket sender;
  private WebCamTexture backCam;
  private const int JOINTS_3D = 26;
  private const int JOINTS_2D = 26;
  private float startTime;

  void Start()
  {
    startTime = Time.time;
    setupCamera();
    setupConnection();
  }

  private void Update() {
    if(Time.time - startTime < 3) { // It's Necessary to wait for camera loading
      return;
    }

    byte[] img = getImageFromCamera();
    var (points3d, points2d) = getHandPoints3dAnd2d(img);
    renderCurrentHand(points3d);
    drawCurrentHandOnBackground(points2d);
  }

  private void drawCurrentHandOnBackground(float[] points) {
    Texture2D backgroundTexture = background
      .GetComponent<Renderer>()
      .material.mainTexture as Texture2D;

    for(int i = 0; i < points.Length; i+=2) {
      Debug.Log($">>>>>>> {i} : ({points[i]}, {points[i + 1]})");
      DrawCircle(
        backgroundTexture,
        (int)points[i],
        backgroundTexture.height - (int)points[i + 1],
        3,
        new Color(255, 0, 0)
      );
    }
  }
  private void renderCurrentHand(float[] points)
  {
    lastSpheres.ForEach(Destroy);
    lastSpheres.Clear();

    for (int i = 0; i < points.Length; i += 3)
    {
      GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      sphere.transform.localScale = new Vector3(10, 10, 10);

      if(points[i] == 0) return;
      sphere.transform.position = new Vector3(
        points[i],
        points[i + 1],
        points[i + 2]
      );

      sphere.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
      lastSpheres.Add(sphere);
    }
  }

  private void setupCamera() {
    backCam = new WebCamTexture(640, 480, 30);
    backCam.Play();
  }

  private byte[] getImageFromCamera() {
    Texture2D snap = new Texture2D(
      backCam.width,
      backCam.height,
      TextureFormat.RGB24,
      false
    );

    snap.SetPixels(backCam.GetPixels());
    snap.Apply();

    var prevTexture = background.GetComponent<Renderer>().material.mainTexture;

    var encondedImage = snap.EncodeToPNG();
    background.GetComponent<Renderer>().material.mainTexture = snap;

    Destroy(prevTexture);
    return encondedImage;
  }

  private void setupConnection() {
    IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
    IPAddress ipAddress = IPAddress.Parse("192.168.42.31");
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

  private Tuple<float[], float[]> getHandPoints3dAnd2d(byte[] img) {
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

  ~RenderHand() {
      lastSpheres.ForEach(Destroy);
      lastSpheres.Clear();

      sender.Shutdown(SocketShutdown.Both);
      sender.Close();
  }

  private void DrawCircle(Texture2D tex, int cx, int cy, int r, Color col) {
    int x, y, px, nx, py, ny, d;

    for (x = 0; x <= r; x++)
    {
      d = (int)Mathf.Round(Mathf.Sqrt(r * r - x * x));
      for (y = 0; y <= d; y++)
      {
        px = cx + x;
        nx = cx - x;
        py = cy + y;
        ny = cy - y;

        tex.SetPixel(px, py, col);
        tex.SetPixel(nx, py, col);

        tex.SetPixel(px, ny, col);
        tex.SetPixel(nx, ny, col);
      }
    }

    tex.Apply();
  }
}