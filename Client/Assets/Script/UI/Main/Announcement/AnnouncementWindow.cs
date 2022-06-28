using Logic;
using DataTable;


public class AnnouncementWindow : tWindow
{
    private TM_FadeOut evt = null;

    public override void Open(object param)
    {
        base.Open(param);

        tNiceData info = param as tNiceData;

        if (info != null)
            SetText("content", ParseContent(info));
    }

    public override void OnOpen()
    {
        evt = EventCenter.Self.StartEvent("TM_FadeOut") as TM_FadeOut;
        evt.mTarget = GetSub("content");
        evt.mBefore = 1.5f;
        evt.mDuration = 0.5f;
        evt.mAfter = 0.5f;
        evt.onElapsed += x => { Close(); Notification.Next(); };
        evt.DoEvent();
    }

    public override void Close()
    {
        if (evt != null && !evt.GetFinished())
            evt.Finish();
        evt = null;
        base.Close();
    }

    private string ParseContent(tNiceData info)
    {
        int type = info.get("TYPE");
        int param1 = info.get("PARAM1"); // ID
        int param2 = info.get("PARAM2"); // element
        int param3 = info.get("PARAM3"); // star
        string param4 = info.get("PARAM4"); // name

        string content = TableCommon.GetStringFromAnnouncementConfig(type, "ANNOUNCEMENT_TXT");

        switch (type)
        {
            case 1:
                return StrengthenPet(content, param4, param1);
            case 2:
                return GetPet(content, param4, param1);
            case 3:
                return GetEquip(content, param4, param3, param1);
            case 4:
                return StrengthenEquip(content, param4, param3, param1);
            case 5:
                return EncounterBoss(content, param4, param1);
            case 6:
                return param4;
            default:
                return "";
        }
    }

	// 1 强化符灵
    // "{0}经过不懈努力，将{1}星{2}强化到了{3}级"
    private string StrengthenPet(string content, string playerName, int petId)
    {
        string petName = TableCommon.GetStringFromActiveCongfig(petId, "NAME");
        int petStar = TableCommon.GetNumberFromActiveCongfig(petId, "STAR_LEVEL");
        int petStrength = TableCommon.GetNumberFromActiveCongfig(petId, "MAX_GROW_LEVEL");
        return string.Format(content, playerName, petStar, petName, petStrength);
    }

	// 2 获取符灵
    // {0}一不小心获得了{1}星的{2}，快来围观他
    private string GetPet(string content, string playerName, int petId)
    {
        string petName = TableCommon.GetStringFromActiveCongfig(petId, "NAME");
        int petStar = TableCommon.GetNumberFromActiveCongfig(petId, "STAR_LEVEL");
        return string.Format(content, playerName, petStar, petName);
    }

    // 3 获取法宝
    // {0}一不小心获得了{1}星的{2}，快来围观他
    private string GetEquip(string content, string playerName, int equipStar, int equipId)
    {
        string equipName = TableCommon.GetStringFromRoleEquipConfig(equipId, "NAME");
        return string.Format(content, playerName, equipStar, equipName);
    }

    // 4 强化法宝
    // {0}经过不懈努力，将{1}星{2}强化到了{3}级
    private string StrengthenEquip(string content, string playerName, int equipStar, int equipId)
    {
        string equipName = TableCommon.GetStringFromRoleEquipConfig(equipId, "NAME");
        int equipLevel = 5;//TableCommon.GetNumberFromRoleEquipEvolutionConfig(equipStar, "MAX_EVOLUTION");
        return string.Format(content, playerName, equipStar, equipName, equipLevel);
    }

    // 5 激活乱入boss
    // 你的好友{0}遭遇了{1}{2}，快去助他一臂之力
    private string EncounterBoss(string content, string playerName, int bossId)
    {
        string bossName = TableCommon.GetStringFromBossConfig(bossId, "NAME");
        int bossLevel = TableCommon.GetNumberFromBossConfig(bossId, "LV");
        return string.Format(content, playerName, bossLevel, bossName);
    }
}