using UnityEngine;

public abstract class FrameProvider {
  protected int width;
  protected int height;
  protected const int FPS = 30;

  public abstract void getFrame(Color32[] frame);
  public FrameProvider(int width, int height) {
    this.width = width;
    this.height = height;
  }
}

