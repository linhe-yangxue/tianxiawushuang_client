using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TypeControler {

    public int maxTypeCount = 0;
    public int maxSoundCount = 0;

    private int mAudioType = -1;  // 声音类型
    private int mPriority = -1;    // 优先级
    private float mVolume = 1;  // 音量
    private List<AudioControler> mList = new List<AudioControler>();

    public TypeControler(int type)
    {
        mAudioType = type;
    }

    public int priority
    {
        get { return mPriority; }
        set { mPriority = value; }
    }

    public int audioType
    {
        get { return mAudioType; }
        set { mAudioType = value; }
    }

    public float volume
    {
        get { return mVolume; }
        set { mVolume = value; }
    }

    public void AddAudio(AudioControler audio)
    {
        int audioType = audio.audioType;
        if (audioType != mAudioType)
        {
            DEBUG.Log(mAudioType + ".AddAudio -- ");
            DEBUG.Log("audio type error -- " + audio.audioType);
            return;
        }
        mList.Add(audio);
    }

    public AudioControler RemoveAudio(AudioControler audio)
    {
        if (mList.Remove(audio))
        {
            return audio;
        }
        return null;
    }

    public AudioControler RemoveAudio(string soundName)
    {
        foreach (AudioControler audio in mList)
        {
            if (audio.audioName == soundName)
            {
                mList.Remove(audio);
                return audio;
            }
        }
        return null;
    }

    public AudioControler PopupAudio()
    {
        if (mList == null || mList.Count == 0) return null;
        AudioControler audio = mList[0];
        mList.Remove(audio);
        return audio;
    }

    public int GetAudioCount()
    {
        return mList.Count;
    }

    public int GetAudioCount(string soundName)
    {
        int count = 0;
        foreach (AudioControler audio in mList)
        {
            if (audio.audioName == soundName)
            {
                count++;
            }
        }
        return count;
    }

    public void Print()
    {
        for (int i = 0; i < mList.Count; ++i)
        {
            Debug.LogError("audio name :" + mList[i].audioName);
        }
    }
}
