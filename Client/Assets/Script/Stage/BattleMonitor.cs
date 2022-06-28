
public interface IBattleMonitor
{
    void OnBattleActive();
    void OnBattleFinish(bool win);
    void OnObjectStart(BaseObject obj);
    void OnObjectDead(BaseObject obj);
    void OnDoSKill(Skill skill);
    void OnSkillDamage(Skill skill, BaseObject target, int damageValue, SKILL_DAMAGE_FLAGS flags);
}


public class BattleMonitor : IBattleMonitor
{
    public virtual void OnBattleActive() { }
    public virtual void OnBattleFinish(bool win) { }
    public virtual void OnObjectStart(BaseObject obj) { }
    public virtual void OnObjectDead(BaseObject obj) { }
    public virtual void OnDoSKill(Skill skill) { }
    public virtual void OnSkillDamage(Skill skill, BaseObject target, int damageValue, SKILL_DAMAGE_FLAGS flags) { }
}