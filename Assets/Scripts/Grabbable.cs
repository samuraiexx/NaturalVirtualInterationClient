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

    gameObject.transform.localPosition = (
      initialPosition
      + (grabber.currentGrabPosition - grabber.initialGrabPosition)
    );
  }

  private bool wasKinematic, wasUsingGravity;
  Transform oldParent = null;

  public void startGrabbing(Grabber grabber) {
    this.grabber = grabber;

    var rigidBody = this.gameObject.GetComponent<Rigidbody>();
    var collider = this.gameObject.GetComponent<Collider>();

    wasKinematic = rigidBody.isKinematic;
    wasUsingGravity = rigidBody.useGravity;
    oldParent = this.transform.parent;

    collider.enabled = false;
    rigidBody.isKinematic = false;
    rigidBody.useGravity = false;
    this.transform.parent = GameObject.FindGameObjectWithTag("MainCamera").transform;

    initialPosition = gameObject.transform.localPosition;
    initialRotation = gameObject.transform.localRotation;
  }

  public void endGrabbing() {
    this.grabber = null;

    var rigidBody = this.gameObject.GetComponent<Rigidbody>();
    var collider = this.gameObject.GetComponent<Collider>();

    collider.enabled = true;
    rigidBody.isKinematic = wasKinematic;
    rigidBody.useGravity = wasUsingGravity;
    this.transform.parent = oldParent;

    rigidBody.angularVelocity = Vector3.zero;
    rigidBody.velocity = Vector3.zero;
  }
}
