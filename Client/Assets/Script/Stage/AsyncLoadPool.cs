using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;


public class AsyncLoadPool : MonoBehaviour
{
    private class AsyncLoadInfo
    {
        public int priority = 0;
        public string path = "";
    }

    private Dictionary<int, int> enqueuedBattleObjects = new Dictionary<int, int>();
    private HashSet<string> loadHistory = new HashSet<string>();
    private List<AsyncLoadInfo> resPathPool = new List<AsyncLoadInfo>();
    private bool _hasStarted = false;
    private bool _hasPaused = false;
    private bool _allLoadDone = false;
    private static AsyncLoadPool _instance;

    private static AsyncLoadPool instance
    {
        get 
        {
            if (_instance == null)
            {
                GameObject helper = new GameObject("async_load_pool");
                _instance = helper.AddComponent<AsyncLoadPool>();
            }

            return _instance;
        }
    }

    public static void Enqueue(string path) { instance._Enqueue(path, 0); }
    public static void Enqueue(string path, int priority) { instance._Enqueue(path, priority); }
    public static void Start() { instance._Start(); }
    public static void Stop() { instance._Stop(); }
    public static void Reset() { instance._Reset(); }

    public static void EnqueueModel(int modelIndex, int priority = 0)
    {
        DataRecord r = DataCenter.mModelTable.GetRecord(modelIndex);

        if (r != null)
        {
            Enqueue((string)r["BODY"], priority);
            Enqueue((string)r["BODY_TEX_1"], priority);
            Enqueue((string)r["ANIMATION"], priority);
        }
    }

    public static void EnqueueEffect(int effectIndex, int priority = 0)
    {
        DataRecord r = DataCenter.mEffect.GetRecord(effectIndex);

        if (r != null)
        {
            Enqueue((string)r["MODEL"], priority);
        }
    }

    public static void EnqueueSkill(int skillIndex, int priority = 0)
    {
        DataRecord r = DataCenter.mSkillConfigTable.GetRecord(skillIndex);

        if (r != null)
        {
            int effect = r["START_EFFECT"];

            if (effect > 0)
                EnqueueEffect(effect, priority);

            effect = r["ATTACK_EFFECT"];

            if (effect > 0)
                EnqueueEffect(effect, priority);

            effect = r["DAMAGE_EFFECT"];

            if (effect > 0)
                EnqueueEffect(effect, priority);

            int buff = r["BUFF_ADDITION"];

            if (buff > 0)
                EnqueueBuff(buff, priority);
        }
    }

    public static void EnqueueBuff(int buffIndex, int priority)
    {
        DataRecord r = DataCenter.mAffectBuffer.GetRecord(buffIndex);

        if (r != null)
        {
            int effect = r["EFFECT"];

            if(effect > 0)
                EnqueueEffect(effect, priority);
        }
    }

    public static void EnqueueBattleObject(int objIndex, int priority = 0)
    {
        int p = 0;

        if (instance.enqueuedBattleObjects.TryGetValue(objIndex, out p) && p >= priority)
            return;
        else
            instance.enqueuedBattleObjects.Add(objIndex, priority);

        OBJECT_TYPE type = ObjectManager.Self.GetObjectType(objIndex);
        NiceTable table;

        if (type == OBJECT_TYPE.BIG_BOSS)
            table = DataCenter.mBossConfig;
        else if (type == OBJECT_TYPE.MONSTER || type == OBJECT_TYPE.MONSTER_BOSS)
            table = DataCenter.mMonsterObject;
        else
            table = DataCenter.mActiveConfigTable;

        var r = table.GetRecord(objIndex);

        if (r != null)
            EnqueueBattleObject(r, priority);          
    }

    private static void EnqueueBattleObject(DataRecord objRecord, int priority)
    {
        int model = objRecord["MODEL"];

        if (model > 0)
            EnqueueModel(model, priority);

        int attackSkillIndex = objRecord["ATTACK_SKILL"];

        if (attackSkillIndex > 0)
            EnqueueSkill(attackSkillIndex, priority);

        for (int i = 1; i <= 4; ++i)
        {
            int skillIndex = objRecord["PET_SKILL_" + i];
            int category = skillIndex / 100000;

            if (category == 1)
                EnqueueSkill(skillIndex, priority);
            else if (category == 3)
                EnqueueSkill(skillIndex, priority);
        }
    }

    public static bool hasStarted
    {
        get { return instance._hasStarted; }
        set { instance._hasStarted = value; }
    }

    public static bool paused
    {
        get { return instance._hasPaused; }
        set { instance._hasPaused = value; }
    }

    public static bool allLoadDone
    {
        get { return instance._allLoadDone; }
    }

    private void _Enqueue(string path, int priority)
    {
        if (!string.IsNullOrEmpty(path) && !loadHistory.Contains(path))
        {
            int index = resPathPool.FindIndex(x => x.path == path);

            if (index >= 0)
            {
                if (resPathPool[index].priority >= priority)
                    return;
                else
                    resPathPool.RemoveAt(index);
            }

            AsyncLoadInfo info = new AsyncLoadInfo();
            info.priority = priority;
            info.path = path;

            int i;

            for (i = resPathPool.Count - 1; i >= 0; --i)
            {
                if (priority <= resPathPool[i].priority)
                {
                    resPathPool.Insert(i + 1, info);
                    break;
                }
            }

            if (i < 0)
            {
                resPathPool.Insert(0, info);
            }

            if (_hasStarted && _allLoadDone)
            {
                StartCoroutine(DoLoadAsync());
            }
        }
    }

    private void _Start()
    {
        if (!_hasStarted)
        {
            _hasStarted = true;
            _hasPaused = false;
            _allLoadDone = true;

            if (resPathPool.Count > 0)
            {
                StartCoroutine(DoLoadAsync());
            }
        }
    }

    private void _Stop()
    {
        _hasStarted = false;
        _hasPaused = false;
        _allLoadDone = false;
        resPathPool.Clear();
        enqueuedBattleObjects.Clear();
    }

    private void _Reset()
    {
        _Stop();
        loadHistory.Clear();
    }

    private IEnumerator DoLoadAsync()
    {
        _allLoadDone = false;
        yield return null;

        while (resPathPool.Count > 0)
        {
            if (_hasPaused)
            {
                yield return null;
            }
            else
            {
                string path = resPathPool[0].path;
                resPathPool.RemoveAt(0);
                loadHistory.Add(path);
                yield return Resources.LoadAsync<UnityEngine.Object>(path);           
            }
        }

        _allLoadDone = true;
    }
}