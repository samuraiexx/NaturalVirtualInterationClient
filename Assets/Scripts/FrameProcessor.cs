using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class FrameProcessor {
  private string ipAddress;
  private int port;
  private const int JOINTS_3D = 21;
  private const int JOINTS_2D = 21;
  Encoder encoder;

  public FrameProcessor(string ipAddress, int port, int width, int height) {
    encoder = Encoder.getEncoder(width, height);
  }

  public IEnumerator getHandPoseData(Color32[] frame) {
    // TODO: Change to BSON
    Byte[] encodedFrame = encoder.processFrame(frame);

    var IESendEncodedFrame = sendEncodedFrame(encodedFrame);

    yield return IESendEncodedFrame;
    var json = (string)IESendEncodedFrame.Current;

    PoseData poseData = JsonUtility.FromJson<SerializablePoseData>(json);

    const float newScale = 0.0015f;
    var root = poseData.lostTrack ? new Vector3() : newScale * poseData.points3d[0];

    for (int i = 0; i < poseData.points3d.Length; i++) {
      poseData.points3d[i] = newScale * poseData.points3d[i];

      poseData.points3d[i] = new Vector3(
        -0.50f + poseData.points3d[i].x - 2.5f * root.x,
        0.25f - poseData.points3d[i].y + 1 * root.y,
        0.3f + poseData.points3d[i].z
      );
    }

    yield return poseData;
  }

  IEnumerator sendEncodedFrame(Byte[] data) {
    UnityWebRequest www = UnityWebRequest.Put("http://127.0.0.1:3030/processFrame", data);
    yield return www.SendWebRequest();

    if (www.isNetworkError || www.isHttpError) {
      Debug.Log(www.error);
      // TODO
    } else {
      yield return www.downloadHandler.text;
    }
  }
}
