using System;
using System.Runtime.Serialization;
using UnityEngine;
using System.Linq;

public class PoseData {
  public bool lostTrack;
  public Vector3[] points3d;
  public Vector2[] points2d;
  public float[] angles;

  public static implicit operator PoseData(SerializablePoseData rValue) {
    return new PoseData {
      lostTrack = rValue.lostTrack,
      points3d = rValue.points3d.Select(rPoint => (Vector3)rPoint).ToArray(),
      points2d = rValue.points2d.Select(rPoint => (Vector2)rPoint).ToArray(),
      angles = rValue.angles,
    };
  }
}

[Serializable]
public class SerializablePoseData {
  public bool lostTrack = true;
  public SerializableVector3[] points3d = null;
  public SerializableVector2[] points2d = null;
  public float[] angles = null;
}

[Serializable]
public struct SerializableVector3 {
  public float x, y, z;
  public SerializableVector3(float rX, float rY, float rZ) {
    x = rX;
    y = rY;
    z = rZ;
  }

  public override string ToString() {
    return String.Format("[{0}, {1}, {2}]", x, y, z);
  }

  public static implicit operator Vector3(SerializableVector3 rValue) {
    return new Vector3(rValue.x, rValue.y, rValue.z);
  }
}

[Serializable]
public struct SerializableVector2 {
  public float x, y;

  public SerializableVector2(float rX, float rY) {
    x = rX;
    y = rY;
  }

  public override string ToString() {
    return String.Format("[{0}, {1}]", x, y);
  }

  public static implicit operator Vector2(SerializableVector2 rValue) {
    return new Vector2(rValue.x, rValue.y);
  }
}