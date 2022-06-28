using UnityEngine;
using System.Collections;
using DataTable;

public class CPlaySound : MonoBehaviour {

	public bool mIsReadTable = true;
	public float mfDelay = 0;

	public float mfInterval = 0;
	public bool mbIsNeedReplay = false;

	void OnEnable()
	{
		StartCoroutine(PlaySound());
	}

	public IEnumerator PlaySound()
	{
        string objName = gameObject.name.Replace("(Clone)", "");
		DataRecord re = DataCenter.mEffectSound.GetRecord(objName);
		if(re != null)
		{
			if(mIsReadTable)
			{
				mfDelay = re.getData("DELAY_TIME");
				mfInterval = re.getData("INTERVAL_TIME");
				mbIsNeedReplay = re.getData("IS_NEED_REPLAY") == 1 ? true : false;
			}


			yield return new WaitForSeconds(mfDelay);

			string soundFile = re.getData("SOUND_FILE");
            int soundType = re.getData("SOUND_TYPE");
            GameCommon.PlaySound(soundFile, GameCommon.GetMainCamera().transform.position, soundType);

            while (mbIsNeedReplay)
            {
                yield return new WaitForSeconds(mfInterval);
                GameCommon.PlaySound(soundFile, GameCommon.GetMainCamera().transform.position, soundType);
            }
		}
	}
}
