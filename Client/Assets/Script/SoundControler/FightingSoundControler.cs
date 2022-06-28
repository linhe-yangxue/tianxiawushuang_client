using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;

public class FightingSoundControler : MonoBehaviour
{
    // private List<AudioControler> mAudioClipList = null;
    public static GameObject mEmptyPrefab = null;
    public static FightingSoundControler Self;
    public delegate void AudioCompleteCalblack(AudioControler clip);
    private Dictionary<int, TypeControler> mControlerList = null;
    private List<int> mPriorityList = null;
    
    // 战斗场景中存在音效的最大数量
    public int mMaxSceneFightingSound = 0;
    // 
    public NiceTable mConfig = null;
    // 声音衰减持续时间
    public float mAttenuationDuration = 0f;

    void Start()
    {
        mEmptyPrefab = Resources.Load("Prefabs/EmptyPrefab") as GameObject;
        if (mEmptyPrefab == null)    DEBUG.LogError("can not find prefab -- Prefabs/EmptyPrefab");
        Self = this;
    }

    void Update()
    {
#if SOUND_CONTROL_TEST
        if(mControlerList == null)  return;
        for(int i=0; i<mControlerList.Count; ++i)
        {
            TypeControler controler = mControlerList[i];
            controler.Print();
        }
        Debug.LogError("-----------------------------");
#endif
    }

    public void Init(int maxEffectSounds, NiceTable configTable, float duration)
    {
        mMaxSceneFightingSound = maxEffectSounds;
        mAttenuationDuration = duration > 0 ? duration : 0;
        mConfig = configTable;
        mControlerList = new Dictionary<int, TypeControler>();
        mPriorityList = new List<int>();
    }

    // 添加一个战斗音效
    public void PlayFightEffectSound(int type, string audioName, Vector3 playPos)
    {
        if (mControlerList == null || audioName == null || audioName == "" || mEmptyPrefab == null) return;
        if (!Settings.IsSoundEffectEnabled()) return;

        // load sound 
        AudioClip audioClip = GameCommon.mResources.LoadSound(audioName);
        if (audioClip == null) return;

        TypeControler typeControler = null; ;
        mControlerList.TryGetValue(type, out typeControler);
        if (typeControler == null)
        {
            DataRecord record = mConfig.GetRecord(type.ToString());
            int priority = record.getData("PRIORITY");
            int volume = record.getData("VOLUME");
            if (volume == Data.NULL) volume = 1;
            typeControler = new TypeControler(type);
            typeControler.maxSoundCount = record["MAX_TYPE_SOUND"];
            typeControler.maxTypeCount = record["MAX_SENCE_SOUND"];
            typeControler.volume = volume;
            mControlerList.Add(type, typeControler);
            // record priority list
            if (!mPriorityList.Contains(priority))
            {
                mPriorityList.Add(priority);
                mPriorityList.Sort(new SoundPriorityCompareRule());
            }
        }

        // check audio count;
        AudioControler audio = null;
        if (typeControler.GetAudioCount(audioName) >= typeControler.maxSoundCount)
        {
            audio = typeControler.RemoveAudio(audioName);
        }
        else if (typeControler.GetAudioCount() >= typeControler.maxTypeCount)
        {
            audio = typeControler.PopupAudio();
        }
        else if (GetAllAudioCount() >= mMaxSceneFightingSound)
        {
            TypeControler leastPriority = GetLeastPriorityList();
            if(leastPriority != null)
                audio = leastPriority.PopupAudio();
        }
        if (audio != null)
        {
            audio.StopSound(mAttenuationDuration, true);
        }

        //  push 
        AudioControler audioControler = StartSound( audioClip, playPos);
        audioControler.Init(type, audioName);
        if(audioControler)  typeControler.AddAudio(audioControler);
    }

    // 播放声音
    private AudioControler StartSound(AudioClip clip, Vector3 playPos)
    {
        GameObject obj = Instantiate(mEmptyPrefab) as GameObject;
        obj.transform.position = playPos;
        obj.name = "SoundControler";

        obj.AddComponent<AudioControler>();
        AudioControler clipControler = obj.GetComponent<AudioControler>() as AudioControler;
        clipControler.onComplete = OnAudioComplete;
        clipControler.PlaySound(clip);

        return clipControler;
    }

    private void OnAudioComplete(AudioControler audio)
    {
        audio.onComplete = null;
        int audioType = audio.audioType;
        TypeControler controler = null;
        mControlerList.TryGetValue(audioType, out controler);
        if(controler != null)  controler.RemoveAudio(audio);
    }

    private int GetAllAudioCount()
    {
        int count = 0;
        foreach (var item in mControlerList)
        {
            TypeControler controler = item.Value;
            count += controler.GetAudioCount();
        }
        return count;
    }

    private TypeControler GetLeastPriorityList()
    {
        if(mPriorityList == null || mPriorityList.Count == 0)  return null;
        foreach (int value in mPriorityList)
        {
            TypeControler controler = null;
            mControlerList.TryGetValue(value, out controler);
            if (controler == null || controler.GetAudioCount() <= 0) continue;
            return controler;
        }
        return null;
    }
}

public class SoundPriorityCompareRule : IComparer<int>
{ 
    public int Compare(int l, int r)
    {
        return r - l;
    }
}