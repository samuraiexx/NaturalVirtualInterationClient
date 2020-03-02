using UnityEngine;

public abstract class FrameProvider 
{
  public GameObject projectionScreen;
  public int WIDTH = 640;
  public int HEIGHT = 480;
  protected const int FPS = 30;

  public abstract byte[] getFrame();
}

