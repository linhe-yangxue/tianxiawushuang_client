using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

/// <summary>
/// Boss出现界面
/// BOSS_APPEAR_WINDOW	boss_appear01_window	BossAppearWindow
/// </summary>
public class BossAppearWindow : tWindow
{
    public RequestBossBattleEvent mRequestEvent;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_start_boss_battle", new DefineFactory<Button_start_boss_battle>());
        EventCenter.Self.RegisterEvent("Button_leave_boss_battle", new DefineFactory<Button_leave_boss_battle>());
        CommonParam.isPveBossFirst = 0;
        Close();
    }
    
    public override void Open(object param)
    {
        base.Open(param);

        BossRaidWindow win = DataCenter.GetData("BOSS_RAID_WINDOW") as BossRaidWindow;
        win.set("IS_RETURN_STAGE", true);

        // 刷新boss状态
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_BOSS, true);

		SetVisible("boss_appear01_inner_window", false);
		GuideManager.ExecuteDelayed(() => SetVisible("boss_appear01_inner_window", true), 1f);

        mRequestEvent = param as RequestBossBattleEvent;

        Boss_GetDemonBossList_BossData bossData = mRequestEvent.mBossData;
        if (bossData != null)
        {
            int bossID = bossData.tid;
            DataRecord bossConfig = DataCenter.mBossConfig.GetRecord(bossID);
            if (bossConfig != null)
            {
                //GameObject bossModel = GameCommon.FindObject(mGameObjUI, "boss_model");
                //if (bossModel != null)
                {
                    //ActiveBirthForUI birthUI = mGameObjUI.GetComponentInChildren<ActiveBirthForUI>();
                    ActiveBirthForUI birthUI = GameCommon.FindComponent<ActiveBirthForUI>(mGameObjUI, "boss_model");
                    if (birthUI != null)
                    {
                        birthUI.mBirthConfigIndex = bossID;
                        birthUI.mObjectType = (int)OBJECT_TYPE.BIG_BOSS;
                        birthUI.Init();
                        if (birthUI.mActiveObject != null)
                        {
                            birthUI.mActiveObject.SetScale(120f);
                            birthUI.mActiveObject.PlayAnim("cute");

                            GameObject tmpObj = null;
                            if(birthUI.mActiveObject.mMainObject != null)
                            {
                                tmpObj = birthUI.mActiveObject.mMainObject;
                                if(tmpObj != null && tmpObj.transform.parent != null)
                                    tmpObj = tmpObj.transform.parent.gameObject;
                                if(tmpObj != null && tmpObj.transform.parent != null)
                                    tmpObj = tmpObj.transform.parent.gameObject;
                            }
                            if (tmpObj != null && tmpObj.name == "3d_scene")
                            {
                                Vector3 tmpPos = tmpObj.transform.localPosition;
                                tmpPos.x = -100000.0f;
                                tmpObj.transform.localPosition = tmpPos;
                            }

                            BossBirthOnApearUI modelScript = birthUI.mActiveObject.mMainObject.AddComponent<BossBirthOnApearUI>();
                            if (modelScript != null)
                                modelScript.mActiveObject = birthUI.mActiveObject;
                        }
                    }
                }
                //                else
                //                {
                //                    EventCenter.Log(LOG_LEVEL.ERROR, "No exist boss_model game object at window");
                //                }

                for (int i = 0; i < 5; i++)
                {
                    GameObject obj = GameCommon.FindObject(mGameObjUI, "first_bossziti_" + i.ToString());
                    if (obj != null)
                        MonoBehaviour.Destroy(obj);
                }
                string strElement = bossConfig.getData("ELEMENT_INDEX");
                GuideManager.ExecuteDelayed(() => { if (mGameObjUI != null) GameCommon.LoadUIPrefabs("first_bossziti_" + strElement, mGameObjUI); }, 0.5f);

                GameCommon.SetUIText(mGameObjUI, "level_num", bossData.bossLevel.ToString()/*bossConfig.getData("LV")*/);
                //??? GameCommon.SetUIText(mGameObjUI, "attack_num", ???);
                GameCommon.SetUIText(mGameObjUI, "health_num", bossData.hpLeft.ToString()/*bossConfig.getData("BASE_HP")*/);

                GameCommon.SetUIText(mGameObjUI, "attack_num", BigBoss.GetBossAttack(bossData.quality, bossData.bossLevel).ToString()/*bossConfig.getData("BASE_ATTACK")*/);

                // Boss Element
                int elementIndex = bossConfig.getData("ELEMENT_INDEX");
                string strAtlasName = TableCommon.GetStringFromElement(elementIndex, "HEAD_ATLAS_NAME");//"ELEMENT_ATLAS_NAME");
                string strSpriteName = TableCommon.GetStringFromElement(elementIndex, "HEAD_SPRITE_NAME");//"ELEMENT_SPRITE_NAME");
                UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
                UISprite element = GameCommon.FindObject(mGameObjUI, "element_logo").GetComponent<UISprite>();
                element.atlas = tu;
                element.spriteName = strSpriteName;

                NiceData buttonData = GameCommon.GetButtonData(mGameObjUI, "start_boss_battle");
                if (buttonData != null)
                    buttonData.set("BOSS_DATA", bossData);

				GameCommon.GetButtonData(mGameObjUI, "leave_boss_battle").set ("LEAVE_BOSS_DATA", bossData);
                return;
            }
        }
        
        EventCenter.Log(LOG_LEVEL.ERROR, "ERROR: may be boss config error, or not set boss data when appear window open");
    }
}

public class Button_start_boss_battle : CEvent
{
    public override bool _DoEvent()
    {
        Boss_GetDemonBossList_BossData bossData = getObject("BOSS_DATA") as Boss_GetDemonBossList_BossData;

        if (bossData != null)
        {
			DataCenter.CloseWindow("BOSS_APPEAR_WINDOW");
         //   MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
			DataRecord config = DataCenter.mBossConfig.GetRecord(bossData.tid);
            DataCenter.SetData("BOSS_STAGE_INFO_WINDOW", "BOSS_DATA", bossData);
			DataCenter.OpenWindow("BOSS_STAGE_INFO_WINDOW", (int)config.get("SCENE_ID"));
			DataCenter.SetData("SCROLL_WORLD_MAP_CHAPTER_NAME_WINDOW", "LEAVE_BOSS_DATA", bossData);
        }
        else
        {
            Log("ERROR: No set boss data to start button [BOSS_DATA]");
        }

        return true;
    }
}

public class Button_leave_boss_battle : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("BOSS_APPEAR_WINDOW");

        BossAppearWindow bossWindow = DataCenter.GetData("BOSS_APPEAR_WINDOW") as BossAppearWindow;
        bossWindow.mRequestEvent.NextActive();

		Boss_GetDemonBossList_BossData bossData = getObject("LEAVE_BOSS_DATA") as Boss_GetDemonBossList_BossData;

        if (bossData != null)
        {
			DataRecord config = DataCenter.mBossConfig.GetRecord(bossData.tid);
			DataCenter.SetData("SCROLL_WORLD_MAP_CHAPTER_NAME_WINDOW", "LEAVE_BOSS_DATA", bossData);
//			DataCenter.OpenWindow("BOSS_STAGE_INFO_WINDOW", (int)config.get("SCENE_ID"));
        }
        else
        {
            Log("ERROR: No set boss data to start button [BOSS_DATA]");
        }

        //MainProcess.ClearBattle();
        //MainProcess.LoadRoleSelScene();

        return true;
    }
}

public class RequestBossBattleEvent : CEvent
{
    public eACTIVE_AFTER_APPEAR mNextActive = eACTIVE_AFTER_APPEAR.SELECT_LEVEL;
    public int mCurrentStageIndex;
    public bool mIsGuide = false;
    public Boss_GetDemonBossList_BossData mBossData;

    public override bool _DoEvent()
    {
		DataCenter.CloseWindow("PVE_ACCOUNT_WIN_WINDOW");

        if (mBossData != null)
        {
            if (mNextActive == eACTIVE_AFTER_APPEAR.CLEAN_LEVEL || mNextActive == eACTIVE_AFTER_APPEAR.QUIT_CLEAN_LEVEL)
            {
                DataCenter.CloseWindow("PVE_ACCOUNT_CLEAN_WINDOW");
                BaseUI.OpenWindow("BOSS_APPEAR_WINDOW", this, false);
            }
            else
            {
                BaseUI.OpenWindow("BOSS_APPEAR_WINDOW", this, true);

                if (mIsGuide)
                    GuideManager.Notify(GuideIndex.TriggerBoss);
            }
            Button_stage_info_clean_multi.stop = true;
            Button_stage_info_clean_multi.forcedStop = true;
            GlobalModule.DoLater(() =>
            {
                DataCenter.CloseWindow("SWEEP_LIST_WINDOW"); DataCenter.CloseWindow("SWEEP_LIST_WINDOW");
                if (GameCommon.FindObject(GameObject.Find("CenterAnchor"), "sweep_list_window") != null)
                    MonoBehaviour.Destroy(GameCommon.FindObject(GameObject.Find("CenterAnchor"), "sweep_list_window"));
            }, 0.5f);

            //天魔出现
            BossRaidWindow.HasNewBossAppear = true;
        }
        else
            NextActive();

        return true;
    }
    public void NextActive()
    {
        // 由于现在挑战关卡拥有次数或时间限制或钻石消耗，因此返回到关卡选择界面，而不是直接重新挑战
        switch (mNextActive)
        {
            case eACTIVE_AFTER_APPEAR.NEXT_LEVEL:
                DataCenter.Set("IS_NEXT", true);
                MainProcess.OpenWordMapWindow();
                break;

            case eACTIVE_AFTER_APPEAR.AGAIN_LEVEL:
                //DataCenter.CloseWindow("BATTLE_END_WINDOW");
                //DataCenter.Set("CURRENT_STAGE", mCurrentStageIndex);
                //MainProcess.ClearBattle();
                //MainProcess.LoadBattleScene();
                DataCenter.Set("IS_CURRENT", true);
                MainProcess.OpenWordMapWindow();
                break;
            case eACTIVE_AFTER_APPEAR.CLEAN_LEVEL:
                DataCenter.CloseWindow("PVE_ACCOUNT_CLEAN_WINDOW");
                DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "REFRESH", null);
				MainProcess.RequestCleanStage();
                break;
            case eACTIVE_AFTER_APPEAR.QUIT_CLEAN_LEVEL:
                DataCenter.CloseWindow("PVE_ACCOUNT_CLEAN_WINDOW");
                DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "REFRESH", null);
                break;
            case eACTIVE_AFTER_APPEAR.BACK_HOME_PAGE:
                MainProcess.QuitBattle();
                MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.RoleSelWindow);
                break;
            default:
                MainProcess.OpenWordMapWindow();
                break;
        }
    }
}
