using UnityEngine;

public abstract class FrameProvider {
  public GameObject projectionScreen;
  public int WIDTH { get; } = 640;
  public int HEIGHT { get; } = 480;
  protected const int FPS = 30;

  public abstract void getFrame(Color32[] frame);
}

