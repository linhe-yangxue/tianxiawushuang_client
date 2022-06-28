using UnityEngine;
using System;
using Utilities.Tasks;


public partial class GuideManager
{
    public class Wait : ATask
    {
        private float delay = 0f;

        public Wait(float delay)
        {
            this.delay = delay;
        }

        public override void Execute()
        {
            GuideManager.ExecuteDelayed(Finish, delay);
        }
    }

    public class Act : ATask
    {
        private Action action;

        public Act(params Action[] actions)
        {
            foreach (var act in actions)
            {
                action += act;
            }
        }

        public override void Execute()
        {
            if (action != null)
                action();

            Finish();
        }
    }


    public class Dialog : ATask
    {
        private GuideIndex index = GuideIndex.None;

        public Dialog(GuideIndex index)
        {
            this.index = index;
        }

        public override void Execute()
        {
            if (!OpenDialog(index, Finish))
            {
                SkipGuide();
            }
        }
    }

    public class Tip : ATask
    {
        private float x;
        private float y;
        private string tip;

        public Tip(string tip, float x, float y)
        {
            this.tip = tip;
            this.x = x;
            this.y = y;
        }

        public override void Execute()
        {
            GuideManager.OpenTip(tip, x, y, Finish);
        }
    }

    public class ButtonMask : ATask
    {
        private string[] path;
        private int leftOffset, rightOffset, bottomOffset, topOffset;
        private int fingerOffsetX, fingerOffsetY;

        public ButtonMask(params string[] path)
            : this(0, 0, 0, 0, 0, 20, path)
        { }

        public ButtonMask(int fingerOffsetX, int fingerOffsetY, params string[] path)
            : this(0, 0, 0, 0, fingerOffsetX, fingerOffsetY, path)
        { }

        public ButtonMask(int leftOffset, int rightOffset, int bottomOffset, int topOffset, params string[] path)
            : this(leftOffset, rightOffset, bottomOffset, topOffset, 0, 20, path)
        { }

        public ButtonMask(int leftOffset, int rightOffset, int bottomOffset, int topOffset, int fingerOffsetX, int fingerOffsetY, params string[] path)
        {
            this.leftOffset = leftOffset;
            this.rightOffset = rightOffset;
            this.bottomOffset = bottomOffset;
            this.topOffset = topOffset;
            this.fingerOffsetX = fingerOffsetX;
            this.fingerOffsetY = fingerOffsetY;
            this.path = path;
        }

        public override void Execute()
        {
            GameObject btn = GameCommon.FindUI(path);

            if (btn != null && btn.activeInHierarchy)
            {
                OpenButtonMask(btn, Finish, leftOffset, rightOffset, bottomOffset, topOffset, fingerOffsetX, fingerOffsetY);
            }
            else
            {
                SkipGuide();
            }
        }
    }

    public class Mask : ATask
    {
        private Vector2 fingerOffset;
        private string[] path;

        public Mask(Vector2 fingerOffset, params string[] path)
        {
            this.fingerOffset = fingerOffset;
            this.path = path;
        }

        public override void Execute()
        {
            GameObject btn = GameCommon.FindUI(path);

            if (btn != null && btn.activeInHierarchy)
            {
                OpenMask(btn, Finish);
                ShowMaskFinger(1f, fingerOffset);
            }
            else
            {
                SkipGuide();
            }
        }
    }

    public class MaskRelative : ATask 
    {
        private Rect region;

        public MaskRelative(Rect region)
        {
            this.region = region;
        }

        public override void Execute()
        {
            OpenMaskRelative(region, Finish, true);
            ShowMaskFinger(1f);
        }
    }

    public class MaskInWorldSpace : ATask
    {
        private GameObject target;
        private float width;
        private float height;
        private Vector2 fingerOffset;

        public MaskInWorldSpace(GameObject target, float width, float height, Vector2 fingerOffset)
        {
            this.target = target;
            this.width = width;
            this.height = height;
            this.fingerOffset = fingerOffset;
        }

        public override void Execute()
        {
            if (target != null)
            {
                OpenMaskInWorldSpace(target.transform.position, width, height, Finish);
                ShowMaskFinger(1f, fingerOffset);
            }
            else
            {
                SkipGuide();
            }         
        }
    }

    public class SaveProcess : Act 
    {
        public SaveProcess(GuideIndex index)
            : base(() => SaveGuideProcess(index))
        { }
    }

    public class WaitWithMask : Wait
    {
        public WaitWithMask(float delay)
            : base(delay)
        { }

        public override void Execute()
        {
            base.Execute();
            OpenMaskWithoutOperateRegion();
        }
    }

    public class Trigger : ATask
    {
        private Func<bool> condition;
        private TM_UpdateEvent evt;

        public Trigger(Func<bool> condition)
        {
            this.condition = condition;
        }

        public override void Execute()
        {
            evt = CreateTriggerEvent(condition, Finish);

            if (evt == null)
            {
                SkipGuide();
            }
        }

        public override void Terminate()
        {
            if (evt != null)
            {
                evt.Finish();
            }
        }
    }

    public class MonsterTrigger : ATask
    {
        private float range;

        public MonsterTrigger(float range)
        {
            this.range = range;
        }

        public override void Execute()
        {
            CreateMonsterTrigger(range, x => x.GetObjectType() == OBJECT_TYPE.MONSTER, Finish);
        }
    }

    public class BossTrigger : ATask
    {
        private float range;

        public BossTrigger(float range)
        {
            this.range = range;
        }

        public override void Execute()
        {
            CreateMonsterTrigger(range, x => x.GetObjectType() == OBJECT_TYPE.MONSTER_BOSS, Finish);
        }
    }
}