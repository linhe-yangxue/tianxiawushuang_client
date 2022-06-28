using UnityEngine;
using WellFired;


[USequencerFriendlyName("Dialog")]
[USequencerEvent("Extensions/Dialog")]
public class USDialog : USEventBase
{
    public int dialogID = 0;
    public float fadeInTime = 0f;
    public float fadeOutTime = 0f;
    
    public override void FireEvent()
    {
        SimpleDialogWindow.OpenDialog(dialogID, fadeInTime, Duration - fadeInTime - fadeOutTime, fadeOutTime);
    }

    public override void ProcessEvent(float runningTime)
    { }

    public override void UndoEvent()
    {
        SimpleDialogWindow.CloseDialog();
    }

    public override void StopEvent()
    {
        UndoEvent();
    }
}