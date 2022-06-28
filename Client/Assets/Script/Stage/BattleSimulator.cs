using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Utilities.Routines;
using Logic;
using DataTable;


public class BattleSimulator : MonoBehaviour
{
    [Serializable]
    public class EquipInfo
    {
        public int tid = 0;                         // ID
        public int strengthenLevel = 1;             // 强化等级
        public int refineLevel = 0;                 // 精炼等级
    }

    [Serializable]
    public class AttributeInfo
    {
        public int maxHp = 1000;                     // 最大HP值       
        public int attack = 100;                     // 攻击力
        public int physicalDefence = 0;              // 物理防御
        public int magicDefence = 0;                 // 魔法防御
        public float criticalStrikeRate = 0f;        // 暴击概率
        public float defenceCriticalStrikeRate = 0f; // 暴击抵抗
        public float hitRate = 1f;                   // 命中率
        public float dodgeRate = 0f;                 // 闪避率       
        public float damageEnhanceRate = 0f;         // 伤害加深
        public float damageMitigationRate = 0f;      // 伤害减免率      
    }

    [Serializable]
    public class ObjectBattleInfoBase
    {
        public int tid = 0;                                             // ID
        public Vector3 birthPoint = Vector3.zero;                       // 出生点
        public int birthIndex = 0;                                      // 出生点索引
    }

    [Serializable]
    public class MonsterBattleInfo : ObjectBattleInfoBase
    {
        public AttributeInfo attributeInfo = new AttributeInfo();       // 属性信息
    }

    [Serializable]
    public class RoleBattleInfo : ObjectBattleInfoBase
    {
        public int level = 1;                                           // 等级
        public int breakLevel = 0;                                      // 突破等级
        public int fateLevel = 0;                                       // 天命等级
        public int[] skillLevels = new int[4] { 1, 1, 1, 1 };           // 技能等级
        public EquipInfo[] equips = new EquipInfo[6];                   // 装备     
    }

    private static int UniqueID() { return ++uniqueID; }
    private static int uniqueID = 0;

    private bool firstUpdateInGlobal = true;
    private bool firstUpdateInBattle = true;
    private Character character;
    private OpponentCharacter opponentCharater;
    public BaseObject[] monsters { get; private set; }
    public SimulatedStage stage { get; private set;}

    private SimilatedBattleMonitor battleMonitor;
    private GUIStyle style = new GUIStyle();
    private int infoType = 1; // 0: 隐藏 1: 统计 2: 属性
    private Vector2 scrollViewPos = Vector2.zero;
    private SimulatorLog simulatorLog;
    public int currentLoop { get; private set; }

    [SerializeField]
    private BATTLE_CONTROL _battleControl = BATTLE_CONTROL.AUTO_SKILL;   // 战斗模式

    [SerializeField]
    private float _battleSpeed = 1f;      // 战斗速度

    [SerializeField]
    private bool _enableGameLog = false;          // 是否开启游戏Log

    public bool loadConfigFromTable = false;    // 是否通过读表获取配置
    public bool saveLogFile = true;             // 是否保存战斗日志到本地
    public Vector3 selfBirthPoint = Vector3.zero;
    public int selfBirthIndex = 0; 
    public Vector3 oppoBirthPoint = Vector3.zero;
    public int oppoBirthIndex = 1;
    public bool isPVE = true;           // PVE or PVP
    public int loopCount = 5;           // 循环次数
    public int battleMaxTime = 0;       // 每局战斗最大时间，0表示无限制，超时按照平局算
    public RoleBattleInfo[] roleInfos = new RoleBattleInfo[10];     // 队伍信息 (0 主角 1-3 上阵宠物 4-9 缘分宠物)
    public int astrologyStar = 0;                                   // 点星
    public RoleBattleInfo[] opRoleInfos = new RoleBattleInfo[10];   // PVP敌方队伍
    public int opAstrologyStar = 0;                                 // PVP敌方点星
    public MonsterBattleInfo[] monsterInfos;                        // 怪物信息  

    public bool battleTimeOut { get; private set; }
    public float fps { get; private set; }
    public float battleAverageFps { get; private set; }

    private float lastFrameCountTime = 0f;
    private float lastBattleStartTime = 0f;
    private float lastTotalFrames = 0;
    private int battleTotalFrames = 0;


    public BATTLE_CONTROL battleControl
    {
        get 
        {
            return _battleControl; 
        }
        set 
        {
            _battleControl = value;
            PVEStageBattle.mBattleControl = _battleControl;
        }
    }

    public float battleSpeed
    {
        get 
        {
            return _battleSpeed;
        }
        set 
        {
            _battleSpeed = Mathf.Max(0f, value);

            if (Application.isPlaying)
                Time.timeScale = _battleSpeed;
            else
                Time.timeScale = 1f;
        }
    }

    public bool enableGameLog
    {
        get
        {
            return _enableGameLog;
        }
        set
        {
            _enableGameLog = value;

            if (Application.isPlaying)
            {
                CommonParam.NeedLog = value;
                CommonParam.LogOnScreen = value;
                CommonParam.LogOnConsole = value;
            }
        }
    }

    private void Reset()
    {
        for (int i = 0; i < 10; ++i)
        {
            roleInfos[i] = new RoleBattleInfo();

            for (int j = 0; j < 6; ++j)
            {
                roleInfos[i].equips[j] = new EquipInfo();
            }
        }

        roleInfos[0].tid = 50001;
        ActiveBirth[] births = FindObjectsOfType<ActiveBirth>();

        if (births.Length > 0)
        {
            roleInfos[0].birthIndex = int.MaxValue;

            for (int i = 0; i < births.Length; ++i)
            {
                if (births[i].mIndex < roleInfos[0].birthIndex)
                {
                    selfBirthIndex = births[i].mIndex;
                    selfBirthPoint = births[i].transform.position;
                    roleInfos[0].birthIndex = selfBirthIndex;
                    roleInfos[0].birthPoint = selfBirthPoint;
                }
            }
        }
        else 
        {
            roleInfos[0].birthIndex = 0;
        }

        for (int i = 0; i < 10; ++i)
        {
            opRoleInfos[i] = new RoleBattleInfo();

            for (int j = 0; j < 6; ++j)
            {
                opRoleInfos[i].equips[j] = new EquipInfo();
            }
        }

        opRoleInfos[0].tid = 50001;

        if (births.Length > 1)
        {
            opRoleInfos[0].birthIndex = int.MaxValue;

            for (int i = 0; i < births.Length; ++i)
            {
                if (births[i].mIndex < opRoleInfos[0].birthIndex && births[i].mIndex != roleInfos[0].birthIndex)
                {
                    oppoBirthIndex = births[i].mIndex;
                    oppoBirthPoint = births[i].transform.position;
                    opRoleInfos[0].birthIndex = oppoBirthIndex;
                    opRoleInfos[0].birthPoint = oppoBirthPoint;
                }
            }
        }
        else
        {
            opRoleInfos[0].birthIndex = 0;
        }
    }

    private void Awake()
    {      
        simulatorLog = new SimulatorLog();
        simulatorLog.simulator = this;
        simulatorLog.saveLogFile = saveLogFile;
        simulatorLog.InitLogDirectory();

        if (loadConfigFromTable)
        {
            isPVE = false;          
            LoadConfigFromTable();
        }

        InitEnvironment();  // 初始化环境
        InitBattle();
    }

    private void Start()
    {
        lastFrameCountTime = Time.realtimeSinceStartup;
    }

    private void InitEnvironment()
    {
        // 初始化UI环境
        var uiRoot = GameCommon.LoadAndIntanciatePrefabs("Prefabs/UI Root");
        uiRoot.name = "UI Root";
        style.normal.background = Texture2D.whiteTexture;

        // 初始化游戏配置
        GlobalModule.InitGameData();

        // 初始化网络模块
        Net.Init();

        // 初始化事件模块
        DataCenter.Self.InitResetGameData();

        // 初始化渲染模块
        HpBar.Init();

        // 初始化战斗配置
        PVEStageBattle.mBattleControl = _battleControl;
        Time.timeScale = Mathf.Max(0f, _battleSpeed);
        GlobalModule.IsSceneLoadComplete = true;
        CommonParam.mOpenDynamicUI = false;
        CommonParam.UsePointMark = false;
        CommonParam.NeedLog = _enableGameLog;
        CommonParam.LogOnScreen = _enableGameLog;
        CommonParam.LogOnConsole = _enableGameLog;
    }

    private void InitBattle()
    {
        // 生成战场
        stage = new SimulatedStage();
        stage.simulator = this;
        MainProcess.mStage = stage;  

        if (isPVE)
        {
            InitData(roleInfos, astrologyStar);
            InitTeam(roleInfos);
            Character.Self.CreatePetsImmediate();
            character = Character.Self;
            InitMonsters();
        }
        else
        {
            InitData(opRoleInfos, opAstrologyStar);
            InitTeam(opRoleInfos);    
            Character.Self.CreatePetsImmediate();
            CloneCurrentTeamToOpponentTeam();
            DestroyCurrentTeam();

            InitData(roleInfos, astrologyStar);
            InitTeam(roleInfos);
            Character.Self.CreatePetsImmediate();
            character = Character.Self;
        }
    }

    private void InitData(RoleBattleInfo[] roleInfos, int astrologyStar)
    {
        // 注册主角数据
        RoleLogicData roleLogic = new RoleLogicData();
        roleLogic.mRoleList = new RoleData[(int)CREATE_ROLE_TYPE.max];
        DataCenter.RegisterData("ROLE_DATA", roleLogic);

        // 注册宠物数据
        PetLogicData petLogic = new PetLogicData();
        DataCenter.RegisterData("PET_DATA", petLogic);

        // 注册装备数据
        RoleEquipLogicData equipLogic = new RoleEquipLogicData();
        DataCenter.RegisterData("EQUIP_DATA", equipLogic);
        equipLogic.mDicEquip.Clear();

        // 注册法器数据
        MagicLogicData magicLogic = new MagicLogicData();
        DataCenter.RegisterData("MAGIC_DATA", magicLogic);

        // 点星
        PointStarLogicData logic = new PointStarLogicData();
        logic.Init(astrologyStar + 1);

        // 设置VIP等级以解锁全部战斗功能
        roleLogic.vipLevel = 10;

        // 设置主角及装备
        roleLogic.character = GenarateRoleData(roleInfos[0]);

        foreach (var equip in roleInfos[0].equips)
        {
            if (equip != null && equip.tid > 0)
            {
                UploadEquip(equip, 0);
            }
        }

        // 设置宠物及装备
        for (int i = 1; i <= 3; ++i)
        {
            if (roleInfos[i].tid > 0)
            {
                PetData d = GenaratePetData(roleInfos[i], i);
                PetLogicData.Self.AddItemData(d);

                foreach (var equip in roleInfos[i].equips)
                {
                    if (equip != null && equip.tid > 0)
                    {
                        UploadEquip(equip, i);
                    }
                }
            }
        }

        for (int i = 4; i <= 9; ++i)
        {
            if (roleInfos[i].tid > 0)
            {
                PetData d = GenaratePetData(roleInfos[i], i);
                PetLogicData.Self.AddItemData(d);
            }
        }

        // 初始化队伍
        TeamManager.InitTeamListData();

        // 初始化缘分
        Relationship.Init();
    }

    private void InitTeam(RoleBattleInfo[] roleInfos)
    {
        // 生成主角及宠物
        if (roleInfos[0].tid > 0)
        {
            var role = CreateObject(roleInfos[0].tid, roleInfos[0].birthPoint) as Role;

            if (role != null)
            {
                role.SetCamp(CommonParam.SelfCamp);
                role.SetActiveData(TeamManager.GetActiveDataByTeamPos(0));
            }
        }       
    }

    private void InitMonsters()
    {
        // 生成怪物
        monsters = new BaseObject[monsterInfos.Length];

        for (int i = 0; i < monsterInfos.Length; ++i)
        {
            MonsterBattleInfo monsterInfo = monsterInfos[i];

            if (monsterInfo.tid > 0)
            {
                var monster = CreateObject(monsterInfo.tid, monsterInfo.birthPoint);
                monster.SetCamp(CommonParam.MonsterCamp);
                monster.mIsAttributeLocked = true;
                monster.mStaticMaxHp = monsterInfo.attributeInfo.maxHp;
                monster.mStaticAttack = monsterInfo.attributeInfo.attack;
                monster.mStaticPhysicalDefence = monsterInfo.attributeInfo.physicalDefence;
                monster.mStaticMagicDefence = monsterInfo.attributeInfo.magicDefence;
                monster.mStaticCriticalStrikeRate = monsterInfo.attributeInfo.criticalStrikeRate;
                monster.mStaticDefenceCriticalStrikeRate = monsterInfo.attributeInfo.defenceCriticalStrikeRate;
                monster.mStaticHitRate = monsterInfo.attributeInfo.hitRate;
                monster.mStaticDodgeRate = monsterInfo.attributeInfo.dodgeRate;
                monster.mStaticDamageEnhanceRate = monsterInfo.attributeInfo.damageEnhanceRate;
                monster.mStaticDamageMitigationRate = monsterInfo.attributeInfo.damageMitigationRate;
                monster.SetAttributeByLevel();
                monsters[i] = monster;
            }
        }
    }

    private void OnFirstUpdateInGlobal()
    {
        battleMonitor = new SimilatedBattleMonitor();
        battleMonitor.simulatorLog = simulatorLog;
        MainProcess.AddBattleMonitor(battleMonitor);
    }

    private void OnFirstUpdateInBattle()
    {
        battleTimeOut = false;
        MainProcess.Self.InitForSimulator();

        // 设置战场摄像机
        MainProcess.mCameraMoveTool.InitConfig((int)CAMERA_TYPE.DEFAULT);
        MainProcess.mCameraMoveTool.Reset();
        MainProcess.mCameraMoveTool.StartUpdate();
        Effect_ShakeCamera.InitCamera(MainProcess.mMainCamera.gameObject);
        MainProcess.mCameraMoveTool.SetCameraShakeEnabled(true);
        MainProcess.Self.mController = new BattleController();

        // 启动战场对象
        foreach (var obj in ObjectManager.Self.mObjectMap)
        {
            if (obj != null && !obj.IsDead() && obj.mbVisible)
            {
                if (!obj.mbHasStarted)
                {
                    obj.Start();
                }
            }
        }

        // 激活战场      
        MainProcess.mStage.ActivateBattle();

        ++currentLoop;

        if (currentLoop == 1)
        {
            OnCampaignStart();
        }

        OnBattleStart();
    }

    private void RevertTeamCampToOpponent(Character ch)
    {
        if (ch != null)
        {
            if (ch.mFriends != null)
            {
                foreach (var f in ch.mFriends)
                {
                    if (f != null)
                    {
                        f.SetCamp(CommonParam.MonsterCamp);
                        (f as Role).mBattlePlayerWindow = "";
                    }
                }
            }

            ch.SetCamp(CommonParam.MonsterCamp);
            ch.mBattlePlayerWindow = "";
        }
    }

    private void CloneCurrentTeamToOpponentTeam()
    {
        if (Character.Self != null)
        {
            CloneOpponentCharacter(Character.Self);

            if (Character.Self.mFriends != null)
            {
                foreach (var f in Character.Self.mFriends)
                {
                    if (f != null)
                    {
                        CloneOpponentPet(f);
                    }
                }
            }
        }
    }

    private void DestroyCurrentTeam()
    {
        if (Character.Self != null)
        {
            if (Character.Self.mFriends != null)
            {
                foreach (var f in Character.Self.mFriends)
                {
                    if (f != null)
                    {
                        f.Destroy();
                        f.OnDestroy();
                    }
                }
            }

            Character.Self.Destroy();
            Character.Self.OnDestroy();
        }
    }

    private void CloneOpponentCharacter(Character c)
    {
        opponentCharater = CreateObject(OBJECT_TYPE.OPPONENT_CHARACTER, c.mConfigIndex, c.GetPosition()) as OpponentCharacter;

        if (opponentCharater != null)
        {
            opponentCharater.SetCamp(CommonParam.MonsterCamp);
            opponentCharater.SetActiveData(c.mActiveData);
            opponentCharater.InitBaseAttribute();
            opponentCharater.InitStaticAttribute();
            opponentCharater.CloneAttributeFrom(c);
            opponentCharater.mIsAttributeLocked = true;
            opponentCharater.SetAttributeByLevel();
        }
    }

    private void CloneOpponentPet(BaseObject pet)
    {
        var oppnonentPet = CreateObject(OBJECT_TYPE.OPPONENT_PET, pet.mConfigIndex, pet.GetPosition()) as OpponentPet;
        Pet p = pet as Pet;

        if (oppnonentPet != null)
        {
            oppnonentPet.SetCamp(CommonParam.MonsterCamp);
            oppnonentPet.mUsePos = p.mUsePos;
            oppnonentPet.SetActiveData(p.mActiveData);
            oppnonentPet.SetOwnerObject(opponentCharater);
            opponentCharater.InitFriendFollowPosition(oppnonentPet, oppnonentPet.mUsePos);

            int petSkillIndex = oppnonentPet.GetConfig("PET_SKILL_1");

            if (petSkillIndex > 0)
            {
                opponentCharater.ActivateCDLooper(petSkillIndex, (SKILL_POS)(p.mUsePos), oppnonentPet.GetConfigSkillLevel(0), 0f);
            }

            opponentCharater.mFriends[p.mUsePos - 1] = oppnonentPet;
            oppnonentPet.InitBaseAttribute();
            oppnonentPet.InitStaticAttribute();
            oppnonentPet.CloneAttributeFrom(pet);
            oppnonentPet.mIsAttributeLocked = true;
            oppnonentPet.SetAttributeByLevel();
            oppnonentPet.Start();

            DataCenter.SetData(opponentCharater.mBattlePlayerWindow, "PET_ALIVE", p.mUsePos);
        }
    }

    private void Update()
    {      
        ++lastTotalFrames;
        ++battleTotalFrames;
        float delta = Time.realtimeSinceStartup - lastFrameCountTime;

        if (delta >= 1f)
        {
            fps = lastTotalFrames / delta;
            lastFrameCountTime = Time.realtimeSinceStartup;
            lastTotalFrames = 0;
        }

        if (firstUpdateInGlobal)
        {
            OnFirstUpdateInGlobal();
            firstUpdateInGlobal = false;
        }

        if (firstUpdateInBattle)
        {
            OnFirstUpdateInBattle();
            firstUpdateInBattle = false;
        }

        // 刷新怪物属性
        RefreshMonsterAttribute();

        Net.gNetEventCenter.Process(Time.deltaTime);
        Logic.EventCenter.Self.Process(Time.deltaTime);
        ObjectManager.Self.Update(Time.deltaTime);

        if (battleMaxTime > 0 && stage != null && stage.mbBattleActive && !stage.mbBattleFinish && stage.mBattleTime >= battleMaxTime)
        {
            battleTimeOut = true;
            stage.OnFinish(false);
        }
    }

    private RoleData GenarateRoleData(RoleBattleInfo info)
    {
        var data = new RoleData();
        data.tid = info.tid;
        data.level = info.level;
        data.breakLevel = info.breakLevel;
        data.fateLevel = info.fateLevel;
        data.skillLevel = info.skillLevels;
        data.teamPos = 0;
        data.itemId = UniqueID();
        return data;
    }

    private PetData GenaratePetData(RoleBattleInfo info, int teamIndex)
    {
        var data = new PetData();
        data.tid = info.tid;
        data.level = info.level;
        data.breakLevel = info.breakLevel;
        data.fateLevel = info.fateLevel;
        data.skillLevel = info.skillLevels;
        data.itemNum = 1;       
        data.itemId = UniqueID();

        if (teamIndex <= 3)
        {
            data.teamPos = teamIndex;
        }
        else
        {
            data.teamPos = teamIndex + 7;
        }

        return data;
    }

    private void UploadEquip(EquipInfo info, int teamPos)
    {
        EquipData data = new EquipData();
        data.tid = info.tid;
        data.itemId = UniqueID();
        data.strengthenLevel = info.strengthenLevel;
        data.refineLevel = info.refineLevel;
        PackageManager.AddItem(data);

        if (PackageManager.GetItemTypeByTableID(data.tid) == ITEM_TYPE.EQUIP)
        {
            RoleEquipLogicData.Self.ChangeTeamPos(data.itemId, data.tid, teamPos);
            var item = RoleEquipLogicData.Self.GetEquipDataByItemId(data.itemId);
            item.strengthenLevel = data.strengthenLevel;
            item.refineLevel = data.refineLevel;
        }
        else
        {
            MagicLogicData.Self.ChangeTeamPos(data.itemId, data.tid, teamPos);
            var item = MagicLogicData.Self.GetEquipDataByItemId(data.itemId);
            item.strengthenLevel = data.strengthenLevel;
            item.refineLevel = data.refineLevel;
        }
    }

    private Vector3 GetBirthPointCoord(int birthIndex)
    {
        ActiveBirth[] births = GameObject.FindObjectsOfType<ActiveBirth>();

        foreach (var birth in births)
        {
            if (birth.mIndex == birthIndex)
            {
                return birth.transform.position;
            }
        }

        return Vector3.zero;
    }

    private BaseObject CreateObject(int objectIndex, Vector3 pos)
    {
        BaseObject obj = ObjectManager.Self.CreateObject(objectIndex);

        if (obj != null)
        {
            obj.InitPosition(pos);
            obj.SetDirection(Vector3.forward);
            obj.SetVisible(true);
            (obj as ActiveObject).mWarnEffect.SetVisible(false);
        }

        return obj;
    }

    private BaseObject CreateObject(OBJECT_TYPE type, int objectIndex, Vector3 pos)
    {
        BaseObject obj = ObjectManager.Self.CreateObject(type, objectIndex);

        if (obj != null)
        {
            obj.InitPosition(pos);
            obj.SetDirection(Vector3.forward);
            obj.SetVisible(true);
            (obj as ActiveObject).mWarnEffect.SetVisible(false);
        }

        return obj;
    }

    private void RefreshMonsterAttribute()
    {
        if (monsters == null)
            return;

        for (int i = 0; i < monsters.Length; ++i)
        {
            BaseObject monster = monsters[i];
            MonsterBattleInfo monsterInfo = monsterInfos[i];

            if (monster != null/* && monster.mbVisible && !monster.IsDead()*/)
            {           
                monster.mStaticAttack = monsterInfo.attributeInfo.attack;
                monster.mStaticPhysicalDefence = monsterInfo.attributeInfo.physicalDefence;
                monster.mStaticMagicDefence = monsterInfo.attributeInfo.magicDefence;
                monster.mStaticCriticalStrikeRate = monsterInfo.attributeInfo.criticalStrikeRate;
                monster.mStaticDefenceCriticalStrikeRate = monsterInfo.attributeInfo.defenceCriticalStrikeRate;
                monster.mStaticHitRate = monsterInfo.attributeInfo.hitRate;
                monster.mStaticDodgeRate = monsterInfo.attributeInfo.dodgeRate;
                monster.mStaticDamageEnhanceRate = monsterInfo.attributeInfo.damageEnhanceRate;
                monster.mStaticDamageMitigationRate = monsterInfo.attributeInfo.damageMitigationRate;

                if (monster.mStaticMaxHp != monsterInfo.attributeInfo.maxHp)
                {
                    int hp = monster.GetHp();
                    float hpRate = (float)hp / monster.GetMaxHp();
                    monster.mStaticMaxHp = monsterInfo.attributeInfo.maxHp;
                    int deltaHp = (int)(monster.GetMaxHp() * hpRate) - hp;
                    monster.ChangeHp(deltaHp);
                }             
            }
        }
    }

    private void OnGUI()
    {
        if (battleMonitor != null)
        {
            GUI.backgroundColor = new Color(0f, 0f, 0f, 0.75f);

            GUILayout.BeginVertical();
            GUI.contentColor = Color.white;

            string btnText = "";

            switch (infoType)
            {
                case 0: btnText = "显示统计信息" + " " + currentCompaign + "-" + currentLoop; break;
                case 1: btnText = "查看属性信息"; break;
                case 2: btnText = "隐藏信息"; break;
            }

            if (GUILayout.Button(btnText))
            {
                switch (infoType)
                {
                    case 0: infoType = 1; break;
                    case 1: infoType = 2; break;
                    case 2: infoType = 0; break;
                }
            }

            if (infoType == 1)
            {
                scrollViewPos = GUILayout.BeginScrollView(scrollViewPos, style, GUILayout.MinWidth(350));
                string titleLabel = "  " + currentCompaign + "-" + currentLoop;
                titleLabel += " " + (stage == null ? "" : TotalSeconds2SpanText(stage.mBattleTime));
                titleLabel += " FPS=" + fps.ToString("f1");
                GUILayout.Label(titleLabel);

                if (character != null)
                {
                    DrawObjectDamageStatistics(character, true);

                    for (int i = 0; i < character.mFriends.Length; ++i)
                    {
                        if (character.mFriends[i] != null)
                        {
                            DrawObjectDamageStatistics(character.mFriends[i], true);
                        }
                    }
                }

                if (isPVE)
                {
                    if (monsters != null)
                    {
                        for (int i = 0; i < monsters.Length; ++i)
                        {
                            if (monsters[i] != null)
                            {
                                DrawObjectDamageStatistics(monsters[i], false);
                            }
                        }
                    }
                }
                else 
                {
                    if (opponentCharater != null)
                    {
                        DrawObjectDamageStatistics(opponentCharater, false);

                        for (int i = 0; i < opponentCharater.mFriends.Length; ++i)
                        {
                            if (opponentCharater.mFriends[i] != null)
                            {
                                DrawObjectDamageStatistics(opponentCharater.mFriends[i], false);
                            }
                        }
                    }
                }

                GUILayout.EndScrollView();
            }
            else if (infoType == 2)
            {
                scrollViewPos = GUILayout.BeginScrollView(scrollViewPos, style, GUILayout.MinWidth(350));
                string titleLabel = "  " + currentCompaign + "-" + currentLoop;
                titleLabel += " " + (stage == null ? "" : TotalSeconds2SpanText(stage.mBattleTime));
                titleLabel += " FPS=" + fps.ToString("f1");
                GUILayout.Label(titleLabel);

                if (character != null)
                {
                    DrawObjectAttributeInfo(character, true);

                    for (int i = 0; i < character.mFriends.Length; ++i)
                    {
                        if (character.mFriends[i] != null)
                        {
                            DrawObjectAttributeInfo(character.mFriends[i], true);
                        }
                    }
                }

                if (isPVE)
                {
                    if (monsters != null)
                    {
                        for (int i = 0; i < monsters.Length; ++i)
                        {
                            if (monsters[i] != null)
                            {
                                DrawObjectAttributeInfo(monsters[i], false);
                            }
                        }
                    }
                }
                else 
                {
                    if (opponentCharater != null)
                    {
                        DrawObjectAttributeInfo(opponentCharater, false);

                        for (int i = 0; i < opponentCharater.mFriends.Length; ++i)
                        {
                            if (opponentCharater.mFriends[i] != null)
                            {
                                DrawObjectAttributeInfo(opponentCharater.mFriends[i], false);
                            }
                        }
                    }
                }

                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
        }
    }

    private void DrawObjectDamageStatistics(BaseObject obj, bool isSelfTeam)
    {
        GUILayout.Label("-----------------------------------------------------------------------------");

        GUILayout.BeginHorizontal();
        GUI.contentColor = isSelfTeam ? Color.green : Color.red;
        GUILayout.Label("*** " + obj.GetConfig("NAME") + "(" + obj.mConfigIndex + ") ***");

        if (obj.IsDead())
        {
            GUI.contentColor = Color.red;
            GUILayout.Label("Dead");
        }

        GUI.contentColor = Color.white;
        GUILayout.EndHorizontal();

        ObjectDamageInfo damageInfo = simulatorLog.skillDamageDistribution.GetObjectDamageDistribution(obj.mID);

        if (damageInfo != null)
        {
            if (damageInfo.totalNormalDamageInfo.skillCount > 0)
            {
                GUILayout.Label(GetDamageItemDesc(damageInfo.totalNormalDamageInfo, true, obj.aiMachine.totalRunTime));
            }

            for (int i = 0; i < damageInfo.totalSkillDamageInfos.Count; ++i)
            {
                GUILayout.Label(GetDamageItemDesc(damageInfo.totalSkillDamageInfos[i], false, obj.aiMachine.totalRunTime));
            }

            float time = stage != null && !stage.mbBattleFinish ? (int)obj.aiMachine.totalRunTime : obj.aiMachine.totalRunTime;
            float totalDPS = obj.aiMachine.totalRunTime > 0.1f ? damageInfo.totalDamage / time : 0f;
            GUI.contentColor = Color.yellow;
            GUILayout.Label("Total DPS = " + totalDPS.ToString("f2"));
        }
        else 
        {
            GUI.contentColor = Color.yellow;
            GUILayout.Label("Total DPS = 0.00");
        }

        GUI.contentColor = Color.white;
    }

    private string GetDamageItemDesc(TotalDamageInfo totalInfo, bool isPhysical, float time)
    {
        string text = isPhysical ? "物理攻击    " : "技能" + totalInfo.skillIndex;
        float dph = (float)totalInfo.totalDamage / totalInfo.skillCount;
        time = stage != null && !stage.mbBattleFinish ? (int)time : time;
        float dps = time > 0.1f ? totalInfo.totalDamage / time : 0f;
        text += "  DPH = " + dph.ToString("f2");
        text += "  DPS = " + dps.ToString("f2");
        return text;
    }

    private void DrawObjectAttributeInfo(BaseObject obj, bool isSelfTeam)
    {
        GUILayout.Label("-----------------------------------------------------------------------------");

        GUILayout.BeginHorizontal();
        GUI.contentColor = isSelfTeam ? Color.green : Color.red;
        GUILayout.Label("*** " + obj.GetConfig("NAME") + "(" + obj.mConfigIndex + ") ***");

        GUI.contentColor = Color.yellow;
        GUILayout.Label("战斗力 " + GameCommon.GetPowerByObject(obj).ToString("f0"));

        GUILayout.EndHorizontal();

        GUI.contentColor = Color.white;
        DrawNumberAttribute("血量", obj.mStaticMaxHp, obj.GetMaxHp());
        DrawNumberAttribute("攻击力 ", obj.mStaticAttack, obj.GetAttack());
        DrawNumberAttribute("物理防御", obj.mStaticPhysicalDefence, obj.GetPhysicalDefence());
        DrawNumberAttribute("魔法防御", obj.mStaticMagicDefence, obj.GetMagicDefence());
        DrawRateAttribute("命中", obj.mStaticHitRate, obj.GetHitRate());
        DrawRateAttribute("闪避", obj.mStaticDodgeRate, obj.GetDodgeRate());
        DrawRateAttribute("暴击概率", obj.mStaticCriticalStrikeRate, obj.GetCriticalStrikeRate());
        DrawRateAttribute("暴击抵抗", obj.mStaticDefenceCriticalStrikeRate, obj.GetDefenceCriticalStrikeRate());
        DrawRateAttribute("伤害加深", obj.mStaticDamageEnhanceRate, obj.GetDamageEnhanceRate());
        DrawRateAttribute("伤害减免", obj.mStaticDamageMitigationRate, obj.GetDamageMitigationRate());
    }

    private void DrawNumberAttribute(string name, int staticValue, int realTimeValue)
    {
        int delta = realTimeValue - staticValue;
        GUILayout.BeginHorizontal();
        GUILayout.Label(name, GUILayout.Width(80));
        GUILayout.Label(staticValue.ToString(), GUILayout.Width(80));

        if (delta > 0) 
            GUI.contentColor = Color.green;
        else if (delta < 0) 
            GUI.contentColor = Color.red;

        string text = realTimeValue.ToString();

        if (delta > 0)
            text += "   (+" + delta + ")";
        else if (delta < 0)
            text += "   (-" + (-delta) + ")";

        GUILayout.Label(text, GUILayout.Width(200));
        GUI.contentColor = Color.white;
        GUILayout.EndHorizontal();
    }

    private void DrawRateAttribute(string name, float staticValue, float realTimeValue)
    {
        float delta = (realTimeValue - staticValue) * 100f;
        GUILayout.BeginHorizontal();
        GUILayout.Label(name, GUILayout.Width(80));
        GUILayout.Label((staticValue * 100f).ToString("f1") + "%", GUILayout.Width(80));

        if (delta > 0.001f)
            GUI.contentColor = Color.green;
        else if (delta < -0.001f)
            GUI.contentColor = Color.red;

        string text = (realTimeValue * 100f).ToString("f1") + "%";

        if (delta > 0.001f)
            text += "   (+" + delta.ToString("f1") + "%)";
        else if (delta < -0.001f)
            text += "   (-" + (-delta).ToString("f1") + "%)";

        GUILayout.Label(text, GUILayout.Width(200));
        GUI.contentColor = Color.white;
        GUILayout.EndHorizontal();
    }

    private string TotalSeconds2SpanText(float seconds)
    {
        var span = TimeSpan.FromSeconds(seconds);
        return span.Hours + ":" + span.Minutes + ":" + span.Seconds;
    }

    public HashSet<int> GetRelateContextSet(int teamPos, bool isSelfTeam)
    {
        HashSet<int> set = new HashSet<int>();
        var infos = isSelfTeam ? roleInfos : opRoleInfos;

        for (int i = 0; i < infos.Length; ++i)
        {
            if (infos[i].tid > 0)
            {
                set.Add(infos[i].tid);
            }
        }

        EquipInfo[] equipInfos = infos[teamPos].equips;

        for (int i = 0; i < equipInfos.Length; ++i)
        {
            if (equipInfos[i].tid > 0)
            {
                set.Add(equipInfos[i].tid);
            }
        }

        return set;
    }

    private void OnCampaignStart()
    {
        simulatorLog.OnCampaignStart();
    }

    private void OnCampaignFinish()
    {
        simulatorLog.OnCampaignFinish();
    }

    private void OnBattleStart()
    {
        battleTotalFrames = 0;
        lastBattleStartTime = Time.realtimeSinceStartup;
        simulatorLog.OnBattleStart();
    }

    private void OnBattleFinish()
    {
        battleAverageFps = battleTotalFrames / (Time.realtimeSinceStartup - lastBattleStartTime);
        simulatorLog.OnBattleFinish();
    }

    public void BattleFinish()
    {
        gameObject.StartRoutine(DoBattleFinish());
    }

    private IEnumerator DoBattleFinish()
    {
        yield return null;
        OnBattleFinish();

        if (currentLoop < loopCount)
        {
            yield return DoRebattle();
        }
        else
        {
            OnCampaignFinish();

            if (loadConfigFromTable && LoadNextBattle())
            {
                yield return DoRebattle();
            }
        }
    }

    private IEnumerator DoRebattle()
    {
        yield return new Delay(2f);

        foreach (var e in EventCenter.Self.mTimeEventManager.mWaitEventList)
        {
            if (e.mWaitEvent is BaseEffect)
            {
                ((BaseEffect)(e.mWaitEvent)).FinishImmediate();
            }
        }

        EventCenter.Self.RemoveAllEvent();
        AutoBattleAI.InitReset();
        ObjectManager.Self.ClearAll();
        MainProcess.ClearPaoPao();
        MainProcess.mMouseCoord.Finish();
        MainProcess.mStage = null;
        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        yield return new Delay(0.5f);

        InitBattle();
        firstUpdateInBattle = true;
    }

    private TableReader developReader;
    private TableReader teamBattleReader;
    private TableReader teamReader;
    private List<int> teamBattles;

    public int currentCompaign { get; private set; }
    public int currentSelfTeamID { get; private set; }
    public int currentOppoTeamID { get; private set; }

    private void LoadConfigFromTable()
    {
        developReader = new TableReader("Config/DevelopConfig.csv");
        teamBattleReader = new TableReader("Config/TeamBattle.csv");
        teamReader = new TableReader("Config/Team.csv");

        if (teamBattleReader.table != null)
        {
            teamBattles = new List<int>(teamBattleReader.table.GetAllRecord().Keys);

            if (teamBattles.Count > 0)
            {
                currentCompaign = 1;
                LoadBattle(teamBattles[currentCompaign - 1]);
            }
        }
    }

    private bool LoadNextBattle()
    {
        ++currentCompaign;

        if (currentCompaign <= teamBattles.Count)
        {
            return LoadBattle(teamBattles[currentCompaign - 1]);
        }

        return false;
    }

    private bool LoadBattle(int battleIndex)
    {
        DataRecord r = teamBattleReader.table.GetRecord(battleIndex);

        if (r == null)
            return false;

        currentSelfTeamID = r["TEAM_IDA"];
        currentOppoTeamID = r["TEAM_IDB"];
        loopCount = r["BATTLE_NUM"];
        battleMaxTime = r["BATTLE_TIME"];
        currentLoop = 0;
        roleInfos = CreateTeamData(currentSelfTeamID, out astrologyStar);
        opRoleInfos = CreateTeamData(currentOppoTeamID, out opAstrologyStar);

        if (roleInfos != null)
        {
            roleInfos[0].birthIndex = selfBirthIndex;
            roleInfos[0].birthPoint = selfBirthPoint;
        }

        if (opRoleInfos != null)
        {
            opRoleInfos[0].birthIndex = oppoBirthIndex;
            opRoleInfos[0].birthPoint = oppoBirthPoint;
        }

        return roleInfos != null && opRoleInfos != null;
    }

    private RoleBattleInfo[] CreateTeamData(int teamIndex, out int astrologyStar)
    {
        astrologyStar = 0;

        if (teamReader.table == null)
            return null;

        DataRecord r = teamReader.table.GetRecord(teamIndex);

        if (r == null)
            return null;

        astrologyStar = r["STAR_LEVEL"];
        RoleBattleInfo[] result = new RoleBattleInfo[10];
        result[0] = CreateRoleData(r["ROLE_ID"]);
        result[1] = CreateRoleData(r["PET_ID1"]);
        result[2] = CreateRoleData(r["PET_ID2"]);
        result[3] = CreateRoleData(r["PET_ID3"]);
        result[4] = CreateRoleData(r["RELATE_ID1"]);
        result[5] = CreateRoleData(r["RELATE_ID2"]);
        result[6] = CreateRoleData(r["RELATE_ID3"]);
        result[7] = CreateRoleData(r["RELATE_ID4"]);
        result[8] = CreateRoleData(r["RELATE_ID5"]);
        result[9] = CreateRoleData(r["RELATE_ID6"]);      
        return result;
    }

    private RoleBattleInfo CreateRoleData(int roleIndex)
    {
        var info = new RoleBattleInfo();

        for (int j = 0; j < 6; ++j)
        {
            info.equips[j] = new EquipInfo();
        }

        if(roleIndex == 0 || developReader.table == null)
            return info;

        DataRecord r = developReader.table.GetRecord(roleIndex);

        if (r == null)
            return info;

        info.tid = r["ACTIVE_ID"];
        info.level = r["LEVEL"];
        info.breakLevel = r["BREAK_LEVEL"];
        info.fateLevel = r["FATE_LEVEL"];
        info.skillLevels[0] = r["SKILL_LEVEL1"];
        info.skillLevels[1] = r["SKILL_LEVEL2"];
        info.skillLevels[2] = r["SKILL_LEVEL3"];
        info.skillLevels[3] = r["SKILL_LEVEL4"];

        info.equips[0].tid = r["ARMS"];
        info.equips[0].strengthenLevel = r["ARMS_STRENGTHEN_LEVLE"];
        info.equips[0].refineLevel = r["ARMS_REFINE_LEVLE"];

        info.equips[1].tid = r["JEWELRY"];
        info.equips[1].strengthenLevel = r["JEWELRY_STRENGTHEN_LEVLE"];
        info.equips[1].refineLevel = r["JEWELRY_REFINE_LEVLE"];

        info.equips[2].tid = r["CLOTHES"];
        info.equips[2].strengthenLevel = r["CLOTHES_STRENGTHEN_LEVLE"];
        info.equips[2].refineLevel = r["CLOTHES_REFINE_LEVLE"];

        info.equips[3].tid = r["SHOES"];
        info.equips[3].strengthenLevel = r["SHOES_STRENGTHEN_LEVLE"];
        info.equips[3].refineLevel = r["SHOES_REFINE_LEVLE"];

        info.equips[4].tid = r["ATT_MAGIC"];
        info.equips[4].strengthenLevel = r["ATT_MAGIC_STRENGTHEN_LEVLE"];
        info.equips[4].refineLevel = r["ATT_MAGIC_REFINE_LEVLE"];

        info.equips[5].tid = r["DEF_MAGIC"];
        info.equips[5].strengthenLevel = r["DEF_MAGIC_STRENGTHEN_LEVLE"];
        info.equips[5].refineLevel = r["DEF_MAGIC_REFINE_LEVLE"];

        return info;
    }
}