using System;
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

  private void drawCurrentHandOnProjectionScreen(Vector2[] points) {
    Texture2D backgroundTexture = projectionScreen 
      .GetComponent<Renderer>()
      .material.mainTexture as Texture2D;

    for(int id = 0; id < points.Length; id++) {
      if(isDuplicate(id)) {
        continue;
      }

      var parentId = getParentId(id); 
      // For some reason the points are mirrored, this is  a hack to fix it
      points[id]= new Vector2(points[id].x, frameProvider.HEIGHT - points[id].y);

      TextureUtils.drawCircle(
        backgroundTexture,
        points[id],
        3,
        id <= 5 ? new Color(0, 255, 0) : new Color(255, 0, 0)
      );

      TextureUtils.drawLine(
        backgroundTexture,
        points[id],
        points[parentId],
        new Color(255, 0, 0)
      );

    }

    backgroundTexture.Apply();
  }
  private void renderCurrentHand(Vector3[] points)
  {
    lastHandObjects.ForEach(Destroy);
    lastHandObjects.Clear();

    var root = ModelUtils.createSphere(points[0]);
    lastHandObjects.Add(root);

    for (int id = 0; id < points.Length; id++)
    {
      if(isDuplicate(id)) {
        continue;
      }

      int parentId = getParentId(id);
      var bone = ModelUtils.createBone(points[parentId], points[id]);

      lastHandObjects.Add(bone);
    }
  }

  ~RenderHand() {
      lastHandObjects.ForEach(Destroy);
      lastHandObjects.Clear();
  }

  private bool isDuplicate(int id) {
    switch(id) {
      case 1:
      case 6:
      case 11:
      case 16:
      case 21:
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