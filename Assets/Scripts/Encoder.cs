using System;
using UnityEngine;
using System.Runtime.InteropServices;


public class Encoder {
  private int width, height;
  private static Encoder encoder;
  private Byte[] yv12DataBuffer, encodedDataBuffer;

  [DllImport("NaturalVirtualInteractionVideoEncoder")]
  extern static int vpx_init(int width, int height);

  [DllImport("NaturalVirtualInteractionVideoEncoder")]
  extern static int vpx_encode(byte[] yv_frame, byte[] encoded, bool force_key_frame);

  [DllImport("NaturalVirtualInteractionVideoEncoder")]
  extern static void vpx_cleanup();

  public static Encoder getEncoder(int width, int height) {
    if (encoder == null || encoder.width != width || encoder.height != height) {
      Debug.Log("Creating new Encoder...");
      return encoder = new Encoder(width, height);
    }
    return encoder;
  }

  public byte[] processFrame(Color32[] frame) {
    // DateTime startTime;
    // double deltaTime;

    int encodedSize;
    //startTime = DateTime.Now;
    convertRGB2YUV12(frame, yv12DataBuffer); // TODO: Improve time

    // deltaTime = (DateTime.Now - startTime).TotalSeconds;
    // Debug.Log($"YUV Convert deltaTime: {deltaTime}");
    // startTime = DateTime.Now;

    encodedSize = vpx_encode(yv12DataBuffer, encodedDataBuffer, false);

    // deltaTime = (DateTime.Now - startTime).TotalSeconds;
    // Debug.Log($"Convert deltaTime: {deltaTime}");
    //startTime = DateTime.Now;

    Byte[] encodedData = new byte[encodedSize];
    Array.Copy(encodedDataBuffer, 0, encodedData, 0, encodedSize);

    //deltaTime = (DateTime.Now - startTime).TotalSeconds;
    //Debug.Log($"Copy array deltaTime: {deltaTime}");

    return encodedData;
  }

  private Encoder(int width, int height) {
    this.width = width;
    this.height = height;
    yv12DataBuffer = new Byte[32 * width * height];
    encodedDataBuffer = new Byte[32 * width * height];
    vpx_init(width, height);
  }

  ~Encoder() {
    vpx_cleanup();
  }

  private void convertRGB2YUV12(Color32[] input, byte[] output) // It also flips the image vertically
  {
    int size = width * height;

    for (int i = 0; i < height; i++) {
      for (int j = 0; j < width; j++) {
        double R = (input[i * width + j].r);
        double G = (input[i * width + j].g);
        double B = (input[i * width + j].b);

        output[size - 1 - (width * i + j)] = Convert.ToByte(R * .299000 + G * .587000 + B * .114000); // Y
      }
    }

    for (int i = 0; i < height / 2; i++) {
      for (int j = 0; j < width / 2; j++) {
        double R = ((input[2 * i * width + 2 * j].r) + (input[(2 * i + 1) * width + 2 * j].r) + (input[2 * i * width + 2 * j + 1].r) + (input[(2 * i + 1) * width + 2 * j + 1].r)) / 4.0;
        double G = ((input[2 * i * width + 2 * j].g) + (input[(2 * i + 1) * width + 2 * j].g) + (input[2 * i * width + 2 * j + 1].g) + (input[(2 * i + 1) * width + 2 * j + 1].g)) / 4.0;
        double B = ((input[2 * i * width + 2 * j].b) + (input[(2 * i + 1) * width + 2 * j].b) + (input[2 * i * width + 2 * j + 1].b) + (input[(2 * i + 1) * width + 2 * j + 1].b)) / 4.0;

        output[5 * size / 4 - 1 - (i * width / 2 + j)] = Convert.ToByte(R * .500000 + G * -.418688 + B * -.081312 + 128); // V
        output[6 * size / 4 - 1 - (i * width / 2 + j)] = Convert.ToByte(R * -.168736 + G * -.331264 + B * .500000 + 128); // U
      }
    }
  }

}
