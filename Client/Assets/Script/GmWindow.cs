using UnityEngine;
using System.Collections;
using Logic;
using System;
using DataTable;
using System.Collections.Generic;
using Utilities;
using System.Text.RegularExpressions;

public class SuperPlayerInfo
{
    public string name=CommonParam.mAccount;
    public int gold;
    public int soulPoint;
    public int unionContr;
    public int reputation;
    public int prestige;
    public int battleAchv;
    public int diamond;
}

public class SuperPetInfo
{
    public int teamPos;
    public int tid;
    public int level=1;
    public int breakLevel;
    public int[] skillLevel= new int[4]{1,1,1,1};
}

public class SuperEquipInfo
{
    public int teamPos;
    public int tid;
    public int strengthenLevel=1;
    public int refineLevel;
}
public enum InputKind
{
    COMMON_MONEY,     // 银币元宝
    STRANGE_MONEY,     //其他货币
    REFINE,
    STRENGTH,
    PET_TID,
    EQUIP_TID,
    LEVEL,
    BREAK_LEVEL,
    NAME,
    SKILL,
}

public class GmWindow : tWindow
{
    public static string UsedGM;
    public List<GameObject> mobjs=new List<GameObject>();
    public bool error;
    int maxLevel = DataCenter.mCharacterLevelExpTable.GetAllRecord().Count;
    int maxBreakLevel = DataCenter.mBreakLevelConfig.GetAllRecord().Count - 1;
    int equipMaxStrengthLevel = DataCenter.mEquipStrengthCostConfig.GetAllRecord().Count;
    int equipMaxRefineLevel = DataCenter.mEquipRefineLvConfig.GetAllRecord().Count;
    int magicMaxStrengthLevel = DataCenter.mMagicEquipLvConfig.GetAllRecord().Count;
    int magicMaxRefineLevel = DataCenter.mMagicEquipRefineConfig.GetAllRecord().Count - 1;
    int skillMaxLevel = DataCenter.mSkillCost.GetAllRecord().Count;
    int quickCreateCount = DataCenter.mDefaultRole.GetAllRecord().Count;
    int maxStageId;
    public static bool hasOpen;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_main_button", new DefineFactory<Button_main_button>());
        EventCenter.Self.RegisterEvent("Button_pet1_button", new DefineFactory<Button_pet1_button>());
        EventCenter.Self.RegisterEvent("Button_pet2_button", new DefineFactory<Button_pet2_button>());
        EventCenter.Self.RegisterEvent("Button_pet3_button", new DefineFactory<Button_pet3_button>());
        EventCenter.Self.RegisterEvent("Button_see_equip_button", new DefineFactory<Button_see_equip_button>());
        EventCenter.Self.RegisterEvent("Button_see_pet_button", new DefineFactory<Button_see_pet_button>());
        EventCenter.Self.RegisterEvent("Button_send_gm_button", new DefineFactory<Button_send_gm_button>());
        EventCenter.Self.RegisterEvent("Button_close_gm_window_button", new DefineFactory<Button_close_gm_window_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);
        if (!hasOpen) 
        {
            refreshLimitAndTable();
            mobjs.Add(GetSub("main"));
            mobjs.Add(GetSub("pet1"));
            mobjs.Add(GetSub("pet2"));
            mobjs.Add(GetSub("pet3"));
            mobjs.Add(GetSub("equip_csv"));
            mobjs.Add(GetSub("pet_csv"));
            hasOpen = true;           
            mobjs.RemoveAll(x=>x==null);
            getMaxStageId();
        }
        closeAll();
        setQuickCreatePopupListData();     
        SetVisible("main",true);
    }

    public void getMaxStageId()
    {
        int maxIndex=0;
        foreach (KeyValuePair<int, DataRecord> temp in DataCenter.mStageTable.GetAllRecord())
        {
            if (temp.Value.getData("TYPE") == 1)
            {
                if (temp.Value.getData("INDEX") > maxIndex && temp.Value.getData("INDEX")<30000)
                {
                    maxIndex = temp.Value.getData("INDEX");
                }
            }
        }
        maxStageId = maxIndex;
    }
    
    public void closeAll(string choice = "")   //点击头4个按钮中的一个将关闭其他显示
    {
        if (mobjs != null)
        {
           foreach(GameObject temp in mobjs)
           {
               if (choice!=temp.name)
                 temp.SetActive(false);
               else
                 temp.SetActive(true);
           }
        }
    }    

    public void refreshLimitAndTable()
    {
        UILabel mlimit = GetComponent<UILabel>("limit_label");     
        mlimit.text=mlimit.text.Replace("{0}",maxLevel.ToString()).Replace("{1}",maxBreakLevel.ToString()).Replace("{2}",equipMaxStrengthLevel.ToString()).
        Replace("{3}", equipMaxRefineLevel.ToString()).Replace("{4}", magicMaxStrengthLevel.ToString()).Replace("{5}", magicMaxRefineLevel.ToString()).Replace("{6}", skillMaxLevel.ToString());
        UILabel pet_label = GetComponent<UILabel>("pet_csv_label");
        UILabel equip_label = GetComponent<UILabel>("equip_csv_label");
        int i = 0;
        foreach (KeyValuePair<int, DataRecord> temp in DataCenter.mActiveConfigTable.GetAllRecord())
        {
            i++;
            if (temp.Value.getData("INDEX") == 30289)
                break;
            if (i % 3 == 0) { pet_label.text += temp.Value.getData("INDEX") + " " + temp.Value.getData("NAME") + " " + temp.Value.getData("INFO") + "\n"; }
            else
            {
                pet_label.text += temp.Value.getData("INDEX") + " " + temp.Value.getData("NAME") + " " + temp.Value.getData("INFO") + "     ";
            }
        }
        int j = 0;
        foreach (KeyValuePair<int, DataRecord> temp in DataCenter.mRoleEquipConfig.GetAllRecord())
        {
            j++;
            if (j % 3 == 0) { equip_label.text += temp.Value.getData("INDEX") + " " + temp.Value.getData("NAME") + " 品质" + temp.Value.getData("QUALITY") + "\n"; }
            else
            {
                equip_label.text += temp.Value.getData("INDEX") + " " + temp.Value.getData("NAME") + " 品质" + temp.Value.getData("QUALITY") + "     ";
            }
        }
    }

    public string getInput(string who, string what)      //获取输入，2层父子关系
    {
        GameObject mwho=GameCommon.FindObject(mGameObjUI, who) ;
        if (mwho!= null)
        {
           GameObject mwhat=GameCommon.FindObject(mwho, what);
           if (mwhat != null)
           {
               UILabel temp = GameCommon.FindObject(mwhat, "input_label").GetComponent<UILabel>();
               if(temp!=null)
                  return temp.text;
           }
        }
        return null;
    }

    public string getInput(string who, string what, string which)     //获取输入，3层父子关系
    {
        GameObject mwho = GameCommon.FindObject(mGameObjUI, who);
        if (mwho != null)
        {
            GameObject mwhat = GameCommon.FindObject(mwho, what);
            if (mwhat != null)
            {
                GameObject mwhich = GameCommon.FindObject(mwhat, which);
                if (mwhich != null)
                {
                    UILabel temp = GameCommon.FindObject(mwhich, "input_label").GetComponent<UILabel>();
                    if (temp != null)
                        return temp.text;
                }
            }
        }
        return null;
    }

    public void getBaseInfo(out SuperPlayerInfo superPlayerInfo, out List<SuperPetInfo> superPetInfo, out List<SuperEquipInfo> superEquipInfo, out int stageId)
    {
        getSuperPlayerInfo(out  superPlayerInfo);
        getSuperPetInfo(out  superPetInfo);
        getSuperEquipInfo(out  superEquipInfo);
        getStageId(out stageId);
    }

    public bool correct(ref string attribute, InputKind kind)       //矫正除去tid和名字外不正确的参数，小的部分设为最低等级，超出的设为最大等级
    {
        if (attribute != null)
        {
            int temp = 1;
            int.TryParse(attribute, out temp);
            switch (kind)
            {
                case InputKind.BREAK_LEVEL:
                    if (temp < 0)
                        temp = 0;
                    if (temp > maxBreakLevel)
                        temp = maxBreakLevel;
                    break;
                case InputKind.LEVEL:
                    if (temp < 1)
                        temp = 1;
                    if (temp > maxLevel)
                        temp = maxLevel;
                    break;
                case InputKind.SKILL:
                    if (temp < 1)
                        temp = 1;
                    if (temp > skillMaxLevel)
                        temp = skillMaxLevel;
                    break;
                case InputKind.REFINE:
                    if (temp < 0)
                        temp = 0;
                    if (temp > equipMaxRefineLevel)
                        temp = equipMaxRefineLevel;
                    break;
                case InputKind.STRENGTH:
                    if (temp < 1)
                        temp = 1;
                    if (temp > equipMaxStrengthLevel)
                        temp = equipMaxStrengthLevel;
                    break;
                case InputKind.COMMON_MONEY:
                    if (temp < 0)
                        temp = 0;
                    if (temp > 999999999)
                        temp = 999999999;
                    break;
                case InputKind.STRANGE_MONEY:
                    if (temp < 0)
                        temp = 0;
                    if (temp > 9999999)
                        temp = 9999999;
                    break;               
                default:
                    break;
            }
            attribute = temp.ToString();
            return true;
        }
        return false;
    }       

    public void trySet(string tid, string strengthenLevel, string refineLevel, SuperEquipInfo superEquipInfo)
    {
        int mtid = 0;
        if (!int.TryParse(tid, out mtid))
        {
            DataCenter.ErrorTipsLabelMessage("神器或装备tid错误");
            error = true;
            return;
        }
        if (DataCenter.mRoleEquipConfig.GetRecord(mtid) == null)
        {
            DataCenter.ErrorTipsLabelMessage("神器或装备tid不存在");
            error = true;
            return;
        }
        superEquipInfo.tid = mtid;
        if (correct(ref strengthenLevel, InputKind.STRENGTH))
        {
            superEquipInfo.strengthenLevel = int.Parse(strengthenLevel);
        }
        if (correct(ref refineLevel, InputKind.REFINE))
        {
            superEquipInfo.refineLevel = int.Parse(refineLevel);
        }
    }

    public void trySet(string tid, string level, string breakLevel, SuperPetInfo superPetInfo)
    {
        int mtid = 0;
        if (!int.TryParse(tid, out mtid))
        {
            DataCenter.ErrorTipsLabelMessage("主角或宠物tid错误");
            error = true;
            return;
        }
        if (DataCenter.mActiveConfigTable.GetRecord(mtid) == null)
        {
            DataCenter.ErrorTipsLabelMessage("主角或宠物tid不存在");
            error = true;
            return;
        }
        superPetInfo.tid = mtid;
        if (correct(ref level, InputKind.LEVEL))
        {
            superPetInfo.level = int.Parse(level);
        }
        if (correct(ref breakLevel, InputKind.BREAK_LEVEL))
        {
            superPetInfo.breakLevel = int.Parse(breakLevel);
        }
    }

    public void trySet(string name, string gold, string diamond, string soulPoint, string reputation, string prestige, string battleAchv, string unionContr, SuperPlayerInfo superPlayerInfo)
    {
        if (name != null)
        {
            if (name.Length <= 0)
            {
                DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_CREATE_ROLE_NAME_CANOT_EMPTY);
                error = true;
                return;
            }
            else if (name.Length > 8)
            {
                DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_CREATE_ROLE_NAME_TOO_LONG);
                error = true;
                return;
            }
            else
            {
                Regex pattern = new Regex(@"[^\u4e00-\u9fa5\w\d]");
                Regex reg2 = new Regex(@"[0-9]");
                if (pattern.IsMatch(name))
                {
                    DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_CREATE_ROLE_NAME_NOT_TEXT);
                    error = true;
                    return;
                }
                else if (reg2.Replace(name, "").Length == 0)
                {
                    DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_CREATE_ROLE_NAME_ALL_NUMBER);
                    error = true;
                    return;
                }
            }
            superPlayerInfo.name = name;
        }
        if (correct(ref gold, InputKind.COMMON_MONEY))
        {
            superPlayerInfo.gold = int.Parse(gold);
        }
        if (correct(ref diamond, InputKind.COMMON_MONEY))
        {
            superPlayerInfo.diamond = int.Parse(diamond);
        }
        if (correct(ref soulPoint, InputKind.STRANGE_MONEY))
        {
            superPlayerInfo.soulPoint = int.Parse(soulPoint);
        }
        if (correct(ref reputation, InputKind.STRANGE_MONEY))
        {
            superPlayerInfo.reputation = int.Parse(reputation);
        }
        if (correct(ref prestige, InputKind.STRANGE_MONEY))
        {
            superPlayerInfo.prestige = int.Parse(prestige);
        }
        if (correct(ref battleAchv, InputKind.STRANGE_MONEY))
        {
            superPlayerInfo.battleAchv = int.Parse(battleAchv);
        }
        if (correct(ref unionContr, InputKind.STRANGE_MONEY))
        {
            superPlayerInfo.unionContr = int.Parse(unionContr);
        }       
    }

    public void getStageId(out int stageId)
    {
        stageId = 0;
        string page=getInput("main", "page");
        string stage = getInput("main", "stage");
        if (!string.IsNullOrEmpty(page) && !string.IsNullOrEmpty(stage))
        {
            int mpage;
            int mstage;
            int mstageId;
            if (int.TryParse(page, out mpage) && int.TryParse(stage, out mstage))
            {
                mstageId = 20000 + (mpage - 1) * 10 + mstage;
                if (DataCenter.mStageTable.GetRecord(mstageId) != null)
                {
                    stageId = mstageId;
                }
                else
                {
                    stageId = maxStageId;
                }
            }
            else
            {
                DataCenter.ErrorTipsLabelMessage("章节号填写格式错误");
                error = true;
                return;
            }
        }
    }

    public void getSuperPlayerInfo(out SuperPlayerInfo superPlayerInfo)
    {
        superPlayerInfo = new SuperPlayerInfo();
        string name=getInput("main", "name");
        string gold = getInput("main", "gold"); /* 硬币 */
        string diamond = getInput("main", "diamond"); /* 元宝 */
        string soulPoint = getInput("main", "soul"); /* 符魂 */
        string reputation = getInput("main", "reputation"); /* 声望 */
        string prestige = getInput("main", "linghe"); /* 威名 */
        string battleAchv = getInput("main", "molin"); /* 战功 */
        string unionContr = getInput("main", "contribute");/* 公会贡献 */
        trySet(name, gold, diamond, soulPoint, reputation, prestige, battleAchv, unionContr, superPlayerInfo);
    }

    public void getSuperPetInfo(out List<SuperPetInfo> superPetInfo)
    {
        superPetInfo = new List<SuperPetInfo>();
        for (int i = 0; i < 4; i++)
        {
            SuperPetInfo temp = new SuperPetInfo();
            temp.teamPos = i;
            string tid;
            string textBeforeConvert;
            string level;
            string breakLevel;
            string[] skillLevel=new string[4];
            if (i == 0)
            {
                textBeforeConvert = getInput("main", "tid");
                tid = convertTextToTid(textBeforeConvert);
                if (tid == null)
                    tid = 50001.ToString();
                level = getInput("main", "level");
                breakLevel = getInput("main", "breakLevel");
            }
            else
            {
                tid = getInput("pet" + i.ToString(), "name");
                if(string.IsNullOrEmpty(tid))
                    continue;
                level = getInput("pet" + i.ToString(), "level");
                breakLevel = getInput("pet" + i.ToString(), "breakLevel");
                skillLevel[0] = getInput("pet" + i.ToString(), "skill", "skill1");
                skillLevel[1] = getInput("pet" + i.ToString(), "skill", "skill2");
                skillLevel[2] = getInput("pet" + i.ToString(), "skill", "skill3");
                skillLevel[3] = getInput("pet" + i.ToString(), "skill", "skill4");
            }            
            if (skillLevel != null)
            {
                int index=0;
                foreach (string mtemp in skillLevel)
                {
                    int mlevel = 1;
                    int.TryParse(mtemp, out mlevel);
                    if (mlevel < 1)
                        mlevel = 1;
                    if (mlevel>skillMaxLevel)
                        mlevel = skillMaxLevel;
                    temp.skillLevel[index] = mlevel;                   
                    index++;
                }
            }
            trySet(tid, level, breakLevel, temp);           
            superPetInfo.Add(temp);
         }
    }

    public void getSuperEquipInfo(out List<SuperEquipInfo> superEquipInfo)
    {
        superEquipInfo = new List<SuperEquipInfo>();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                SuperEquipInfo temp = new SuperEquipInfo();
                temp.teamPos = i;
                string tid;
                string strengthenLevel;
                string refineLevel;
                if (j < 4)
                {
                    if (i == 0)
                    {
                        tid = getInput("main", "e" + (j + 1).ToString(), "tid");
                        if (string.IsNullOrEmpty(tid))
                            continue;
                        strengthenLevel = getInput("main", "e" + (j + 1).ToString(), "qianghua");
                        refineLevel = getInput("main", "e" + (j + 1).ToString(), "jinglian");
                    }
                    else
                    {
                        tid = getInput("pet" + i.ToString(), "e" + (j + 1).ToString(), "tid");
                        if (string.IsNullOrEmpty(tid))
                            continue;
                        strengthenLevel = getInput("pet" +i.ToString(), "e" + (j + 1).ToString(), "qianghua");
                        refineLevel = getInput("pet" + i.ToString(), "e" + (j + 1).ToString(), "jinglian");
                    }                   
                }
                else
                {
                    if (i == 0)
                    {
                        tid = getInput("main", "m" + (j-3).ToString(), "tid");
                        if (string.IsNullOrEmpty(tid))
                            continue;
                        strengthenLevel = getInput("main", "m" + (j - 3).ToString(), "qianghua");
                        refineLevel = getInput("main", "m" + (j - 3).ToString(), "jinglian");
                    }
                    else
                    {
                        tid = getInput("pet" +i.ToString(), "m" + (j - 3).ToString(), "tid");
                        if (string.IsNullOrEmpty(tid))
                            continue;
                        strengthenLevel = getInput("pet" + i.ToString(), "m" + (j - 3).ToString(), "qianghua");
                        refineLevel = getInput("pet" + i.ToString(), "m" + (j - 3).ToString(), "jinglian");
                    }       
                }
                trySet(tid, strengthenLevel, refineLevel,temp); 
                superEquipInfo.Add(temp);
            }
        }
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if (keyIndex == "EQUIP_CSV")
        {
            SetVisible("equip_csv", (bool)objVal);
            SetVisible("pet_csv", false);
            Button_see_pet_button.P_isOpen = false;
        }
        else if (keyIndex == "PET_CSV")
        {
            SetVisible("pet_csv", (bool)objVal);
            SetVisible("equip_csv", false);
            Button_see_equip_button.E_isOpen = false;
        }
        else if (keyIndex == "MAIN")
        {
            closeAll("main");
        }
        else if (keyIndex == "PET_1")
        {
            closeAll("pet1");
        }
        else if (keyIndex == "PET_2")
        {
            closeAll("pet2");
        }
        else if (keyIndex == "PET_3")
        {
            closeAll("pet3");
        }
        else if (keyIndex == "CREATE")
        {
            SuperPlayerInfo superPlayerInfo;
            List<SuperPetInfo> superPetInfo;
            List<SuperEquipInfo> superEquipInfo;
            int stageId;
            getBaseInfo(out  superPlayerInfo, out superPetInfo, out superEquipInfo, out stageId);
            if(!error)
                NetManager.RequestGmSuperAccount(superPlayerInfo, superPetInfo.ToArray(), superEquipInfo.ToArray(), stageId);
            error = false;
        }
        else if (keyIndex == "CREATE_SUCCESS")
        {
            hasOpen = false;
        }
    }

    public string convertTextToTid(string TextBeforeConvert)
    {
        string text = TextBeforeConvert.Replace("\r", string.Empty).Replace("\n", string.Empty);
        string tid;
        switch (text)
        {
            case "蓝色品质": tid = "50001"; break;
            case "紫色品质": tid = "50002"; break;
            case "橙色品质": tid = "50003"; break;
            default: tid = "50001"; break;
        }
        return tid;
    }

    public string convertTidToText(string tidBeforeConvert)
    {
        string text;
        switch (tidBeforeConvert)
        {
            case "50001": text = "蓝色品质\r"; break;
            case "50002": text = "紫色品质\r"; break;
            case "50003": text = "橙色品质"; break;
            default: text = "蓝色品质"; break;
        }
        return text;
    }

    public void setQuickCreatePopupListData()
    {
        GameObject QuickCreateObj = GameCommon.FindObject(mGameObjUI, "quick_create_button");
      
        if(QuickCreateObj != null)
        {
            UIPopupList popupList = QuickCreateObj.GetComponent<UIPopupList>();
            if (popupList != null)
            {
                popupList.items.Clear();
                popupList.items.Add("--请选择--");

                for (int i = 1; i <= quickCreateCount; i++)
                {
                    popupList.items.Add(TableCommon.GetStringFromDefaultRole(i, "TITLE"));
                }
      
                EventDelegate.Add(popupList.onChange,callSetQuickCreateData);
            }
        }
    }
    //下拉菜单被点击时调用
   public  void callSetQuickCreateData()
    {
       GameObject QuickCreateObj = GameCommon.FindObject(mGameObjUI, "quick_create_button");
       List<string> mlist = new List<string>();

       for (int i = 1; i <= quickCreateCount; i++)
       {
          
           mlist.Add(TableCommon.GetStringFromDefaultRole(i, "TITLE"));
       }
       if (QuickCreateObj != null)
       {
           UIPopupList popupList = QuickCreateObj.GetComponent<UIPopupList>();
           if(popupList != null)
           {
               String value = popupList.value;
               int id = mlist.IndexOf(value)+1;
               setQuickCreateData(id, value);
           }
       }
    }
    //解析配置数据后 调用设置操作
    public void setQuickCreateData(int id, string value)
    {
        string mvalue = value.Replace("\r", string.Empty).Replace("\n", string.Empty);
        if (mvalue == "--请选择--")
        {
            return;
        }
        else
        {
            string[] roleInfo = TableCommon.GetStringFromDefaultRole(id, "ROLE_INFO").Split(new char[] { '|' });
            string[] petInfo1 = TableCommon.GetStringFromDefaultRole(id, "PET_INFO_1").Split(new char[] { '|' });
            string[] petInfo2 = TableCommon.GetStringFromDefaultRole(id, "PET_INFO_2").Split(new char[] { '|' });
            string[] petInfo3 = TableCommon.GetStringFromDefaultRole(id, "PET_INFO_3").Split(new char[] { '|' });

            List<string> superLevelInfo = new List<string>();
            List<string> superEquipInfoRole = new List<string>();
            List<string> superEquipInfoPet1 = new List<string>();
            List<string> superEquipInfoPet2 = new List<string>();
            List<string> superEquipInfoPet3 = new List<string>();
            List<List<string>> superEquipInfo = new List<List<string>>();
            List<string> superPetSkillInfo = new List<string>();

            for (int i = 0; i < 8;i++ )
            {
                if (i == 0)
                {
                    superLevelInfo.Add(roleInfo[i]);
                    superLevelInfo.Add(petInfo1[i]);
                    superLevelInfo.Add(petInfo2[i]);
                    superLevelInfo.Add(petInfo3[i]);
                }
                else if(i < 7) 
                {
                    superEquipInfoRole.Add(roleInfo[i]);
                    superEquipInfoPet1.Add(petInfo1[i]);
                    superEquipInfoPet2.Add(petInfo2[i]);
                    superEquipInfoPet3.Add(petInfo3[i]);
                }
                else if (i == 7)
                {
                    superPetSkillInfo.Add(petInfo1[i]);
                    superPetSkillInfo.Add(petInfo2[i]);
                    superPetSkillInfo.Add(petInfo3[i]);
                } 
            }
            superEquipInfo.Add(superEquipInfoRole);
            superEquipInfo.Add(superEquipInfoPet1);
            superEquipInfo.Add(superEquipInfoPet2);
            superEquipInfo.Add(superEquipInfoPet3);

            setSuperLevelInfo(superLevelInfo);
            setSuperPetSkillInfo(superPetSkillInfo);
            setSuperEquipInfo(superEquipInfo);
        }    
    }
    //设置 等级 突破
    public void setSuperLevelInfo(List<string> superLevelInfo)
    {
        for (int i = 0; i < 4; i++)
        {
            string[] levelInfo = superLevelInfo[i].Split(new char[] { '#' });
            if (levelInfo.Length < 3) continue;
            if (i == 0)
            {
                GameObject chat_input =  mGameObjUI.transform.Find("main/scroll_view/tid/chat_input").gameObject;
                    if(chat_input != null)
                    {
                        UIPopupList popuplist = chat_input.GetComponent<UIPopupList>();
                        if (popuplist != null)
                        {
                            popuplist.value = convertTidToText(levelInfo[0]);
                        }
                    }
                    setInput("main", "level", levelInfo[1]);
                    setInput("main", "breakLevel", levelInfo[2]);
            }
            else
            {
                setInput("pet" + i.ToString(), "name", levelInfo[0]);
                setInput("pet" + i.ToString(), "level", levelInfo[1]);
                setInput("pet" + i.ToString(), "breakLevel", levelInfo[2]);
            }
        }
    }
    //设置 宠物技能
    public void setSuperPetSkillInfo(List<string> superPetSkillInfo)
    {
        for (int i = 0; i < 3; i++)
        {
            string[] petSkillInfo = superPetSkillInfo[i].Split(new char[] { '#' });
            if (petSkillInfo.Length < 4) continue; ;
            setInput("pet" + (i + 1).ToString(), "skill", "skill1", petSkillInfo[0]);
            setInput("pet" + (i + 1).ToString(), "skill", "skill2", petSkillInfo[1]);
            setInput("pet" + (i + 1).ToString(), "skill", "skill3", petSkillInfo[2]);
            setInput("pet" + (i + 1).ToString(), "skill", "skill4", petSkillInfo[3]);
        }
    }
    //设置 装备 法器
    public void setSuperEquipInfo(List<List<string>> superEquipInfo)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                List<string> EquipInfoList = superEquipInfo[i];
                string[] EquipInfo = EquipInfoList[j].Split(new char[] { '#' });
                if (EquipInfo.Length < 3) continue; ;
                if (j < 4)
                {
                    if (i == 0)
                    {
                        setInput("main", "e" + (j + 1).ToString(), "tid", EquipInfo[0]);
                        setInput("main", "e" + (j + 1).ToString(), "qianghua", EquipInfo[1]);
                        setInput("main", "e" + (j + 1).ToString(), "jinglian", EquipInfo[2]);
                    }
                    else
                    {
                        setInput("pet" + i.ToString(), "e" + (j + 1).ToString(), "tid", EquipInfo[0]);
                        setInput("pet" + i.ToString(), "e" + (j + 1).ToString(), "qianghua", EquipInfo[1]);
                        setInput("pet" + i.ToString(), "e" + (j + 1).ToString(), "jinglian", EquipInfo[2]);
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        setInput("main", "m" + (j - 3).ToString(), "tid", EquipInfo[0]);
                        setInput("main", "m" + (j - 3).ToString(), "qianghua", EquipInfo[1]);
                        setInput("main", "m" + (j - 3).ToString(), "jinglian", EquipInfo[2]);
                    }
                    else
                    {
                        setInput("pet" + i.ToString(), "m" + (j - 3).ToString(), "tid", EquipInfo[0]);
                        setInput("pet" + i.ToString(), "m" + (j - 3).ToString(), "qianghua", EquipInfo[1]);
                        setInput("pet" + i.ToString(), "m" + (j - 3).ToString(), "jinglian", EquipInfo[2]);
                    }
                }
            }
        }
    }
    //给input赋值，2层父子关系
    public string setInput(string who, string what,string text)      
    {
        GameObject mwho = GameCommon.FindObject(mGameObjUI, who);
        if (mwho != null)
        {
            GameObject mwhat = GameCommon.FindObject(mwho, what);
            if (mwhat != null)
            {
                UIInput temp = GameCommon.FindObject(mwhat, "chat_input").GetComponent<UIInput>();
                if (temp != null)
                    temp.value = text;
            }
        }
        return null;
    }
    //给input赋值，3层父子关系
    public string setInput(string who, string what, string which,string text)     
    {
        GameObject mwho = GameCommon.FindObject(mGameObjUI, who);
        if (mwho != null)
        {
            GameObject mwhat = GameCommon.FindObject(mwho, what);
            if (mwhat != null)
            {
                GameObject mwhich = GameCommon.FindObject(mwhat, which);
                if (mwhich != null)
                {
                    UIInput temp = GameCommon.FindObject(mwhich, "chat_input").GetComponent<UIInput>();
                    if (temp != null)
                        temp.value = text;
                }
            }
        }
        return null;
    }
}

public class Button_main_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("GM_WINDOW","MAIN",null);
        return true;
    }
}

public class Button_pet1_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("GM_WINDOW", "PET_1", null);
        return true;
    }
}
public class Button_pet2_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("GM_WINDOW", "PET_2", null);
        return true;
    }
}
public class Button_pet3_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("GM_WINDOW", "PET_3", null);
        return true;
    }
}
public class Button_see_equip_button : CEvent
{
    public static bool E_isOpen=false;
    public override bool _DoEvent()
    {
        E_isOpen = !E_isOpen;
        DataCenter.SetData("GM_WINDOW", "EQUIP_CSV", E_isOpen);
        return true;
    }
}
public class Button_see_pet_button : CEvent
{
    public static bool P_isOpen = false;
    public override bool _DoEvent()
    {
        P_isOpen = !P_isOpen;
        DataCenter.SetData("GM_WINDOW", "PET_CSV", P_isOpen);
        return true;
    }
}
public class Button_close_gm_window_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("GM_WINDOW");
        return true;
    }
}
public class Button_send_gm_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("GM_WINDOW", "CREATE", null);
        return true;
    }
}
