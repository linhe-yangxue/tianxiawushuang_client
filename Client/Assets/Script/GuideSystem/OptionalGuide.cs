using UnityEngine;
using System;
using System.Collections.Generic;
using Utilities.Events;
using Utilities.Tasks;


public class OptionalGuide
{
    private static List<GuideQueue> queueList = new List<GuideQueue>();

    public static void ClearGuide()
    {
        foreach (var queue in queueList)
        {
            queue.Clear();
        }

        queueList.Clear();
    }

    public static bool NotifyPlayerLevelUp(int level)
    {
        switch (level)
        {
            case 10:
                StartGuide(new PlayerLevelUpGuideQueue_Lv10());
                return true;

            case 13:
                StartGuide(new PlayerLevelUpGuideQueue_Lv13());
                return true;

            case 15:
                StartGuide(new PlayerLevelUpGuideQueue_Lv15());
                return true;

            case 24:
                StartGuide(new PlayerLevelUpGuideQueue_lv24());
                return true;

            case 25:
                StartGuide(new PlayerLevelUpGuideQueue_lv25());
                return true;
        }

        return false;
    }

    public static void StartGuide(params GuideQueue[] queues)
    {
        ClearGuide();

        foreach (var queue in queues)
        {         
            queueList.Add(queue);
            queue.Start();
        }
    }
}


public class GuideQueue
{
    private TaskQueue<ITask> queue = new TaskQueue<ITask>(100, false);

    public void Accept(ITask task)
    {
        queue.Accept(task);
    }

    public void Start()
    {
        queue.Start();
    }

    public void Clear()
    {
        queue.Clear();
    }
}


public class SubscribeTask : ATask
{
    protected Subscriber sbr { get; private set; }
    protected Observer observer { get; private set; }
    protected string evt { get; private set; }

    public SubscribeTask(Observer observer, string evt)
    {
        this.sbr = new Subscriber();
        this.observer = observer;
        this.evt = evt;
    }

    public override void Execute()
    {
        sbr.Subscribe(observer, evt, OnEvent, EventCallBackMode.Repeat);
    }

    public override void Terminate()
    {
        sbr.CancleAll();
    }

    protected virtual void OnEvent(object arg)
    {
        Finish();
    }

    public void Finish()
    {
        sbr.CancleAll();
        base.Finish();
    }
}


public class OnEventButtonGuide : ATask
{
    private Subscriber sbr = new Subscriber();
    private Observer observer;
    private string evt = "";
    private Func<GameObject> finder;
    private IDecorator decorator;

    public OnEventButtonGuide(Observer observer, string evt, IDecorator decorator, Func<GameObject> finder)
    {
        this.observer = observer;
        this.evt = evt;
        this.decorator = decorator;
        this.finder = finder;        
    }

    public override void Execute()
    {
        OpenGuide();
        sbr.Subscribe(observer, evt, OnEvent, EventCallBackMode.Repeat);
    }

    public override void Terminate()
    {
        CloseGuide();
    }

    public virtual void OnEvent(object arg)
    {
        OpenGuide();
    }

    public virtual void OnClick(object arg)
    {
        CloseGuide();
        Finish();
    }

    protected void OpenGuide()
    {
        GameObject target = finder();

        if (target != null && target.activeInHierarchy)
        {
            decorator.Decorate(target);
            string btnEvt = "Button_" + target.name;
            sbr.Subscribe(OnButtonEventObserver.Instance, btnEvt, OnClick, EventCallBackMode.Repeat);
        }
    }

    protected void CloseGuide()
    {
        decorator.Cancle();
        sbr.CancleAll();
    }
}

public class SecrectSpaceButtonGuide : OnEventButtonGuide
{
    public SecrectSpaceButtonGuide(IDecorator decorator)
        : base(OnWindowOpenObserver.Instance, "ACTIVE_STAGE_WINDOW", decorator, Finder)
    { }

    private static GameObject Finder()
    {
        GameObject win = GameCommon.FindUI("active_stage_window", "class_list", "grid");
        UIButtonEvent[] evts = win.GetComponentsInChildren<UIButtonEvent>();

        foreach (var evt in evts)
        {
            if (evt.mData.get("TAB_KEY") == 102)
            {
                return evt.gameObject;
            }
        }

        return null;
    }

    public override void OnClick(object arg)
    {
        Logic.tEvent evt = arg as Logic.tEvent;

        if (evt != null && evt.get("TAB_KEY") == 102)
        {
            base.OnClick(arg);
        }
    }
}


public class OnWindowOpenButtonGuide : OnEventButtonGuide
{
    public OnWindowOpenButtonGuide(string evt, IDecorator decorator, params string[] path)
        : base(OnWindowOpenObserver.Instance, evt, decorator, () => GameCommon.FindUI(path))
    { }
}

public class OnWindowCloseButtonGuide : OnEventButtonGuide
{
    public OnWindowCloseButtonGuide(string evt, IDecorator decorator, params string[] path)
        : base(OnWindowCloseObserver.Instance, evt, decorator, () => GameCommon.FindUI(path))
    { }
}

public class OnWindowRefreshButtonGuide : OnEventButtonGuide
{
    public OnWindowRefreshButtonGuide(string evt, IDecorator decorator, params string[] path)
        : base(OnWindowRefreshObserver.Instance, evt, decorator, () => GameCommon.FindUI(path))
    { }
}

public class OnButtonEventButtonGuide : OnEventButtonGuide
{
    public OnButtonEventButtonGuide(string evt, IDecorator decorator, params string[] path)
        : base(OnButtonEventObserver.Instance, evt, decorator, () => GameCommon.FindUI(path))
    { }
}


public class MailGuideQueue : GuideQueue
{
    public MailGuideQueue()
    {
        Accept(new SubscribeTask(OnWindowOpenObserver.Instance, "ROLE_SEL_TOP_LEFT_GROUP"));
        Accept(new OnWindowOpenButtonGuide("ROLE_SEL_TOP_LEFT_GROUP", new FingerTipDecorator(new Vector3(0, 30, 0), new Vector3(35, -50, 0), "点击打开邮箱"), "role_sel_top_left_group", "MailBtn"));
        Accept(new OnWindowRefreshButtonGuide("MAIL_WINDOW", new FingerTipDecorator("点击领取奖励"), "mail_window(Clone)", "get_mail_info_cell(Clone)_0", "get_mail_button"));
    }
}


public class PlayerLevelUpGuideQueue_Lv10 : GuideQueue
{
    public PlayerLevelUpGuideQueue_Lv10()
    {
        Accept(new OnWindowOpenButtonGuide("ROLE_SEL_BOTTOM_LEFT_GROUP", new FingerTipDecorator(new Vector3(0, 80, 0), "点击进入主角界面"), "role_sel_bottom_left_group", "CharacterBtn"));
        Accept(new OnWindowOpenButtonGuide("ROLE_INFO_WINDOW", new FingerTipDecorator(new Vector3(0, 70, -100), "点击进入法宝养成"), "all_role_attribute_info_window(Clone)", "RoleEquipCultivateBtn"));
        Accept(new OnWindowOpenButtonGuide("ROLE_EQUIP_CULTIVATE_WINDOW", new FingerTipDecorator(new Vector3(0, -50, 0), "点击升级法宝"), "role_equip_cultivate_window", "RoleEquipStrengthenBtn"));
        Accept(new OnButtonEventButtonGuide("Button_RoleEquipStrengthenBtn", new FingerTipDecorator(new Vector3(10, -50, 0), "选择升级材料"), "RoleEquipWindow", "role_equip_icon_group(Clone)_0", "role_equip_strengthen_icon_btn"));
        Accept(new OnButtonEventButtonGuide("Button_role_equip_strengthen_icon_btn", new FingerTipDecorator(new Vector3(0, 80, 0), "点击升级法宝"), "all_role_attribute_info_window(Clone)", "RoleEquipStrengthenOKBtn"));
    }
}


public class PlayerLevelUpGuideQueue_Lv13 : GuideQueue
{
    public PlayerLevelUpGuideQueue_Lv13()
    {
        Accept(new OnWindowOpenButtonGuide("ROLE_SEL_BOTTOM_RIGHT_GROUP", new FingerTipDecorator(new Vector3(-15, 80, 0), "点击进入封灵"), "role_sel_bottom_right_group", "PVEBtn"));
        Accept(new OnWindowOpenButtonGuide("SCROLL_WORLD_MAP_BOTTOM_RIGHT", new FingerTipDecorator(new Vector3(0, 80, 0), "进入每日副本"), "scroll_world_map_bottom_right_window", "map_active_list"));
        Accept(new OnWindowOpenButtonGuide("ACTIVE_STAGE_WINDOW", new FingerTipDecorator(new Vector3(0, -50, 0), "选择金钱关卡"), "active_stage_window", "cell(Clone)_0", "active_stage_item"));
    }
}


public class PlayerLevelUpGuideQueue_Lv15 : GuideQueue
{
    public PlayerLevelUpGuideQueue_Lv15()
    {
        Accept(new OnWindowOpenButtonGuide("ROLE_SEL_BOTTOM_RIGHT_GROUP", new FingerTipDecorator(new Vector3(0, 80, 0), "点击进入斗法"), "role_sel_bottom_right_group", "PVPBtn"));
        Accept(new OnWindowOpenButtonGuide("PVP_SELECT_ENTER", new FingerTipDecorator(new Vector3(0, -50, 0), "点击进入竞技场"), "pvp_select_enter", "top_battle_button"));
        Accept(new OnWindowOpenButtonGuide("PVP_FOUR_VS_FOUR_ENTER", new FingerTipDecorator(new Vector3(0, 80, 0), "点击开始对决"), "pvp_four_vs_four_enter", "four_vs_four_pk_button"));
    }
}


public class PlayerLevelUpGuideQueue_lv24 : GuideQueue
{
    public PlayerLevelUpGuideQueue_lv24()
    {
        Accept(new OnWindowOpenButtonGuide("ROLE_SEL_BOTTOM_RIGHT_GROUP", new SpotDecorator(new Vector3(20, 20, 0)), "role_sel_bottom_right_group", "PVEBtn"));
        Accept(new OnWindowOpenButtonGuide("SCROLL_WORLD_MAP_BOTTOM_RIGHT", new SpotDecorator(new Vector3(20, 20, 0)), "scroll_world_map_bottom_right_window", "map_active_list"));
        Accept(new SecrectSpaceButtonGuide(new SpotDecorator(new Vector3(100, 30, 0))));
    }
}


public class PlayerLevelUpGuideQueue_lv25 : GuideQueue
{
    public PlayerLevelUpGuideQueue_lv25()
    {
        Accept(new OnWindowOpenButtonGuide("ROLE_SEL_BOTTOM_RIGHT_GROUP", new SpotDecorator(new Vector3(20, 20, 0)), "role_sel_bottom_right_group", "PVPBtn"));
        Accept(new OnWindowOpenButtonGuide("PVP_SELECT_ENTER", new SpotDecorator(new Vector3(100, 30, 0)), "pvp_select_enter", "pet_battle_button"));
    }
}


public class ClearCurrentDifficultyGuideQueue : GuideQueue
{
    public ClearCurrentDifficultyGuideQueue()
    {
        Accept(new OnWindowOpenButtonGuide("SCROLL_WORLD_MAP_BOTTOM_LEFT", new FingerTipDecorator(new Vector3(0, 20, 0), new Vector3(0, 100, 0), "挑战更高难度"), "scroll_world_map_bottom_left_window", "map_difficulty"));
    }
}