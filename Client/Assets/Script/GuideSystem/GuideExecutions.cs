using UnityEngine;
using System;
using Logic;
using Utilities;


public partial class GuideManager
{
    private static void OnNotifySucceed(GuideIndex index)
    {
        if (achieved && (int)index >= (int)GuideIndex.EnterMainUIForMap)
        {
            DataCenter.OpenWindow("GUIDE_SKIP_WINDOW");
        }

        switch (index)
        {
            case GuideIndex.Prologue:
                OnPrologue();
                break;

            case GuideIndex.SelectRole:
                OnSelectRole();
                break;

            case GuideIndex.SelectPet:
                OnSelectPet();
                break;

            case GuideIndex.SelectName:
                OnSelectName();
                break;

            case GuideIndex.NamingSucceed:
                OnNamingSucceed();
                break;

            case GuideIndex.EnterMainUIForShop:
                OnEnterMainUIForShop();
                break;

            case GuideIndex.BuyFirstFreePet:
                OnBuyFirstFreePet();
                break;
            
            case GuideIndex.GetFirstFreePet:
                OnGetFirstFreePet();
                break;
            
            case GuideIndex.GetFirstFreePetEnd:
                OnGetFirstFreePetEnd();
                break;

            //case GuideIndex.BuySecondFreePet:
            //    OnBuySecondFreePet();
            //    break;
            //
            //case GuideIndex.GetSecondFreePet:
            //    OnGetSecondFreePet();
            //    break;
            //
            //case GuideIndex.GetSecondFreePetEnd:
            //    OnGetSecondFreePetEnd();
            //    break;

            case GuideIndex.EnterMainUIForMap:
                OnEnterMainUIForMap();
                break;

            case GuideIndex.EnterWorldMap:
                OnEnterWorldMap();
                break;

            case GuideIndex.EnterStageInfo:
                OnEnterStageInfo();
                break;

            case GuideIndex.EnterTeamWindow:
                OnEnterTeamWindow();
                break;

            case GuideIndex.ChangeTeamOK:
                OnChangeTeamOK();
                break;

            case GuideIndex.ReturnToStageInfo:
                OnReturnToStageInfo();
                break;

            case GuideIndex.EnterBattle:
                OnEnterBattle();
                break;

            case GuideIndex.ReadyBattle:
                OnReadyBattle();
                break;

            case GuideIndex.EncounterMonster:
                OnEncounterMonster();
                break;

            case GuideIndex.EncounterBoss:
                OnEncounterBoss();
                break;

            case GuideIndex.BattleAccountStart:
                OnBattleAccountStart();
                break;

            case GuideIndex.BattleAccountEnd:
                OnBattleAccountEnd();
                break;

            case GuideIndex.EnterMainUIForTask:
                OnEnterMainUIForTask();
                break;

            case GuideIndex.EnterTaskWindow:
                OnEnterTaskWindow();
                break;

            case GuideIndex.AcceptTaskAward:
                OnAcceptTaskAward();
                break;

            case GuideIndex.EnterMainUIForPet:
                OnEnterMainUIForPet();
                break;

            case GuideIndex.EnterTeamWindow2:
                OnEnterTeamWindow2();
                break;

            case GuideIndex.EnterUpgradeWindow:
                OnEnterUpgradeWindow();
                break;

            case GuideIndex.PetUpgradeOK:
                OnPetUpgradeOK();
                break;

            case GuideIndex.EnterMainUIForMap2:
                OnEnterMainUIForMap2();
                break;

            case GuideIndex.EnterWorldMap2:
                OnEnterWorldMap2();
                break;

            case GuideIndex.EnterStageInfo2:
                OnEnterStageInfo2();
                break;

            case GuideIndex.EnterBattle2:
                OnEnterBattle2();
                break;

            case GuideIndex.ReadyBattle2:
                OnReadyBattle2();
                break;

            case GuideIndex.BattleAccountStart2:
                OnBattleAccountStart2();
                break;

            case GuideIndex.BattleAccountEnd2:
                OnBattleAccountEnd2();
                break;

            case GuideIndex.EnterMainUIForRolePage:
                OnEnterMainUIForRolePage();
                break;

            case GuideIndex.EnterRolePage:
                OnEnterRolePage();
                break;

            case GuideIndex.LoadRoleEquipOK:
                OnLoadRoleEquipOK();
                break;

            case GuideIndex.EnterMainUIForMap3:
                OnEnterMainUIForMap3();
                break;

            case GuideIndex.EnterWorldMap3:
                OnEnterWorldMap3();
                break;

            case GuideIndex.EnterStageInfo3:
                OnEnterStageInfo3();
                break;

            case GuideIndex.EnterFriendHelpWindow:
                OnEnterFriendHelpWindow();
                break;

            case GuideIndex.EnterBattle3:
                OnEnterBattle3();
                break;

            case GuideIndex.ReadyBattle3:
                OnReadyBattle3();
                break;

            case GuideIndex.EncounterBoss2:
                OnEncounterBoss2();
                break;

            case GuideIndex.BattleAccountStart3:
                OnBattleAccountStart3();
                break;

            case GuideIndex.BattleAccountEnd3:
                OnBattleAccountEnd3();
                break;

            case GuideIndex.TriggerBoss:
                OnTriggerBoss();
                break;

            case GuideIndex.EnterMainUIForShop2:
                OnEnterMainUIForShop2();
                break;

            case GuideIndex.BuySecondFreePet:
                OnBuySecondFreePet();
                break;

            case GuideIndex.GetSecondFreePet:
                OnGetSecondFreePet();
                break;

            case GuideIndex.EnterMainUIForPet2:
                OnEnterMainUIForPet2();
                break;

            case GuideIndex.EnterTeamWindow3:
                OnEnterTeamWindow3();
                break;

            case GuideIndex.ChangeTeamOK2:
                OnChangeTeamOK2();
                break;

            case GuideIndex.EnterMainUIForBoss:
                OnEnterMainUIForBoss();
                break;

            case GuideIndex.EnterBossListWindow:
                OnEnterBossListWindow();
                break;

            case GuideIndex.EnterBossBattle:
                OnEnterBossBattle();
                break;

            case GuideIndex.BossAccountStart:
                OnBossAccountStart();
                break;

            case GuideIndex.BossAccountEnd:
                OnBossAccountEnd();
                break;

            case GuideIndex.EnterMainUIForFriend:
                OnEnterMainUIForFriend();
                break;

            case GuideIndex.EnterFriendWindow:
                OnEnterFriendWindow();
                break;

            case GuideIndex.EnterAddFriendPage:
                OnEnterAddFriendPage();
                break;

            //case GuideIndex.AddFriendSucceed:
            //    OnAddFriendSucceed();
            //    break;
        }
    }

    private static bool OpenDialog(GuideIndex index, Action onDialogFinish)
    {
        switch (index)
        {
            case GuideIndex.SelectRole:
                OpenDialog(1010, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.SelectPet:
                OpenDialog(1020, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.SelectName:
                OpenDialog(1030, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.NamingSucceed:
                OpenDialog(1040, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.EnterMainUIForShop:
                OpenDialog(30001, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.EnterMainUIForMap:
                OpenDialog(30002, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.EnterStageInfo:
                OpenDialog(30009, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.ReadyBattle:
                OpenDialog(30011, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.EncounterBoss:
                OpenDialog(30013, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.BattleAccountEnd:
                OpenDialog(30022, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.EnterMainUIForTask:
                OpenDialog(30015, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.EnterMainUIForPet:
                OpenDialog(30019, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.EnterMainUIForMap2:
                OpenDialog(30024, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.ReadyBattle2:
                OpenDialog(30026, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.BattleAccountEnd2:
                OpenDialog(30027, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.EnterMainUIForRolePage:
                OpenDialog(30031, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.EnterMainUIForMap3:
                OpenDialog(30033, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.EnterFriendHelpWindow:
                OpenDialog(30035, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.EncounterBoss2:
                OpenDialog(30038, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.BattleAccountEnd3:
                OpenDialog(30039, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.TriggerBoss:
                OpenDialog(30043, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.EnterMainUIForFriend:
                OpenDialog(30046, 0.5f, onDialogFinish);
                return true;

            case GuideIndex.EnterAddFriendPage:
                OpenDialog(30052, 0.5f, onDialogFinish);
                return true;

            default:
                return false;
        }
    }

    private static void OnPrologue()
    {
        PrepareNextGuide();
    }

    private static void OnSelectRole()
    {
        OpenDialog(GuideIndex.SelectRole, PrepareNextGuide);
    }

    private static void OnSelectPet()
    {
        OpenDialog(GuideIndex.SelectPet, PrepareNextGuide);
    }

    private static void OnSelectName()
    {
        OpenDialog(GuideIndex.SelectName, PrepareNextGuide);
    }

    private static void OnNamingSucceed()
    {
        ExecuteQueue(
            new Act(() => SaveProcessOnLocal((int)GuideIndex.EnterMainUIForShop)),
            new Dialog(GuideIndex.NamingSucceed),
            new Act(() => PrepareGuide(GuideIndex.EnterMainUIForShop))
            );
    }

    private static void OnEnterMainUIForShop()
    {
        ExecuteQueueThenNext(
            new Dialog(GuideIndex.EnterMainUIForShop),
            new ButtonMask("role_sel_top_right_group", "ShopBtn")
            );
    }

    //private static void OnBuyFirstFreePet()
    //{
    //    ExecuteQueue(
    //        new WaitWithMask(0.5f),
    //        new ButtonMask("shop_window(Clone)", "tab_context_panel", "scrollview_context_1", "item_shop_Info(Clone)_1")
    //        );
    //}
    //
    //private static void OnGetFirstFreePet()
    //{
    //    SaveGuideProcess(GuideIndex.BuySecondFreePet);
    //    PrepareNextGuide();     
    //}
    //
    //private static void OnGetFirstFreePetEnd()
    //{
    //    ExecuteQueueThenNext(
    //        new WaitWithMask(2f),
    //        new ButtonMask("shop_window(Clone)", "shop_gain_pet_window", "shop_gain_pet_window_close_btn")
    //        );
    //}

    private static void OnBuyFirstFreePet()
    {
        ExecuteQueue(
            new WaitWithMask(0.5f),
            new ButtonMask("shop_window(Clone)", "tab_context_panel", "scrollview_context_1", "item_shop_Info(Clone)_0")
            );
    }

    private static void OnGetFirstFreePet()
    {
        SaveGuideProcess(GuideIndex.EnterMainUIForMap);
        OpenMaskWithoutOperateRegion();
        PrepareNextGuide();
    }

    private static void OnGetFirstFreePetEnd()
    {
        ExecuteQueueThenNext(
            new WaitWithMask(2f),
            new ButtonMask("shop_window(Clone)", "shop_gain_pet_window", "shop_gain_pet_window_close_btn"),
            new WaitWithMask(0.5f),
            new ButtonMask(0, 0, "shop_window_back")
            );
    }

    private static void OnEnterMainUIForMap()
    {
        ExecuteQueue(
            new Dialog(GuideIndex.EnterMainUIForMap),
            new ButtonMask("role_sel_bottom_right_group", "PVEBtn")
            );
    }

    private static void OnEnterWorldMap()
    {
        ExecuteQueueThenNext(       
            new WaitWithMask(0.8f),
            new ButtonMask(5, 5, 0, 50, "scroll_world_map_window(Clone)", "map_anchor", "map_0", "point_0", "world_map_point")
            );
    }

    private static void OnEnterStageInfo()
    {
        ExecuteQueue(
            new WaitWithMask(0.3f),
            new Dialog(GuideIndex.EnterStageInfo),
            new ButtonMask("stage_info_window", "change_team_button")
            );
    }

    private static void OnEnterTeamWindow()
    {
        ExecuteQueue(
            new WaitWithMask(0.3f),
            new ButtonMask("all_pet_attribute_info_window(Clone)", "pet_info_auto_join_button")
            );
    }

    private static void OnChangeTeamOK()
    {
        ExecuteQueueThenNext(
            new SaveProcess(GuideIndex.ReturnToStageInfo),
            new WaitWithMask(0.3f),
            new ButtonMask(0, 0, "AllPetAttributeInfoBack")
            );
    }

    private static void OnReturnToStageInfo()
    {
        ExecuteQueue(
            new WaitWithMask(0.3f),
            new Mask(new Vector2(0, 20), "stage_info_window", "stage_info_start"),
            new Act(() => { DataCenter.Set("CURRENT_STAGE", GuideManager.GUIDE_STAGE_INDEX); MainProcess.StartBattle(null, false); })
            );
    }

    private static void OnEnterBattle()
    {
        PrepareNextGuide();    
    }

    private static void OnReadyBattle()
    {
        ExecuteQueueThenNext(
            new Act(TM_WaitToPlayPveBeginTextEffect.Play, () => Character.Self.StartAffect(Character.Self, GUIDE_BUFF_INDEX)),
            new WaitWithMask(1.5f),
            new Dialog(GuideIndex.ReadyBattle),
            new MaskRelative(new Rect(0.3f, 0.25f, 0.1f, 0.15f)),
            new Act(MainProcess.Self.OnClick),
            new WaitWithMask(2f),
            new Act(BattleStopAI, () => SetPetSkillBtnVisible(true)),
            new ButtonMask("battle_skill_window", "do_skill_father_1"),
            new WaitWithMask(0.1f),
            new ButtonMask("battle_skill_window", "do_skill_father_2")
            //new Act(BattleRestartAI)
            //new MonsterTrigger(12f)
            );
    }

    private static void OnEncounterMonster()
    {
        BattleStopAI();
        ActiveObject target = FindInVisibleAliveObjects(x => x.GetObjectType() == OBJECT_TYPE.MONSTER && InRange(Character.Self, x, 12f)) as ActiveObject;

        if (target != null)
        {
            ExecuteQueueThenNext(
                new WaitWithMask(0.5f),
                new MaskInWorldSpace(target.mMainObject, 0.12f, 0.18f, new Vector2(0, 35)),
                new Act(BattleRestartAI),
                new Wait(0),
                new Act(() => Character.Self.Attack(target)),
                new BossTrigger(8f)
                );
        }
        else
        {
            BattleRestartAI();
            PrepareNextGuide();
            CreateMonsterTrigger(8f, x => x.GetObjectType() == OBJECT_TYPE.MONSTER_BOSS, () => Notify(GuideIndex.EncounterBoss));
        }
    }

    private static void OnEncounterBoss()
    {
        BattleStopAI();
        SetCharacterSkillBtnVisible(true);
        Character.Self.StopAffect(10001);
        ActiveObject target = FindInVisibleAliveObjects(x => x.GetObjectType() == OBJECT_TYPE.MONSTER_BOSS && InRange(Character.Self, x, 10f)) as ActiveObject;

        if (target != null)
        {
            Action doSkill = () =>
            {
                tLogicData skillData = DataCenter.GetData("SKILL_UI");
                tLogicData d = skillData.getData("do_skill_0");
                int skillIndex = d.get("SKILL_INDEX");
                Character.Self.SkillAttack(skillIndex, target, d);
            };

            ExecuteQueue(false,
                new Dialog(GuideIndex.EncounterBoss),
                new Mask(new Vector2(0, 30), "do_skill_0"),
                new Act(BattleRestartAI, doSkill)
                );
        }
        else
        {
            PrepareNextGuide();
        }
    }

    private static void OnBattleAccountStart()
    {
        CloseDialog();       
        DestroyMonsterTrigger();
        ClearQueue();
        SaveGuideProcess(GuideIndex.EnterMainUIForTask);
        PrepareNextGuide();
        OpenMaskWithoutOperateRegion();
    }

    private static void OnBattleAccountEnd()
    {
        Action onClick = () =>
        {
            MainUIScript.mLoadingFinishAction = () => Notify(GuideIndex.EnterMainUIForTask);
            MainProcess.ClearBattle();
            MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.RoleSelWindow);
        };

        ExecuteQueue(
            new Dialog(GuideIndex.BattleAccountEnd),
            new Mask(new Vector2(0, 20), "pve_account_win_window", "but_back_home_page"),
            new Act(onClick)
            );
    }

    private static void OnEnterMainUIForTask()
    {
        ExecuteQueue(
            new Dialog(GuideIndex.EnterMainUIForTask),
            new ButtonMask("role_sel_top_right_group", "TaskBtn")
            );
    }

    private static void OnEnterTaskWindow()
    {
        ExecuteQueue(
            new WaitWithMask(0.5f),
            new ButtonMask("mission_window(Clone)", "gp_mission(Clone)_0", "but_get_task_award")
            );
    }

    private static void OnAcceptTaskAward()
    {
        ExecuteQueueThenNext(
            new SaveProcess(GuideIndex.EnterMainUIForPet),
            new WaitWithMask(0.5f),
            new ButtonMask("get_rewards_window", "get_rewards_window_button"),
            new WaitWithMask(0.3f),
            new ButtonMask(0, 0, "MissionWindowBack"),
            new Wait(0f)
            );
    }

    private static void OnEnterMainUIForPet()
    {
        ExecuteQueue(
            new Dialog(GuideIndex.EnterMainUIForPet),
            new ButtonMask("role_sel_bottom_left_group", "PetBtn")
            );
    }

    private static void OnEnterTeamWindow2()
    {
        ExecuteQueueThenNext(
            new WaitWithMask(1f),
            new ButtonMask("PetInfoWindow", "Upgrade")
            );
    }

    private static void OnEnterUpgradeWindow()
    {
        ExecuteQueue(
            new WaitWithMask(0.5f),
            new ButtonMask("PetUpgradeWindow", "pet_upgrade_auto_add_pet_btn"),
            new WaitWithMask(0.5f),
            new ButtonMask("PetUpgradeWindow", "PetUpgradeBtnOK"),
            new SaveProcess(GuideIndex.EnterMainUIForMap2),
            new Act(OpenMaskWithoutOperateRegion)
            );
    }

    private static void OnPetUpgradeOK()
    {
        ExecuteQueueThenNext(
            new WaitWithMask(0.5f),
            new ButtonMask("UpGradeAndStrengthenResultWindow", "UpGradeAndStrengthenWindowOKBtn"),
            new WaitWithMask(0.5f),
            new ButtonMask(0, 0, "AllPetAttributeInfoBack"),
            new Wait(0f)
            );
    }

    private static void OnEnterMainUIForMap2()
    {
        ExecuteQueue(
            new Dialog(GuideIndex.EnterMainUIForMap2),
            new ButtonMask("role_sel_bottom_right_group", "PVEBtn")
            );
    }

    private static void OnEnterWorldMap2()
    {
        ExecuteQueueThenNext(
            new WaitWithMask(0.8f),
            new ButtonMask(5, 5, 0, 50, "scroll_world_map_window(Clone)", "map_anchor", "map_0", "point_1", "world_map_point")
            );
    }

    private static void OnEnterStageInfo2()
    {
        ExecuteQueue(
            new WaitWithMask(0.5f),
            new Mask(new Vector2(0, 20), "stage_info_window", "stage_info_start"),
            new Act(() => { DataCenter.Set("CURRENT_STAGE", GuideManager.GUIDE_STAGE_INDEX + 1); MainProcess.StartBattle(null, false); })
            );
    }

    private static void OnEnterBattle2()
    {
        PrepareNextGuide();
    }

    private static void OnReadyBattle2()
    {
        ExecuteQueue(false,
            new Act(TM_WaitToPlayPveBeginTextEffect.Play),
            new WaitWithMask(1.5f),
            new Dialog(GuideIndex.ReadyBattle2),
            new ButtonMask("auto_fight")
            );
    }

    private static void OnBattleAccountStart2()
    {
        SaveGuideProcess(GuideIndex.EnterMainUIForRolePage);
        OpenMaskWithoutOperateRegion();
        PrepareNextGuide();
    }

    private static void OnBattleAccountEnd2()
    {
        Action onClick = () =>
        {
            MainUIScript.mLoadingFinishAction = () => Notify(GuideIndex.EnterMainUIForRolePage);
            MainProcess.ClearBattle();
            MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.RoleSelWindow);
        };

        ExecuteQueue(
            new Dialog(GuideIndex.BattleAccountEnd2),
            new Mask(new Vector2(0, 20), "pve_account_win_window", "but_back_home_page"),
            new Act(onClick)
            );
    }

    private static void OnEnterMainUIForRolePage()
    {
        ExecuteQueue(
            new Dialog(GuideIndex.EnterMainUIForRolePage),
            new ButtonMask("role_sel_bottom_left_group", "CharacterBtn")
            );
    }

    private static void OnEnterRolePage()
    {
        ExecuteQueue(
            new WaitWithMask(0.5f),
            new ButtonMask("all_role_attribute_info_window(Clone)", "RoleEquipCultivateBtn"),
            new WaitWithMask(0.5f),
            new ButtonMask("role_equip_cultivate_window", "RoleEquipUseBtn")
            );
    }

    private static void OnLoadRoleEquipOK()
    {
        ExecuteQueueThenNext(
            new SaveProcess(GuideIndex.EnterMainUIForMap3),
            new WaitWithMask(0.5f),
            new ButtonMask(0, 0, "AllRoleAttributeInfoBack")
            );
    }

    private static void OnEnterMainUIForMap3()
    {
        ExecuteQueue(
            new Dialog(GuideIndex.EnterMainUIForMap3),
            new ButtonMask("role_sel_bottom_right_group", "PVEBtn")
            );
    }

    private static void OnEnterWorldMap3()
    {
        ExecuteQueueThenNext(
            new WaitWithMask(0.8f),
            new ButtonMask(5, 5, 0, 50, "scroll_world_map_window(Clone)", "map_anchor", "map_0", "point_2", "world_map_point")
            );
    }

    private static void OnEnterStageInfo3()
    {
        ExecuteQueueThenNext(
            new WaitWithMask(0.5f),
            new ButtonMask("stage_info_window", "stage_info_start")
            );
    }

    private static void OnEnterFriendHelpWindow()
    {
        ExecuteQueue(
            new Dialog(GuideIndex.EnterFriendHelpWindow),
            //new ButtonMask("friend_help_window", "subcell(Clone)_0", "friend_help_select_button"),
            new WaitWithMask(0.5f),
            new ButtonMask("friend_help_window", "friend_help_start_button")
            );
    }

    private static void OnEnterBattle3()
    { 
        GameObject helpBtn = GameCommon.FindUI("get_friend_help_button");
        helpBtn.SetActive(false);
        PrepareNextGuide();
    }

    private static void OnReadyBattle3()
    {
        ExecuteQueueThenNext(
            new Act(TM_WaitToPlayPveBeginTextEffect.Play),
            new BossTrigger(12f)
            );
    }

    private static void OnEncounterBoss2()
    {
        GameObject friendHelpBtn = GameCommon.FindUI("do_skill_father_4");

        if (friendHelpBtn != null && friendHelpBtn.transform.GetChild(0).gameObject.activeSelf)
        {
            ExecuteQueue(false,
                new Act(BattleStopAI, () => GameCommon.FindUI("do_skill_father_4").SetActive(true)),
                new Dialog(GuideIndex.EncounterBoss2),
                new ButtonMask("do_skill_father_4"),
                new Act(BattleRestartAI)
                );
        }
        else 
        {
            PrepareNextGuide();
        }
    }

    private static void OnBattleAccountStart3()
    {
        CloseDialog();       
        DestroyMonsterTrigger();
        ClearQueue();
        SaveGuideProcess(GuideIndex.EnterMainUIForFriend);
        OpenMaskWithoutOperateRegion();
        PrepareNextGuide();
    }

    private static void OnBattleAccountEnd3()
    {
        Action onClick = () =>
        {
			/*
            CS_RequestAppearBoss evt = Net.StartEvent("CS_RequestAppearBoss") as CS_RequestAppearBoss;
            evt.mCurrentStageIndex = DataCenter.Get("CURRENT_STAGE");
            evt.mNextActive = eACTIVE_AFTER_APPEAR.SELECT_LEVEL;
            evt.set("GUIDE", true);
            evt.DoEvent();
            */
        };

        ExecuteQueue(
            new Dialog(GuideIndex.BattleAccountEnd3),
            new Mask(new Vector2(0, 20), "pve_account_win_window", "but_back_home_page"),
            new Act(onClick)
            );
    }

    private static void OnTriggerBoss()
    {
        Action onClick = () =>
        {
            MainUIScript.mLoadingFinishAction = () => Notify(GuideIndex.EnterMainUIForShop2);
            MainProcess.ClearBattle();
            MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.RoleSelWindow);
        };

        ExecuteQueue(
            new WaitWithMask(1.5f),
            new Dialog(GuideIndex.TriggerBoss),
            //new ButtonMask("boss_appear01_window", "start_boss_battle")
            new Mask(new Vector2(0, 20), "boss_appear01_window", "leave_boss_battle"),
            new Act(onClick)
            );
    }

    private static void OnEnterMainUIForShop2()
    {
        ExecuteQueueThenNext(
                   new WaitWithMask(0.5f),
                   new ButtonMask("role_sel_top_right_group", "ShopBtn")
            );
    }

    private static void OnBuySecondFreePet()
    {
        ExecuteQueue(
            new WaitWithMask(0.5f),
            new ButtonMask("shop_window(Clone)", "tab_context_panel", "scrollview_context_1", "item_shop_Info(Clone)_1")
            );
    }

    private static void OnGetSecondFreePet()
    {
        Action onGet = () => 
        {
            if (GameCommon.bIsWindowOpen("shop_gain_item_window"))
            {
                ExecuteQueueThenNext(
                    new WaitWithMask(2f),
                    new ButtonMask("shop_window(Clone)", "shop_gain_item_window", "shop_gain_item_window_close_btn"),
                    new WaitWithMask(0.5f),
                    new ButtonMask(0, 0, "shop_window_back"));
            }
            else
            {
                ExecuteQueueThenNext(
                    new WaitWithMask(2f),
                    new ButtonMask("shop_window(Clone)", "shop_gain_pet_window", "shop_gain_pet_window_close_btn"),
                    new WaitWithMask(0.5f),
                    new ButtonMask(0, 0, "shop_window_back"));
            }
        };

        ExecuteQueue(
            new Trigger(() => GameCommon.bIsWindowOpen("shop_gain_item_window") || GameCommon.bIsWindowOpen("shop_gain_pet_window")),
            new Act(onGet)
            );        
    }

    private static void OnEnterMainUIForPet2()
    {
        ExecuteQueue(
            new WaitWithMask(0.5f), 
            new ButtonMask("role_sel_bottom_left_group", "PetBtn")
            );
    }

    private static void OnEnterTeamWindow3()
    {
        ExecuteQueue(
            new WaitWithMask(0.5f),
            new ButtonMask("all_pet_attribute_info_window(Clone)", "pet_info_auto_join_button")
            );
    }

    private static void OnChangeTeamOK2()
    {
        ExecuteQueueThenNext(
            new WaitWithMask(0.5f),
            new ButtonMask(0, 0, "AllPetAttributeInfoBack")
            );
    }

    private static void OnEnterMainUIForBoss()
    {
        GameObject bossEntry = GameObject.Find("Mainmenu_bg/arena_4v4/zjm_tianmozhu01/ec_ui_tianmo/dowm");

        if (bossEntry == null || !bossEntry.activeInHierarchy)
        {
            SkipGuide();
            return;
        }

        Action onClick = () =>
        {
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
            DataCenter.OpenWindow("BOSS_RAID_WINDOW");
        };

        ExecuteQueue(
            new WaitWithMask(0.5f),
            new MaskInWorldSpace(bossEntry, 0.1f, 0.15f, new Vector2(0, 35)),
            new Act(onClick)
            );
    }

    private static void OnEnterBossListWindow()
    {
        ExecuteQueue(
            new WaitWithMask(0.3f),
            new ButtonMask("boss_info_subcell(Clone)_0", "fight_button"),
            new WaitWithMask(0.5f),
            new ButtonMask("stage_info_window", "stage_info_start")
            );
    }

    private static void OnEnterBossBattle()
    {
        PrepareNextGuide();
    }

    private static void OnBossAccountStart()
    {
        OpenMaskWithoutOperateRegion();
        PrepareNextGuide();
    }

    private static void OnBossAccountEnd()
    {
        string windowName = "boss_account_win_window";
        GameObject window = GameCommon.FindUI("boss_account_win_window");

        if (window == null || !window.activeSelf)
            windowName = "boss_account_lose_window";

        Action onClick = () =>
        {
            MainUIScript.mLoadingFinishAction = () => Notify(GuideIndex.EnterMainUIForFriend);
            MainProcess.ClearBattle();
            MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.RoleSelWindow);
        };

        ExecuteQueue(
            new WaitWithMask(0.5f),
            new Mask(new Vector2(0, 20), windowName, "but_back_home_page"),
            new Act(onClick)
            );
    }

    private static void OnEnterMainUIForFriend()
    {
        ExecuteQueue(
            new Dialog(GuideIndex.EnterMainUIForFriend),
            new ButtonMask("role_sel_bottom_left_group", "FriendBtn")
            );
    }

    private static void OnEnterFriendWindow()
    {
        ExecuteQueue(
            new WaitWithMask(0.5f),
            new ButtonMask("friend_window(Clone)", "add_friend_button")
            );
    }

    private static void OnEnterAddFriendPage()
    {
        ExecuteQueue(
            new SaveProcess(GuideIndex.Max),
            new WaitWithMask(0.5f),
            new Tip("此处显示所有的好友申请", -200, 50),
            new Tip("此处可以查找并向玩家申请好友", 300, 50),
            new Dialog(GuideIndex.EnterAddFriendPage),
            new Act(() => OptionalGuide.StartGuide(new MailGuideQueue()))
            //new ButtonMask("friend_window(Clone)", "add_friend_window", "request_friend_info_cell(Clone)_0", "agreed_button")
            );
    }

    private static void OnAddFriendSucceed()
    {
        ExecuteQueueThenNext(
            new SaveProcess(GuideIndex.Max)    
            );
    }
    /*
        private static void OnPrologue()
        {
            DataCenter.Set("ENTER_GS", false);
            DataCenter.CloseWindow("LOGIN_WINDOW");
            DataCenter.SetData("LANDING_WINDOW", "REFRESH", true);
            DataCenter.CloseWindow("LANDING_WINDOW");
            DataCenter.OpenWindow("PROLOGUE_WINDOW");

            PrepareNextGuide();

            CommonParam.bIsNetworkGame = false;
            CS_RequestRoleData.ReadRoleFromData(null);
            CS_RequestRoleData.ReadPetFromData(null);	
            CS_RequestRoleData.ReadGemFromData(null);	
            CS_RequestRoleData.ReadMapFromData(null);	
            CS_RequestRoleData.ReadTujianFromData(null);
            CS_RequestRoleData.ReadTaskFromData(null, false);
            CS_RequestRoleData.ReadRoleEquipFromData(null);
            CS_RequestRoleData.ReadOnHookFromData(null);
            CommonParam.bIsNetworkGame = true;   
        }

        private static void OnPrologueBossHPHigh()
        {
 
        }

        private static void OnPrologueBossHPHalf()
        { }

        private static void OnPrologueBossHPLow()
        { }

        private static void OnSelectRole()
        {
            OpenDialog(GuideIndex.SelectRole, PrepareNextGuide);
        }

        private static void OnSelectPet()
        {
            OpenDialog(GuideIndex.SelectPet, PrepareNextGuide);
        }

        private static void OnSelectName()
        {
            OpenDialog(GuideIndex.SelectName, PrepareNextGuide);
        }

        private static void OnNamingSucceed()
        {
            Action onDialogFinish = () =>
            {
                PrepareNextGuide();
                DataCenter.Set("FIRST_LANDING", false);
                tEvent evt = Net.StartEvent("CS_RequestRoleData");
                evt.set("ENTER_GUIDE_STAGE", false);
                evt.DoEvent();

                MainUIScript.mLoadingFinishAction = () =>
                {
                    GuideManager.Notify(GuideIndex.EnterMainUI);
                };
            };

            OpenDialog(GuideIndex.NamingSucceed, onDialogFinish);
        }

        private static void OnEnterMainUI()
        {
            GameObject pveBtn = null;

            Action onClick = () =>
            {
                PrepareNextGuide();
                DestroyFinger();
                pveBtn.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
                Notify(GuideIndex.EnterWorldMap);
            };

            Action onDialogFinish = () =>
            {
                pveBtn = GameCommon.FindUI("role_sel_bottom_right_group", "PVEBtn");
                OpenMask(pveBtn, onClick);
                ShowMaskFinger(1f);
            };

            OpenDialog(GuideIndex.EnterMainUI, onDialogFinish);
        }

        private static void OnEnterWorldMap()
        {
            GameObject point = null;

            Action onClick = () =>
            {
                PrepareNextGuide();
                DestroyFinger();
                point.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
                Notify(GuideIndex.EnterStageInfo);
            };

            Action onDialogFinish = () =>
            {
                point = GameCommon.FindUI("scroll_world_map_window(Clone)", "map_anchor", "map_0", "point_0", "world_map_point");
                OpenMask(point, onClick, 5, 5, 0, 50);
                ShowMaskFinger(1f);
            };

            OpenDialog(GuideIndex.EnterWorldMap, onDialogFinish);
        }

        private static void OnEnterStageInfo()
        {
            GameObject start = GameCommon.FindUI("stage_info_window", "stage_info_start");

            Action onClick = () =>
            {
                PrepareNextGuide();
                DestroyFinger();
                DataCenter.Set("CURRENT_STAGE", GuideManager.GUIDE_STAGE_INDEX);
                MainProcess.StartBattle(null, false);
            };

            ExecuteDelayed(() =>
            {
                OpenMask(start, onClick);
                ShowMaskFinger(1f);
            }, 0f);    
        }

        private static void OnEnterBattle()
        {
            Action onDialogFinish = () =>
            {
                PrepareNextGuide();

                BattleRestartAI();
                DataCenter.OpenWindow("PVE_BEGIN_BATTLE_WINDOW");
                tWindow t = DataCenter.GetData("PVE_BEGIN_BATTLE_WINDOW") as tWindow;

                if (t != null)
                    MonoBehaviour.Destroy(t.mGameObjUI, 2f);
            };

            Action onEnterBattle = () =>
            {
                OpenDialog(GuideIndex.EnterBattle, onDialogFinish);
                SetBattleAutoFightEnabled(false);
                SetPVEBattleSettingEnabled(false);
                BattleStopAI();
            };

            DataCenter.Set("FIRST_LANDING", false);
            ExecuteDelayed(onEnterBattle, 0f);
        }

        private static void OnEncounterMonster()
        {
            BattleStopAI();
            OpenDialog(GuideIndex.EncounterMonster, () => { BattleRestartAI(); PrepareNextGuide(); });
        }

        private static void OnEncounterBoss()
        {
            BattleStopAI();
            OpenDialog(GuideIndex.EncounterBoss, () => { BattleRestartAI(); PrepareNextGuide(); });
        }

        private static void OnWinBattle()
        {
            GameObject petBtn = null;

            Action onClick = () =>
            {
                PrepareNextGuide();
                DestroyFinger();
                petBtn.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
                Notify(GuideIndex.OpenPetWindow);
            };

            Action onDialogFinish = () =>
            {
                petBtn = GameCommon.FindUI("role_sel_bottom_left_group", "PetBtn");         
                OpenMask(petBtn, onClick);
                ShowMaskFinger(1f);
            };

            Action onWin = () =>
            {
                MainProcess.ClearBattle();            
                MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.RoleSelWindow);

                MainUIScript.mLoadingFinishAction = () =>
                    {
                        OpenMaskWithoutOperateRegion();
                        ExecuteDelayed(() => OpenDialog(GuideIndex.WinBattle, onDialogFinish), 0.5f);
                    };
            };
        
            ExecuteDelayed(onWin, 1f);
        }


        private static void OnOpenPetWindow()
        {
            GameObject petBtn = null;
            GameObject okBtn = null;
            GameObject petGrid = null;

            Action onClickCommit = () =>
            {
                PrepareNextGuide();
                DestroyFinger();

                if (petGrid != null && petGrid.activeSelf)
                    petGrid.SendMessage("OnClick", SendMessageOptions.RequireReceiver);

                ExecuteDelayed(() => DataCenter.OpenMessageWindow("本阶段引导完成"), 0.5f);
            };

            Action afterClickOK = () =>
            {
                petGrid = GameCommon.FindUI("all_pet_attribute_info_window(Clone)", "PetInfoWindow", "pet_info(Clone)_1", "pet_play_flag_btn");

                if(petGrid == null || !petGrid.activeSelf)
                    petGrid = GameCommon.FindUI("all_pet_attribute_info_window(Clone)", "PetInfoWindow", "pet_info(Clone)_1", "pet_play_check_box_btn");

                OpenMask(petGrid, onClickCommit);
                ShowMaskFinger(1f);
            };

            Action onClickOK = () =>
            {
                DestroyFinger();
                okBtn.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
                OpenMaskWithoutOperateRegion();
                ExecuteDelayed(afterClickOK, 0.5f);
            };

            Action afterClickIcon = () =>
            {
                okBtn = GameCommon.FindUI("common_pet_info_single_window", "PetInfoSingleOKBtn");
                OpenMask(okBtn, onClickOK);
                ShowMaskFinger(1f, new Vector2(0f, 35f));
            };

            Action onClickIcon = () =>
            {
                DestroyFinger();            
                petBtn.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
                OpenMaskWithoutOperateRegion();
                ExecuteDelayed(afterClickIcon, 0.5f);
            };

            Action onDialogFinish = () =>
            {
                petBtn = GameCommon.FindUI("bag_info_window", "PetIcons", "group(Clone)_1", "pet_icon_check_btn");       
                OpenMask(petBtn, onClickIcon);
                ShowMaskFinger(1f);
            };

            OpenMaskWithoutOperateRegion();
            ExecuteDelayed(() => OpenDialog(GuideIndex.OpenPetWindow, onDialogFinish), 1f);
        }
        */
}