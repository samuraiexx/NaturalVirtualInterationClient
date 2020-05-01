using System.Collections.Generic;
using UnityEngine;

public class CameraFrameProvider : FrameProvider
{
  private WebCamTexture backCam;
  private float startTime;

  public CameraFrameProvider(GameObject projectionScreen, bool useRemoteCamera = false) {
    startTime = Time.time;
    this.projectionScreen = projectionScreen;

    if(useRemoteCamera){
      return;
    }

    backCam = new WebCamTexture(WIDTH, HEIGHT, FPS);
    backCam.Play();
  }

  public override byte[] getFrame() {
    if(Time.time - startTime > 2 && backCam == null) {
      backCam = new WebCamTexture("Remote Camera 0", WIDTH, HEIGHT, FPS);
      backCam.Play();
    }

    if(Time.time - startTime < 3) { // It's Necessary to wait for camera loading
      return null;
    }

    Texture2D snap = new Texture2D(
      backCam.width,
      backCam.height,
      TextureFormat.RGB24,
      false
    );

    snap.SetPixels(backCam.GetPixels());
    snap.Apply();

    var encondedImage = snap.EncodeToPNG();
    projectionScreen.GetComponent<Renderer>().material.mainTexture = snap;

    return encondedImage;
  }
}
