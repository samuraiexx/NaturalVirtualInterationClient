using System;
using UnityEngine;
using System.IO;

public class TestFrameProvider : FrameProvider {
  private int frameCounter = 0;
  private const int FRAMES = 598;

  public TestFrameProvider(int width, int height) : base(width, height) { }

  public override Color32[] getFrame() {
    var image = File.ReadAllBytes($"{Application.dataPath}//hands//hand{frameCounter}.png");

    var texture = new Texture2D(0, 0);
    texture.LoadImage(image);

    frameCounter = (frameCounter + 1) % FRAMES;

    return texture.GetPixels32();
  }
}