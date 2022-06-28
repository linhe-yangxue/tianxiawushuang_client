using UnityEngine;
using System.Collections.Generic;
using WellFired;


[USequencerFriendlyName("Set Camera")]
[USequencerEvent("Extensions/Set Camera")]
public class USSetCamera : USEventBase
{
    private static List<Camera> cameraHistory = new List<Camera>();

    public Camera camera;

    public override void FireEvent()
    {
        if (cameraHistory.Count == 0)
        {
            cameraHistory.Add(Camera.main);
        }

        if (camera != null)
        {
            cameraHistory.Add(camera);
            SetActive(cameraHistory.Count - 2, false);
            SetActive(cameraHistory.Count - 1, true);
        }
    }

    public override void ProcessEvent(float runningTime)
    { }

    public override void UndoEvent()
    {
        int count = cameraHistory.Count;
        SetActive(cameraHistory.Count - 1, false);
        SetActive(cameraHistory.Count - 2, true);
        cameraHistory.RemoveAt(cameraHistory.Count - 1);
    }

    public override void StopEvent()
    {
        if (cameraHistory.Count > 0)
        {
            SetActive(0, true);
            cameraHistory.Clear();
        }
    }

    private void SetActive(int index, bool active)
    {
        if (cameraHistory[index].gameObject != null)
            cameraHistory[index].gameObject.SetActive(active);
    }
}