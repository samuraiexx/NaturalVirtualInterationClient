using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class RenderHand : MonoBehaviour
{
  FrameProcessor frameProcessor;
  FrameProvider frameProvider;
  private List<GameObject> lastSpheres = new List<GameObject>();
  public string IP_ADDRESS = "127.0.0.1";
  public GameObject projectionScreen;
  public bool debugMode = false;

  void Start()
  {
    frameProcessor = new FrameProcessor(IP_ADDRESS);
    if(debugMode) {
      frameProvider = new TestFrameProvider(projectionScreen);
      return;
    }
    
    frameProvider = new CameraFrameProvider(projectionScreen);
  }

  private void Update() {
    byte[] img = frameProvider.getFrame();
    if(img == null) {
      return;
    }

    var (points3d, points2d) = frameProcessor.getHandPoints3dAnd2d(img);
    renderCurrentHand(points3d);
    drawCurrentHandOnProjectionScreen(points2d);
  }

  private void drawCurrentHandOnProjectionScreen(float[] points) {
    Texture2D backgroundTexture = projectionScreen 
      .GetComponent<Renderer>()
      .material.mainTexture as Texture2D;

    for(int i = 0; i < points.Length; i+=2) {
      TextureUtils.DrawCircle(
        backgroundTexture,
        (int)points[i],
        frameProvider.HEIGHT - (int)points[i + 1],
        3,
        new Color(255, 0, 0)
      );
    }
  }
  private void renderCurrentHand(float[] points)
  {
    lastSpheres.ForEach(Destroy);
    lastSpheres.Clear();

    for (int i = 0; i < points.Length; i += 3)
    {
      GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      sphere.transform.localScale = new Vector3(10, 10, 10);

      if(points[i] == 0) return;
      sphere.transform.position = new Vector3(
        points[i],
        points[i + 1],
        points[i + 2]
      );

      sphere.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
      lastSpheres.Add(sphere);
    }
  }

  ~RenderHand() {
      lastSpheres.ForEach(Destroy);
      lastSpheres.Clear();
  }
}