using UnityEngine;
using Logic;
using DataTable;
using Utilities;


public class DialogWindow : tWindow
{
    private static float openDelay = 0f;
    private DataRecord record;
    private BaseObject model = null;
    private AudioSource audioSource = null;

    public static int currentDialogID { get; private set; }

    public static void SetOpenDelay(float delay)
    {
        openDelay = delay;
    }

    public override void Open(object param)
    {
        base.Open(param);

        record = null;
        currentDialogID = 0; 
        Refresh(param);
    }

    public override void Close()
    {
        base.Close();

        if (mGameObjUI != null)
        {
            PlotTools.DestroyModel(model);
            GameCommon.FindComponent<UISprite>(mGameObjUI, "dialog").alpha = 0f;         
        }

        GuideManager.CloseMask();
        ObserverCenter.Notify(GuideEvent.OnDialogFinish);
        record = null;
        currentDialogID = 0;
    }

    public override bool Refresh(object param)
    {
        GameCommon.FindComponent<UISprite>(mGameObjUI, "dialog").alpha = 0f;

        if (param is int)
        {
            DataRecord r = DataCenter.mDialog.GetRecord((int)param);

            if (r != null)
            {
                GuideManager.OpenMaskWithoutOperateRegion();
                currentDialogID = (int)param;
                RefreshDialog(r);               
                return true;
            }
        }
      
        Close();
        return true;
    }

    private void RefreshDialog(DataRecord r)
    {
        string text = r.get("DIALOG");
        int isLeft = r.get("ISLEFT");
        int modelIndex = r.get("MODEL");
        float scale = r.get("SCALE");
        float offset = r.get("OFFSET");
        string sound = r.get("SOUND");

        PlaySound(sound);

        UISprite boxSprite = GetComponent<UISprite>("dialog");
        UILabel dialogLabel = GetComponent<UILabel>("label");
        GameObject head = GetSub("head");
        GameObject fingerPoint = GetSub("finger_point");

        float width = PlotTools.GetLocalWidth();
        float height = PlotTools.GetLocalHeight();
        boxSprite.width = (int)width;
        fingerPoint.transform.localPosition = new Vector3(width / 2f - 25f, fingerPoint.transform.localPosition.y, 0f);

        if (isLeft == 0)
        {
            head.transform.localPosition = new Vector3(width / 2f - 125f, offset * height + 60f, 500f);
        }
        else
        {
            head.transform.localPosition = new Vector3(-width / 2f + 125f, offset * height + 60f, 500f);
        }

        if (record == null || openDelay > 0.01f || modelIndex != (int)record.get("MODEL"))
        {
            if (model != null)
            {
                PlotTools.DestroyModel(model);
                model = null;
            }

            if (modelIndex > 0)
            {
                model = PlotTools.ShowModel(modelIndex, head, scale);
            }

            if (model != null && openDelay > 0.01f)
            {
                model.SetVisible(false);
                this.ExecuteDelayed(() => { if (model != null) model.SetVisible(true); }, openDelay);
            }
        }
        else if(model != null)
        {
            model.SetPosition(head.transform.position);
            PlotTools.SetModelScale(model, scale);
        }

        record = r;

        dialogLabel.width = (int)width - 60;
        dialogLabel.transform.localPosition = new Vector3(-width / 2f + 30f, dialogLabel.transform.localPosition.y, 0f);
        dialogLabel.text = text;
        boxSprite.alpha = 0f;       

        FadeIn();
    }

    private void FadeIn()
    {
        TM_FadeIn evt = EventCenter.Start("TM_FadeIn") as TM_FadeIn;
        evt.mTarget = GetSub("dialog");
        evt.mBefore = openDelay;
        evt.mDuration = 0.3f;
        evt.curve = x => 2f * x - x * x;
        evt.mAfter = 0f;
        evt.onElapsed += x => OnDialogComplete();
        evt.DoEvent();

        openDelay = 0f;
    }

    private void Next()
    {
        if (record != null)
        {
            int nextDialogIndex = record.get("NEXT");

            if (nextDialogIndex == 0 || DataCenter.mDialog.GetRecord(nextDialogIndex) == null)
            {            
                Close();               
            }
            else
            {
                Refresh(nextDialogIndex);              
            }
        }
    }

    private void ShowFinger()
    {
        GameObject point = GetSub("finger_point");
        GuideManager.ShowFinger(point, 0.7f);
    }

    private void OnDialogComplete()
    {
        ShowFinger();
        GuideManager.OpenMask(OnMouseClick);
    }

    private void OnMouseClick()
    {
        GuideManager.DestroyFinger();
        Next();       
    }

    private void PlaySound(string soundName)
    {
        if (!Settings.IsSoundEffectEnabled())
            return;

        if (soundName == "" || soundName == "0")
        {
            if (audioSource != null)
                audioSource.Stop();

            return;
        }

        if (audioSource == null && mGameObjUI != null)
        {
            GameObject audioSourceObj = new GameObject("dialog_sound");
            audioSourceObj.transform.parent = mGameObjUI.transform;
            audioSourceObj.transform.position = GameCommon.GetMainCamera().transform.position;
            audioSource = audioSourceObj.AddComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            AudioClip clip = GameCommon.LoadAudioClip(soundName);

            if (clip != null)             
                audioSource.PlayOneShot(clip, CommonParam.DefaultVolume);
        }
    }
}