using UnityEngine;
using System;
using System.Collections;
using Logic;
using DataTable;
using Utilities;


public class GuideDialogWindow : tWindow
{
    public static GameObject winObj;
    private BaseObject model = null;
    private Action onClick = null;
    private bool locked = false;
    private AudioSource audioSource = null;
    private int guideCharsPerSecond = 20;

    public static bool isOpened { get; private set; }

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_dialog_mask", new DefineFactory<Button_dialog_mask>());
    }

    protected override void OpenInit()
    {
        base.OpenInit();
    }

    public override void Open(object param)
    {
        base.Open(param);

        isOpened = true;

        guideCharsPerSecond = (int)(DataCenter.mGlobalConfig.GetData("GUIDE_CHARS_PER_SECOND", "VALUE"));

        DataRecord r = DataCenter.mDialog.GetRecord((int)param);
        RefreshDialog(r);

        if (mGameObjUI.GetComponent<UIPanel>().alpha < 0.999f)
        {
            this.StartCoroutine(FadeIn(3f));
        }
        winObj = mGameObjUI;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "SET_ACTION":
                onClick = (Action)objVal;
                break;

            case "ON_CLICK":
                if (!locked && onClick != null)
                {
                    onClick();
                }
                break;
        }
    }

    public override void Close()
    {
        base.Close();

        if (model != null)
        {
            GuideKit.DestroyModel(model);
            model = null;
        }

        if (mGameObjUI != null)
        {
            mGameObjUI.GetComponent<UIPanel>().alpha = 0f;
        }

        isOpened = false;
    }

    private void RefreshDialog(DataRecord r)
    {
        if (r == null)
            return;

        string text = r.get("DIALOG");
        int isLeft = r.get("ISLEFT");
        int modelIndex = r.get("MODEL");
        float scale = r.get("SCALE");
        float offset = r.get("OFFSET");
        string sound = r.get("SOUND");

        PlaySound(sound);

        UISprite boxSprite = GetComponent<UISprite>("dialog");
        UISprite bgSprite = GetComponent<UISprite>("dialog_bg");
        UILabel dialogLabel = GetComponent<UILabel>("label");
        dialogLabel.gameObject.AddComponent<TypewriterEffect>().charsPerSecond = guideCharsPerSecond;
       
        GameObject head = GetSub("head");
        GameObject fingerPoint = GetSub("finger_point");

        float width = GuideKit.GetLocalWidth();
        float height = GuideKit.GetLocalHeight();
        boxSprite.width = (int)width;
        bgSprite.width = (int)width - 70;
        fingerPoint.transform.localPosition = new Vector3(width / 2f - 50f, fingerPoint.transform.localPosition.y, 0f);

        if (isLeft == 0)
        {
            head.transform.localPosition = new Vector3(width / 2f - 125f, offset * height + 60f, 500f);
        }
        else
        {
            head.transform.localPosition = new Vector3(-width / 2f + 125f, offset * height + 60f, 500f);
        }

        if (model == null || model.mConfigIndex != modelIndex)
        {
            if (model != null)
            {
                GuideKit.DestroyModel(model);
                model = null;
            }

            if (modelIndex > 0)
            {
                model = GuideKit.ShowModel(modelIndex, head, scale);
            }
        }
        else
        {
            model.SetPosition(head.transform.position);
            GuideKit.SetModelScale(model, scale);
        }

        dialogLabel.width = (int)width - 90;
        dialogLabel.transform.localPosition = new Vector3(-width / 2f + 50f, dialogLabel.transform.localPosition.y, 0f);
        GameObject dialogLabelObj = GameCommon.FindObject(mGameObjUI, "label");
        if (dialogLabelObj != null)
        {
            dialogLabelObj.SetActive(false);
            dialogLabel.text = text;
            dialogLabelObj.SetActive(true);
            boxSprite.alpha = 1f;
        }
        //GuideKit.OpenCursor(fingerPoint, Vector2.zero, 0.7f);
    }

    private IEnumerator FadeIn(float speed)
    {
        UIPanel panel = mGameObjUI.GetComponent<UIPanel>();

        while (panel.alpha < 0.999f)
        {
            yield return null;
            panel.alpha += speed * Time.deltaTime;
        }

        panel.alpha = 1f;
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


public class Button_dialog_mask : CEvent
{
    public override bool _DoEvent()
    {
        if (GuideDialogWindow.winObj == null) return true;
        TypewriterEffect typeEffect = GameCommon.FindComponent<TypewriterEffect>(GuideDialogWindow.winObj, "label");
        if (typeEffect != null)
        {
            typeEffect.allText = true;
        }
        else
        {
            DataCenter.SetData("GUIDE_DIALOG_WINDOW", "ON_CLICK", true);
        }
        return true;
    }
}