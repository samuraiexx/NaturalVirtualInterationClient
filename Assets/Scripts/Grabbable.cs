using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour {
  public bool rotates = false;
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

  private bool wasKinematic, wasUsingGravity;

  public void startGrabbing(Grabber grabber) {
    this.grabber = grabber;
    initialPosition = gameObject.transform.position;
    initialRotation = gameObject.transform.rotation;

    var rigidBody = this.gameObject.GetComponent<Rigidbody>();
    var collider = this.gameObject.GetComponent<Collider>();

    wasKinematic = rigidBody.isKinematic;
    wasUsingGravity = rigidBody.useGravity;

    collider.enabled = false;
    rigidBody.isKinematic = false;
    rigidBody.useGravity = false;
  }

  public void endGrabbing() {
    this.grabber = null;

    var rigidBody = this.gameObject.GetComponent<Rigidbody>();
    var collider = this.gameObject.GetComponent<Collider>();

    collider.enabled = true;
    rigidBody.isKinematic = wasKinematic;
    rigidBody.useGravity = wasUsingGravity;

    rigidBody.angularVelocity = Vector3.zero;
    rigidBody.velocity = Vector3.zero;
  }
}
