using UnityEngine;

public class ModelUtils 
{
  static public GameObject createBone(Vector3 orig, Vector3 dest) {
    const float modelHeight = 3.66f;
    Object prefab = Resources.Load<Object>("bone");
    GameObject bone = (GameObject) Object.Instantiate(prefab);
    var delta = dest - orig;

    float scaleFactor = delta.magnitude/modelHeight;
    bone.transform.localScale = 
        new Vector3(400, 400, 100*scaleFactor);

    bone.transform.SetPositionAndRotation(
      orig + delta/modelHeight,
      Quaternion.FromToRotation(
        new Vector3(0, 0, -1), delta
      )
    );

    bone.GetComponent<Renderer>().material.color = new Color(255, 0, 0);

    return bone;
  }

  static public GameObject createSphere(Vector3 position) {

      GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      sphere.transform.localScale = new Vector3(10, 10, 10);
      sphere.transform.position = position;
      sphere.GetComponent<Renderer>().material.color = new Color(255, 0, 0);

      return sphere;
  }
}
