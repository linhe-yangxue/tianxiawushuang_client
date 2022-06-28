using UnityEngine;
using System.Collections;
using Utilities.Routines;


//public class GuideNodeFactory
//{
//    private static int[] mEntries = new int[] 
//    {
//        1, 9, 17, 26, 31, 35
//    };

//    public static GuideNodeRoutine Create(int index)
//    {
//        GuideNodeRoutine r = CreateNode(index);

//        if (r != null)
//        {
//            r.Init(index);
//            return r;
//        }

//        return null;
//    }

//    public static bool IsEntryIndex(int index)
//    {
//        int len = mEntries.Length;

//        for (int i = 0; i < len; ++i)
//        {
//            if (mEntries[i] == index)
//            {
//                return true;
//            }
//        }

//        return false;
//    }

//    public static int GetNextEntryIndex(int savedIndex)
//    {
//        int len = mEntries.Length;

//        for (int i = 0; i < len; ++i)
//        {
//            if (mEntries[i] > savedIndex)
//            {
//                return mEntries[i];
//            }
//        }

//        return Guide.MAX_INDEX;
//    }

//    private static GuideNodeRoutine CreateNode(int index)
//    {
//        switch (index)
//        {
//            // 初次冒险
//            case 1: return new MG_ClickButton("PVEBtn");
//            case 2: return new MG_ClickButton("map_anchor", "map_0", "point_0", "world_map_point");
//            case 3: return new MG_ClickButton("stage_info_start").PostDelay(new WaitBattleActive());
//            case 4: return new MG_Stage1_ClickGroud();
//            case 5: return new MG_Stage1_AttackMonster();
//            case 6: return new MG_Stage1_ClickGroud2();
//            case 7: return new MG_SkillAttackBoss(0);
//            case 8: return new MG_StageBack("but_back_home_page");

//            // 抽卡及上阵
//            case 9: return new MG_ClickButton("ShopBtn");
//            case 10: return new MG_ClickButton("item_shop_Info(Clone)_1", "but_shop_buy_advance_one").WaitRespThenSave("SC_PreciousLottery").PreDelay(new ListenNetResp("SC_LotteryQuery"));
//            case 11: return new MG_ClickButton("shop_gain_pet_window_close_btn");
//            case 12: return new MG_ClickButton("shop_window_back");
//            case 13: return new MG_ClickButton("CharacterBtn");
//            case 14: return new MG_ClickButton("team_icon_info_buttons_grid", "icon_infos(Clone)_1", "team_add_pos_btn");
//            case 15: return new MG_ClickButton("bag_pet_window", "group(Clone)_0", "bag_pet_single_btn").WaitRespThenSave("SC_PetLineupChange");
//            case 16: return new MG_ClickButton("team_window_back_btn");

//            // 再次冒险
//            case 17: return new MG_ClickButton("PVEBtn");
//            case 18: return new MG_ClickButton("map_anchor", "map_0", "point_1", "world_map_point");
//            case 19: return new MG_ClickButton("stage_info_start");
//            case 20: return new MG_ClickAutoBattle();
//            case 21: return new MG_SkillAttackBoss(1);
//            case 22: return new MG_StageBack("but_select");

//            // 领取奖励宝箱（暂无，先跳过）
//            case 23: return new GuideNodeRoutine();
//            case 24: return new GuideNodeRoutine();
//            case 25: return new MG_ClickButton("world_map_back");

//            // 主角突破
//            case 26: return new MG_ClickButton("CharacterBtn");
//            case 27: return new MG_ClickButton("team_icon_info_buttons_grid", "icon_infos(Clone)_0", "team_pos_head_btn");
//            case 28: return new MG_ClickButton("go_team_break_button");
//            case 29: return new MG_ClickButton("break_button").WaitRespThenSave("SC_BreakUpgrade");
//            case 30: return new MG_ClickButton("break_result_close_button").PostDelay(new ButtonMaskRoutine("team_window_back_btn"));

//            // 第三次冒险
//            case 31: return new MG_ClickButton("PVEBtn");
//            case 32: return new MG_ClickButton("map_anchor", "map_0", "point_2", "world_map_point");
//            case 33: return new MG_ClickButton("stage_info_start");
//            case 34: return new MG_StageBack("but_back_home_page");

//            // 符灵升级
//            case 35: return new MG_ClickButton("CharacterBtn");
//            case 36: return new MG_ClickButton("team_icon_info_buttons_grid", "icon_infos(Clone)_1", "team_pos_head_btn");
//            case 37: return new MG_ClickButton("go_team_upgrade");
//            case 38: return new MG_ClickButton("pet_level_up_expend_group", "cur_pet_icon(Clone)_0", "add_upgrade_pet_btn");
//            case 39: return new MG_SelectPetUpgradeMaterial();
//            case 40: return new MG_ClickButton("pet_level_up_btn").WaitRespThenSave("SC_PetUpgrade").PostDelay(new ButtonMaskRoutine("message_window_button"));
//            case 41: return new MG_ClickButton("team_window_back_btn");

//            // 第四次冒险
//            case 42: return new MG_ClickButton("PVEBtn");
//            case 43: return new MG_ClickButton("map_anchor", "map_0", "point_3", "world_map_point");
//            case 44: return new MG_ClickButton("stage_info_start");
//        }

//        return null;
//    }
//}


///// <summary>
///// 引导点击按钮
///// </summary>
//public class MG_ClickButton : GuideNodeRoutine
//{
//    public string[] path { get; private set; }
//    public GameObject button { get; private set; }

//    private string respName = null;


//    public MG_ClickButton(params string[] path)
//    {
//        this.path = path;
//        this.button = null;
//    }

//    protected override IEnumerator DoBefore()
//    {
//        var w = new WaitUIVisible(path);
//        yield return w;
//        button = w.target;
//    }

//    protected override IEnumerator DoMain()
//    {
//        yield return new ButtonMaskRoutine(button, true);

//        if (!string.IsNullOrEmpty(respName))
//        {
//            yield return new WaitNetRespThenSave(respName, index);
//        }
//    }

//    public MG_ClickButton WaitRespThenSave(string respName)
//    {
//        this.respName = respName;
//        return this;
//    }
//}


///// <summary>
///// 第一关：引导移动
///// </summary>
//public class MG_Stage1_ClickGroud : GuideNodeRoutine
//{
//    protected override IEnumerator DoBefore()
//    {
//        yield return new WaitBattleActive();
//        MainProcess.mStage.DisableAI();
//        ActiveBirth.mEnableMonsterBossUIInfo = false;
//    }

//    protected override IEnumerator DoMain()
//    {       
//        yield return new MaskRoutine(new Vector2(0.3f, 0.25f), 0.1f, 0.15f);
//        Vector3 screenCoord = MainProcess.mMainCamera.ViewportToScreenPoint(new Vector3(0.8f, 0.75f, 0f));
//        RaycastHit hit;

//        if (GuideKit.RaycastToObstruct(MainProcess.mMainCamera, screenCoord, out hit))
//        {
//            NavMeshPath path;
//            NavMeshPathStatus pathStatus = AIKit.CalculatePath(Character.Self, hit.point, out path);

//            if (pathStatus == NavMeshPathStatus.PathComplete)
//            {
//                GuideKit.SetAllFriendsAI(x => new FollowRoutine(x, Character.Self, x.mFollowOffset, false));
//                yield return new MoveToRoutine(Character.Self, hit.point);
//                GuideKit.SetAllFriendsAI(null);
//            }
//        }
//    }

//    protected override void OnBreak()
//    {
//        GuideKit.SetAllFriendsAI(null);
//        MainProcess.mStage.EnableAI();
//        ActiveBirth.mEnableMonsterBossUIInfo = true;
//    }
//}


///// <summary>
///// 引导攻击小怪
///// </summary>
//public class MG_Stage1_AttackMonster : GuideNodeRoutine
//{
//    protected override IEnumerator DoMain()
//    {
//        BaseObject enemy = AIKit.FindNearestEnemy(Character.Self, true);

//        if (enemy != null)
//        {
//            yield return new MoveCameraRoutine(enemy.GetPosition(), 0.5f);

//            GuideKit.SetCursorOffset(0f, 50f);
//            yield return new MaskRoutine(MainProcess.mMainCamera, enemy.GetPosition(), 0.15f, 0.2f);

//            CameraMoveEvent.BindMainObject(Character.Self);
//            enemy.StartAI();
//            GuideKit.SetAllFriendsAI(x => new TryKillTargetRoutine(x, enemy, 0, null));

//            while (!enemy.IsDead())
//            {
//                yield return new TryKillTargetRoutine(Character.Self, enemy, 0, null);
//                yield return null;
//            }

//            GuideKit.SetAllFriendsAI(null);
//        }
//    }

//    protected override void OnBreak()
//    {
//        GuideKit.SetAllFriendsAI(null);
//        MainProcess.mStage.EnableAI();
//        CameraMoveEvent.BindMainObject(Character.Self);
//        ActiveBirth.mEnableMonsterBossUIInfo = true;
//    }
//}



///// <summary>
///// 再次引导移动
///// </summary>
//public class MG_Stage1_ClickGroud2 : GuideNodeRoutine
//{
//    protected override IEnumerator DoMain()
//    {
//        yield return new MaskRoutine(new Vector2(0.3f, 0.2f), 0.1f, 0.15f);

//        BaseObject boss = AIKit.FindNearestTarget(Character.Self, x => x.GetObjectType() == OBJECT_TYPE.MONSTER_BOSS);

//        if (boss != null)
//        {
//            GuideKit.SetAllFriendsAI(x => new FollowRoutine(x, Character.Self, x.mFollowOffset, false));
//            yield return new MoveTowardsRoutine(Character.Self, boss, Character.Self.LookBound() - 0.1f);
//        }

//        GuideKit.SetAllFriendsAI(null);
//    }

//    protected override void OnBreak()
//    {
//        GuideKit.SetAllFriendsAI(null);
//        MainProcess.mStage.EnableAI();
//        ActiveBirth.mEnableMonsterBossUIInfo = true;
//    }
//}


///// <summary>
///// 引导技能攻击Boss
///// </summary>
//public class MG_SkillAttackBoss : GuideNodeRoutine
//{
//    private int skill = 0;

//    public MG_SkillAttackBoss(int skill)
//    {
//        this.skill = skill;
//    }

//    protected override IEnumerator DoMain()
//    {
//        MonsterBoss boss = AIKit.FindNearestTarget(Character.Self, x => x.GetObjectType() == OBJECT_TYPE.MONSTER_BOSS) as MonsterBoss;

//        if (boss != null)
//        {
//            boss.ShowInfoUI();

//            yield return new MoveCameraRoutine(boss.GetPosition(), 0.5f);
//            yield return new MaskRoutine("do_skill_" + skill);
           
//            MainProcess.mStage.EnableAI();
//            CameraMoveEvent.BindMainObject(Character.Self);

//            GuideKit.SetAllFriendsAI(x => new TryKillTargetRoutine(x, boss, 0, null));

//            var skillData = AIKit.GetSkillButtonData(skill);
//            int skillIndex = skillData.get("SKILL_INDEX");
//            yield return new TryKillTargetRoutine(Character.Self, boss, skillIndex, skillData);

//            GuideKit.SetAllFriendsAI(null);
//            ActiveBirth.mEnableMonsterBossUIInfo = true;
//        }
//    }

//    protected override void OnBreak()
//    {
//        GuideKit.SetAllFriendsAI(null);
//        MainProcess.mStage.EnableAI();
//        CameraMoveEvent.BindMainObject(Character.Self);
//        ActiveBirth.mEnableMonsterBossUIInfo = true;
//    }
//}


///// <summary>
///// 结算后返回
///// </summary>
//public class MG_StageBack : GuideNodeRoutine
//{
//    private string btn;

//    public MG_StageBack(string btnName)
//    {
//        this.btn = btnName;
//    }

//    protected override IEnumerator DoBefore()
//    {      
//        yield return new WaitNetRespThenSave("SC_BattleMainResult", index);
//        GuideKit.OpenMask();
//        yield return new Wait(2.5f);
//    }

//    protected override IEnumerator DoMain()
//    {
//        yield return new ButtonMaskRoutine("pve_account_win_window", btn);
//    }

//    protected override IEnumerator DoAfter()
//    {
//        yield return new WaitMainUILoadingDone();
//    }
//}


///// <summary>
///// 点击自动战斗
///// </summary>
//public class MG_ClickAutoBattle : GuideNodeRoutine
//{
//    protected override IEnumerator DoBefore()
//    {
//        yield return new WaitFor(() => MainProcess.mStage != null && MainProcess.mStage.mbBattleActive);
//        MainProcess.mStage.DisableAI();
//        ActiveBirth.mEnableMonsterBossUIInfo = false;
//    }

//    protected override IEnumerator DoMain()
//    {
//        GameObject btn = GameCommon.FindUI("auto_fight");
//        var w = new ButtonMaskRoutine(btn, false);
//        yield return w;
//        MainProcess.mStage.EnableAI();
//        w.Callback();

//        BaseObject boss = AIKit.FindNearestTarget(Character.Self, x => x.GetObjectType() == OBJECT_TYPE.MONSTER_BOSS);
//        yield return new WaitFor(() => AIKit.InBounds(Character.Self.GetPosition(), boss.GetPosition(), Character.Self.LookBound() + 1f));

//        MainProcess.mStage.DisableAI();
//    }

//    protected override void OnBreak()
//    {
//        GuideKit.SetAllFriendsAI(null);
//        MainProcess.mStage.EnableAI();
//        ActiveBirth.mEnableMonsterBossUIInfo = true;
//    }
//}


//public class MG_SelectPetUpgradeMaterial : GuideNodeRoutine
//{
//    protected override IEnumerator DoMain()
//    {
//        GameObject win = GameCommon.FindUI("bag_pet_window", "up_group");
        
//        for (int i = 0; i < 6; ++i)
//        {
//            GameObject btn = GameCommon.FindObject(win, "group(Clone)_" + i, "bag_pet_multiple_icon_btn");

//            if (btn == null || !btn.activeInHierarchy)
//            {
//                break;
//            }

//            GuideKit.SetCursorOffset(180f, -20f);
//            yield return new ButtonMaskRoutine(btn, true);
//        }

//        yield return new ButtonMaskRoutine("pet_bag_ok_button");
//    }
//}