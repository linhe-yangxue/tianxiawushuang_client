using UnityEngine;
using System.Collections;
using DataTable;
using Utilities;


public class SimpleDialogWindow : tWindow
{
    public static float fadeInTime = 0f;
    public static float durationTime = 1f;
    public static float fadeOutTime = 0f;

    public static void OpenDialog(int index, float fadeIn, float duration, float fadeOut)
    {
        SimpleDialogWindow.fadeInTime = fadeIn;
        SimpleDialogWindow.durationTime = duration;
        SimpleDialogWindow.fadeOutTime = fadeOut;
        DataCenter.OpenWindow("SIMPLE_DIALOG_WINDOW", index);
    }

    public static void CloseDialog()
    {
        DataCenter.CloseWindow("SIMPLE_DIALOG_WINDOW");
    }

    public override void Open(object param)
    {
        base.Open(param);
        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        if (!(param is int))
        {
            Close();
            return true;
        }

        DataRecord record = DataCenter.mDialog.GetRecord((int)param);

        if (record == null)
        {
            Close();
            return true;
        }

        string text = record.get("DIALOG");

        UISprite boxSprite = GetComponent<UISprite>("dialog");
        UILabel dialogLabel = GetComponent<UILabel>("label");

        float width = PlotTools.GetLocalWidth();
        float height = PlotTools.GetLocalHeight();

        boxSprite.width = (int)width;
        dialogLabel.width = (int)width - 60;
        dialogLabel.transform.localPosition = new Vector3(-width / 2f + 30f, dialogLabel.transform.localPosition.y, 0f);
        dialogLabel.text = text;
        boxSprite.alpha = 0f;

        this.StopAllCoroutines();
        this.StartCoroutine(FadeInOut(boxSprite, fadeInTime, durationTime, fadeOutTime));

        return true;
    }

    private IEnumerator FadeInOut(UISprite sprite, float fadeIn, float duration, float fadeOut)
    {
        float time = 0f;
        sprite.alpha = 0f;

        while (time < fadeIn)
        {
            sprite.alpha = time / fadeIn;
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        sprite.alpha = 1f;
        time = 0f;

        while (time < duration)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        time = 0f;

        while (time < fadeOut)
        {
            sprite.alpha = 1f - time / fadeOut;
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        sprite.alpha = 0f;
        Close();
    }
}