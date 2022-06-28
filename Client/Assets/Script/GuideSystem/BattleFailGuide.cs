//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using DataTable;
//using Utilities.Routines;



//public class BattleFailGuideCondition
//{
//    public static float teamPetAverageLevel { get; private set; }       // 上阵符灵的平均等级
//    public static int teamPetAboveOrangeCount { get; private set; }     // 橙色以上品质的上阵符灵数
//    public static int totalEquipStrengthenLevel { get; private set; }   // 装备强化等级总数
//    public static int equippedCount { get; private set; }               // 已装备数
//    public static int playerLevel { get; private set; }                 // 玩家等级

//    /// <summary>
//    /// 刷新各个状态条件参数，在判定条件之前先调用此方法
//    /// </summary>
//    public static void Refresh()
//    {      
//        teamPetAverageLevel = GetTeamPetAverageLevel();
//        teamPetAboveOrangeCount = GetTeamPetAboveOrangeCount();
//        totalEquipStrengthenLevel = GetTotalEquipStrengthenLevel();
//        equippedCount = GetEquippedCount();
//        playerLevel = RoleLogicData.GetMainRole().level;
//    }

//    /// <summary>
//    /// 获取上阵符灵的平均等级
//    /// </summary>
//    /// <returns> 上阵符灵的平均等级 </returns>
//    public static float GetTeamPetAverageLevel()
//    {
//        int petCount = 0;
//        int totalLevel = 0;

//        for (int i = (int)TEAM_POS.PET_1; i < (int)TEAM_POS.MAX; ++i)
//        {
//            var petData = TeamManager.GetPetDataByTeamPos(i);

//            if (petData != null)
//            {
//                ++petCount;
//                totalLevel += petData.level;
//            }
//        }

//        return petCount > 0 ? (float)totalLevel / petCount : 0f;
//    }

//    /// <summary>
//    /// 获取橙色以上品质的上阵符灵数
//    /// </summary>
//    /// <returns> 橙色以上品质的上阵符灵数 </returns>
//    public static int GetTeamPetAboveOrangeCount()
//    {
//        int count = 0;

//        for (int i = (int)TEAM_POS.PET_1; i < (int)TEAM_POS.MAX; ++i)
//        {
//            var petData = TeamManager.GetPetDataByTeamPos(i);

//            if (petData != null && petData.starLevel >= (int)PET_QUALITY.ORANGE)
//            {
//                ++count;
//            }
//        }

//        return count;
//    }

//    /// <summary>
//    /// 获取装备强化等级总数
//    /// </summary>
//    /// <returns> 装备强化等级总数 </returns>
//    public static int GetTotalEquipStrengthenLevel()
//    {
//        int totalLevel = 0;

//        foreach (var pair in RoleEquipLogicData.Self.mDicEquip)
//        {
//            if (pair.Value != null)
//            {
//                totalLevel += pair.Value.strengthenLevel;
//            }
//        }

//        return totalLevel;
//    }

//    /// <summary>
//    /// 获取已装备数
//    /// </summary>
//    /// <returns> 已装备数 </returns>
//    public static int GetEquippedCount()
//    {
//        int count = 0;

//        foreach (var pair in RoleEquipLogicData.Self.mDicEquip)
//        {
//            if (pair.Value != null && pair.Value.teamPos >= 0)
//            {
//                ++count;
//            }
//        }

//        return count;
//    }

//    public int type { get; private set; }
//    public int param { get; private set; }

//    public BattleFailGuideCondition(int type, int param)
//    {
//        this.type = type;
//        this.param = param;
//    }

//    /// <summary>
//    /// 判定条件是否通过
//    /// </summary>
//    /// <returns></returns>
//    public bool Pass()
//    {
//        switch (type)
//        {
//            case 0: // 只要失败就会弹出提示
//                return true;

//            case 1: // 上阵符灵平均等级小于指定值
//                return (int)teamPetAverageLevel < param;

//            case 2: // 上阵橙卡数小于指定值
//                return teamPetAboveOrangeCount < param;

//            case 3: // 装备强化等级总数小于指定值
//                return totalEquipStrengthenLevel < param;

//            case 4: // 已装备数小于指定值
//                return equippedCount < param;

//            default:
//                return false;
//        }
//    }
//}


//public class BattleFailGuideItem
//{
//    public ConfigParam uiParam { get; private set; }
//    public ConfigParam tipParam { get; private set; }
//    public int minLevel { get; private set; }
//    public int maxLevel { get; private set; }
//    public int maxCount { get; private set; }
//    public float frequency { get; private set; }
//    public float lastGuideRealTime { get; set; }
//    public int guideCount { get; set; }

//    public BattleFailGuideCondition[] conditions { get; private set; }

//    public BattleFailGuideItem(DataRecord r)
//    {
//        minLevel = r["MIN_LEVEL"];
//        maxLevel = r["MAX_LEVEL"];
//        maxCount = r["TIP_COUNT"];
//        frequency = r["SHOW_FREQUENCY"];
//        lastGuideRealTime = -9999f;
//        guideCount = 0;
//        uiParam = new ConfigParam((string)r["UI"]);
//        tipParam = new ConfigParam((string)r["TEXT"]);

//        conditions = null;
//        string conditionText = r["CONTENT"];

//        if (!string.IsNullOrEmpty(conditionText))
//        {
//            List<BattleFailGuideCondition> conditionList = new List<BattleFailGuideCondition>();
//            string[] split = conditionText.Split('|');

//            foreach (var s in split)
//            {
//                int n = s.IndexOf('#');

//                if (n > 0)
//                {
//                    string typeStr = s.Substring(0, n);
//                    string paramStr = s.Substring(n + 1, s.Length - n - 1);
//                    int type = 0;
//                    int param = 0;
//                    int.TryParse(typeStr, out type);
//                    int.TryParse(paramStr, out param);
//                    conditionList.Add(new BattleFailGuideCondition(type, param));
//                }
//            }

//            conditions = conditionList.ToArray();
//        }
//    }

//    public bool Pass()
//    {
//        if (conditions == null || conditions.Length == 0
//            || BattleFailGuideCondition.playerLevel < minLevel
//            || BattleFailGuideCondition.playerLevel > maxLevel
//            || guideCount >= maxCount
//            || Time.realtimeSinceStartup - lastGuideRealTime < frequency)
//        {
//            return false;
//        }

//        // 各个条件之间是“或”的关系
//        foreach (var condition in conditions)
//        {
//            if (condition.Pass())
//            {
//                return true;
//            }
//        }

//        return false;
//    }
//}


//public class BattleFailGuideRoutine : Routine
//{
//    public BattleFailGuideRoutine(BattleFailGuideItem[] items)
//    {
//        Bind(DoBattleFailGuide(items));
//    }

//    private IEnumerator DoBattleFailGuide(BattleFailGuideItem[] items)
//    {
//        if (items != null && items.Length > 0)
//        {
//            float delta = 0.5f / items.Length;
//            WaitUIVisible[] routines = new WaitUIVisible[items.Length];

//            // 协程的耗时操作FindUI()错开时间
//            for (int i = 0; i < routines.Length; ++i)
//            {
//                routines[i] = new WaitUIVisible(delta * i, 0.5f, items[i].uiParam.GetStringArray("ui_name"));
//            }

//            yield return new Any(routines);

//            for (int i = 0; i < routines.Length; ++i)
//            {
//                if (routines[i].status == RoutineStatus.Done)
//                {
//                    ShowGuide(routines[i].target, items[i]);
//                    break;
//                }
//            }
//        }
//    }

//    private void ShowGuide(GameObject button, BattleFailGuideItem item)
//    {
//        if (button == null)
//            return;

//        item.guideCount++;
//        item.lastGuideRealTime = Time.realtimeSinceStartup;

//        Vector2 cursorOffset = item.uiParam.GetVector2("offset");
//        float cursorRotate = item.uiParam.GetFloat("rotate");
//        bool cursorFlip = item.uiParam.GetInt("flip") > 0;
//        GuideKit.SetCursorOffset(cursorOffset.x, cursorOffset.y);
//        Vector3 cursorAngles = new Vector3(0f, cursorFlip ? 180f : 0f, cursorRotate);
//        GuideKit.SetCursorRotate(cursorAngles);
//        GuideKit.OpenCursor(button, 1f);

//        Vector2 tipOffset = item.tipParam.GetVector2("offset");
//        string tipText = item.tipParam.GetString("text");

//        if (!string.IsNullOrEmpty(tipText))
//        {
//            GuideKit.SetTipOffset(tipOffset.x, tipOffset.y);
//            GuideKit.OpenTip(button, tipText);
//        }
//    }

//    protected override void OnBreak()
//    {
//        GuideKit.CloseCursorImmediate();
//        GuideKit.CloseTipImmediate();
//    }
//}


//public class BattleFailGuide
//{
//    public static BattleFailGuideItem[] guideItems;

//    static BattleFailGuide()
//    {
//        StageBattle.mOnStageFinish += OnStageFinish;
//    }

//    public static void Reset()
//    {
//        List<BattleFailGuideItem> list = new List<BattleFailGuideItem>();

//        foreach (var pair in DataCenter.mFailGuide.GetAllRecord())
//        {
//            if (pair.Key > 0)
//            {
//                list.Add(new BattleFailGuideItem(pair.Value));
//            }
//        }

//        guideItems = list.ToArray();
//    }

//    private static void OnStageFinish()
//    {
//        if (MainProcess.mStage != null && !MainProcess.mStage.mbSucceed && !Guide.isActive && guideItems != null)
//        {
//            BattleFailGuideCondition.Refresh();
//            List<BattleFailGuideItem> items = new List<BattleFailGuideItem>();

//            foreach (var item in guideItems)
//            {
//                if (item.Pass())
//                {
//                    items.Add(item);
//                }
//            }

//            MainProcess.Self.gameObject.StartRoutine(new BattleFailGuideRoutine(items.ToArray()));
//        }
//    }
//}