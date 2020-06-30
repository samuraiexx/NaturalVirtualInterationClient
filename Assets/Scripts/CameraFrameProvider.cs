using System.Collections.Generic;
using UnityEngine;

public class CameraFrameProvider : FrameProvider {
  private WebCamTexture backCam;
  private float startTime;

  public CameraFrameProvider(int width, int height, bool useRemoteCamera = false) : base(width, height) {
    startTime = Time.time;

    if (useRemoteCamera) {
      return;
    }

    backCam = new WebCamTexture(width, height, FPS);
    backCam.Play();
  }

  public override Color32[] getFrame() {
    if (Time.time - startTime > 2 && backCam == null) {
      backCam = new WebCamTexture("Remote Camera 0", width, height, FPS);
      backCam.Play();
    }

    return backCam.GetPixels32();
  }
}
