using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;
using Utilities;

public class UnionPkWinWindow : UnionBase
{
    public GuildBossBattleAccountInfo accountInfo = null;

    public List<ItemDataBase> ilistItemBase = null;

	public override void Init ()
	{
        EventCenter.Self.RegisterEvent("Button_quit_guild_boss_battle", new DefineFactory<Button_quit_guild_boss_battle>());
	}
	
	public override void Open (object param)
	{
		base.Open (param);

        if (param is GuildBossBattleAccountInfo)
        {
            accountInfo = (GuildBossBattleAccountInfo)param;
        }

        initUI();
	}

    public void IsWinShowLastHitTips()
    {
        GetSub("player_damage_info_label").SetActive(accountInfo.isWin ? false : true);
        GetSub("union_pk_win_tips").SetActive(accountInfo.isWin ? true : false);
        string temp = string.Format(TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_LAST_HIT_TIPS), GetGuildBossLastHitContri(GetGuildBossIndex()));
        SetText("union_pk_win_tips", temp);
    }

    public void SetDynamicLable()
    {
        DoCoroutine(SetDamageAndReward());
    }

    private IEnumerator SetDamageAndReward()
    {
        //伤害
        UILabel damage = GetComponent<UILabel>("player_damage_info_label");
        yield return DoCoroutine(UIKIt.PushNumberLabel(damage, 0, accountInfo.demage));

        yield return new WaitForSeconds(1.0f);

        //贡献
        UILabel contriLabel = GetComponent<UILabel>("number_contri_label");
        if (accountInfo.isWin)
        {
            yield return DoCoroutine(UIKIt.PushNumberLabel(contriLabel, 0, GetGuildBossAttackContri(GetGuildBossIndex()) + GetGuildBossLastHitContri(GetGuildBossIndex())));
        }
        else
        {
            yield return DoCoroutine(UIKIt.PushNumberLabel(contriLabel, 0, GetGuildBossAttackContri(GetGuildBossIndex())));
        }

       
    }

    public void initUI()
    {
        //获取表头
        int index = GetGuildBossIndex();

        //名字
        setBossInfo(index);

        //last hit tips
        IsWinShowLastHitTips();
    }

    public void setBossInfo(int index)
    {
        //name icon damage 
        setMainInfo(index);

    }

    public void setMainInfo(int index)
    {
        //name
        SetText("boss_name", GetGuildBossName(index));

        //icon
        GameCommon.SetItemIconNew(mGameObjUI, "boss_icon", GetGuildBossMonsteId(index));

        //damage
        SetText("player_damage_info_label", accountInfo.demage.ToString());
        
        //贡献
        int contri = GetGuildBossAttackContri(index);
        SetText("number_contri_label", contri.ToString());
    }

    public void showModel()
    {
        GameObject uiPoint = GameCommon.FindObject(mGameObjUI, "UIPoint");
        BaseObject model = GameCommon.ShowCharactorModel(uiPoint, 1.6f);

        if (guildBossObject.monsterHealth <= 0)
            model.PlayAnim("cute", false);
        else
            model.PlayAnim("lose");
    }

    public void setCurProgressBar(int index)
    {
        //progress
        int monsterBaseBlood = GetMonsterBaseHp(index);
        if (guildBossObject.monsterHealth <= 0)
        {
            guildBossObject.monsterHealth = 0;
        }
        float percent = guildBossObject.monsterHealth * 1.0f / monsterBaseBlood;
        GameCommon.FindObject(mGameObjUI, "Progress Bar").GetComponent<UISlider>().value = percent;

        string strBase = monsterBaseBlood.ToString();
        string strCurr = guildBossObject.monsterHealth.ToString();
        string numStr = string.Format("{0}/{1}", strCurr, strBase);
        GameCommon.SetUIText(mGameObjUI, "boss_hp", numStr);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "PK_WIN_SHOW_REWARD":
                {
                    //获取表头
                    int index = GetGuildBossIndex();
                    setCurProgressBar(index);

                    //动态显示奖励
                    SetDynamicLable();

                    showModel();

                    //请求回来-显示出奖励来
                    if(objVal is List<ItemDataBase>)
                    {
                        ilistItemBase = (List<ItemDataBase>)objVal;
                        DoCoroutine(ShowItems());
                    }
                }
                break;

            default:
                break;
        }
    }

    public void showReward(IEnumerable<ItemDataBase> itemDataList)
    {
        List<ItemDataBase> listNew = new List<ItemDataBase>();
        GameCommon.MergeItemDataBase((List <ItemDataBase>)itemDataList, out listNew);
    
        var grid = GetUIGridContainer("Grid"); ;
        grid.MaxCount = listNew.Count;
        var gridList = grid.controlList;
        int index = 0;
        foreach (var item in listNew)
        {
            ItemDataBase itemTemp = item;
            if(gridList[index] != null)
            {
                GameObject board = gridList[index];
                refreshBoard(board, itemTemp);
            }
            index++;
        }
    }

    private IEnumerator ShowItems()
    {
        List<ItemDataBase> drops = new List<ItemDataBase>();
        GameCommon.MergeItemDataBase((List<ItemDataBase>)ilistItemBase, out drops);

        UIGridContainer grid = GetSub("Grid").GetComponent<UIGridContainer>();
        //grid.transform.localPosition = new Vector3(166 - 46 * drops.Count, -27, 0);

        ItemDataProvider provider = new ItemDataProvider(drops);
        ItemGrid itemGrid = new ItemGrid(grid);
        itemGrid.Reset();
        itemGrid.Set(provider);

        yield return this.StartCoroutine(ShowItemsInTurn(grid));
    }

    private IEnumerator ShowItemsInTurn(UIGridContainer container)
    {
        foreach (var item in container.controlList)
        {
            item.SetActive(false);
        }

        foreach (var item in container.controlList)
        {
            yield return new WaitForSeconds(0.2f);
            item.SetActive(true);
            container.Reposition();
        }
    }

    public void refreshBoard(GameObject board, ItemDataBase item)
    {
        GameCommon.SetItemIconNew(board, "item_icon", item.tid);
        GameCommon.SetUIText(board, "num_label", item.itemNum.ToString());
		AddButtonAction (GameCommon.FindObject (board, "item_icon"), () => GameCommon.SetAccountItemDetailsWindow (item.tid));
    }

	public override void Close ()
	{
		base.Close ();
		
	}

	public static void CloseAllWindow()
	{

	}
	
	public override bool Refresh(object param)
	{
		base.Refresh (param);
		return true;
	}
}

public class Button_quit_guild_boss_battle : CEvent
{
    public override bool _DoEvent()
    {
        MainProcess.OpenUnionPkPrepareWindow();

        return true;
    }
}
