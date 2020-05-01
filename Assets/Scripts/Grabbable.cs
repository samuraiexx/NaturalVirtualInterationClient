using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
  public bool rotates = true;
  private Grabber grabber = null;
  private Vector3 initialPosition;
  private Quaternion initialRotation;

  void Update() {
    if (grabber == null) { 
      return;
    }

    if (rotates) {
      var deltaRotation = Quaternion
        .FromToRotation(grabber.currentGrabDirection, grabber.initialGrabDirection)
        .eulerAngles;

      gameObject.transform.rotation = Quaternion
        .Euler(initialRotation.eulerAngles + deltaRotation);
    }

    gameObject.transform.position = (
      initialPosition
      + (grabber.currentGrabPosition - grabber.initialGrabPosition)
    );
  }

  public void startGrabbing(Grabber grabber)
  {
    this.grabber = grabber;
    initialPosition = gameObject.transform.position;
    initialRotation = gameObject.transform.rotation;
  }

  public void endGrabbing()
  {
    this.grabber = null;
  }
}
