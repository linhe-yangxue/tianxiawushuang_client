using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;

/// <summary>
/// 历练快捷入口窗口
/// </summary>
public class TrialEasyJumpWindow : tWindow
{
    private List<TrialEasyJumpType> mListEasyJump = new List<TrialEasyJumpType>()
    {
        TrialEasyJumpType.ArenaShopAward,
        TrialEasyJumpType.RammbockShopAward,
        TrialEasyJumpType.RammbockFreeReset,
        TrialEasyJumpType.GrabTreasureCombine,
        TrialEasyJumpType.FairylandCanExplore,
        TrialEasyJumpType.FairylandCanHarvest,
        TrialEasyJumpType.BossNewAppear,
        TrialEasyJumpType.BossActive1,
        TrialEasyJumpType.BossActive2,
        TrialEasyJumpType.BossMeritAward
    };

    public override void Init()
    {
        base.Init();
    }

    public override void Open(object param)
    {
        base.Open(param);

        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        UIAlignGridContainer tmpGridContainer = GameCommon.FindComponent<UIAlignGridContainer>(mGameObjUI, "jump_grids");
        if (tmpGridContainer == null)
            return true;

        List<TrialEasyJumpType> tmpListEasyJump = __GetEasyJumpList();
        int tmpCount = tmpListEasyJump.Count;
        tmpGridContainer.MaxCount = tmpCount;
        for (int i = 0; i < tmpCount; i++)
        {
            GameObject tmpGOItem = tmpGridContainer.controlList[i];
            TrialEasyJumpType tmpEasyJumpType = tmpListEasyJump[i];
            __RefreshItem(tmpGOItem, tmpEasyJumpType);
        }

        return true;
    }

    /// <summary>
    /// 获取当前快捷跳转按钮列表
    /// </summary>
    /// <returns></returns>
    private List<TrialEasyJumpType> __GetEasyJumpList()
    {
        if(mListEasyJump == null)
            return null;

        List<TrialEasyJumpType> tmpListEasyJump = new List<TrialEasyJumpType>();
        for (int i = 0, count = mListEasyJump.Count; i < count; i++)
        {
            if (TrialEasyJumpVisibleHelper.Instance.IsEasyJumpVisible(mListEasyJump[i]))
                tmpListEasyJump.Add(mListEasyJump[i]);
        }
        return tmpListEasyJump;
    }
    /// <summary>
    /// 刷新指定按钮元素
    /// </summary>
    /// <param name="goItem"></param>
    /// <param name="easyJumpType"></param>
    private void __RefreshItem(GameObject goItem, TrialEasyJumpType easyJumpType)
    {
        if (goItem == null)
            return;

        DataRecord tmpRecord = DataCenter.mSufferingTriggerConfig.GetRecord((int)easyJumpType);
        if (tmpRecord == null)
            return;

        //设置图标
        string tmpAtlas = "", tmpSprite = "";
        if (!tmpRecord.get("ICON_ATLAS_NAME", out tmpAtlas) ||
            !tmpRecord.get("ICON_SPRITE_NAME", out tmpSprite))
            return;
        GameObject tmpGOSprite = GameCommon.FindObject(goItem, "Sprite");
        GameCommon.SetSprite(tmpGOSprite, tmpAtlas, tmpSprite);

        //设置回调
        GameCommon.AddButtonAction(tmpGOSprite, () =>
        {
            TrialEasyJumpHelper.Instance.EasyJump(easyJumpType);
        });
    }
}
