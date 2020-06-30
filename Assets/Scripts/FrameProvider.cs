using UnityEngine;

public abstract class FrameProvider {
  protected int width;
  protected int height;
  protected const int FPS = 30;

  public abstract Color32[] getFrame();
  public FrameProvider(int width, int height) {
    this.width = width;
    this.height = height;
  }
}

