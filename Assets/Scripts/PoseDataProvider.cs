using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RunModes { FAKE_FRAMES, UNITY_REMOTE, PRODUCTION };
public class PoseDataProvider : MonoBehaviour {
  public RunModes runMode = RunModes.PRODUCTION;
  public bool encode = true;
  public string ipAddress = "127.0.0.1";
  public int port = 3030;
  public int width = 640, height = 480;
  public List<Action> onUpdatePose = new List<Action>();

  public Color32[] frame { get; private set; }
  public PoseData poseData { get; private set; }

  private FrameProcessor frameProcessor;
  private FrameProvider frameProvider;
  private Encoder encoder;

  private DateTime lastUpdateTime;
  private double deltaTime = 0;
  private int processCount = 0;

  void Start() {
    switch (runMode) {
      case RunModes.FAKE_FRAMES:
        frameProvider = new TestFrameProvider(width, height);
        break;
      case RunModes.UNITY_REMOTE:
        frameProvider = new CameraFrameProvider(width, height, true);
        break;
      case RunModes.PRODUCTION:
        frameProvider = new CameraFrameProvider(width, height);
        break;
    }

    frameProcessor = new FrameProcessor(
      ipAddress, port, width, height
    );

    frame = new Color32[width * height];
    encoder = Encoder.getEncoder(width, height);

    lastUpdateTime = DateTime.Now;

    StartCoroutine(getPointsAndProcess());
  }

  private IEnumerator getPointsAndProcess() {
    Color32[] frame = frameProvider.getFrame();

    IEnumerator poseDataEnumerator;

    if (encode) {
      IEnumerator frameEnumerator = encoder.processFrame(frame);
      yield return frameEnumerator;

      Byte[] encodedFrame = (Byte[])frameEnumerator.Current;

      poseDataEnumerator = frameProcessor.getHandPoseData(encodedFrame, true);
    } else {
      var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
      texture.SetPixels32(frame);
      texture.Apply();
      var plainFrame = texture.EncodeToPNG();

      poseDataEnumerator = frameProcessor.getHandPoseData(plainFrame, false);
    }

    yield return poseDataEnumerator;
    StartCoroutine(getPointsAndProcess());

    poseData = (PoseData)poseDataEnumerator.Current;
    this.frame = frame;

    onUpdatePose.ForEach(callback => callback());

    deltaTime += (DateTime.Now - lastUpdateTime).TotalMilliseconds;
    lastUpdateTime = DateTime.Now;
    processCount++;
  }

  private void OnDestroy() {
    Debug.Log($"Mean Delta Time: {deltaTime / processCount}");
  }
}

