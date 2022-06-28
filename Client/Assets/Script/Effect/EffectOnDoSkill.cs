//using UnityEngine;
//using System.Collections;
//using DataTable;


//public class EffectOnDoSkill : MonoBehaviour
//{
//    public static readonly float DELAY = 0.1f;
//    public static float HEIGHT_SCALE = 0.5f;

//    public Skill skill;
//    private float delay = 0f;
//    private float duration = 3f;
//    private float elapse = 0f;
//    private int effIndex = 0;
//    private bool isEffectPlayed = false;
//    private int previousLayerMask = 0;
//    private Color previousBackground;

//    public static bool isActive { get; private set; }
    
//    public static void Activate(Skill skill, float delay)
//    {
//        if (MainProcess.mCameraObject != null && skill != null)
//        {
//            EffectOnDoSkill eff = MainProcess.mCameraObject.AddComponent<EffectOnDoSkill>();
//            eff.skill = skill;
//            eff.delay = delay;
//        }
//    }

//    private void Awake()
//    {
//        if (isActive)
//            DestroyImmediate(this);
//        else
//            isActive = true;
//    }

//    private void Start()
//    {
//        StartCoroutine(StartEffect());
//    }

//    private IEnumerator StartEffect()
//    {
//        yield return new WaitForSeconds(delay);
//        effIndex = skill.mConfig.get("ANIME_EFFECT");
//        duration = skill.mConfig.get("PAUSE_TIME");

//        if (MainProcess.mCameraMoveTool != null
//            && MainProcess.mCameraObject != null
//            && MainProcess.mMainCamera != null
//            && Character.Self != null
//            && TimeSetting.Self.currentState == TimeState.Default
//            && effIndex > 0
//            && Character.Self.mCurrentAI == skill)
//        {          
//            Vector3 lookDir = -MainProcess.mCameraObject.transform.TransformDirection(MainProcess.mCameraMoveTool.mCameraDirection);
//            lookDir.Normalize();
//            float lookLen = Vector3.Dot(Character.Self.mMainObject.transform.position - MainProcess.mMainCamera.transform.position, lookDir);
//            Vector3 targetPos = Character.Self.mMainObject.transform.position + lookDir * lookLen * (1f - HEIGHT_SCALE);
//            TimeSetting.Self.Apply(TimeState.NonStrictPause);
//            MainProcess.mCameraMoveTool.MoveTo(targetPos, DELAY);
//            //DataCenter.OpenWindow("SHADOW_WINDOW");

//            Camera mainCamera = MainProcess.mMainCamera.camera; 
//            int mask = 1 << CommonParam.CharacterLayer;

//            if (mainCamera != null && mainCamera.cullingMask != mask)
//            {
//                previousLayerMask = mainCamera.cullingMask;
//                previousBackground = mainCamera.backgroundColor;
//                mainCamera.cullingMask = mask;
//                mainCamera.backgroundColor = Color.black;
//            }

//            GameObject textPanel = GameCommon.FindUI("text_panel");

//            if (textPanel != null)
//                textPanel.SetActive(false);
//        }
//        else
//        {
//            Destroy(this);
//        }
//    }

//    private void Update()
//    {
//        elapse += TimeSetting.realDeltaTime;

//        if (elapse > delay)
//        {
//            MainProcess.mCameraMoveTool.Update(TimeSetting.realDeltaTime);
//        }

//        if (elapse > delay + DELAY && !isEffectPlayed)
//        {
//            PlayEffect(effIndex);
//            isEffectPlayed = true;
//        }

//        if (elapse > delay + DELAY + duration)
//        {
//            Destroy(this);
//        }
//    }

//    private void OnDestroy()
//    {
//        isActive = false;

//        if (TimeSetting.Self.currentState == TimeState.NonStrictPause)
//        {
//            TimeSetting.Self.Revert();
//        }

//        if (MainProcess.mCameraMoveTool != null && Character.Self != null)
//        {
//            MainProcess.mCameraMoveTool._MoveByObject(Character.Self, 0.1f);
//        }

//        if (MainProcess.mMainCamera != null)
//        {
//            Camera mainCamera = MainProcess.mMainCamera.camera;

//            if (mainCamera != null && previousLayerMask != 0)
//            {
//                mainCamera.cullingMask = previousLayerMask;
//                mainCamera.backgroundColor = previousBackground;
//                previousLayerMask = 0;
//            }
//        }

//        GameObject textPanel = GameCommon.FindUI("text_panel");

//        if (textPanel != null)
//            textPanel.SetActive(true);
//        //DataCenter.CloseWindow("SHADOW_WINDOW");
//    }

//    private void PlayEffect(int effIndex)
//    {
//        BaseEffect eff = BaseEffect.CreateEffect(Character.Self, null, effIndex);

//        if (eff != null)
//        {
//            eff.mGraphObject.AddComponent<RealTimeParticleSystem>();
//            GameCommon.SetLayer(eff.mGraphObject, CommonParam.CharacterLayer);
//            //eff.mGraphObject.transform.position = Character.Self.mMainObject.transform.position;
//        }
//        else
//        {
//            Destroy(this);
//        }
//    }
//}