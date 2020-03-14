using UnityEngine;

public class ModelUtils 
{
  static public GameObject createHand() {
    Object prefab = Resources.Load<Object>("left_hand");
    return (GameObject) Object.Instantiate(prefab);
  }

  private static void debugHandJoint(GameObject joint) {
    Debug.Log(joint.name);
    for(int i=0; i<joint.transform.childCount; i++) {
      GameObject child = joint.transform.GetChild(i).gameObject;
      debugHandJoint(child);
    }
  }
  
  static public GameObject createBone(Vector3 orig, Vector3 dest, Transform parent) {
    const float modelHeight = 3.66f;
    Object prefab = Resources.Load<Object>("bone");
    GameObject bone = (GameObject) Object.Instantiate(prefab);
    bone.transform.SetParent(parent);
    var delta = dest - orig;

    float scaleFactor = delta.magnitude/modelHeight;
    bone.transform.localScale = 
        new Vector3(0.5f, 0.5f, 100*scaleFactor);

    bone.transform.localRotation = 
      Quaternion.FromToRotation(
        new Vector3(0, 0, -1), delta
      );

    bone.transform.localPosition = orig + delta/modelHeight;

    bone.GetComponent<Renderer>().material.color = new Color(255, 0, 0);
    bone.AddComponent<CapsuleCollider>();

    return bone;
  }

  static public GameObject createSphere(Vector3 position, Transform parent = null) {

      GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      sphere.transform.SetParent(parent);
      sphere.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
      sphere.transform.localPosition = position;
      sphere.GetComponent<Renderer>().material.color = new Color(255, 0, 0);
      sphere.AddComponent<CapsuleCollider>();

      var rigidBody = sphere.AddComponent<Rigidbody>();
      rigidBody.useGravity = false;
      rigidBody.mass = 1;

      return sphere;
  }
}
