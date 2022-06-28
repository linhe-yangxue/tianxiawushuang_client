using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;


public class SimulatorLog
{
    public BattleSimulator simulator;
    public bool saveLogFile = true;
    public readonly string rootDirectory = Application.dataPath + "/../Simulator Log/";
    public string logDirectory { get; private set; }
    public string currentCampaignLogFile { get; private set; }
    public string currentBattleLogFile { get; private set; }

    public SkillDamageDistribution skillDamageDistribution = new SkillDamageDistribution();
    private int[] objectIDs = new int[8];
    private long[] totalDamages = new long[8];
    private float[] totalTimes = new float[8];
    private int totalWinCount = 0;
    private int totalLoseCount = 0;

    public void LogSkillDamage(SkillDamageInfo damageInfo)
    {
        skillDamageDistribution.Add(damageInfo);
    }

    public void InitLogDirectory()
    {
        if (!saveLogFile)
            return;

        string foldName = DateTime.Now.ToString("yyyyMMdd HH_mm_ss");
        logDirectory = rootDirectory + foldName + "/";

        if (!Directory.Exists(rootDirectory))
        {
            Directory.CreateDirectory(rootDirectory);
        }

        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
    }

    public void OnCampaignStart()
    {
        if (!saveLogFile)
            return;

        totalWinCount = 0;
        totalLoseCount = 0;
        totalDamages = new long[8];
        totalTimes = new float[8];
        currentCampaignLogFile = logDirectory + "第" + simulator.currentCompaign + "场.txt";
        StreamWriter writer = new StreamWriter(currentCampaignLogFile, File.Exists(currentCampaignLogFile), Encoding.Unicode);

        if (simulator.loadConfigFromTable)
        {
            writer.WriteLine(" ***** " + simulator.currentSelfTeamID + " VS " + simulator.currentOppoTeamID + " *****");
            writer.WriteLine();
        }

        writer.WriteLine("           ID       等级    突破等级  天命等级    武器      饰品      衣服       鞋    攻击神器  防御神器   名称");
        ForEachRole((i, obj) => LogObjectInfo(writer, obj, i < 4 ? simulator.roleInfos[i] : simulator.opRoleInfos[i - 4], i));
        writer.WriteLine();
        writer.WriteLine("          血量       攻击       物防       法防        命中        闪避      暴击率      抗暴率    伤害加深    伤害减免");
        ForEachRole((i, obj) => LogAttribute(writer, obj, i));
        writer.Close();
    }

    public void OnCampaignFinish()
    {
        if (!saveLogFile)
            return;

        StreamWriter writer = new StreamWriter(currentCampaignLogFile, File.Exists(currentCampaignLogFile), Encoding.Unicode);
        writer.WriteLine();
        writer.WriteLine(" ----- 合计 A胜 {0}  B胜 {1} 平 {2} -----", totalWinCount, totalLoseCount, simulator.loopCount - totalWinCount - totalLoseCount);
        writer.WriteLine("              总伤害         总DPS");

        for (int i = 0; i < 8; ++i)
        {
            string text = string.Format("{0,4}{1,16}{2,14:f0}", Index2Tag(i), totalDamages[i], totalTimes[i] > 0.001f ? totalDamages[i] / totalTimes[i] : 0f);
            writer.WriteLine(text);
        }

        writer.Close();
    }

    public void OnBattleStart()
    {
        if (!saveLogFile)
            return;

        currentBattleLogFile = logDirectory + simulator.currentCompaign + "-" + simulator.currentLoop + ".txt";
        skillDamageDistribution = new SkillDamageDistribution();
        objectIDs = new int[8];
        ForEachRole((i, obj) => objectIDs[i] = obj.mID);
    }

    public void OnBattleFinish()
    {
        if (!saveLogFile)
            return;

        if (!simulator.battleTimeOut)
        {
            if (simulator.stage.mbSucceed)
                ++totalWinCount;
            else
                ++totalLoseCount;
        }

        StreamWriter writer = new StreamWriter(currentCampaignLogFile, File.Exists(currentCampaignLogFile), Encoding.Unicode);
        writer.WriteLine();
        string titleLabel = " ----- 第" + simulator.currentLoop + "轮 ";
        titleLabel += simulator.battleTimeOut ? "平" : (simulator.stage.mbSucceed ? "胜" : "负");
        titleLabel += " FPS=" + simulator.battleAverageFps.ToString("f1") + " -----";
        writer.WriteLine(titleLabel);
        writer.WriteLine("    状态 战斗时间 Total DPS  普攻(DPH/DPS)        技能(ID = DPH/DPS)");
        ForEachRole((i, obj) => LogObjectTotalDamage(writer, obj, i));
        writer.Close();

        writer = new StreamWriter(currentBattleLogFile, File.Exists(currentBattleLogFile), Encoding.Unicode);
        writer.WriteLine("时间 攻击者   技能ID      总伤害    伤害分布(受击者:伤害(M = miss, C = 暴击))");

        foreach (var i in skillDamageDistribution.allDamageInfos)
        {
            writer.WriteLine(GetMultiDamageText(i));
        }

        writer.Close();
    }

    private void LogObjectInfo(StreamWriter writer, BaseObject targetObject, BattleSimulator.RoleBattleInfo info, int index)
    {
        string text = string.Format("{0,-4}", Index2Tag(index));
        text += string.Format("{0,10}", targetObject.mConfigIndex);     
        text += string.Format("{0,10}", info.level);
        text += string.Format("{0,10}", info.breakLevel);
        text += string.Format("{0,10}", info.fateLevel);
        text += string.Format("{0,10}", info.equips[0].tid);
        text += string.Format("{0,10}", info.equips[1].tid);
        text += string.Format("{0,10}", info.equips[2].tid);
        text += string.Format("{0,10}", info.equips[3].tid);
        text += string.Format("{0,10}", info.equips[4].tid);
        text += string.Format("{0,10}", info.equips[5].tid);
        text += "   " + (string)targetObject.mConfigRecord["NAME"];
        writer.WriteLine(text);
    }

    private void LogAttribute(StreamWriter writer, BaseObject targetObject, int index)
    {
        var attr = CreateAttributeInfo(targetObject);
        writer.WriteLine(string.Format("{0,-4}", Index2Tag(index)) + MakeTextFromAttributeInfo(attr));
    }

    private BattleSimulator.AttributeInfo CreateAttributeInfo(BaseObject targetObject)
    {
        var attr = new BattleSimulator.AttributeInfo();
        attr.maxHp = targetObject.mStaticMaxHp;
        attr.attack = targetObject.mStaticAttack;
        attr.physicalDefence = targetObject.mStaticPhysicalDefence;
        attr.magicDefence = targetObject.mStaticMagicDefence;
        attr.hitRate = targetObject.mStaticHitRate;
        attr.dodgeRate = targetObject.mStaticDodgeRate;
        attr.criticalStrikeRate = targetObject.mStaticCriticalStrikeRate;
        attr.defenceCriticalStrikeRate = targetObject.mStaticDefenceCriticalStrikeRate;
        attr.damageEnhanceRate = targetObject.mStaticDamageEnhanceRate;
        attr.damageMitigationRate = targetObject.mStaticDamageMitigationRate;
        return attr;
    }

    private string MakeTextFromAttributeInfo(BattleSimulator.AttributeInfo info)
    {
        return string.Format("{0,10} {1,10} {2,10} {3,10} {4,10:f1}% {5,10:f1}% {6,10:f1}% {7,10:f1}% {8,10:f1}% {9,10:f1}%",
            info.maxHp,
            info.attack,
            info.physicalDefence,
            info.magicDefence,
            info.hitRate * 100,
            info.dodgeRate * 100,
            info.criticalStrikeRate * 100,
            info.defenceCriticalStrikeRate * 100,
            info.damageEnhanceRate * 100,
            info.damageMitigationRate * 100);
    }

    private void LogObjectTotalDamage(StreamWriter writer, BaseObject obj, int index)
    {
        string text = string.Format("{0,-4}", Index2Tag(index));
        text += obj.IsDead() ? "死亡" : "存活";
        float time = obj.aiMachine.totalRunTime;
        totalTimes[index] += time;
        text += string.Format("{0,8:f1}", time);
        ObjectDamageInfo damageInfo = skillDamageDistribution.GetObjectDamageDistribution(obj.mID);

        if (damageInfo != null)
        {
            totalDamages[index] += damageInfo.totalDamage;
            float totalDPS = damageInfo.totalDamage / time;
            text += string.Format("    {0,-10:f0}", totalDPS);
            text += string.Format("{0,-20}", FormatDamageInfo(damageInfo.totalNormalDamageInfo, time));

            for (int i = 0; i < damageInfo.totalSkillDamageInfos.Count; ++i)
            {
                text += string.Format("{0} = {1,-20}", damageInfo.totalSkillDamageInfos[i].skillIndex, FormatDamageInfo(damageInfo.totalSkillDamageInfos[i], time));
            }
        }
        else 
        {
            text += "    0         0/0";
        }

        writer.WriteLine(text);
    }

    private void ForEachRole(Action<int, BaseObject> action)
    {
        if (Character.Self != null)
        {
            action(0, Character.Self);

            if (Character.Self.mFriends != null)
            {
                for (int i = 0; i < Character.Self.mFriends.Length; ++i)
                {
                    if (Character.Self.mFriends[i] != null)
                    {
                        action(i + 1, Character.Self.mFriends[i]);
                    }
                }
            }
        }

        if (OpponentCharacter.mInstance != null)
        {
            action(4, OpponentCharacter.mInstance);

            if (OpponentCharacter.mInstance.mFriends != null)
            {
                for (int i = 0; i < OpponentCharacter.mInstance.mFriends.Length; ++i)
                {
                    if (OpponentCharacter.mInstance.mFriends[i] != null)
                    {
                        action(i + 5, OpponentCharacter.mInstance.mFriends[i]);
                    }
                }
            }
        }
    }

    private string FormatDamageInfo(TotalDamageInfo info, float time)
    {
        string text = info.skillCount > 0 ? (info.totalDamage / (float)info.skillCount).ToString("f0") : "0";
        text += "/";
        text += (info.totalDamage / time).ToString("f0");
        return text;
    }

    private string ID2Tag(int id)
    {
        for (int i = 0; i < objectIDs.Length; ++i)
        {
            if (objectIDs[i] == id)
            {
                return Index2Tag(i);
            }
        }

        return "";
    }

    private string Index2Tag(int index)
    {
        if (index >= 0 && index < 8)
        {
            return index < 4 ? "A" + index : "B" + (index - 4);
        }

        return "";
    }

    private string GetMultiDamageText(MultiSkillDamageInfo info)
    {
        string text = string.Format("{0,-7:f1}{1,4}{2,11}{3,10}    ", 
            info.timeSinceBattleStart, 
            ID2Tag(info.attackerID), 
            info.skillIndex + (Skill.IsNormalAttack(info.skillIndex) ? "(普)" : "(法)"), 
            info.totalDamage);

        for (int i = 0; i < info.count; ++i)
        {
            var t = info.GetDamageInfo(i);
            text += string.Format("{0}:{1}", ID2Tag(t.damagerID), t.damageValue);

            if ((t.flags & SKILL_DAMAGE_FLAGS.IS_MISSING) > 0)
            {
                text += "(M)";
            }
            if ((t.flags & SKILL_DAMAGE_FLAGS.IS_CRITICAL) > 0)
            {
                text += "(C)";
            }

            text += "    ";
        }

        return text;
    }
}


public class SkillDamageInfo
{
    public float timeSinceBattleStart = 0f;
    public int skillIndex = 0;
    public int skillUniqueID = 0;
    public int attackerID = 0;
    public int damagerID = 0;
    public int damageValue = 0;
    public SKILL_DAMAGE_FLAGS flags = SKILL_DAMAGE_FLAGS.NONE;
}

public class TotalDamageInfo
{
    public int skillIndex = 0;
    public int skillCount = 0;
    public int totalDamage = 0;
}

public class ObjectDamageInfo
{
    public int attackerID = 0;
    public TotalDamageInfo totalNormalDamageInfo = new TotalDamageInfo();
    public List<TotalDamageInfo> totalSkillDamageInfos = new List<TotalDamageInfo>();

    public int totalDamage
    {
        get
        {
            int total = totalNormalDamageInfo.totalDamage;

            for (int i = 0; i < totalSkillDamageInfos.Count; ++i)
            {
                total += totalSkillDamageInfos[i].totalDamage;
            }

            return total;
        }
    }
}

public class SkillDamageDistribution
{
    public List<ObjectDamageInfo> objectDamageInfos = new List<ObjectDamageInfo>();
    private HashSet<int> applyedSkillUniqueIDs = new HashSet<int>();
    public List<MultiSkillDamageInfo> allDamageInfos = new List<MultiSkillDamageInfo>();

    public void Add(SkillDamageInfo info)
    {
        ObjectDamageInfo objInfo = objectDamageInfos.Find(x => x.attackerID == info.attackerID);

        if (objInfo == null)
        {
            objInfo = new ObjectDamageInfo();
            objInfo.attackerID = info.attackerID;
            objectDamageInfos.Add(objInfo);
        }

        TotalDamageInfo totalInfo;

        if (Skill.IsNormalAttack(info.skillIndex))
        {
            totalInfo = objInfo.totalNormalDamageInfo;
        }
        else
        {
            totalInfo = objInfo.totalSkillDamageInfos.Find(x => x.skillIndex == info.skillIndex);

            if (totalInfo == null)
            {
                totalInfo = new TotalDamageInfo();
                totalInfo.skillIndex = info.skillIndex;
                objInfo.totalSkillDamageInfos.Add(totalInfo);
            }
        }

        if (applyedSkillUniqueIDs.Add(info.skillUniqueID))
        {
            totalInfo.skillCount++;
            var multiInfo = new MultiSkillDamageInfo();
            multiInfo.Add(info);
            allDamageInfos.Add(multiInfo);
        }
        else 
        {
            var multiInfo = allDamageInfos.Find(x => x.skillUniqueID == info.skillUniqueID);
            multiInfo.Add(info);
        }

        totalInfo.totalDamage += info.damageValue;
    }

    public ObjectDamageInfo GetObjectDamageDistribution(int attackerID)
    {
        return objectDamageInfos.Find(x => x.attackerID == attackerID);
    }
}

public class MultiSkillDamageInfo
{
    public class DamageInfo
    {
        public int damagerID { get; private set; }
        public int damageValue { get; set; }
        public SKILL_DAMAGE_FLAGS flags { get; set; }

        public DamageInfo(int damagerID, int damageValue, SKILL_DAMAGE_FLAGS flags)
        {
            this.damagerID = damagerID;
            this.damageValue = damageValue;
            this.flags = flags;
        }

        public DamageInfo(SkillDamageInfo skillDamageInfo)
        {
            this.damagerID = skillDamageInfo.damagerID;
            this.damageValue = skillDamageInfo.damageValue;
            this.flags = skillDamageInfo.flags;
        }
    }

    public float timeSinceBattleStart { get; private set; }
    public int skillIndex { get; private set; }
    public int skillUniqueID { get; private set; }
    public int attackerID { get; private set; }
    public int count { get; private set; }
    public int totalDamage { get; private set; }

    private DamageInfo singleInfo = null;
    private List<DamageInfo> multiInfo = null;

    public void Add(SkillDamageInfo info)
    {
        for (int i = 0; i < count; ++i)
        {
            var t = GetDamageInfo(i);

            if (t.damagerID == info.damagerID)
            {
                totalDamage += info.damageValue;
                t.damageValue += info.damageValue;
                t.flags |= info.flags;
                return;
            }
        }

        if (count == 0)
        {
            timeSinceBattleStart = info.timeSinceBattleStart;
            skillIndex = info.skillIndex;
            skillUniqueID = info.skillUniqueID;
            attackerID = info.attackerID;
            count = 1;
            totalDamage += info.damageValue;
            singleInfo = new DamageInfo(info);
        }
        else
        {
            if (count == 1)
            {
                multiInfo = new List<DamageInfo>();
                multiInfo.Add(singleInfo);
                singleInfo = null;
            }

            ++count;
            totalDamage += info.damageValue;
            multiInfo.Add(new DamageInfo(info));
        }
    }

    public DamageInfo GetDamageInfo(int i)
    {
        if (i < 0 || i >= count)
        {
            return null;
        }

        return count == 1 ? singleInfo : multiInfo[i];
    }
}