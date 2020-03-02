using UnityEngine;

public class CameraFrameProvider : FrameProvider
{
  private WebCamTexture backCam;
  private float startTime;

  public CameraFrameProvider(GameObject projectionScreen) {
    startTime = Time.time;
    this.projectionScreen = projectionScreen;
    backCam = new WebCamTexture(WIDTH, HEIGHT, FPS);
    backCam.Play();
  }

  public override byte[] getFrame() {
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
