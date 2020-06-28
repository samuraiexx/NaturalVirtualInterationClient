using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public enum RunModes { FAKE_FRAMES, UNITY_REMOTE, PRODUCTION };
public class RenderHand : MonoBehaviour {
  public string ipAddress = "127.0.0.1";
  public int port = 3030;
  public GameObject projectionScreen;
  public RunModes runMode = RunModes.PRODUCTION;
  public Vector3[] currentJointsPosition {
    get;
    private set;
  }

  private FrameProcessor frameProcessor;
  private FrameProvider frameProvider;
  private Grabber grabber;
  private List<GameObject> lastHandObjects = new List<GameObject>();
  private GameObject mainCamera;
  private GameObject hand;
  /** The pulse joint is not the root node, so we store it
  /** in another GameObject **/
  private GameObject handPulseJoint;
  private DateTime lastFrameProcessTime;
  private double processMinDeltaTime = 0.030;
  private Color32[] frame;
  private PoseData poseData = null;
  private bool destroyed = false;

  void Start() {
    mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    hand = ModelUtils.createHand();
    handPulseJoint = hand.transform.GetChild(0).GetChild(0).gameObject;

    grabber = new Grabber();

    switch (runMode) {
      case RunModes.FAKE_FRAMES:
        frameProvider = new TestFrameProvider(projectionScreen);
        break;
      case RunModes.UNITY_REMOTE:
        frameProvider = new CameraFrameProvider(projectionScreen, true);
        break;
      case RunModes.PRODUCTION:
        frameProvider = new CameraFrameProvider(projectionScreen);
        break;
    }

    frameProcessor = new FrameProcessor(
      ipAddress, port, frameProvider.WIDTH, frameProvider.HEIGHT
    );
    lastFrameProcessTime = DateTime.Now;

    frame = new Color32[frameProvider.WIDTH * frameProvider.HEIGHT];

    StartCoroutine(getPointsAndProcess());
  }

  void Update() {
  }

  private IEnumerator getPointsAndProcess() {
    while (!destroyed) {
      double deltaTime = (DateTime.Now - lastFrameProcessTime).TotalSeconds;
      // Debug.Log($"Processing deltaTime: {deltaTime}");

      if (deltaTime < processMinDeltaTime) {
        yield return new WaitForSecondsRealtime((float)(processMinDeltaTime - deltaTime));
      }
      lastFrameProcessTime = DateTime.Now;

      frameProvider.getFrame(frame);
      drawCurrentHandOnProjectionScreen();

      IEnumerator poseDataEnumerator = frameProcessor.getHandPoseData(frame);
      yield return poseDataEnumerator;

      poseData = (PoseData)poseDataEnumerator.Current;

      if (poseData.lostTrack) {
        // TODO: Remove current pose?
        continue;
      }

      renderCurrentHand();
      grabber.processGrabbAction(poseData.points3d);
    }
  }

  private void drawCurrentHandOnProjectionScreen() {
    if (poseData == null || poseData.lostTrack) {
      return;
    }

    Vector2[] points = new Vector2[poseData.points2d.Length];
    Array.Copy(poseData.points2d, points, poseData.points2d.Length);

    Texture2D backgroundTexture = projectionScreen
      .GetComponent<Renderer>()
      .material.mainTexture as Texture2D;

    for (int id = 0; id < points.Length; id++) {
      var parentId = getParentId(id);
      // For some reason the points are mirrored, this is  a hack to fix it
      points[id] = new Vector2(points[id].x, frameProvider.HEIGHT - points[id].y);

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
    destroyed = true;
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