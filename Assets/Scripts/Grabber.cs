using System.Collections.Generic;
using UnityEngine;

public class Grabber {
  public bool rotateGrabbedObject;
  private GameObject grabbedObject = null;
  public Vector3 initialGrabPosition { get; private set; }
  public Vector3 initialGrabDirection { get; private set; }
  public Vector3 currentGrabPosition { get; private set; }
  public Vector3 currentGrabDirection { get; private set; }
  public Grabber(bool rotateGrabbedObject = true) {
    this.rotateGrabbedObject = rotateGrabbedObject;
  }

  private bool isGrabbing = false;

  public void processGrabbAction(Vector3[] currentJointsPosition) {
    var thumbPosition = currentJointsPosition[5];
    var indexPosition = currentJointsPosition[10];
    var thumbDirection = currentJointsPosition[4] - currentJointsPosition[5];
    var indexDirection = currentJointsPosition[9] - currentJointsPosition[10];

    currentGrabPosition = (thumbPosition + indexPosition) / 2;
    currentGrabDirection = (thumbDirection + indexDirection).normalized;

    var wasGrabbing = isGrabbing;
    isGrabbing =
      (thumbPosition - indexPosition).magnitude < (wasGrabbing ? 0.08 : 0.03);

    // Debug.Log($">>>>> {isGrabbing} {(thumbPosition - indexPosition).magnitude}");

    if (!wasGrabbing && isGrabbing) startGrabbing();
    if (wasGrabbing && !isGrabbing) endGrabbing();
  }

  private void startGrabbing() {
    initialGrabPosition = currentGrabPosition;
    initialGrabDirection = currentGrabDirection;

    grabbedObject = new List<Collider>(Physics.OverlapSphere(initialGrabPosition, 0.001f))
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
