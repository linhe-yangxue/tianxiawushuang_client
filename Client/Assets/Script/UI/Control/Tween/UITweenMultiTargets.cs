using UnityEngine;
using System.Collections.Generic;

public abstract class UITweenMultiTargets : UITweener
{
    [SerializeField]
    protected List<UIRect> m_Targets = new List<UIRect>();
    [SerializeField]
    protected bool m_AutoTargetChildren = false;              //自动查找所有子集

    void Awake()
    {
        UIRect tmpRect = GetComponent<UIRect>();
        if (tmpRect != null && m_Targets.IndexOf(tmpRect) == -1)
            m_Targets.Add(tmpRect);
        if (m_AutoTargetChildren)
        {
            foreach (UIRect tmpChildRect in GetComponentsInChildren<UIRect>())
            {
                if (m_Targets.IndexOf(tmpChildRect) == -1)
                    m_Targets.Add(tmpChildRect);
            }
        }
    }

    public List<UIRect> Targets
    {
        set { m_Targets = value; }
        get { return m_Targets; }
    }
}
