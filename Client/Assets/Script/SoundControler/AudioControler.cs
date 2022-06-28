using UnityEngine;
using System.Collections;

public class AudioControler : MonoBehaviour 
{
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private int mAudioType;
    private string mAudioName;
    private AudioSource mAudioSource = null;
    public FightingSoundControler.AudioCompleteCalblack onComplete;

    public void Init(int type, string name)
    {
        mAudioType = type;
        mAudioName = name;
    }

    public int audioType
    {
        get { return mAudioType; }
        set { mAudioType = value; }
    }

    public string audioName
    {
        get { return mAudioName; }
        set { mAudioName = value; }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip == null || !Settings.IsSoundEffectEnabled())
        {
            if (onComplete != null) onComplete(this);
            return;
        }

        if (mAudioSource == null)
        {
            gameObject.AddComponent<AudioSource>();
            mAudioSource = gameObject.GetComponent<AudioSource>();
        }
        else
        {
            mAudioSource.Stop();
        }

        // 播放声音
        mAudioSource.clip = clip;
        StartCoroutine(PlayAudio(mAudioSource));
    }

    public void StopSound(float duration = 0f, bool autoDestory = true)
    {
        StopCoroutine("PlayAudio");
        if (mAudioSource == null) return;
        StartCoroutine(WaitForDestory(duration, autoDestory));
        // 添加声音衰减
        if (duration > 0)
            TweenVolume.Begin(this.gameObject, duration, 0f);
    }

    private IEnumerator PlayAudio(AudioSource audio)
    {
        audio.Play();
        yield return new WaitForSeconds(audio.clip.length);
        if (onComplete != null)
        {
            onComplete(this);
        }
        Destroy(gameObject);
    }

    private IEnumerator WaitForDestory(float duration, bool autoDestory)
    {
        yield return new WaitForSeconds(duration + 0.2f);
        if (autoDestory)
            Destroy(gameObject);
    }
}