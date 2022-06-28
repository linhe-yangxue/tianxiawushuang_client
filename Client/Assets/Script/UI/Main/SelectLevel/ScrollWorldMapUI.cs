using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic;
using DataTable;
using Utilities;


public class ScrollWorldMapUI : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return (new WaitForEndOfFrame());

        tWindow win = new ScrollWorldMapWindow() { mGameObjUI = gameObject, mWinName = "SCROLL_WORLD_MAP_WINDOW" };
        DataCenter.Self.registerData("SCROLL_WORLD_MAP_WINDOW", win);
        DataCenter.OpenWindow("SCROLL_WORLD_MAP_WINDOW");

        if (MainUIScript.Self.mStrWorldMapSubWindowName != "")
        {
            DataCenter.OpenWindow(MainUIScript.Self.mStrWorldMapSubWindowName);
            MainUIScript.Self.mStrWorldMapSubWindowName = "";
            
        }
    }

    private void OnDisable()
    {
        //DataCenter.CloseWindow("SCROLL_WORLD_MAP_WINDOW");
    }

    private void OnDestroy()
    {
        if(this.gameObject.activeInHierarchy)
            DataCenter.CloseWindow("SCROLL_WORLD_MAP_WINDOW");

        DataCenter.Remove("SCROLL_WORLD_MAP_WINDOW");
    }
}


public class ScrollWorldMapWindow : tWindow
{
    public static int MAP_COUNT = 5;
    
    private const string MAP_RES_PATH = "Prefabs/UI/WorldMap/";   
    private const string MAP_NAME_PREFIX = "map_";
    private const string MAP_POINT_PREFIX = "point_";
    private const string POINT_RES_NAME = "world_map_point";
    private const int MAP_WIDTH = 1136;
    private const float TWEEN_DURATION = 0.6f;
    private const int STUN_EFFECT = 8022;

    public const float openEffDelay = 0.5f;
    public const float openEffDuration = 0.3f;

    public static int mDifficulty = 1;
    public static int mPage = -1;
    public static int mPointIndex = 0; 

    private List<WorldMapPoint> mPointsInCurrentDifficulty = null;
    private WorldMapPoint mDefaultPoint = null;

    private TM_TweenPosition tweener;
    private TM_TweenScale scaleTweener;
    private TM_TweenPosition leftCloudTweener;
    private TM_TweenPosition rightCloudTweener;
    private bool locked = false;

    public int mCurrentPage = 0;
    private int CanMovePageMax = int.Parse((string)DataCenter.mGlobalConfig.GetData("CAN_MOVE_PAGE_MAX", "VALUE"));
    private bool FogIsVisible = false;

    public int GetCurrentPage()
    {
        return mCurrentPage;
    }

    public void SetCurrentPage(int page)                                              
    {
        mCurrentPage = page;
    }

    public bool CanMoveStep(int changePage)
    {
        int newPage = mPage + changePage;
        if (changePage > 0 && newPage - GetDefaultPage() <= CanMovePageMax)
        {
            return true;
        }
        if (changePage < 0 && newPage >= 0)
        {
            return true;
        }
        return false;
    }

    public static void Reset()
    {
        mDifficulty = 1;
        mPage = -1;
        mPointIndex = 0;
    }

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_world_map_back", new DefineFactory<Button_world_map_back>());
        EventCenter.Self.RegisterEvent("Button_world_map_go_forward", new DefineFactory<Button_world_map_go_forward>());
        EventCenter.Self.RegisterEvent("Button_world_map_go_back", new DefineFactory<Button_world_map_go_back>());
        EventCenter.Self.RegisterEvent("Button_map_active_list", new DefineFactory<Button_map_active_list>());
        EventCenter.Self.RegisterEvent("Button_map_boss_list", new DefineFactory<Button_map_boss_list>());
        EventCenter.Self.RegisterEvent("Button_map_difficulty", new DefineFactory<Button_map_difficulty>());
        EventCenter.Self.RegisterEvent("Button_" + POINT_RES_NAME, new DefineFactory<Button_world_map_point>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        DataCenter.OpenWindow("SCROLL_WORLD_MAP_TOP_LEFT");
        DataCenter.OpenWindow("SCROLL_WORLD_MAP_BOTTOM_RIGHT");
        DataCenter.OpenWindow("SCROLL_WORLD_MAP_BOTTOM_LEFT");
        DataCenter.OpenWindow("SCROLL_WORLD_MAP_LEFT");
        DataCenter.OpenWindow("SCROLL_WORLD_MAP_RIGHT");

		if(PetLogicData.mFreindPetData != null) PetLogicData.mFreindPetData = null;
		if(FriendLogicData.mSelectPlayerData != null) FriendLogicData.mSelectPlayerData = null;

        RefreshPointsData();
        RefreshBossState();
        InitPopupList();
      
        if (param is int)
        {
            InitMap((int)param);
        }
        else
        {
            WorldMapPoint lastPt = WorldMapPoint.Create(mPointIndex);

            if (lastPt == null || lastPt.difficulty != mDifficulty)
            {
                SetCurrentPage(GetDefaultPage());
                InitMap(GetCurrentPage());
            }
            else
            {
                SetCurrentPage(lastPt.page);
                InitMap(GetCurrentPage());
            }
        }      

        //lhcworldmap
        StartOpenTween();
        //StartOpenTween();

		if(DataCenter.Get ("IS_NEXT"))
		{
            SelectPoint(GetNextStagePoint());
            DataCenter.Set("IS_NEXT", false);
		}

        if (DataCenter.Get("IS_CURRENT"))
        {
            //int currentStage = DataCenter.Get("CURRENT_STAGE");
            //DataCenter.OpenWindow("STAGE_INFO_WINDOW", currentStage);
            SelectPoint(WorldMapPoint.Create(mPointIndex));
            DataCenter.Set("IS_CURRENT", false);
        }

		if(DataCenter.Get ("IS_WINDOW_BACK"))
		{
            int currentStage = DataCenter.Get("CURRENT_STAGE");
            DataCenter.OpenWindow("STAGE_INFO_WINDOW", currentStage);
            DataCenter.Set("IS_WINDOW_BACK", false);
		}

        if (DataCenter.Get("IS_DAILY_STAGE_BACK"))
        {
            DataCenter.Set("IS_DAILY_STAGE_BACK", false);
            DataCenter.CloseWindow("SCROLL_WORLD_MAP_WINDOW");
            DataCenter.OpenWindow(UIWindowString.daily_stage_main_window);
        }
        else if (DataCenter.Get("IS_PVE_STAGE_BACK"))
        {
            DataCenter.Set("IS_PVE_STAGE_BACK", false);
            var pt = GetNextStagePoint();

            if (pt != null)
            {
                if (pt.page != mPage)
                {
                    // 获取每章最后一关的星数（可以自动切换到下一章时的当前关卡即为本章的最后一关）
                    StageProperty property = StageProperty.Create(WorldMapPoint.Create(mPointIndex).mId);
                    string tmpKey = "STAR_" + property.mIndex.ToString() + "_" + CommonParam.mUId + "_" + CommonParam.mZoneID + "_" + CommonParam.LoginIP + ":" + CommonParam.LoginPort;
                    int beforeStar = PlayerPrefs.GetInt(tmpKey);
                    int currentStar = property.mBestStar;
                    SetCurrentPage(pt.page);

                    //跳转之前若前一章的最后一个关卡的星数有增加，则显示完打星效果后再跳转
                    if (currentStar > beforeStar)
                    {
                        // 等待打星效果结束，之后跳转界面
                        DoCoroutine(WaitToShowStarEffact(pt, currentStar - beforeStar));
                    }
                    else
                    {
                        SkipToPage(pt.page);
                    }
                }

                if (pt.unlocked)
                {
                    mPointIndex = pt.mIndex;
                }
            }
        }

        //DataCenter.OpenWindow("FRIEND_HELP_WINDOW", false);
        //DataCenter.CloseWindow ("FRIEND_HELP_WINDOW");
    }

    private IEnumerator WaitToShowStarEffact(WorldMapPoint pt, int addStarNum)
    {
        yield return new WaitForSeconds(2 + addStarNum * 0.2f);
        SkipToPage(pt.page);
    }

    public override void Close()
    {
        DataCenter.CloseWindow("ACTIVE_STAGE_WINDOW");
        DataCenter.CloseWindow("BOSS_RAID_WINDOW");
        DataCenter.CloseWindow("STAGE_INFO_WINDOW");
        DataCenter.CloseWindow("FRIEND_HELP_WINDOW");

        DataCenter.CloseWindow("SCROLL_WORLD_MAP_TOP_LEFT");
        DataCenter.CloseWindow("SCROLL_WORLD_MAP_BOTTOM_RIGHT");
        DataCenter.CloseWindow("SCROLL_WORLD_MAP_BOTTOM_LEFT");
        DataCenter.CloseWindow("SCROLL_WORLD_MAP_LEFT");
        DataCenter.CloseWindow("SCROLL_WORLD_MAP_RIGHT");

        DataCenter.CloseWindow("SCROLL_WORLD_MAP_CHAPTER_NAME_WINDOW");

        DataCenter.CloseWindow(UIWindowString.task_reward_box);

        mGameObjUI.StopAllCoroutines();
        Release();
        base.Close();
    }

    public override bool Refresh(object param)
    {
        RefreshPointsData();
        RefreshCurrentPage();
        RefreshBossState();
        return true;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "FORWARD":
                if(CanMoveStep(1))
                {
                    StartMoveStep(true);
                }
                else
                {
                    string str = string.Format(TableCommon.getStringFromStringList(STRING_INDEX.WORLD_MAP_FORWARD_TIPS), GetDefaultPage() + 1);
                    DataCenter.OnlyTipsLabelMessage(str);
                }
                break;

            case "BACK":
                if (CanMoveStep(-1))
                {
                    StartMoveStep(false);
                }
                else
                {
                    return;
                }
                break;

            case "SELECT_POINT":
                if (!locked)
                {
                    int pointIndex = new Data(objVal);
                    SelectPoint(WorldMapPoint.Create(pointIndex));
                }
                break;

            case "SELECT_DIFFICULTY":
                if (objVal is int)
                    SelectDifficulty((int)objVal);
                break;

            case "ACCEPT_BONUS":
                int bonusIndex = new Data(objVal);
                AcceptBonus(bonusIndex);                
                break;

            case "SKIP_TO_PAGE":
                SkipToPage((int)objVal);
                break;
        }
    }

    private void InitPopupList()
    {
        DataCenter.SetData("SCROLL_WORLD_MAP_BOTTOM_LEFT", "INIT_POPUP_LIST", mDifficulty);
    }

    private void InitMap(int current)
    {
        mPage = Mathf.Clamp(current, 0, MAP_COUNT - 1);
        Load(mPage - 1);
        Load(mPage);
        Load(mPage + 1);
        locked = false;
        GetSub("map_anchor").transform.localPosition = new Vector3(-mPage * MAP_WIDTH, 0, 0);

        SetBackAndForwardButton();
        Focus(mPage,true);

        //mPointIndex = 0;
        SetChapterName();
    }

    private bool StartMoveStep(bool forward)
    {
        if (locked || (forward && mPage == MAP_COUNT - 1) || (!forward && mPage == 0))
            return false;

        mGameObjUI.StopAllCoroutines();
        mPage = forward ? mPage + 1 : mPage - 1;
        OnStepStart(forward);
        StartTween(forward);
        return true;
    }

    private void Load(int page)
    {
        if (page >= 0 && page < MAP_COUNT)
        {
            GameObject map = CreateMap(page);
            OnMapLoad(map);
        }

        Unfocus(page);
    }

    private void Unload(int page)
    {
        if (page >= 0 && page < MAP_COUNT)
        {
            DestroyMap(page);
        }
    }

    private void Focus(int page,bool isUseAsync)
    {
        if (page >= 0 && page < MAP_COUNT)
        {
            GameObject map = GetMap(page);
            OnMapFocus(map,isUseAsync);
        }
    }

    private void Unfocus(int page)
    {
        if (page >= 0 && page < MAP_COUNT)
        {
            GameObject map = GetMap(page);
            OnMapUnfocus(map);
        }
    }

    private void OnStepStart(bool forward)
    {        
        if (forward)
        {
            Load(mPage + 1);            
        }
        else
        {
            Load(mPage - 1);
        }

        Focus(mPage,false);
        //mPointIndex = 0;

        SetBackAndForwardButton();
        SetChapterName();
    }

    private void OnStepEnd(bool forward)
    {
        if (forward)
        {
            Unfocus(mPage - 1);
            Unload(mPage - 2);
        }
        else
        {
            Unfocus(mPage + 1);
            Unload(mPage + 2);
        }
    }

    private void OnMapLoad(GameObject map)
    {
        ForEachMapPoint(map, (point, i) => point.GetComponent<UISprite>().enabled = false);
    }

    private void OnMapFocus(GameObject map,bool isUseAsync)
    {
        DataCenter.OpenWindow(UIWindowString.task_reward_box);
        ObjectManager.Self.ClearAll();
        if(isUseAsync) mGameObjUI.StartCoroutine(SetMapPointActiveIE(map,true));
        else SetMapPointActive(map,true);
        
    }

    private void OnMapUnfocus(GameObject map)
    {
        SetMapPointActiveIE(map, false);
    }

    private void SetBackAndForwardButton()
    {
        //SetVisible("world_map_go_forward", true);
        //SetVisible("world_map_go_back", true);
        DataCenter.OpenWindow("SCROLL_WORLD_MAP_LEFT");
        DataCenter.OpenWindow("SCROLL_WORLD_MAP_RIGHT");

        if (mPage == 0)
            DataCenter.CloseWindow("SCROLL_WORLD_MAP_LEFT");
        if (mPage == MAP_COUNT - 1)
            DataCenter.CloseWindow("SCROLL_WORLD_MAP_RIGHT");

        //added by xuke 箭头红点
        AdventureNewMarkManager.Self.RefreshArrowNewMark(ScrollWorldMapWindow.mDifficulty,ScrollWorldMapWindow.mPage);
        //end
    }
    /// <summary>
    /// 设置章节名称
    /// </summary>
    private void SetChapterName() 
    {
        DataCenter.OpenWindow("SCROLL_WORLD_MAP_CHAPTER_NAME_WINDOW");
    }

    private GameObject CreateMap(int page)
    {
        GameObject map = GetMap(page);

        if (map != null)
            return map;

        map = GameCommon.LoadAndIntanciateUIPrefabs(MAP_RES_PATH, GetMapName(page), "map_anchor");
        map.transform.localScale = Vector3.one;
        map.transform.localPosition = new Vector3(MAP_WIDTH * page, 0, 0);
        map.name = GetMapName(page);
        return map;
    }

    private GameObject GetMap(int page)
    {
        return GetSub(GetMapName(page));
    }

    private void DestroyMap(int page)
    {
        GameObject map = GetMap(page);

        if (map != null)
            GameObject.DestroyImmediate(map);
        //Resources.UnloadUnusedAssets();
    }

    private GameObject CreateMapPointInfoObject(GameObject point)
    {
        //if (point == null || (point != null && point.transform.childCount > 0))
        //    return point;
        //

        GameObject obj = GameCommon.LoadAndIntanciateUIPrefabs(MAP_RES_PATH, POINT_RES_NAME, point);
        obj.name = POINT_RES_NAME;
        return obj;
    }

    private void DestroyMapPointInfoObject(GameObject point)
    {
        GameObject infoObj = GameCommon.FindObject(point, "world_map_point");

        if (infoObj != null)
            GameObject.DestroyImmediate(infoObj);
        //Resources.UnloadUnusedAssets();
    }

    private GameObject GetMapPoint(GameObject map, int number)
    {
        return GameCommon.FindObject(map, GetMapPointName(number));
    }

    private void SetMapPointActive(GameObject map,bool active) {
        //  mGameObjUI.StopAllCoroutines();
        if(active) {
            int i=0;
            GameObject point=GetMapPoint(map,i);

            while(point!=null) {
                GameObject info = GameCommon.FindObject(point, "world_map_point");
                if (info == null)
                {
                     info = CreateMapPointInfoObject(point);                   
                }
                else
                {
                     info = GameCommon.FindObject(point, "world_map_point");
                }
                InitMapPointInfo(info, i);
                point = GetMapPoint(map, ++i);                
            }
        } else {
            int i=0;
            GameObject point=GetMapPoint(map,i);
            while(point!=null) {
                DestroyMapPointInfoObject(point);
                point=GetMapPoint(map,++i);
            }
        }
        //ForEachMapPoint(map,(obj,i) => DestroyMapPointInfoObject(obj));
    }


    private IEnumerator SetMapPointActiveIE(GameObject map, bool active)
    {
        //  mGameObjUI.StopAllCoroutines();
        if(active) {
            int i=0;
            GameObject point=GetMapPoint(map,i);

            while(point!=null) {
                GameObject info = GameCommon.FindObject(point, "world_map_point");
                if (info == null)
                {
                     info = CreateMapPointInfoObject(point);                   
                }
                else
                {
                     info = GameCommon.FindObject(point, "world_map_point");
                }
                InitMapPointInfo(info, i);
                point=GetMapPoint(map,++i);
                yield return null;
            }
        }
        else {
            int i=0;
            GameObject point=GetMapPoint(map,i);
            while(point!=null) {
                DestroyMapPointInfoObject(point);
                point=GetMapPoint(map,++i);
            }
        }
            //ForEachMapPoint(map,(obj,i) => DestroyMapPointInfoObject(obj));
    }

    private void ForEachMapPoint(GameObject map, Action<GameObject, int> act)
    {
        int i = 0;
        GameObject point = GetMapPoint(map, i);

        while (point != null)
        {
            act(point, i);
            point = GetMapPoint(map, ++i);
        }
    }

    private int GetMapPointCount(GameObject map)
    {
        int count = 0;
        ForEachMapPoint(map, (obj, i) => ++count);
        return count;
    }

    private string GetMapName(int page)
    {
        return MAP_NAME_PREFIX + page.ToString();
    }

    private string GetMapPointName(int number)
    {
        return MAP_POINT_PREFIX + number.ToString();
    }

    private void InitMapPointInfo(GameObject obj, int number)
    {
        WorldMapPoint point = WorldMapPoint.Create(mDifficulty, mPage, number);

        if (point != null)
            InitMapPointInfo(obj, point);
    }

    private void InitMapPointInfo(GameObject obj, WorldMapPoint point)
    {
        GameObject basic = GameCommon.FindObject(obj, "basic");
        InitPointButton(obj, point.mIndex);
        InitPointBasic(basic, point.unlocked);

        // 战争迷雾已弃用 xxt
        // 战争迷雾
        //GameCommon.SetUIVisiable(obj.transform.parent.gameObject, "fog", FogIsVisible);//!point.unlocked);

        if (point.mType == MapPointType.Bonus && point.mId > 0)
        {
            StageBonus bonus = StageBonus.Create(point.mId);
            if (bonus != null)
                InitPointBonusInfo(obj,bonus,point);
        }
        else if (point.mType == MapPointType.Normal && point.mId > 0)
        {
            StageProperty property = StageProperty.Create(point.mId);

            if (property != null)
                InitPointStageInfo(obj, property, point);
        }
        else if (point.mType == MapPointType.Boss && point.mId > 0)
        {
            StageProperty property = StageProperty.Create(point.mId);
            //lhcworldmap
            if(property!=null)
                InitPointBossInfo(obj,property,point);
        }
    }

    private void InitPointButton(GameObject obj, int index)
    {
        UIButtonEvent evt = obj.GetComponent<UIButtonEvent>();
        evt.mData.set("POINT_INDEX", index);
    }

    private void InitPointBonusInfo(GameObject obj, StageBonus bonus, WorldMapPoint point)
    {
        GameCommon.SetUIVisiable(obj, "bonus", true);
        GameCommon.SetUIVisiable(obj, "element", false);
        GameCommon.SetUIVisiable(obj, "stage_number", false);
        GameCommon.SetUIVisiable(obj, "star_level", false);
        GameObject bonusBox = GameCommon.FindObject(obj, "bonus");

        if (bonus.accepted)
            bonusBox.GetComponent<UISprite>().spriteName = "ui_box_open";
        else
            bonusBox.GetComponent<UISprite>().spriteName = "ui_box_close";

        GameCommon.SetUIVisiable(obj, "ec_ui_baoxiangeffect", !bonus.accepted);
        GameCommon.SetUIVisiable(obj, "unlock_finger", bonus.unlocked && !bonus.accepted);
    }

    private void InitPointStageInfo(GameObject obj, StageProperty property, WorldMapPoint point)
    {
        InitStageInfo(obj, property);
        //lhcworldmap
        if (mDefaultPoint != null && WorldMapPoint.Compare(point, mDefaultPoint) == 0)
        {
            ShowCharactorModel(obj);
            GameCommon.SetUIVisiable(obj, "can_battle", true); 
        }
    }

    private void InitPointBossInfo(GameObject obj, StageProperty property, WorldMapPoint point)
    {
        InitStageInfo(obj, property);

        if (mDefaultPoint != null && WorldMapPoint.Compare(point, mDefaultPoint) == 0)
        {
            //lhcworldmap
            ShowCharactorModel(obj);
            GameCommon.SetUIVisiable(obj, "can_battle", true); 
        }
        else if (point.unlocked || (mDefaultPoint != null && point.page <= mDefaultPoint.page))
        {
            ShowBossModel(obj,property);
        }
    }

    private void InitStageInfo(GameObject obj, StageProperty property)
    {
        GameCommon.SetUIVisiable(obj, "can_battle", false); 
        GameCommon.SetUIVisiable(obj, "bonus", false);
        GameCommon.SetUIVisiable(obj, "element", false);
        GameCommon.SetUIVisiable(obj, "stage_number", true);
        GameCommon.SetUIVisiable(obj, "star_level", true);
        //ELEMENT_TYPE element = property.stageElement;
        //int number = property.level;
        string label = TableCommon.GetStringFromStageConfig(property.mIndex, "STAGENUMBER");//GetElementNumber(element).ToString() + "-" + number.ToString();
        GameCommon.SetUIText(obj, "stage_number", label);
        //GameCommon.SetElementIcon(obj, "element", (int)element);

        GameObject[] stars = new GameObject[3] {
            GameCommon.FindObject(obj, "star_1"), 
            GameCommon.FindObject(obj, "star_2"), 
            GameCommon.FindObject(obj, "star_3") };

        if (property.passed)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].SetActive(true);
            }
            GlobalModule.DoCoroutine(ShowStars(stars, property));
            //GameCommon.SetUIVisiable(obj, "star_1", property.mBestStar > 0);
            //GameCommon.SetUIVisiable(obj, "star_2", property.mBestStar > 1);
            //GameCommon.SetUIVisiable(obj, "star_3", property.mBestStar > 2);
        }
        else
        {
            GameCommon.SetUIVisiable(obj, "star_1", false);
            GameCommon.SetUIVisiable(obj, "star_2", false);
            GameCommon.SetUIVisiable(obj, "star_3", false);
        }
    }

    private IEnumerator ShowStars(GameObject[] stars, StageProperty property)
    {
        string tmpKey = "STAR_" + property.mIndex.ToString() + "_" + CommonParam.mUId + "_" + CommonParam.mZoneID + "_" + CommonParam.LoginIP + ":" + CommonParam.LoginPort;
        int beforeStar = PlayerPrefs.GetInt(tmpKey);
        PlayerPrefs.SetInt(tmpKey, property.mBestStar);
        for (int i = 0; i < beforeStar; i++) // 设置已得星数的sprite
        {
            stars[i].GetComponent<UISprite>().spriteName = "ui_jiesuan_wujiaoxing";
        }
        yield return new WaitForSeconds(1.0f);
        for (int i = beforeStar; i < property.mBestStar; ++i) // 播放新得星数的特效，并设置sprite
        {
            if (stars[i] != null)
            {
                GameCommon.SetUIVisiable(stars[i], "new_ui_map_star", false);
                GameCommon.SetUIVisiable(stars[i], "new_ui_map_star", true);
            }
            yield return new WaitForSeconds(0.2f);
            if(stars[i]!=null)
                stars[i].GetComponent<UISprite>().spriteName = "ui_jiesuan_wujiaoxing";
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void InitPointBasic(GameObject obj, bool unlocked)
    {
        UISprite sprite = obj.GetComponent<UISprite>();

        if (!unlocked)
        {
            sprite.spriteName = "ui_point_off";
        }
        else
        {
            switch (mDifficulty)
            {
                case 1:
                    sprite.spriteName = "ui_point_normal";
                    break;
                case 2:
                    sprite.spriteName = "ui_point_hard";
                    break;
                case 3:
                    sprite.spriteName = "ui_point_mast";
                    break;
            }
        }
    }

    private List<WorldMapPoint> GetAllPointsInLoadedPage(int page)
    {
        GameObject map = GetMap(page);

        if (map == null)
            return null;

        List<WorldMapPoint> points = new List<WorldMapPoint>();
        ForEachMapPoint(map, (point, i) => points.Add(WorldMapPoint.Create(mDifficulty, page, i)));
        return points;
    }

    private void RefreshPointsData()
    {
        mPointsInCurrentDifficulty = WorldMapPoint.CreateListByDifficulty(mDifficulty);
        mDefaultPoint = null;

        if (mPointsInCurrentDifficulty.Count == 0)
            return;

        for (int i = 0; i < mPointsInCurrentDifficulty.Count; ++i)
        {
            WorldMapPoint pt = mPointsInCurrentDifficulty[i];

            if (pt == null)
                break;

            if ((pt.mType == MapPointType.Normal || pt.mType == MapPointType.Boss) && pt.unlocked && !pt.passed)
            {
                if (mDefaultPoint == null || WorldMapPoint.Compare(mDefaultPoint, pt) > 0)
                {
                    mDefaultPoint = pt;
                }
            }        
        }
    }

    private int GetDefaultPage()
    {
        return mDefaultPoint == null ? 0 : mDefaultPoint.page;
    }

    private void SelectDifficulty(int difficulty)
    {
        if (mDifficulty == difficulty)
            return;

        Release();
        mDifficulty = difficulty;
        RefreshPointsData();

        InitMap(GetDefaultPage());
    }

    private void SelectDifficultyByPopupList(int index)
    {
        SelectDifficulty(3 - index);
    }

    private void ShowBossModel(GameObject obj, StageProperty property)
    {
        
        int bossId = TableCommon.GetNumberFromStageConfig(property.mIndex, "HEADICON");

        if (bossId > 0)
        {
        
            GameObject uiPoint = GameCommon.FindObject(obj, "uiPoint");
            //lhcworldmap
            BaseObject boss=GameCommon.ShowModel(bossId,uiPoint,0.5f);
            //yield return new WaitForSeconds(0.1f);
            //GlobalModule.DoCoroutine(GameCommon.CreateMainBossInFrames(bossId,new GameObject("_role_")));

             
              if (boss == null || boss.mMainObject == null)
                      return;

                  ChangeParticleRenderQueue changeQueue = boss.mMainObject.GetComponentInChildren<ChangeParticleRenderQueue>();
            
                  if (changeQueue != null)
                  {
                      // 避免模型层级的变化破坏UI层级
                      Component.DestroyImmediate(changeQueue);
                  }

                  if (property.passed)
                  {
                      boss.PlayAnim("stun");
                      AttachEffect(boss, 30002, 4f);
                  }
                  else
                  {
                      tEvent playIdle = EventCenter.Start("RoleSelUI_PlayIdleEvent");
                      float delay = UnityEngine.Random.Range(0f, 8f);
                      Func<float> repeatAct = () => { boss.PlayMotion("cute", playIdle); return UnityEngine.Random.Range(8f, 12f); };
                      Func<bool> finishCondition = () => !GuideManager.ExistInVisibleAliveObjects(x => x == boss);
                      GuideManager.CreateRepeatEvent(delay, repeatAct, finishCondition);
                  }        

        }
    }

    private void ShowCharactorModel(GameObject obj)
    {
        GameObject uiPoint = GameCommon.FindObject(obj, "uiPoint");
        BaseObject character = GameCommon.ShowCharactorModel(uiPoint, 0.65f);
        character.mMainObject.transform.localEulerAngles = new Vector3(0f, 125f, 0f);
        character.PlayAnim("run");
        AttachEffect(character, 30001, 5f);
    }

    private void AttachEffect(BaseObject owner, int effectIndex, float scale)
    {
        BaseEffect effect = owner.PlayEffect(effectIndex, null);

        if (effect != null)
        {
            effect.mGraphObject.transform.localScale = Vector3.one * scale;
            ChangeParticleRenderQueue scrip = effect.mGraphObject.AddComponent<ChangeParticleRenderQueue>();
            scrip.renderQueue = 3001;
        }
    }

    private void StartTween(bool forward)
    {
        StopTween();
        locked = true;

        int from = -MAP_WIDTH * (forward ? mPage - 1 : mPage + 1);
        int to = -MAP_WIDTH * mPage;

        tweener = EventCenter.Start("TM_TweenPosition") as TM_TweenPosition;
        tweener.mTarget = GetSub("map_anchor");
        tweener.mBefore = 0f;
        tweener.mDuration = TWEEN_DURATION;
        tweener.mAfter = 0f;
        tweener.mFrom = new Vector3(from, 0, 0);
        tweener.mTo = new Vector3(to, 0, 0);
        tweener.curve = x => 1 - (1 - x) * (1 - x);
        tweener.onElapsed += x => { locked = false; OnStepEnd(forward); };
        tweener.DoEvent();
    }

    private void StopTween()
    {
        if (tweener != null && !tweener.GetFinished())
        {
            tweener.Finish();
            tweener = null;
        }

        locked = false;
    }

    private void StartOpenTween()
    {
        StopOpenTween();

        scaleTweener = EventCenter.Start("TM_TweenScale") as TM_TweenScale;
        scaleTweener.mTarget = mGameObjUI;
        scaleTweener.mBefore = openEffDelay;
        scaleTweener.mDuration = openEffDuration;
        scaleTweener.mFrom = Vector3.one * 1.2f;
        scaleTweener.mTo = Vector3.one;
        scaleTweener.DoEvent();

        GameObject leftCloud = GetSub("cloud_left");
        GameObject rightCloud = GetSub("cloud_right");
        leftCloud.SetActive(true);
        rightCloud.SetActive(true);
        
        
        leftCloudTweener = EventCenter.Start("TM_TweenPosition") as TM_TweenPosition;
        leftCloudTweener.mTarget = leftCloud;
        leftCloudTweener.mBefore = openEffDelay;
        leftCloudTweener.mDuration = openEffDuration;
        leftCloudTweener.mFrom = new Vector3(-200, 0, -500);
        leftCloudTweener.mTo = new Vector3(-1000, 0, -500);
        leftCloudTweener.onElapsed += x => x.SetActive(false);
        leftCloudTweener.DoEvent();
        
        rightCloudTweener = EventCenter.Start("TM_TweenPosition") as TM_TweenPosition;
        rightCloudTweener.mTarget = rightCloud;
        rightCloudTweener.mBefore = openEffDelay;
        rightCloudTweener.mDuration = openEffDuration;
        rightCloudTweener.mFrom = new Vector3(200, 0, -500);
        rightCloudTweener.mTo = new Vector3(1000, 0, -500);
        rightCloudTweener.onElapsed += x => x.SetActive(false);
        rightCloudTweener.DoEvent();
    }

    private void StopOpenTween()
    {
        if (scaleTweener != null && !scaleTweener.GetFinished())
        {
            scaleTweener.Finish();
            scaleTweener = null;
            mGameObjUI.transform.localScale = Vector3.one;
        }

        if (leftCloudTweener != null && !leftCloudTweener.GetFinished())
        {
            leftCloudTweener.Finish();
            leftCloudTweener = null;
            SetVisible("cloud_left", false);
        }

        if (rightCloudTweener != null && !rightCloudTweener.GetFinished())
        {
            rightCloudTweener.Finish();
            rightCloudTweener = null;
            SetVisible("cloud_right", false);
        }
    }

    private void Release()
    {
        ObjectManager.Self.ClearAll();
        StopTween();
        StopOpenTween();

        for (int i = 0; i < MAP_COUNT; ++i)
            Unload(i);

        mPointsInCurrentDifficulty = null;
        mDefaultPoint = null;
    }

    private void SelectPoint(WorldMapPoint point)
    {
        if (point == null)
            return;

        if (point.page != mPage)
            SkipToPage(point.page);

        if (point.unlocked)
            mPointIndex = point.mIndex;

        if (point.mType == MapPointType.Bonus)
            OnSelectBonus(point.mId);
        else
            OnSelectStage(point.mId);
    }

    private void OnSelectStage(int stageIndex)
    {
        StageProperty property = StageProperty.Create(stageIndex);

        if(property.unlocked)
            DataCenter.OpenWindow("STAGE_INFO_WINDOW", stageIndex);       
    }

    private void OnSelectBonus(int bonusIndex)
    {
        StageBonus bonus = StageBonus.Create(bonusIndex);

        if (bonus.accepted)
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_HAD_GET_AWARD);
        else
            DataCenter.OpenWindow("WORLD_MAP_BONUS_WINDOW", bonusIndex);
    }

    private void AcceptBonus(int bonusIndex)
    {
        StageBonus bonus = StageBonus.Create(bonusIndex);

        if (bonus != null && bonus.Accept())
        {          
            RefreshCurrentPage();

            if(StageBonusLogicData.GetAcceptedCount() == 1)
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_HAD_GET_AWARD, () => DataCenter.OpenWindow("ON_HOOK_TIP_WINDOW"));
            else
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_HAD_GET_AWARD);     
        }
    }

    private void RefreshCurrentPage()
    {
        Unfocus(mPage);
        Focus(mPage,false);
    }

    private void SkipToPage(int page)
    {
        if (page == mPage)
            return;

        page = Mathf.Clamp(page, 0, MAP_COUNT - 1);
        StopTween();

        Unload(mPage - 1);
        Unload(mPage);
        Unload(mPage + 1);

        mPage = page;

        Load(mPage - 1);
        Load(mPage);
        Load(mPage + 1);

        GetSub("map_anchor").transform.localPosition = new Vector3(-mPage * MAP_WIDTH, 0, 0);
        SetBackAndForwardButton();
        Focus(mPage,true);

        SetChapterName();
    }

    private WorldMapPoint GetNextStagePoint()
    {
        WorldMapPoint pt = WorldMapPoint.Create(mPointIndex);

        if (pt == null || pt.mType == MapPointType.Bonus || pt.mType == MapPointType.Invalid || pt.difficulty != mDifficulty)
            return null;

        StageProperty property = StageProperty.Create(pt.mId);

        if (property != null)
        {
            int nextStageIndex = property.nextIndex;

            if (nextStageIndex > 0)
                return mPointsInCurrentDifficulty.Find(p => (p.mType == MapPointType.Normal || p.mType == MapPointType.Boss) && p.mId == nextStageIndex);
        }

        return null;
    }

    private void RefreshBossState()
    {
        DataCenter.SetData("SCROLL_WORLD_MAP_BOTTOM_RIGHT", "REFRESH", null);
    }
}


public class ScrollWorldMapBottomLeftWindow : tWindow
{
    //by chenliang
    //begin

    private GameObject mPopupListWindow;

    //end
    public override void OnOpen()
    {
        //by chenliang
        //begin

//         PopupListListener listener = GetComponent<PopupListListener>("map_difficulty");
//         listener.onSelect = SelectDifficulty;
//----------------
        if (mPopupListWindow == null)
            mPopupListWindow = GameObject.Find("scroll_world_map_bottom_left_window_test");
        MainSelMissionPopupList tmpPopupList = GameCommon.FindComponent<MainSelMissionPopupList>(mPopupListWindow, "popup_list");
        tmpPopupList.SelectCallback += SelectDifficulty;


        //end
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "INIT_POPUP_LIST":
                InitPopupList((int)objVal);
                break;
        }
    }

    private void InitPopupList(int difficulty)
    {
        //by chenliang
        //begin

//        PopupListListener listener = GetComponent<PopupListListener>("map_difficulty");
//        listener.Select(3 - difficulty);
//-------------------
        if (mPopupListWindow == null)
            mPopupListWindow = GameObject.Find("scroll_world_map_bottom_left_window_test");
        MainSelMissionPopupList tmpPopupList = GameCommon.FindComponent<MainSelMissionPopupList>(mPopupListWindow, "popup_list");
        tmpPopupList.Init();
        tmpPopupList.SetCurrentButton((MAIN_POPUP_LIST_BUTTON_TYPE)(difficulty - 1));

        //end
    }

    private void SelectDifficulty(int index)
    {
        //by chenliang
        //begin

//        DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "SELECT_DIFFICULTY", 3 - index);
//-----------------
        DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "SELECT_DIFFICULTY", index + 1);

        //end
    }
}


public class ScrollWorldMapBottomRightWindow : tWindow
{
    public override void OnOpen()
    {
        base.OnOpen();
        DataCenter.SetData("TOTAL_TASK_WINDOW", "REFRESH_TASK_NEWMARK", true);
        //added by xuke 添加队伍按钮红点提示
        DataCenter.SetData("SCROLL_WORLD_MAP_BOTTOM_RIGHT", "REFRESH_TEAM_BTN_NEW_MARK", null);
        //end
    }

    public override bool Refresh(object param)
    {
        //GetSub("ec_ui_huodong").SetActive(GameCommon.FunctionIsUnlock(UNLOCK_FUNCTION_TYPE.DAILY_COPY_LEVEL));
		GameObject obj =  GetSub("ec_ui_tianmo");
		if(obj != null)
		{
			obj.SetActive(GameCommon.FunctionIsUnlock(UNLOCK_FUNCTION_TYPE.BOSS_RAIN) && SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_BOSS));
		}
        
        return true;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex) 
        {
            case "REFRESH_NEW_MARK":
                GameObject _newMark = GetCurUIGameObject("NewMark");
                _newMark.SetActive(false);
                List<bool> _newMarkStateList = (List<bool>)objVal;
                foreach(bool stateInfo in _newMarkStateList)
                {
                    if (stateInfo)
                    {
                        _newMark.SetActive(true);
                        break;
                    }
                }

                if (!Guide.isActive && TotalTaskManager.popupFlag)
                {
                    // 弹出关卡任务窗口，提示玩家可领取任务奖励
                    DataCenter.OpenWindow(UIWindowString.total_task);
                }
            break;
            case "REFRESH_TEAM_BTN_NEW_MARK":
                CheckTeamBtn_NewMark();
                break;//DataCenter.SetData("TOTAL_TASK_WINDOW", "REFRESH_TEAM_BTN_NEW_MARK", null);
        }
    }

    /// <summary>
    /// 检测队伍按钮的红点状态
    /// </summary>
    private void CheckTeamBtn_NewMark()
    {
        GameObject _teamBtnObj = GameCommon.FindObject(mGameObjUI, "world_map_CharacterBtn");
        GameCommon.SetNewMarkVisible(_teamBtnObj, TeamManager.CheckTeamHasNewMark());
    }
}


public class Button_world_map_go_forward : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "FORWARD", null);
        return true;
    }
}

public class Button_world_map_go_back : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "BACK", null);
        return true;
    }
}

public class Button_world_map_point : CEvent
{
    public override bool _DoEvent()
    {
        int index = get("POINT_INDEX");
        DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "SELECT_POINT", index);
        return base._DoEvent();
    }
}

public class Button_map_active_list : CEvent
{
    public override bool _DoEvent()
    {
		//if(!GameCommon.FunctionIsUnlock(UNLOCK_FUNCTION_TYPE.DAILY_COPY_LEVEL))
		//{
		//	DataCenter.OpenWindow ("PLAYER_LEVEL_UP_SHOW_WINDOW", false);
		//	DataCenter.SetData ("PLAYER_LEVEL_UP_SHOW_WINDOW", "NEED_LEVEL", UNLOCK_FUNCTION_TYPE.DAILY_COPY_LEVEL);
		//	return true;
		//}
        //

        //DataCenter.OpenWindow("ACTIVE_STAGE_WINDOW");
        //GlobalModule.DoCoroutine(DoRequest());
        return true;
    }

    //private IEnumerator DoRequest()
    //{
    //    //var refreshReq = new BattleActiveFreshRequester();
    //   // yield return refreshReq.Start();

    //    //if (refreshReq.respMsg.freshFlag == 1)
    //    //{
    //        //yield return new BattleActiveMapRequester().Start();
    //    //}

    //    //DataCenter.OpenWindow("ACTIVE_STAGE_WINDOW");
    //    yield return () => { };
    //}
}

public class Button_map_boss_list : CEvent
{
    public override bool _DoEvent()
    {
		if(!GameCommon.FunctionIsUnlock(UNLOCK_FUNCTION_TYPE.BOSS_RAIN))
		{
			DataCenter.OpenWindow ("PLAYER_LEVEL_UP_SHOW_WINDOW", false);
			DataCenter.SetData ("PLAYER_LEVEL_UP_SHOW_WINDOW", "NEED_LEVEL", UNLOCK_FUNCTION_TYPE.BOSS_RAIN);
			return true;
		}

//        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.BossRaidWindow);

//        EventCenter.Start("Button_trial_window_back_btn").DoEvent();
		DataCenter.OpenWindow ("BOSS_RAID_WINDOW");
        DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.FEATS);
        return true;
    }
}

public class Button_map_difficulty : CEvent
{
    public override bool _DoEvent()
    {
        return base._DoEvent();
    }
}

public class Button_world_map_back : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("SCROLL_WORLD_MAP_WINDOW");
        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
        return true;
    }
}