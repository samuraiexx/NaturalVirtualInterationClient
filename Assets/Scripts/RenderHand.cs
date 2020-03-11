using System;
using System.Collections.Generic;
using UnityEngine;

public class RenderHand : MonoBehaviour
{
  private FrameProcessor frameProcessor;
  private FrameProvider frameProvider;
  private List<GameObject> lastHandObjects = new List<GameObject>();
  private GameObject mainCamera;
  private GameObject hand;
  /** The pulse joint is not the root node, so we store it
  /** in another GameObject **/
  private GameObject handPulseJoint;
  public string IP_ADDRESS = "127.0.0.1";
  public GameObject projectionScreen;
  public bool debugMode = false;


  public Vector3[] currentJointsPosition;

  void Start()
  {
    mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    frameProcessor = new FrameProcessor(IP_ADDRESS);
    hand = ModelUtils.createHand();
    handPulseJoint = hand.transform.GetChild(0).GetChild(0).gameObject;

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
    currentJointsPosition = points3d;
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

  private void setPulseRotation(Vector3[] points) {
    Vector3 fingerJoint = handPulseJoint.transform.GetChild(1).position;
    Vector3 pinkyJoint = handPulseJoint.transform.GetChild(4).position;

    Vector3 currentPulseToFinger = fingerJoint - handPulseJoint.transform.position;
    Vector3 currentPulseToPinky = pinkyJoint - handPulseJoint.transform.position;

    Vector3 newPulseToFinger = points[7] - points[0];
    Vector3 newPulseToPinky = points[22] - points[0];

    handPulseJoint.transform.Rotate(
      Quaternion.FromToRotation(currentPulseToFinger, newPulseToFinger).eulerAngles
    );

    handPulseJoint.transform.Rotate(
      newPulseToFinger,
      Vector3.Angle(currentPulseToPinky, newPulseToPinky)
    );
  }

  private void updateJointsPositions(GameObject joint,
    Vector3[] points, int id = 0) {
    if (hand == null) {
      return;
    }
    if (isDuplicate(id)) {
      id++;
    }
    moveJoint(joint, points[id], points[getParentId(id)]);
    for(int i = 0; i < joint.transform.childCount; i++) {
      GameObject child = joint.transform.GetChild(i).gameObject;
      updateJointsPositions(child, points, id+1);
    }
  }

  private void moveJoint(GameObject joint, Vector3 orig, Vector3 dest) {
    var delta = dest - orig;

    joint.transform.SetPositionAndRotation(
      orig + delta,
      Quaternion.FromToRotation(
        new Vector3(0, 0, -1), delta
      )
    );
  }

  private void renderCurrentHand(Vector3[] points)
  {
    lastHandObjects.ForEach(Destroy);
    lastHandObjects.Clear();

    var root = ModelUtils.createSphere(points[0], mainCamera.transform);

    lastHandObjects.Add(root);

    for (int id = 0; id < points.Length; id++)
    {
      if(isDuplicate(id)) {
        continue;
      }

      int parentId = getParentId(id);
      var bone = ModelUtils.createBone(points[parentId], points[id], mainCamera.transform);

      lastHandObjects.Add(bone);
    }
  }

  ~RenderHand() {
      lastHandObjects.ForEach(Destroy);
      lastHandObjects.Clear();
      Destroy(hand);
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