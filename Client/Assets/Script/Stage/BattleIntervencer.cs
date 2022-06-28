using UnityEngine;
using DataTable;


public interface IBattleIntervencer : IBattleMonitor
{
    /// <summary>
    /// 根据目标（如果被干预）当前受到的伤害值计算干预后的伤害值
    /// </summary>
    /// <param name="skill"> 伤害技能 </param>
    /// <param name="target"> 待计算目标 </param>
    /// <param name="damage"> 受到的伤害值 </param>
    /// <returns> 干预后的伤害值 </returns>
    int FinalDamage(Skill skill, BaseObject target, int damage);
}


public class PvpBattleIntervencer : BattleMonitor, IBattleIntervencer
{
    public enum INTERVENCED_RESULT
    {
        UNDETERMINED = 0,
        SELF_WIN = 1,
        SELF_LOSE = 2,
    }

    public INTERVENCED_RESULT result { get; private set; }

    private int winTeamInitTotalHp = 10000;

    public PvpBattleIntervencer()
    {
        this.result = INTERVENCED_RESULT.UNDETERMINED;
    }

    public void SetPvpResult(bool selfWin)
    {
        result = selfWin ? INTERVENCED_RESULT.SELF_WIN : INTERVENCED_RESULT.SELF_LOSE;
        winTeamInitTotalHp = GetTeamTotalHp(selfWin);
    }

    private int GetTeamTotalHp(bool selfTeam)
    {
        int hp = 0;

        if (selfTeam && Character.Self != null)
        {
            hp += Character.Self.GetHp();

            for (int i = 0; i < Character.msFriendsCount; ++i)
            {
                var f = Character.Self.mFriends[i];

                if (f != null)
                    hp += f.GetHp();
            }
        }
        else if (!selfTeam && OpponentCharacter.mInstance != null)
        {
            hp += OpponentCharacter.mInstance.GetHp();

            for (int i = 0; i < OpponentCharacter.msFriendsCount; ++i)
            {
                var f = OpponentCharacter.mInstance.mFriends[i];

                if (f != null)
                    hp += f.GetHp();
            }
        }

        return hp;
    }

    public int FinalDamage(Skill skill, BaseObject target, int damage)
    {
        if (result == INTERVENCED_RESULT.UNDETERMINED)
        {
            return damage;
        }
        else
        {
            int current = GetTeamTotalHp(result == INTERVENCED_RESULT.SELF_WIN);
            float rate = (float)current / winTeamInitTotalHp;
            float winFactor = Mathf.Clamp01((rate - 0.1f) / 0.3f);
            float loseFactor = Mathf.Max(1f, 1.5f / (winFactor + 0.5f));

            if ((result == INTERVENCED_RESULT.SELF_WIN) ^ (target.GetCamp() != CommonParam.PlayerCamp))
            {
                return Mathf.Min((int)(damage * winFactor), current - 1);
            }
            else
            {
                return (int)(damage * loseFactor);
            }
        }
    }
}


public class BossBattleIntervencer : BattleMonitor, IBattleIntervencer
{
    public BossBattle bossBattle;
    public int bossStartHp = 0;         // Boss起始血量
    public int bossFinalHp = 0;         // Boss最终血量
    public float battleFinalTime = 0f;  // 战斗持续时长

    public int FinalDamage(Skill skill, BaseObject target, int damage)
    {
        if (bossBattle != null && target == bossBattle.mBoss)
        {
            if (bossBattle.mbBattleFinish)
            {
                return 0;
            }
            else if (battleFinalTime - bossBattle.mBattleTime < 2f && bossFinalHp <= 0)
            {
                return target.GetHp() + 10;
            }
        }

        return damage;
    }
}


public class PveBattleIntervencer : BattleMonitor, IBattleIntervencer
{
    private float coeffToRole = 1f;
    private float coeffToMonster = 1f;

    public PveBattleIntervencer(int stageID)
    {
        DataRecord r = DataCenter.mStageTable.GetRecord(stageID);

        if (r != null)
        {
            float teamPower = GameCommon.GetPower();
            int monsterPower = r["NEED_BATTLE"];
            int difficultyNum = r["DIFFICULTY_NUM"];
            float rate = teamPower / Mathf.Max(1, monsterPower);

            if (difficultyNum == 0)
            {
                coeffToRole = 1f;
                coeffToMonster = 1f;
            }
            else
            {
                coeffToRole = Mathf.Pow(1f / rate, difficultyNum);
                coeffToMonster = Mathf.Pow(rate, difficultyNum);
            }
        }
        else 
        {
            coeffToRole = 1f;
            coeffToMonster = 1f;
        }
    }

    public int FinalDamage(Skill skill, BaseObject target, int damage)
    {
        if (target.GetCamp() == CommonParam.MonsterCamp)
        {
            return (int)(damage * coeffToMonster);
        }
        else
        {
            return (int)(damage * coeffToRole);
        }
    }
}