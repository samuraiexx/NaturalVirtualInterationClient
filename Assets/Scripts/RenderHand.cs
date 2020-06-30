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
  private float[] latestRotation;

  void Start() {
    mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    hand = ModelUtils.createHand();
    handPulseJoint = hand.transform.GetChild(0).GetChild(0).gameObject;
    latestRotation = new float[24];

    poseDataProvider = PoseDataProviderObject.GetComponent<PoseDataProvider>();

    poseDataProvider.onUpdatePose.Add(drawCurrentHandOnProjectionScreen);
    poseDataProvider.onUpdatePose.Add(updateHand);

    // grabber = new Grabber(); <= Move to a different Object
  }

  private void drawCurrentHandOnProjectionScreen() {
    var poseData = poseDataProvider.poseData;
    Texture2D texture = new Texture2D(poseDataProvider.width, poseDataProvider.height);
    texture.SetPixels32(poseDataProvider.frame);

    if (poseData.lostTrack) {
      texture.Apply();
      projectionScreen
        .GetComponent<Renderer>()
        .material
        .mainTexture = texture;
      return;
    }

    Vector2[] points = new Vector2[poseData.points2d.Length];
    Array.Copy(poseData.points2d, points, poseData.points2d.Length);


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

  private void updateHand() {
    var poseData = poseDataProvider.poseData;
    if (poseData.lostTrack) {
      return;
    }
    var points = poseData.points3d;

    lastHandObjects.ForEach(Destroy);
    lastHandObjects.Clear();

    var root = ModelUtils.createSphere(points[0], mainCamera.transform);

    lastHandObjects.Add(root);

    handPulseJoint.transform.position = poseData.points3d[0];

    for (int i=1; i<=handPulseJoint.transform.childCount; i++) {
      rotateJointDfs(handPulseJoint.transform.GetChild(i-1), poseData.angles, 4*i);
    }

    for (int i=0; i<latestRotation.Length; i++) {
      latestRotation[i] = poseData.angles[i];
    }

    for (int id = 0; id < points.Length; id++) {
      int parentId = getParentId(id);
      var bone = ModelUtils.createBone(points[parentId], points[id], mainCamera.transform);

      lastHandObjects.Add(bone);
    }
  }

  private void rotateJointDfs(Transform joint, float[] angles, int id) {
    if (id % 4 == 0) {
      joint.RotateAround(joint.position, joint.right, Mathf.Rad2Deg*(angles[id] - latestRotation[id]));
      joint.RotateAround(joint.position, joint.forward, Mathf.Rad2Deg*(angles[id+1] - latestRotation[id+1]));

      rotateJointDfs(joint.GetChild(0), angles, id+2);
      return;
    }

    joint.RotateAround(joint.position, joint.right, Mathf.Rad2Deg*(angles[id] - latestRotation[id]));
    
    if (id % 4 == 3) return;
    rotateJointDfs(joint.GetChild(0), angles, id+1);
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