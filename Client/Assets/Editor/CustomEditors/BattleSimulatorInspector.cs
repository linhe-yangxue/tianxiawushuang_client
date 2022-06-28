using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using DataTable;


[CustomEditor(typeof(BattleSimulator))]
public class BattleSimulatorInspector : Editor
{
    private static readonly float MIN_HIT_RATE = 0f;         // 最小命中率
    private static readonly float MAX_HIT_RATE = 1f;         // 最大命中率
    private static readonly float MIN_DODGE_RATE = 0f;       // 最小闪避率
    private static readonly float MAX_DODGE_RATE = 1f;       // 最大闪避率
    private static readonly float MIN_CRITICAL_RATE = 0f;    // 最小暴击率
    private static readonly float MAX_CRITICAL_RATE = 1f;    // 最大暴击率
    private static readonly float MIN_CRITICAL_DEF = 0f;     // 最小暴击抵抗
    private static readonly float MAX_CRITICAL_DEF = 1f;     // 最大暴击抵抗
    private static readonly float MIN_ENHANCE = 0f;          // 最小伤害加深
    private static readonly float MAX_ENHANCE = 5f;          // 最大伤害加深
    private static readonly float MIN_MITIGATIONG = 0f;      // 最小伤害减免
    private static readonly float MAX_MITIGATIONG = 1f;      // 最大伤害减免

    private static readonly int DEFAULT_MONSTER = 70001;     // 新建怪物默认ID

    [MenuItem("Simulator/New Simulator", priority = 0)]
    public static void CreateNewSimulator()
    {
        var already = GameObject.FindObjectOfType<BattleSimulator>();

        if (already != null)
        {
            Debug.Log("Simulator already exists in current scene!");
            Selection.activeGameObject = already.gameObject;
            return;
        }

        GameObject obj = new GameObject("battle simulator");
        var p = obj.AddComponent<BattleSimulator>();
        Selection.activeGameObject = obj;
    }

    private class EquipList
    {
        public int[] tidArray;
        public string[] descArray;
    }

    public enum BATTLE_MODE
    {
        PVE,
        PVP
    }

    private BattleSimulator simulator;
    private SerializedProperty teamProp;
    private SerializedProperty opTeamProp;
    private SerializedProperty monsterProp;
    private Dictionary<int, Vector3> birthPoints;
    private TableReader activeTableReader;
    private TableReader monsterTableReader;
    private TableReader equipTableReader;
    private TableReader relateTableReader;
    private int[] birthIntArray;
    private string[] birthStrArray;
    private EquipList[] equipList = new EquipList[6];
    private HashSet<int> usedBirth = new HashSet<int>();
    private bool isSelfRelateInfoExpanded = false;
    private bool isOpponentRelateInfoExpanded = false;

    private void OnEnable()
    {
        simulator = target as BattleSimulator;
        teamProp = serializedObject.FindProperty("roleInfos");
        opTeamProp = serializedObject.FindProperty("opRoleInfos");
        monsterProp = serializedObject.FindProperty("monsterInfos");

        activeTableReader = new TableReader("Config/ActiveObject.csv");
        monsterTableReader = new TableReader("Config/MonsterObject.csv");
        equipTableReader = new TableReader("Config/RoleEquipConfig.csv");
        relateTableReader = new TableReader("Config/RelateConfig.csv");

        birthPoints = new Dictionary<int, Vector3>();
        ActiveBirth[] births = FindObjectsOfType<ActiveBirth>();
        List<int> birthIntList = new List<int>();

        for (int i = 0; i < births.Length; ++i)
        {
            if (!birthPoints.ContainsKey(births[i].mIndex))
            {
                birthPoints.Add(births[i].mIndex, births[i].transform.position);
                birthIntList.Add(births[i].mIndex);
            }
        }

        birthIntList.Sort();
        birthIntArray = birthIntList.ToArray();
        birthStrArray = new string[birthIntArray.Length];

        for (int i = 0; i < birthStrArray.Length; ++i)
        {
            birthStrArray[i] = birthIntArray[i].ToString();
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.BeginVertical();

        DrawSpace(2);
        simulator.loadConfigFromTable = EditorGUILayout.ToggleLeft(new GUIContent("  通过配置表加载配置项"), simulator.loadConfigFromTable);
        DrawSpace(1);
        simulator.saveLogFile = EditorGUILayout.ToggleLeft(new GUIContent("  保存战斗日志"), simulator.saveLogFile);
        DrawSpace(1);
        simulator.enableGameLog = EditorGUILayout.ToggleLeft(new GUIContent("  开启游戏LOG(长时间运行请不要开启)"), simulator.enableGameLog);
        DrawSpace(2);

        if (simulator.loadConfigFromTable)
        {
            SerializedProperty selfBirthIndexProp = serializedObject.FindProperty("selfBirthIndex");
            SerializedProperty selfBirthPointProp = serializedObject.FindProperty("selfBirthPoint");
            int selfBirthIndex = selfBirthIndexProp.intValue;
            Vector3 selfBirthPoint = selfBirthPointProp.vector3Value;
            DrawBirthInfo("己方出生点", ref selfBirthIndex, ref selfBirthPoint, Color.green);
            selfBirthIndexProp.intValue = selfBirthIndex;
            selfBirthPointProp.vector3Value = selfBirthPoint;

            SerializedProperty oppoBirthIndexProp = serializedObject.FindProperty("oppoBirthIndex");
            SerializedProperty oppoBirthPointProp = serializedObject.FindProperty("oppoBirthPoint");
            int oppoBirthIndex = oppoBirthIndexProp.intValue;
            Vector3 oppoBirthPoint = oppoBirthPointProp.vector3Value;
            DrawBirthInfo("敌方出生点", ref oppoBirthIndex, ref oppoBirthPoint, Color.red);
            oppoBirthIndexProp.intValue = oppoBirthIndex;
            oppoBirthPointProp.vector3Value = oppoBirthPoint;

            if (!Application.isPlaying)
            {
                DrawSpace(2);
                int selectedControl = EditorGUILayout.IntPopup("控制模式", (int)simulator.battleControl, new string[3] { "手动战斗", "自动战斗", "自动技能" }, new int[3] { 0, 1, 2 });
                simulator.battleControl = (BATTLE_CONTROL)selectedControl;
                simulator.battleSpeed = EditorGUILayout.Slider(new GUIContent("战斗速度"), simulator.battleSpeed, 0f, 10f);
            }

            DrawSpace(2);
        }

        if (!simulator.loadConfigFromTable || Application.isPlaying)
        {
            Enum e = EditorGUILayout.EnumPopup(new GUIContent("战斗模式"), simulator.isPVE ? BATTLE_MODE.PVE : BATTLE_MODE.PVP);
            simulator.isPVE = Enum.Equals(e, BATTLE_MODE.PVE);

            var loopProp = serializedObject.FindProperty("loopCount");
            EditorGUILayout.PropertyField(loopProp, new GUIContent("循环次数"));

            int selectedControl = EditorGUILayout.IntPopup("控制模式", (int)simulator.battleControl, new string[3] { "手动战斗", "自动战斗", "自动技能" }, new int[3] { 0, 1, 2 });
            simulator.battleControl = (BATTLE_CONTROL)selectedControl;
            simulator.battleMaxTime = EditorGUILayout.IntField(new GUIContent("时间限制(秒,0无限制)"), simulator.battleMaxTime);
            simulator.battleSpeed = EditorGUILayout.Slider(new GUIContent("战斗速度"), simulator.battleSpeed, 0f, 10f);
            DrawSpace(3);

            if (!simulator.isPVE)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField("我方队伍", GUILayout.MaxWidth(60));
                GUI.color = Color.white;
                DrawSpace(1);
            }

            DrawTeamInfo(teamProp, true);

            if (!simulator.isPVE)
            {
                DrawSpace(5);
                GUI.color = Color.red;
                EditorGUILayout.LabelField("敌方队伍", GUILayout.MaxWidth(60));
                GUI.color = Color.white;
                DrawSpace(1);
                DrawTeamInfo(opTeamProp, false);
            }

            if (simulator.isPVE)
            {
                for (int i = 0; i < monsterProp.arraySize; ++i)
                {
                    DrawSpace(5);
                    DrawMonsterInfo(monsterProp.GetArrayElementAtIndex(i), i);
                }

                GUI.backgroundColor = new Color(1f, 1f, 0.3f);
                DrawSpace(2);

                if (GUILayout.Button("添 加 怪 物", EditorStyles.miniButton))
                {
                    int nextIndex = monsterProp.arraySize;
                    monsterProp.InsertArrayElementAtIndex(nextIndex);
                    SerializedProperty nextMonsterProp = monsterProp.GetArrayElementAtIndex(nextIndex);

                    if (birthIntArray.Length > 0)
                    {
                        int birthIndex = 0;

                        for (int i = 0; i < birthIntArray.Length; ++i)
                        {
                            if (!usedBirth.Contains(birthIntArray[i]))
                            {
                                birthIndex = i;
                                break;
                            }
                        }

                        usedBirth.Add(birthIntArray[birthIndex]);
                        nextMonsterProp.FindPropertyRelative("tid").intValue = DEFAULT_MONSTER;
                        nextMonsterProp.FindPropertyRelative("birthIndex").intValue = birthIntArray[birthIndex];
                        nextMonsterProp.FindPropertyRelative("birthPoint").vector3Value = birthPoints[birthIntArray[birthIndex]];
                        SerializedProperty attrProp = nextMonsterProp.FindPropertyRelative("attributeInfo");
                        attrProp.FindPropertyRelative("hitRate").floatValue = 1f;
                        RefreshMonsterAttribute(nextMonsterProp.FindPropertyRelative("attributeInfo"), DEFAULT_MONSTER);
                    }
                }
            }

            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();        
    }

    private void DrawTeamInfo(SerializedProperty teamProp, bool isSelfTeam)
    {
        SerializedProperty charProp = teamProp.GetArrayElementAtIndex(0);
        SerializedProperty charBirthProp = charProp.FindPropertyRelative("birthIndex");
        SerializedProperty charBirthPoint = charProp.FindPropertyRelative("birthPoint");
        int birthIndex = charBirthProp.intValue;
        Vector3 birthPoint = charBirthPoint.vector3Value;
        DrawBirthInfo("主角出生点", ref birthIndex, ref birthPoint, Color.white);
        charBirthProp.intValue = birthIndex;
        charBirthPoint.vector3Value = birthPoint;
        DrawSpace(1);
        EditorGUILayout.PropertyField(serializedObject.FindProperty(isSelfTeam ? "astrologyStar" : "opAstrologyStar"), new GUIContent("点星"));

        DrawSpace(3);
        DrawTableTitles(60f, 0f, "配置索引", "等级", "突破", "天命");

        for (int i = 0; i < 4; ++i)
        {
            DrawRoleInfo(teamProp.GetArrayElementAtIndex(i), i, isSelfTeam);
            DrawSpace(3);
        }

        DrawRelateTeam(teamProp, isSelfTeam);
    }

    private void DrawBirthInfo(string title, ref int intValue, ref Vector3 vector3Value, Color titleColor)
    {
        EditorGUILayout.BeginHorizontal();
        GUI.color = titleColor;
        EditorGUILayout.LabelField(title, GUILayout.MaxWidth(60));
        GUI.color = Color.white;
        int selected = EditorGUILayout.IntPopup(intValue, birthStrArray, birthIntArray, GUILayout.MaxWidth(40));
        usedBirth.Add(selected);

        if (intValue != selected)
        {
            usedBirth.Remove(intValue);
            intValue = selected;
            vector3Value = birthPoints[selected];
        }

        vector3Value = EditorGUILayout.Vector3Field(new GUIContent(), vector3Value);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawRoleInfo(SerializedProperty prop, int index, bool isSelfTeam)
    {
        EditorGUILayout.BeginHorizontal();
        string label = index == 0 ? "主角" : "宠物" + index;
        GUI.color = isSelfTeam ? Color.green : Color.red;
        EditorGUILayout.LabelField(label, GUILayout.Width(35));
        GUI.color = Color.white;

        if (GUILayout.Button("+"))
        {
            WizardSelectObject wizard;

            if (index == 0)
                wizard = ScriptableWizard.DisplayWizard<WizardSelectCharacter>("选择主角");
            else 
                wizard = ScriptableWizard.DisplayWizard<WizardSelectPet>("选择上阵宠物");

            wizard.onCommit += selected => 
            { 
                prop.FindPropertyRelative("tid").intValue = selected;
                prop.serializedObject.ApplyModifiedProperties();
            };           
        }

        EditorGUILayout.PropertyField(prop.FindPropertyRelative("tid"), new GUIContent());
        EditorGUILayout.PropertyField(prop.FindPropertyRelative("level"), new GUIContent());
        EditorGUILayout.PropertyField(prop.FindPropertyRelative("breakLevel"), new GUIContent());
        EditorGUILayout.PropertyField(prop.FindPropertyRelative("fateLevel"), new GUIContent());
        EditorGUILayout.EndHorizontal();

        SerializedProperty skillProp = prop.FindPropertyRelative("skillLevels");
        EditorGUILayout.PropertyField(skillProp, new GUIContent("技能等级"), false);

        if (skillProp.isExpanded)
        {
            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < 4; ++i)
            {
                EditorGUILayout.PropertyField(skillProp.GetArrayElementAtIndex(i), new GUIContent());
            }

            EditorGUILayout.EndHorizontal();
        }

        SerializedProperty equipProp = prop.FindPropertyRelative("equips");
        EditorGUILayout.PropertyField(equipProp, new GUIContent("装备法器"), false);

        if (equipProp.isExpanded)
        {
            InitEquipList();
            DrawTableTitles(20f, 0f, "类型", "索引", "强化等级", "精炼等级");
            ++EditorGUI.indentLevel;                    
            DrawEquipInfo(equipProp.GetArrayElementAtIndex(0), 0, "武器");
            DrawEquipInfo(equipProp.GetArrayElementAtIndex(1), 1, "饰品");
            DrawEquipInfo(equipProp.GetArrayElementAtIndex(2), 2, "衣服");
            DrawEquipInfo(equipProp.GetArrayElementAtIndex(3), 3, "鞋");
            DrawEquipInfo(equipProp.GetArrayElementAtIndex(4), 4, "攻击法器");
            DrawEquipInfo(equipProp.GetArrayElementAtIndex(5), 5, "防御法器");
            --EditorGUI.indentLevel;
        }
    }

    private void DrawMonsterInfo(SerializedProperty prop, int index)
    {
        EditorGUILayout.BeginHorizontal();
        string label = "怪物" + (index + 1);
        GUI.color = Color.red;
        EditorGUILayout.LabelField(label, GUILayout.Width(35));
        GUI.color = Color.white;
        EditorGUILayout.PropertyField(prop.FindPropertyRelative("tid"), new GUIContent());

        GUI.backgroundColor = new Color(1f, 0.3f, 0.3f); ;

        if (GUILayout.Button("删除", EditorStyles.miniButton))
        {
            var willDelProp = monsterProp.GetArrayElementAtIndex(index);
            usedBirth.Remove(willDelProp.FindPropertyRelative("birthIndex").intValue);
            monsterProp.DeleteArrayElementAtIndex(index);
            return;
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("出生点", GUILayout.MaxWidth(60));
        SerializedProperty birthProp = prop.FindPropertyRelative("birthIndex");
        int selected = EditorGUILayout.IntPopup(birthProp.intValue, birthStrArray, birthIntArray, GUILayout.MaxWidth(40));
        usedBirth.Add(selected);

        if (birthProp.intValue != selected)
        {
            usedBirth.Remove(birthProp.intValue);
            birthProp.intValue = selected;
            prop.FindPropertyRelative("birthPoint").vector3Value = birthPoints[selected];
        }

        EditorGUILayout.PropertyField(prop.FindPropertyRelative("birthPoint"), new GUIContent());
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        SerializedProperty attrProp = prop.FindPropertyRelative("attributeInfo");
        EditorGUILayout.PropertyField(attrProp, new GUIContent("属性(可实时修改)"), false);
        GUI.backgroundColor = new Color(1f, 1f, 0.3f);

        if (GUILayout.Button("刷   新", EditorStyles.miniButton))
        {
            RefreshMonsterAttribute(attrProp, prop.FindPropertyRelative("tid").intValue);
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        if (attrProp.isExpanded)
        {
            ++EditorGUI.indentLevel;
            DrawMonsterAttribute(attrProp);
            --EditorGUI.indentLevel;
        }
    }

    private void DrawSpace(int spaceCount)
    {
        for (int i = 0; i < spaceCount; ++i)
        {
            EditorGUILayout.Space();
        }
    }

    private void DrawTableTitles(float leftSpace, float rightSpace, params string[] titles)
    {
        if (titles.Length == 0)
            return;

        float width = (EditorGUIUtility.currentViewWidth - leftSpace - rightSpace - 60f) / titles.Length;
        GUILayoutOption op = GUILayout.MaxWidth(width);
        EditorGUILayout.BeginHorizontal();          
        EditorGUILayout.LabelField("", GUILayout.MaxWidth(leftSpace));

        for (int i = 0; i < titles.Length; ++i)
        {
            EditorGUILayout.LabelField(titles[i], op);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawEquipInfo(SerializedProperty prop, int index, string label)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.MaxWidth(65));
        SerializedProperty tidProp = prop.FindPropertyRelative("tid");

        if (equipList[index] != null)
        {
            tidProp.intValue = EditorGUILayout.IntPopup(tidProp.intValue, equipList[index].descArray, equipList[index].tidArray, GUILayout.MinWidth(70));
        }

        EditorGUILayout.PropertyField(prop.FindPropertyRelative("strengthenLevel"), new GUIContent());
        EditorGUILayout.PropertyField(prop.FindPropertyRelative("refineLevel"), new GUIContent());
        EditorGUILayout.EndHorizontal();
    }

    private void InitEquipList()
    {
        if (equipTableReader.table != null)
        {
            List<int>[] listArray = new List<int>[6];

            for (int i = 0; i < 6; ++i)
            {
                listArray[i] = new List<int>();
                listArray[i].Add(0);
            }

            foreach (var pair in equipTableReader.table.GetAllRecord())
            {
                int type = pair.Value["EQUIP_TYPE"];

                if (type > 0 && type <= 4)
                {
                    listArray[type - 1].Add(pair.Key);
                }
                else if (type == 6 || type == 7)
                {
                    listArray[type - 2].Add(pair.Key);
                }
            }

            for (int i = 0; i < 6; ++i)
            {
                List<int> list = listArray[i];
                list.Sort();
                equipList[i] = new EquipList();
                equipList[i].tidArray = new int[list.Count];
                equipList[i].descArray = new string[list.Count];

                for (int j = 0; j < list.Count; ++j)
                {
                    equipList[i].tidArray[j] = list[j];
                    equipList[i].descArray[j] = list[j] == 0 ? "无" : list[j].ToString() + " " + equipTableReader.table.GetData(list[j], "NAME");
                }
            }
        }
    }

    private void DrawRelateTeam(SerializedProperty teamProp, bool isSelfTeam)
    {
        GUI.color = isSelfTeam ? Color.green : Color.red;
        EditorGUILayout.LabelField("缘分助战队伍");
        GUI.color = Color.white;

        EditorGUILayout.BeginHorizontal();

        for (int i = 4; i <= 6; ++i)
        {
            DrawRelatePet(teamProp.GetArrayElementAtIndex(i), isSelfTeam);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        for (int i = 7; i <= 9; ++i)
        {
            DrawRelatePet(teamProp.GetArrayElementAtIndex(i), isSelfTeam);
        }

        EditorGUILayout.EndHorizontal();

        if (isSelfTeam)
        {
            isSelfRelateInfoExpanded = EditorGUILayout.Foldout(isSelfRelateInfoExpanded, "缘分信息");
        }
        else 
        {
            isOpponentRelateInfoExpanded = EditorGUILayout.Foldout(isOpponentRelateInfoExpanded, "缘分信息");
        }

        if (isSelfTeam ? isSelfRelateInfoExpanded : isOpponentRelateInfoExpanded)
        {
            ++EditorGUI.indentLevel;

            for (int i = 0; i <= 3; ++i)
            {
                DrawRelateInfo(teamProp.GetArrayElementAtIndex(i), i, isSelfTeam);
            }

            --EditorGUI.indentLevel;
        }
    }

    private void DrawRelatePet(SerializedProperty prop, bool isSelfTeam)
    {
        SerializedProperty tidProp = prop.FindPropertyRelative("tid");

        if (GUILayout.Button("+"))
        {
            WizardSelectRelatePet wizard = ScriptableWizard.DisplayWizard<WizardSelectRelatePet>("选择助战宠物");
            wizard.roleInfos = isSelfTeam ? simulator.roleInfos : simulator.opRoleInfos;
            wizard.RefreshSelectArray();

            wizard.onCommit += selected =>
            {
                prop.FindPropertyRelative("tid").intValue = selected;
                prop.serializedObject.ApplyModifiedProperties();
            };     
        }

        EditorGUILayout.PropertyField(tidProp, new GUIContent());
    }

    private void DrawRelateInfo(SerializedProperty prop, int teamPos, bool isSelfTeam)
    {
        int tid = prop.FindPropertyRelative("tid").intValue;

        if (activeTableReader.table != null && relateTableReader.table != null && tid > 0)
        {
            DataRecord record = activeTableReader.table.GetRecord(tid);

            if (record != null)
            {
                EditorGUILayout.LabelField(record["NAME"]);

                ++EditorGUI.indentLevel;

                for (int i = 1; i <= 15; ++i)
                {
                    int relTid = record["RELATE_ID_" + i];

                    if (relTid > 0)
                    {
                        DataRecord relRecord = relateTableReader.table.GetRecord(relTid);

                        if (relRecord != null)
                        {
                            if (IsRelateActive(relRecord, teamPos, isSelfTeam))
                                GUI.color = isSelfTeam ? Color.green : Color.red;

                            EditorGUILayout.LabelField(relRecord["RELATE_NAME"] + "   " + relRecord["RELATE_DWSCRIBE"]);
                            GUI.color = Color.white;
                        }
                    }
                }

                --EditorGUI.indentLevel;
            }
        }
    }

    private bool IsRelateActive(DataRecord relateRecord, int teamPos, bool isSelfTeam)
    {
        HashSet<int> condition = Relationship.GetConditionTidSet(relateRecord);
        return condition.IsSubsetOf(simulator.GetRelateContextSet(teamPos, isSelfTeam));
    }

    private void RefreshMonsterAttribute(SerializedProperty attrProp, int tid)
    {
        if (monsterTableReader.table != null)
        {
            DataRecord monsterConfig = monsterTableReader.table.GetRecord(tid);

            if (monsterConfig != null)
            {
                attrProp.FindPropertyRelative("maxHp").intValue = monsterConfig["BASE_HP"];
                attrProp.FindPropertyRelative("attack").intValue = monsterConfig["BASE_ATTACK"];
                attrProp.FindPropertyRelative("physicalDefence").intValue = monsterConfig["BASE_PHYSICAL_DEFENCE"];
                attrProp.FindPropertyRelative("magicDefence").intValue = monsterConfig["BASE_MAGIC_DEFENCE"];
                attrProp.FindPropertyRelative("hitRate").floatValue = monsterConfig["HIT"] / 10000f;
                attrProp.FindPropertyRelative("dodgeRate").floatValue = monsterConfig["DODGE"] / 10000f;
                attrProp.FindPropertyRelative("criticalStrikeRate").floatValue = monsterConfig["CRITICAL_STRIKE"] / 10000f;
                attrProp.FindPropertyRelative("defenceCriticalStrikeRate").floatValue = 0f;
                attrProp.FindPropertyRelative("damageEnhanceRate").floatValue = 0f;
                attrProp.FindPropertyRelative("damageMitigationRate").floatValue = monsterConfig["MITIGATIONG"] / 10000f;
            }
        }
    }

    private void DrawMonsterAttribute(SerializedProperty attrProp)
    {
        EditorGUILayout.PropertyField(attrProp.FindPropertyRelative("maxHp"), new GUIContent("血量"));
        EditorGUILayout.PropertyField(attrProp.FindPropertyRelative("attack"), new GUIContent("攻击力"));
        EditorGUILayout.PropertyField(attrProp.FindPropertyRelative("physicalDefence"), new GUIContent("物理防御"));
        EditorGUILayout.PropertyField(attrProp.FindPropertyRelative("magicDefence"), new GUIContent("魔法防御"));
        EditorGUILayout.Slider(attrProp.FindPropertyRelative("hitRate"), MIN_HIT_RATE, MAX_HIT_RATE, new GUIContent("命中"));
        EditorGUILayout.Slider(attrProp.FindPropertyRelative("dodgeRate"), MIN_DODGE_RATE, MAX_DODGE_RATE, new GUIContent("闪避"));
        EditorGUILayout.Slider(attrProp.FindPropertyRelative("criticalStrikeRate"), MIN_CRITICAL_RATE, MAX_CRITICAL_RATE, new GUIContent("暴击概率"));
        EditorGUILayout.Slider(attrProp.FindPropertyRelative("defenceCriticalStrikeRate"), MIN_CRITICAL_DEF, MAX_CRITICAL_DEF, new GUIContent("暴击抵抗"));
        EditorGUILayout.Slider(attrProp.FindPropertyRelative("damageEnhanceRate"), MIN_ENHANCE, MAX_ENHANCE, new GUIContent("伤害加深"));
        EditorGUILayout.Slider(attrProp.FindPropertyRelative("damageMitigationRate"), MIN_MITIGATIONG, MAX_MITIGATIONG, new GUIContent("伤害减免"));
    }
}


public class WizardSelectObject : ScriptableWizard
{
    public event Action<int> onCommit;

    public int selectedObjectIndex { get; private set; }
    public string selectedObjectContent { get; private set; }
    protected int[] objectIndexArray { get; private set; }
    protected string[] objectContentArray { get; private set; }
    protected int minIndex { get; private set; }
    protected int maxIndex { get; private set; }
    
    private GUIStyle selectedStyle;
    private NiceTable _configTable;

    protected NiceTable configTable
    {
        get 
        {
            if (_configTable == null)
                _configTable = InitConfigTable();

            return _configTable;
        }
    }

    protected virtual void RestoreFilterArrayOnEnable() { }
    protected virtual void ReserveFilterArrayOnDisable() { }
    protected virtual void OnDrawFilter() { }
    protected virtual NiceTable InitConfigTable() { return null; }
    protected virtual string GetObjectContent(int index, DataRecord config) { return ""; }

    protected virtual bool IsSuitable(int index, DataRecord config) 
    {
        return minIndex <= index && index <= maxIndex;
    }

    public static void RestoreFilterArray(string key, bool[] filterArray, int defaultMask)
    {
        int mask = EditorPrefs.GetInt(key, defaultMask);

        for (int i = 0; i < filterArray.Length; ++i)
        {
            filterArray[i] = (mask & (1 << i)) > 0;
        }
    }

    public static void ReserveFilterArray(string key, bool[] filterArray)
    {
        int mask = 0;

        for (int i = 0; i < filterArray.Length; ++i)
        {
            if (filterArray[i])
            {
                mask |= 1 << i;
            }
        }

        EditorPrefs.SetInt(key, mask);
    }

    protected void RestoreFilterRange(string minKey, string maxKey)
    {
        minIndex = EditorPrefs.GetInt(minKey, 0);
        maxIndex = EditorPrefs.GetInt(maxKey, 999999);
    }

    protected void ReserveFilterRange(string minKey, string maxKey)
    {
        EditorPrefs.SetInt(minKey, minIndex);
        EditorPrefs.SetInt(maxKey, maxIndex);
    }

    public static string GetObjectQualityContent(int starLevel)
    {
        switch (starLevel)
        {
            case 1: return "白色";
            case 2: return "绿色";
            case 3: return "蓝色";
            case 4: return "紫色";
            case 5: return "橙色";
            case 6: return "红色";
            case 7: return "粉色";
            default: return "";
        }
    }

    public static string GetObjectElementContent(int element)
    {
        switch (element)
        {
            case 0: return "火";
            case 1: return "水";
            case 2: return "木";
            case 3: return "阳";
            case 4: return "阴";
            default: return "";
        }
    }

    public static string GetObjectBattleTypeContent(int battleType)
    {
        switch (battleType)
        {
            case 0: return "进攻型";
            case 1: return "防御型";
            case 2: return "辅助型";
            default: return "";
        }
    }

    private void OnEnable()
    {
        selectedStyle = new GUIStyle(GUI.skin.textArea);
        selectedStyle.fontSize = 18;
        selectedStyle.normal.textColor = Color.yellow;
        RestoreFilterArrayOnEnable();
        RefreshSelectArray();
    }

    private void OnGUI()
    {
        OnDrawFilter();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
      
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("筛选范围", GUILayout.Width(60));
        int oldMinIndex = minIndex;
        minIndex = EditorGUILayout.IntField(minIndex, GUILayout.MaxWidth(80));
        EditorGUILayout.LabelField("--->", GUILayout.Width(30));
        int oldMaxIndex = maxIndex;
        maxIndex = EditorGUILayout.IntField(maxIndex, GUILayout.MaxWidth(80));
        EditorGUILayout.EndHorizontal();

        if (oldMinIndex != minIndex || oldMaxIndex != maxIndex)
        {
            RefreshSelectArray();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("请选择", GUILayout.Width(60));

        if (objectContentArray == null || objectIndexArray == null)
        {
            objectContentArray = new string[0];
            objectIndexArray = new int[0];
        }

        selectedObjectIndex = EditorGUILayout.IntPopup(selectedObjectIndex, objectContentArray, objectIndexArray, GUILayout.MinWidth(70));
        EditorGUILayout.LabelField("共" + objectIndexArray.Length + "项符合条件");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (selectedObjectIndex > 0)
        {
            for (int i = 0; i < objectIndexArray.Length; ++i)
            {
                if (objectIndexArray[i] == selectedObjectIndex)
                {
                    selectedObjectContent = objectContentArray[i];
                    break;
                }
            }

            EditorGUILayout.LabelField("当前选择    " + selectedObjectContent, selectedStyle);         
        }
        else 
        {
            selectedObjectContent = null;
        }

        GUI.backgroundColor = new Color(0.3f, 1f, 0.3f);

        if (GUI.Button(new Rect(position.size.x - 105, position.size.y - 35, 100, 30), "确定"))
        {
            if (selectedObjectIndex > 0 && onCommit != null)
            {
                onCommit(selectedObjectIndex);
            }

            Close();
        }

        GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);

        if (GUI.Button(new Rect(position.size.x - 225, position.size.y - 35, 100, 30), "取消"))
        {
            Close();
        }

        GUI.backgroundColor = Color.white;
    }

    private void OnDisable()
    {
        ReserveFilterArrayOnDisable();
    }

    public void RefreshSelectArray()
    {
        if (configTable == null)
        {
            return;
        }

        List<int> indexList = new List<int>();

        foreach (var pair in configTable.GetAllRecord())
        {
            if (IsSuitable(pair.Key, pair.Value))
            {
                indexList.Add(pair.Key);
            }
        }

        indexList.Sort();
        objectIndexArray = indexList.ToArray();
        objectContentArray = new string[objectIndexArray.Length];

        for (int i = 0; i < objectIndexArray.Length; ++i)
        {
            DataRecord record = configTable.GetRecord(objectIndexArray[i]);
            objectContentArray[i] = GetObjectContent(objectIndexArray[i], record);
        }
    }

    protected bool FilterCheckBox(string name, int minWidth, ref bool value)
    {
        bool oldValue = value;
        value = EditorGUILayout.ToggleLeft(name, value, GUILayout.MinWidth(minWidth));

        if (oldValue != value)
        {
            RefreshSelectArray();
            return true;
        }

        return false;
    }
}


public class WizardSelectCharacter : WizardSelectObject
{
    private bool[] charTypeFilter = new bool[2];
    private bool[] qualityFilter = new bool[7];

    protected override void RestoreFilterArrayOnEnable()
    {
        RestoreFilterRange("CHAR_MIN_INDEX", "CHAR_MAX_INDEX");
        RestoreFilterArray("CHAR_TYPE_MASK", charTypeFilter, ~0);
        RestoreFilterArray("CHAR_QUALITY_MASK", qualityFilter, ~0);
    }

    protected override void ReserveFilterArrayOnDisable()
    {
        ReserveFilterRange("CHAR_MIN_INDEX", "CHAR_MAX_INDEX");
        ReserveFilterArray("CHAR_TYPE_MASK", charTypeFilter);
        ReserveFilterArray("CHAR_QUALITY_MASK", qualityFilter);
    }

    protected override NiceTable InitConfigTable()
    {
        return GameCommon.LoadTable("Config/ActiveObject.csv", LOAD_MODE.UNICODE);
    }

    protected override string GetObjectContent(int index, DataRecord config)
    {
        string content = index.ToString();
        content += " " + config["NAME"];
        content += " " + GetObjectQualityContent(config["STAR_LEVEL"]);
        return content;
    }

    protected override void OnDrawFilter()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("主角类型", GUILayout.MinWidth(50));
        FilterCheckBox("女", 30, ref charTypeFilter[0]);
        FilterCheckBox("男", 30, ref charTypeFilter[1]);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("主角品质", GUILayout.MinWidth(50));

        for (int i = 0; i < 7; ++i)
        {
            FilterCheckBox(GetObjectQualityContent(i + 1), 40, ref qualityFilter[i]);
        }

        EditorGUILayout.EndHorizontal();
    }

    protected override bool IsSuitable(int index, DataRecord record)
    {
        if (base.IsSuitable(index, record))
        {
            string type = record["CLASS"];
            int starLevel = record["STAR_LEVEL"];
            int charType = GameCommon.GetRoleType(index);

            if (type == "CHAR" && starLevel > 0 && starLevel - 1 < 7 && charType < 2)
            {
                return qualityFilter[starLevel - 1] && charTypeFilter[charType];
            }
        }

        return false;
    }
}


public class WizardSelectPet : WizardSelectObject
{
    private bool[] qualityFilter = new bool[7];
    private bool[] elementFilter = new bool[5];
    private bool[] battleTypeFilter = new bool[4];

    protected override void RestoreFilterArrayOnEnable()
    {
        RestoreFilterRange("PET_MIN_INDEX", "PET_MAX_INDEX");
        RestoreFilterArray("PET_QUALITY_MASK", qualityFilter, (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6));
        RestoreFilterArray("PET_ELEMENT_MASK", elementFilter, ~0);
        RestoreFilterArray("PET_BATTLE_TYPE_MASK", battleTypeFilter, ~0);
    }

    protected override void ReserveFilterArrayOnDisable()
    {
        ReserveFilterRange("PET_MIN_INDEX", "PET_MAX_INDEX");
        ReserveFilterArray("PET_QUALITY_MASK", qualityFilter);
        ReserveFilterArray("PET_ELEMENT_MASK", elementFilter);
        ReserveFilterArray("PET_BATTLE_TYPE_MASK", battleTypeFilter);
    }

    protected override NiceTable InitConfigTable()
    {
        return GameCommon.LoadTable("Config/ActiveObject.csv", LOAD_MODE.UNICODE);
    }

    protected override string GetObjectContent(int index, DataRecord config)
    {
        string content = index.ToString();
        content += " " + config["NAME"];
        content += " " + GetObjectQualityContent(config["STAR_LEVEL"]);
        content += " " + GetObjectElementContent(config["ELEMENT_INDEX"]);
        content += " " + GetObjectBattleTypeContent(config["PET_TYPE"]);
        return content;
    }

    protected override void OnDrawFilter()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("宠物品质", GUILayout.MinWidth(50));

        for (int i = 0; i < 7; ++i)
        {
            FilterCheckBox(GetObjectQualityContent(i + 1), 40, ref qualityFilter[i]);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("宠物元素", GUILayout.MinWidth(50));

        for (int i = 0; i < 5; ++i)
        {
            FilterCheckBox(GetObjectElementContent(i), 30, ref elementFilter[i]);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("战斗类型", GUILayout.MinWidth(50));

        for (int i = 0; i < 3; ++i)
        {
            FilterCheckBox(GetObjectBattleTypeContent(i), 50, ref battleTypeFilter[i]);
        }

        FilterCheckBox("其它", 50, ref battleTypeFilter[3]);
        EditorGUILayout.EndHorizontal();
    }

    protected override bool IsSuitable(int index, DataRecord record)
    {
        if (base.IsSuitable(index, record))
        {
            string type = record["CLASS"];
            int starLevel = record["STAR_LEVEL"];
            int element = record["ELEMENT_INDEX"];
            int battleType = record["PET_TYPE"];

            if (type == "PET" && starLevel > 0 && starLevel - 1 < 7 && element < 5 && battleType < 4)
            {
                return qualityFilter[starLevel - 1] && elementFilter[element] && battleTypeFilter[battleType];
            }
        }

        return false;
    }
}


public class WizardSelectRelatePet : WizardSelectPet
{
    public BattleSimulator.RoleBattleInfo[] roleInfos { get; set; }

    private Dictionary<int, string> _relateDict;
    private NiceTable _relateTable;

    private NiceTable relateTable
    {
        get
        {
            if (_relateTable == null)
                _relateTable = GameCommon.LoadTable("Config/RelateConfig.csv", LOAD_MODE.UNICODE);

            return _relateTable;
        }
    }

    private Dictionary<int, string> relateDict
    {
        get
        {
            if (roleInfos == null)
            {
                return new Dictionary<int, string>();
            }

            if (_relateDict == null)
            {
                InitRelateDict();
            }

            return _relateDict;
        }
    }

    private bool onlyDisplayCanActivateRelatePet = false;

    protected override void RestoreFilterArrayOnEnable()
    {
        base.RestoreFilterArrayOnEnable();
        onlyDisplayCanActivateRelatePet = EditorPrefs.GetBool("ONLY_DISPLAY_RELATE_PET", true);
    }

    protected override void ReserveFilterArrayOnDisable()
    {
        base.ReserveFilterArrayOnDisable();
        EditorPrefs.SetBool("ONLY_DISPLAY_RELATE_PET", onlyDisplayCanActivateRelatePet);
    }

    protected override void OnDrawFilter()
    {
        base.OnDrawFilter();
        EditorGUILayout.Space();
        FilterCheckBox("仅显示与上阵队伍缘分相关的宠物", 100, ref onlyDisplayCanActivateRelatePet);
    }

    protected override string GetObjectContent(int index, DataRecord config)
    {
        string content = base.GetObjectContent(index, config);
        string relateContent;

        if (relateDict.TryGetValue(index, out relateContent))
        {
            content += " --- " + relateContent;
        }

        return content;
    }

    protected override bool IsSuitable(int index, DataRecord record)
    {
        if (base.IsSuitable(index, record))
        {
            return !onlyDisplayCanActivateRelatePet || relateDict.ContainsKey(index);
        }

        return false;
    }



    private void InitRelateDict()
    {
        _relateDict = new Dictionary<int, string>();

        if (roleInfos != null)
        {
            List<int> existPets = new List<int>();

            for (int i = 0; i < 4; ++i)
            {
                BattleSimulator.RoleBattleInfo info = roleInfos[i];

                if(info.tid == 0)
                    continue;

                DataRecord record = configTable.GetRecord(info.tid);

                for (int j = 1; j <= 15; ++j)
                {
                    int relTid = record["RELATE_ID_" + j];

                    if (relTid > 0)
                    {
                        DataRecord relRecord = relateTable.GetRecord(relTid);

                        if (relRecord != null)
                        {
                            string content = record["NAME"] + " " + relRecord["RELATE_NAME"];

                            for (int k = 1; k <= 6; ++k)
                            {
                                int tid = relRecord["NEED_CONTENT_" + k];

                                if (tid > 0 && PackageManager.GetItemTypeByTableID(tid) == ITEM_TYPE.PET && !_relateDict.ContainsKey(tid))
                                {
                                    _relateDict.Add(tid, content);
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < roleInfos.Length; ++i)
            {
                BattleSimulator.RoleBattleInfo info = roleInfos[i];

                if (info.tid > 0)
                    _relateDict.Remove(info.tid);
            }
        }
    }
}