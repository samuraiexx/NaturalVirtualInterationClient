using System;
using UnityEngine;
using System.IO;

public class TestFrameProvider : FrameProvider {
  private int frameCounter = 0;
  private const int FRAMES = 598;

  public TestFrameProvider(int width, int height) : base(width, height) { }

  public override void getFrame(Color32[] frame) {
    var image = File.ReadAllBytes($"{Application.dataPath}//hands//hand{frameCounter}.png");

    var texture = new Texture2D(0, 0);
    texture.LoadImage(image);

    frameCounter = (frameCounter + 1) % FRAMES;

    var tmpFrame = texture.GetPixels32();
    Array.Copy(tmpFrame, frame, tmpFrame.Length);
  }
}