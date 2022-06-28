using UnityEngine;
using System.Collections;

/// <summary>
/// 商店红点管理
/// </summary>
public class ShopNewMarkManager
{
    private static ShopNewMarkManager self = null;
    public static ShopNewMarkManager Self
    {
        //set { self = value; }
        get
        {
            if (self == null)
                self = new ShopNewMarkManager();
            return self;
        }
    }
    private ShopNewMarkManager() { }

    #region 检测
    /// <summary>
    /// 当前是否可以免费普通抽卡
    /// </summary>
    public bool CheckNormalFreeDraw { set; get; }
    /// <summary>
    /// 当前是否可以免费高级抽卡
    /// </summary>
    public bool CheckAdvanceFreeDraw { set; get; }
    /// <summary>
    /// 检测是否有银符令
    /// </summary>
    public bool CheckSilverFL { set; get; }
    /// <summary>
    /// 检测是否有10个银符令
    /// </summary>
    public bool CheckTenSilverFL { set; get; }
    /// <summary>
    /// 检测是否有金符令
    /// </summary>
    public bool CheckGoldFL { set; get; }
    /// <summary>
    /// 检测是否有符灵
    /// </summary>
    public bool CheckHasFL 
    {
        get { return CheckSilverFL || CheckGoldFL; }
    }
    /// <summary>
    /// 检测打折购买权限
    /// </summary>
    public bool CheckSalePackage { set; get; }

    #endregion


    #region 可见性
    /// <summary>
    /// 抽卡页签红点是否可见
    /// </summary>
    public bool PetTabVisible { get { return CheckNormalFreeDraw || CheckAdvanceFreeDraw || CheckHasFL; } }
    /// <summary>
    /// 普通抽卡按钮红点是否可见
    /// </summary>
    public bool NormalDrawVisible { get { return CheckNormalFreeDraw || CheckSilverFL; } }
    /// <summary>
    /// 普通十连抽红点是否可见
    /// </summary>
    public bool NormalTenDrawVisible { get { return CheckTenSilverFL; } }
    /// <summary>
    /// 高级抽卡按钮红点是否可见
    /// </summary>
    public bool AdvanceDrawVisible { get { return CheckAdvanceFreeDraw || CheckGoldFL; } }
    /// <summary>
    /// 礼包页签红点是否可见
    /// </summary>
    public bool CharacterSkinTabVisible { get { return CheckSalePackage; } }
    /// <summary>
    /// 商店按钮红点是否可见
    /// </summary>
    public bool ShopBtnVisible { get { return PetTabVisible || CharacterSkinTabVisible; } }
    #endregion

}
