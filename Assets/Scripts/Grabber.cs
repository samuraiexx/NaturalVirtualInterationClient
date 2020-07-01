using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour {
  public bool rotateGrabbedObject;
  public GameObject PoseDataProviderObject;

  public Vector3 initialGrabPosition { get; private set; }
  public Vector3 initialGrabDirection { get; private set; }
  public Vector3 currentGrabPosition { get; private set; }
  public Vector3 currentGrabDirection { get; private set; }

  private GameObject grabbedObject = null;
  private PoseDataProvider poseDataProvider;

  void Start() {
    poseDataProvider = PoseDataProviderObject.GetComponent<PoseDataProvider>();
    Debug.Log($"poseDataProvider: {poseDataProvider == null} {poseDataProvider.onUpdatePose == null}");
    poseDataProvider.onUpdatePose.Add(processGrabbAction);
  }

  private bool isGrabbing = false;

  private void processGrabbAction() {
    if (poseDataProvider.poseData.lostTrack == true) {
      if (isGrabbing) endGrabbing();
      return;
    }

    var currentJointsPosition = poseDataProvider.poseData.points3d;
    var thumbPosition = currentJointsPosition[4];
    var indexPosition = currentJointsPosition[8];
    var thumbDirection = currentJointsPosition[4] - currentJointsPosition[3];
    var indexDirection = currentJointsPosition[8] - currentJointsPosition[9];

    currentGrabPosition = (thumbPosition + indexPosition) / 2;
    currentGrabDirection = (thumbDirection + indexDirection).normalized;

    var wasGrabbing = isGrabbing;
    isGrabbing =
      (thumbPosition - indexPosition).magnitude < (wasGrabbing ? 0.10 : 0.07);

    // if (isGrabbing != wasGrabbing) {
    //   Debug.Log($">>>>> IsGrabbing: {isGrabbing}; Distance: {(thumbPosition - indexPosition).magnitude}");
    // }

    if (!wasGrabbing && isGrabbing) startGrabbing();
    if (wasGrabbing && !isGrabbing) endGrabbing();
  }

  private void startGrabbing() {
    initialGrabPosition = currentGrabPosition;
    initialGrabDirection = currentGrabDirection;

    grabbedObject = new List<Collider>(Physics.OverlapSphere(initialGrabPosition, 0.100f))
      .ConvertAll<GameObject>(collider => collider.gameObject)
      .Find(gameObject => gameObject.GetComponent<Grabbable>() != null);

    if (grabbedObject == null) return;
    grabbedObject.GetComponent<Grabbable>().startGrabbing(this);
  }

  private void endGrabbing() {
    if (grabbedObject == null) return;
    grabbedObject.GetComponent<Grabbable>().endGrabbing();
    grabbedObject = null;
  }
}
