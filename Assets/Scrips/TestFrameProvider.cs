using UnityEngine;
using System.IO;

public class TestFrameProvider : FrameProvider
{
  private float startTime;
  private int frameCounter = 0;
  const int FRAMES = 256;

  public TestFrameProvider(GameObject projectionScreen) {
    startTime = Time.time;
    this.projectionScreen = projectionScreen;
  }

  public override byte[] getFrame() {
    var frame = File.ReadAllBytes($"{Application.dataPath}//hands//hand{frameCounter}.png");

    var texture = new Texture2D(0, 0);
    texture.LoadImage(frame);
    projectionScreen.GetComponent<Renderer>().material.mainTexture = texture;

    frameCounter = (frameCounter + 1)%FRAMES;
    return frame; 
  }
}