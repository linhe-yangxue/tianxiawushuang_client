using UnityEngine;
using System.Collections.Generic;
using DataTable;
using Logic;


public class TeamRelateEffectWindow : tWindow
{
    private UIGridContainer grid = null;
//    private int currentTotal = 0;
    //public const int RELATE_HEIGHT = 60;

    public override void Open(object param)
    {
        base.Open(param);

        grid = GetComponent<UIGridContainer>("Grid");
        grid.MaxCount = 4;
        grid.Reposition();
        grid.repositionNow = false;
        AppendDelegate<int, HashSet<int>, HashSet<int>>(Relationship.onRelationChanged, OnRelationChanged);
        //SetRelateLocation();
        Refresh(param);
    }

    public override void OnClose()
    {
        grid = null;
    }

    public override bool Refresh(object param)
    {
        //currentTotal = 0;

        for (int i = 0; i < 4; ++i)
        {
            RefreshByTeamPos(i);
        }

        //grid.Reposition();
        return true;
    }

    private void OnRelationChanged(int teamPos, HashSet<int> added, HashSet<int> removed)
    {
        if (teamPos < (int)TEAM_POS.MAX)
        {
            RefreshByTeamPos(teamPos);
        }
    }

    private void RefreshByTeamPos(int teamPos)
    {
        GameObject target = grid.controlList[teamPos];
//        target.transform.localPosition = new Vector3(0f, -66f * currentTotal, 0f);
        ActiveData activeData = TeamManager.GetActiveDataByTeamPos(teamPos);

        if (activeData == null)
        {
            target.SetActive(false);
        }
        else 
        {
            target.SetActive(true);
            List<Relationship> relList = Relationship.GetCachedRelationshipList(teamPos);//Relationship.AllRelationships(teamPos);
            ShowRelateTitle(target, activeData);
            ShowRelateList(target, relList);
            //currentTotal += relList.Count;
        }    
    }
    //void SetRelateLocation()
    //{

    //    for(int i = 0; i < (int)TEAM_POS.MAX; i++)
    //    {
    //        GameObject target = grid.controlList[i];
    //        ActiveData activeData = TeamManager.GetActiveDataByTeamPos(i);
			
    //        if (activeData == null)
    //        {
    //            target.SetActive(false);
    //        }
    //        else 
    //        {
    //            var p = grid.controlList[0].transform.localPosition;

    //            if(i > 0)
    //            {
    //                List<Relationship> relList = Relationship.AllRelationships(i - 1);
    //                UISprite bgSprite = grid.controlList[i - 1].transform.Find ("bg").GetComponent<UISprite>();
    //                UISprite labelBgSprite = grid.controlList[i - 1].transform.Find ("bg/label_bg").GetComponent<UISprite>();
    //                bgSprite.height = 120;
    //                labelBgSprite.height = 80;
    //                bgSprite.height = bgSprite.height + RELATE_HEIGHT * ( relList.Count - 1);
    //                labelBgSprite.height = labelBgSprite.height + RELATE_HEIGHT * (relList.Count - 1);
    //                grid.controlList[i].transform.localPosition = new Vector3(p.x, grid.controlList[i -1].transform.localPosition.y - bgSprite.height - 12.0f, p.z);
    //            }else
    //            {
    //                grid.controlList[i].transform.localPosition = p;
    //            }
    //        }
    //    }
    //}

    private void ShowRelateTitle(GameObject target, ActiveData activeData)
    {
        //if (activeData.teamPos == 0)
        //{
        //    GameCommon.SetUIVisiable(target, "role_name_sprit", true);
        //    GameCommon.SetUIVisiable(target, "pet_name_sprit", false);
        //}
        //else
        //{
        //    GameCommon.SetUIVisiable(target, "role_name_sprit", false);
        //    GameCommon.SetUIVisiable(target, "pet_name_sprit", true);
        //}

        string activeName = TableCommon.GetStringFromActiveCongfig(activeData.tid, "NAME");
        GameCommon.SetUIText(target, "role_or_pet_name", activeName);
    }

    private void ShowRelateList(GameObject target, List<Relationship> relList)
    {
        UIGridContainer container = GameCommon.FindComponent<UIGridContainer>(target, "lucky_chance_group_grid");
        container.CellHeight = 40;
		container.MaxCount = relList.Count;
        container.Reposition();
        container.repositionNow = false;

        //UISprite bgSprite = target.transform.Find ("bg").GetComponent<UISprite>();
        //UISprite labelBgSprite = target.transform.Find ("bg/label_bg").GetComponent<UISprite>();
        //bgSprite.height = 120;
        //labelBgSprite.height = 80;
        //bgSprite.height = bgSprite.height + RELATE_HEIGHT * ( relList.Count - 1);
        //labelBgSprite.height = labelBgSprite.height + RELATE_HEIGHT * (relList.Count - 1);

        for (int i = 0; i < container.MaxCount; ++i)
        {
            ShowRelateItem(container.controlList[i], relList[i]);
        }
    }

    private void ShowRelateItem(GameObject item, Relationship relationship)
    {
        DataRecord relRecord = DataCenter.mRelateConfig.GetRecord(relationship.tid);

        if (relRecord == null)
            return;

        string relName = relRecord["RELATE_NAME"];
        string relDesc = relRecord["RELATE_DWSCRIBE"];
        var nameLab = GameCommon.FindComponent<UILabel>(item, "relate_chance_name");
        var descLab = GameCommon.FindComponent<UILabel>(item, "relate_chance_tips");
        nameLab.text = relName;
        descLab.text = relDesc;

        if (relationship.active)
        {
            //modified by xuke
            nameLab.color = Color.red;
            //descLab.color = Color.red;
            descLab.text = "[ff0000]"+descLab.text+"[-]";
            //end
        }
        else 
        {
            nameLab.color = Color.white;
            descLab.color = Color.white;
        }
    }
}