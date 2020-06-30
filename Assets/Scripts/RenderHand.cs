using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class RenderHand : MonoBehaviour {
  public GameObject projectionScreen;
  public GameObject PoseDataProviderObject;
  private PoseDataProvider poseDataProvider;
  private List<GameObject> lastHandObjects = new List<GameObject>();
  private GameObject mainCamera;
  private GameObject hand;
  /** The pulse joint is not the root node, so we store it
  /** in another GameObject **/
  private GameObject handPulseJoint;

  void Start() {
    mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    hand = ModelUtils.createHand();
    handPulseJoint = hand.transform.GetChild(0).GetChild(0).gameObject;

    poseDataProvider = PoseDataProviderObject.GetComponent<PoseDataProvider>();

    poseDataProvider.onUpdatePose.Add(drawCurrentHandOnProjectionScreen);
    poseDataProvider.onUpdatePose.Add(renderCurrentHand);

    // grabber = new Grabber(); <= Move to a different Object
  }

  private void drawCurrentHandOnProjectionScreen() {
    var poseData = poseDataProvider.poseData;
    if (poseData == null || poseData.lostTrack) {
      return;
    }

    Vector2[] points = new Vector2[poseData.points2d.Length];
    Array.Copy(poseData.points2d, points, poseData.points2d.Length);

    Texture2D texture = new Texture2D(poseDataProvider.width, poseDataProvider.height);

    texture.SetPixels32(poseDataProvider.frame);

    for (int id = 0; id < points.Length; id++) {
      var parentId = getParentId(id);
      // For some reason the points are mirrored, this is  a hack to fix it
      points[id] = new Vector2(points[id].x, poseDataProvider.height - points[id].y);

      TextureUtils.drawCircle(
        texture,
        points[id],
        3,
        id <= 5 ? new Color(0, 255, 0) : new Color(255, 0, 0)
      );

      TextureUtils.drawLine(
        texture,
        points[id],
        points[parentId],
        new Color(255, 0, 0)
      );
    }

    texture.Apply();

    projectionScreen
      .GetComponent<Renderer>()
      .material
      .mainTexture = texture;
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
    moveJoint(joint, points[id], points[getParentId(id)]);
    for (int i = 0; i < joint.transform.childCount; i++) {
      GameObject child = joint.transform.GetChild(i).gameObject;
      updateJointsPositions(child, points, id + 1);
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

  private void renderCurrentHand() {
    var poseData = poseDataProvider.poseData;
    if (poseData == null || poseData.lostTrack) {
      return;
    }
    var points = poseData.points3d;

    lastHandObjects.ForEach(Destroy);
    lastHandObjects.Clear();

    var root = ModelUtils.createSphere(points[0], mainCamera.transform);

    lastHandObjects.Add(root);

    for (int id = 0; id < points.Length; id++) {
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

  private int getParentId(int id) {
    switch (id) {
      case 0:
      case 1:
      case 5:
      case 9:
      case 13:
      case 17:
        return 0;
      default:
        return id - 1;
    }
  }
}