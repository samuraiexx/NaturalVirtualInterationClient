using System;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;

public class Encoder {

  [DllImport("NaturalVirtualInteractionVideoEncoder")]
  extern static int vpx_init(int width, int height);

  [DllImport("NaturalVirtualInteractionVideoEncoder")]
  extern static int vpx_encode(byte[] rgb_frame, byte[] encoded, bool force_key_frame);

  [DllImport("NaturalVirtualInteractionVideoEncoder")]
  extern static void vpx_cleanup();

  private int width, height;
  private static Encoder encoder;
  private Byte[] encodedDataBuffer, rgbDataBuffer;
  private bool encodingFrame = false;

  public static Encoder getEncoder(int width, int height) {
    if (encoder == null || encoder.width != width || encoder.height != height) {
      Debug.Log("Creating new Encoder...");
      return encoder = new Encoder(width, height);
    }
    return encoder;
  }

  public IEnumerator processFrame(Color32[] frame) {
    yield return new WaitUntil(() => encodingFrame == false);

    encodingFrame = true;

    for (int i = 0; i < height; i++) {
      int lineOffset = 3 * width * height - 3 * width * (i + 1);
      for (int j = 0; j < width; j++) {
        rgbDataBuffer[lineOffset + 3 * j] = frame[width * i + j].r;
        rgbDataBuffer[lineOffset + 3 * j + 1] = frame[width * i + j].g;
        rgbDataBuffer[lineOffset + 3 * j + 2] = frame[width * i + j].b;
      }
    }

    // DateTime startTime = DateTime.Now;
    int encodedSize = vpx_encode(rgbDataBuffer, encodedDataBuffer, false);

    Byte[] encodedData = new byte[encodedSize];
    Array.Copy(encodedDataBuffer, 0, encodedData, 0, encodedSize);

    // double deltaTime = (DateTime.Now - startTime).TotalSeconds;
    // Debug.Log($"Copy array deltaTime: {deltaTime}");

    yield return encodedData;
    encodingFrame = false;
  }

  private Encoder(int width, int height) {
    this.width = width;
    this.height = height;
    encodedDataBuffer = new Byte[32 * width * height];
    rgbDataBuffer = new Byte[3 * width * height];
    vpx_init(width, height);
  }

  ~Encoder() {
    vpx_cleanup();
  }
}
