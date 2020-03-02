using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class RenderHand : MonoBehaviour
{
  FrameProcessor frameProcessor;
  FrameProvider frameProvider;
  private List<GameObject> lastHandObjects = new List<GameObject>();
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
    lastHandObjects.ForEach(Destroy);
    lastHandObjects.Clear();

    var root = ModelUtils.createSphere(new Vector3(points[0], points[1], points[2]));
    lastHandObjects.Add(root);

    for (int i = 3; i < points.Length; i += 3)
    {
      int id = i/3;
      int parentId = getParentId(id);

      if(isDuplicate(id)) {
        continue;
      }

      var bone = ModelUtils.createBone(
        new Vector3(
          points[3*parentId],
          points[3*parentId + 1],
          points[3*parentId + 2]
        ), 
        new Vector3(
          points[3*id],
          points[3*id + 1],
          points[3*id + 2]
        ) 
      );

      lastHandObjects.Add(bone);
    }
  }

  ~RenderHand() {
      lastHandObjects.ForEach(Destroy);
      lastHandObjects.Clear();
  }

  private bool isDuplicate(int id) {
    switch(id) {
      case 2:
      case 7:
      case 12:
      case 17:
      case 22:
        return true;
      default:
        return false;
    }
  }
  private int getParentId(int id) {
    switch(id) {
      case 0:
      case 1:
      case 2:
      case 6:
      case 7:
      case 11:
      case 12:
      case 16:
      case 17:
      case 21:
      case 22:
        return 0;
      default:
        return id - 1;
    }
  }

}