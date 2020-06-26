using System.Collections.Generic;
using UnityEngine;

public class CameraFrameProvider : FrameProvider {
  private WebCamTexture backCam;
  private float startTime;

  public CameraFrameProvider(GameObject projectionScreen, bool useRemoteCamera = false) {
    startTime = Time.time;
    this.projectionScreen = projectionScreen;

    if (useRemoteCamera) {
      return;
    }

    backCam = new WebCamTexture(WIDTH, HEIGHT, FPS);
    backCam.Play();
  }

  public override void getFrame(Color32[] frame) {
    if (Time.time - startTime > 2 && backCam == null) {
      backCam = new WebCamTexture("Remote Camera 0", WIDTH, HEIGHT, FPS);
      backCam.Play();
    }

    Texture2D snap = new Texture2D(
      backCam.width,
      backCam.height,
      TextureFormat.RGBA32,
      false
    );

    backCam.GetPixels32(frame);
    snap.SetPixels32(frame);
    snap.Apply();

    projectionScreen.GetComponent<Renderer>().material.mainTexture = snap;

    return;
  }
}
