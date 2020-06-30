using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class FrameProcessor {
  private string ipAddress;
  private int port;
  private const int JOINTS_3D = 21;
  private const int JOINTS_2D = 21;
  private int width, height;
  public static float AverageSendTime = 0.120f;
  public static long SendCounter = 0;
  private long lastId = 0;
  private bool sendingFrame = false;


  public FrameProcessor(string ipAddress, int port, int width, int height) {
    this.ipAddress = ipAddress;
    this.port = port;
    this.width = width;
    this.height = height;
  }

  public IEnumerator getHandPoseData(Byte[] frame, bool encoded) {
    long id = ++lastId;
    yield return new WaitUntil(() => sendingFrame == false || lastId > id);
    if (lastId > id) {
      yield return null;
      yield break;
    }

    sendingFrame = true;

    IEnumerator IESendFrame = sendFrame(frame, encoded);
    yield return IESendFrame;
    sendingFrame = false;

    var json = (string)IESendFrame.Current;

    PoseData poseData = JsonUtility.FromJson<SerializablePoseData>(json);

    if (poseData.lostTrack) {
      yield return poseData;
      yield break;
    }

    const float newScale = 0.0015f;
    var root = newScale * poseData.points3d[0];

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

  IEnumerator sendFrame(Byte[] data, bool encoded) {
    // Debug.Log($"Data: {data.Length}");
    DateTime startTime = DateTime.Now;
    UnityWebRequest www = UnityWebRequest.Put($"http://{ipAddress}:{port}/processFrame", data);
    if (encoded) {
      www.SetRequestHeader("Encoded", "vp8");
    }
    yield return www.SendWebRequest();

    if (www.isNetworkError || www.isHttpError) {
      throw new Exception(www.error);
      // TODO: Deal with erros from server properly
    } else {
      yield return www.downloadHandler.text;
    }
    double deltaTime = (DateTime.Now - startTime).TotalSeconds;
    AverageSendTime = (float)(AverageSendTime * SendCounter + deltaTime) / (++SendCounter);

    // Debug.Log($"Send data deltaTime: {deltaTime}");
  }
}
