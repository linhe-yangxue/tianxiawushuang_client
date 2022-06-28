using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GridItem数据
/// </summary>
public class GridCell
{
    private bool mIsNeedRefresh = true;     //是否需要刷新
    private GameObject mGO;                 //对应Cell的GameObject
    private bool mNeedCalcRelativeBounds = true;   //是否需要计算相对边框
    private Bounds mRelativeBounds;         //对应Cell相对于Panel的边框

    public bool IsNeedRefresh
    {
        set { mIsNeedRefresh = value; }
        get { return mIsNeedRefresh; }
    }
    public GameObject go
    {
        set { mGO = value; }
        get { return mGO; }
    }
    public bool NeedCalcRelativeBounds
    {
        set { mNeedCalcRelativeBounds = value; }
        get { return mNeedCalcRelativeBounds; }
    }
    public Bounds RelativeBounds
    {
        set { mRelativeBounds = value; }
        get { return mRelativeBounds; }
    }
}

/// <summary>
/// 超出Panel范围的类型
/// </summary>
public enum PANEL_OUT_SIDE
{
    INSIDE,
    UNKNOWN,
    LEFT,
    TOP,
    RIGHT,
    BOTTOM
}

/// <summary>
/// 已经优化版本的UIGridContainer
/// 元素的创建采用延迟创建，如果元素缓存不足，每次仅创建最多可显示的
/// 元素个数，之后在Update中处理将不显示的元素隐藏
/// 更新每个元素改为回调函数FuncRefreshItem处理，每次新创建一个元素或需要更新元素时会调用回调函数FuncRefreshItem
/// 如果需要主动更新元素，可以调用RefreshAllCell（刷新全部元素）、RefreshCell（刷新指定元素）
/// </summary>
[ExecuteInEditMode]
public class GridsContainer : MonoBehaviour
{
    public enum Arrangement
    {
        Horizontal,
        Vertical,
    }

    public Arrangement arrangement = Arrangement.Horizontal;
    [HideInInspector]
    [SerializeField]
    private int maxPerLine = -1;
    [ExposeProperty]
    public int MaxPerLine
    {
        get { return maxPerLine; }
        set
        {
            if (maxPerLine == value)
                return;
            maxPerLine = value;
            Reposition();
        }
    }
    [HideInInspector]
    [SerializeField]
    private float cellWidth = 200f;
    [ExposeProperty]
    public float CellWidth
    {
        get { return cellWidth; }
        set
        {
            if (cellWidth == value)
                return;
            cellWidth = (int)value;
            Reposition();
        }
    }

    [HideInInspector]
    [SerializeField]
    private float cellHeight = 200f;
    [ExposeProperty]
    public float CellHeight
    {
        get { return cellHeight; }
        set
        {
            if (cellHeight == value)
                return;
            cellHeight = (int)value;
            Reposition();
        }
    }

    [HideInInspector]
    [SerializeField]
    private bool isSrollEnabled = false;
    [ExposeProperty]
    public bool IsSrollEnabled
    {
        get { return isSrollEnabled; }
        set { isSrollEnabled = value; }
    }

    [HideInInspector]
    [SerializeField]
    private Rect scrollRect;
    [ExposeProperty]
    public UnityEngine.Rect ScrollRect
    {
        get { return scrollRect; }
        set { scrollRect = value; }
    }

    [HideInInspector]
    [SerializeField]
    private UIPanel panel;
    public UIPanel Panel
    {
        set { panel = value; }
        get
        {
            if (panel == null)
            {
                panel = GetComponent<UIPanel>();
                if (panel == null)
                {
                    Transform tmpTrans = transform.parent;
                    while (tmpTrans != null)
                    {
                        panel = tmpTrans.gameObject.GetComponent<UIPanel>();
                        if (panel != null)
                            break;
                        tmpTrans = tmpTrans.transform.parent;
                    }
                }
            }
            return panel;
        }
    }

    private float scrollDetla = 0.0f;

    public bool repositionNow = false;
    public bool sorted = false;
    public bool fastResize = true;

    bool mStarted = false;

    [SerializeField]
    protected bool m_LogPos = false;

    protected Vector2 m_MaxShowCount = new Vector2();   //最大可以显示的列数、行数
    protected int m_FirstShowIndex = -1;                //第一个显示的索引值，-1为未设置
    protected int m_LastShowIndex = -1;                 //最后一个显示的索引值，-1为未设置
    [SerializeField]
    [HideInInspector]
    protected int m_EdgeShowLimitCount = 5;             //边界显示限制个数
    public int EdgeShowLimitCount
    {
        set
        {
            m_EdgeShowLimitCount = Mathf.Max(value, 0);
            if (m_AutoResetMaxCreateCount)
                __ResetMaxCreateCount();
        }
        get { return m_EdgeShowLimitCount; }
    }
    protected Bounds m_TemplateRelativeBounds;          //模板元素本地Bounds
    [SerializeField]
    [HideInInspector]
    protected bool m_NeedDynamicCreateCell = false;     //是否需要动态创建元素
    public bool NeedDynamicCreateCell
    {
        set { m_NeedDynamicCreateCell = value; }
        get { return m_NeedDynamicCreateCell; }
    }
    [SerializeField]
    [HideInInspector]
    protected int m_MaxCreateCountPerTime = 20;         //每次创建最大个数
    public int MaxCreateCountPerTime
    {
        set { m_MaxCreateCountPerTime = Mathf.Max(value, 10); }
        get { return m_MaxCreateCountPerTime; }
    }
    protected int m_NeedCreateNewItemLimit = 6;         //每次需要动态创建新元素的临界值
    [SerializeField]
    [HideInInspector]
    protected bool m_AutoResetMaxCreateCount = true;    //自动动态更改每次创建最大个数
    public bool AutoResetMaxCreateCount
    {
        set { m_AutoResetMaxCreateCount = value; }
        get { return m_AutoResetMaxCreateCount; }
    }
    protected int m_AutoCreateMulti = 1;                //自动动态更改最大个数时创建的倍数

    protected Action<int, GameObject> m_FuncRefreshCell;            //刷新元素数据
    public Action<int, GameObject> FuncRefreshCell
    {
        set { m_FuncRefreshCell = value; }
        get { return m_FuncRefreshCell; }
    }

    void Start()
    {
        mStarted = true;
        Reposition();
        __CalculateMaxShowCount();
        if (m_AutoResetMaxCreateCount)
            __ResetMaxCreateCount();
    }

    void Update()
    {
        if (repositionNow)
        {
            repositionNow = false;
            Reposition();
        }

        if (isSrollEnabled)
        {
            if (!Input.GetAxis("Mouse ScrollWheel").Equals(0.0f))
            {
                scrollDetla -= Input.GetAxis("Mouse ScrollWheel") * 50;
                if (scrollDetla < 0.0f)
                {
                    scrollDetla = 0.0f;
                }

                if (scrollDetla <= scrollRect.height)
                {
                    scrollDetla = 0.0f;
                }

                if (scrollDetla > scrollRect.height && scrollDetla > controlList.Count * (int)cellHeight - scrollRect.height)
                {
                    scrollDetla = controlList.Count * (int)cellHeight - (int)scrollRect.height;
                }



                Reposition();
            }
        }
    }

    void LateUpdate()
    {
        if (repositionNow)
            return;

        if (m_FirstShowIndex == -1 && m_LastShowIndex == -1)
        {
            //此时应该检查是否创建新的Cell
            __CheckAndRecreateCells();
        }
        int tmpOldFirstShowIndex = m_FirstShowIndex;
        int tmpOldLastShowIndex = m_LastShowIndex;
        __UpdateFirstShowIndex();
        __UpdateLastShowIndex();
        if (tmpOldFirstShowIndex != m_FirstShowIndex || tmpOldLastShowIndex != m_LastShowIndex)
        {
            //只有显示索引有改变时，才会执行
            __AdjustFirstAndLastShowIndex();
            __CheckAndRecreateCells();
            __UpdateAllCellVisible();
        }
		__RefreshFirstAndLastCells();

        if (m_LogPos)
        {
            string tmpLog = "";
            UIPanel tmpPanel = Panel;

            //Grids
            controlTemplate.SetActive(true);
            Bounds tmpTemplateRelativeBounds = NGUIMath.CalculateRelativeWidgetBounds(tmpPanel.transform, controlTemplate.transform);
            controlTemplate.SetActive(false);
            tmpLog += (", Temp(" + tmpTemplateRelativeBounds + ")");
            for (int i = 0, count = controlList.Count; i < count; i++)
            {
                GridCell tmpCell = controlList[i];

                Vector3 tmpTopLeft = tmpCell.go.transform.TransformPoint(tmpCell.go.transform.localPosition);
                Vector3 tmpSize = new Vector3(CellWidth, CellHeight, 0.0f);
                //                tmpLog += ("Grid" + i + "(" + tmpTopLeft + ")");

                Bounds tmpRelativeBounds = NGUIMath.CalculateRelativeWidgetBounds(tmpPanel.transform, tmpCell.go.transform);
                Vector3 tmpOffset = tmpPanel.CalculateConstrainOffset(
                    new Vector2(tmpRelativeBounds.min.x, tmpRelativeBounds.min.y),
                    new Vector2(tmpRelativeBounds.max.x, tmpRelativeBounds.max.y));
                tmpLog += (", Grid" + i + "(offset:" + tmpOffset + ", bounds:" + tmpRelativeBounds + ")");//" + __GetCellOutSideOfPanel(i) + ")");
            }

            //Panel
            if (tmpPanel != null)
            {
                Vector3[] tmpPanelWorldCorner = tmpPanel.worldCorners;
                tmpLog += ", World(";
                for (int i = 0, count = tmpPanelWorldCorner.Length; i < count; i++)
                {
                    if (i != 0)
                        tmpLog += ",";
                    tmpLog += (tmpPanelWorldCorner[i].ToString());
                }
                tmpLog += ")";
            }

            DEBUG.Log(tmpLog);
        }
    }

    static public int SortByName(Transform a, Transform b) { return string.Compare(a.name, b.name); }

    /// <summary>
    /// Recalculate the position of all elements within the grid, sorting them alphabetically if necessary.
    /// </summary>

    public void Reposition()
    {
        if (isNotReposWhenStart)
        {
            return;
        }

        if (!mStarted)
        {
            repositionNow = true;
            return;
        }

        Transform myTrans = transform;

        int x = 0;
        int y = 0;

        if (sorted)
        {
            List<Transform> list = new List<Transform>();

            for (int i = 0, count = controlList.Count; i < count; ++i)
            {
                GridCell tmpGridCell = controlList[i];
                tmpGridCell.NeedCalcRelativeBounds = true;
                Transform t = tmpGridCell.go.transform;

                if (t)
                    list.Add(t);
            }
            list.Sort(SortByName);

            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                Transform t = list[i];

                float depth = t.localPosition.z;
//                 t.localPosition = (arrangement == Arrangement.Vertical) ?
//                     new Vector3((int)cellWidth * x, -(int)cellHeight * y, depth) :
//                     new Vector3((int)cellWidth * y, -(int)cellHeight * x, depth);
                t.localPosition = new Vector3((int)cellWidth * x, -(int)cellHeight * y, depth);

                if (++x >= maxPerLine && maxPerLine > 0)
                {
                    x = 0;
                    ++y;
                }
            }
        }
        else
        {
            for (int i = 0, count = controlList.Count; i < count; ++i)
            {
                GridCell tmpGridCell = controlList[i];
                tmpGridCell.NeedCalcRelativeBounds = true;
                Transform t = tmpGridCell.go.transform;

                float depth = t.localPosition.z;
//                 t.localPosition = (arrangement == Arrangement.Vertical) ?
//                     new Vector3((int)cellWidth * x, scrollDetla - (int)cellHeight * y, depth) :
//                     new Vector3((int)cellWidth * y, scrollDetla - (int)cellHeight * x, depth);
                t.localPosition = new Vector3((int)cellWidth * x, scrollDetla - (int)cellHeight * y, depth);

                if (++x >= maxPerLine && maxPerLine > 0)
                {
                    x = 0;
                    ++y;
                }
            }

            LabelPosYReSet(myTrans);
        }
    }

    [HideInInspector]
    public List<GridCell> buffer = new List<GridCell>();

    public List<GridCell> controlList = new List<GridCell>();

    [HideInInspector]
    [SerializeField]
    private GameObject controlTemplate;
    public GameObject ControlTemplate
    {
        set
        {
            bool tmpIsRecalcMaxShowCount = (controlTemplate != value);
            controlTemplate = value;
            if (tmpIsRecalcMaxShowCount)
            {
                __CalculateMaxShowCount();
                if (m_AutoResetMaxCreateCount)
                    __ResetMaxCreateCount();
            }
        }
        get { return controlTemplate; }
    }
    [HideInInspector]
    public UIScrollBar uiScrollBar;

    [HideInInspector]
    [SerializeField]
    private int maxCount = 0;
    [ExposeProperty]
    public int MaxCount
    {
        get { return maxCount; }
        set
        {
            if (maxCount == value) 
            {
                //added by xuke
                HandleTweenPos();
                //end
                return;
            }
            
            maxCount = value;

            if (fastResize)
            {
                FastUpdateCells();
            }
            else
            {
                if (buffer.Count > 0)
                {
                    foreach (var b in buffer)
                    {
                        if (b != null && b.go != null)
                            DestroyImmediate(b.go);
                    }

                    buffer.Clear();
                }

                UpdateCells();
            }
            //added by xuke
            HandleTweenPos();
            //end
        }
    }

    private void HandleTweenPos()
    {
        Transform _parentTrans = this.transform.parent;
        if (_parentTrans == null)
            return;
        GridTransformTweenAnim _gridTween = _parentTrans.GetComponent<GridTransformTweenAnim>();
        if (_gridTween == null)
            return;
        _gridTween.PlayGridTween(this);
    }

    private void RebuildCells()
    {
        if (controlTemplate == null)
            return;

        foreach (GridCell c in controlList)
        {
            if (c == null || c.go == null)
                continue;

            DestroyImmediate(c.go);
        }

        controlList.Clear();

        int rows = maxPerLine == -1 ? maxCount : maxPerLine;
        int cols = maxPerLine == -1 ? 1 : maxCount % maxPerLine == 0 ? maxCount / maxPerLine : (int)(maxCount / maxPerLine) + 1;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (controlList.Count >= maxCount)
                    break;

                GridCell c = new GridCell();
                c.go = UnityEngine.Object.Instantiate(controlTemplate) as GameObject;
                c.go.name += "_" + (i * cols + j).ToString();
                c.go.transform.parent = this.transform;

                c.go.transform.localScale = Vector3.one;

                if (c.go.GetComponent<UILabel>() != null)
                    c.go.transform.localScale = new Vector3(19, 19, 1);

                //c.transform.localPosition = new Vector3(0, 0, c.transform.position.z);

                c.go.transform.localPosition = Vector3.zero;

                LabelPosYReSet(c.go.transform);

                //c.active = true;
                c.go.SetActive(true);

                controlList.Add(c);
            }
        }

        if (uiScrollBar != null)
            uiScrollBar.value = 0;

        repositionNow = true;
    }

    public void UpdateCells()
    {
        if (controlTemplate == null)
            return;

        RebuildCells();
    }

    /// <summary>
    /// 新创建元素时，需要调用回调m_FuncRefreshItem设置元素内容
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private GridCell __AppendCell(int index)
    {
        GridCell c;

        if (buffer.Count > 0)
        {
            c = buffer[buffer.Count - 1];
            buffer.RemoveAt(buffer.Count - 1);

            if (c != null && c.go != null)
            {
                c.NeedCalcRelativeBounds = true;
                controlList.Add(c);
                RefreshCell(index);
                return c;
            }
        }

        c = new GridCell();
        c.go = UnityEngine.Object.Instantiate(controlTemplate) as GameObject;
        c.go.name += "_" + index.ToString();
        c.go.transform.parent = this.transform;
        c.go.transform.localScale = Vector3.one;

        if (c.go.GetComponent<UILabel>() != null)
            c.go.transform.localScale = new Vector3(19, 19, 1);

        c.go.transform.localPosition = Vector3.zero;
        LabelPosYReSet(c.go.transform);
        controlList.Add(c);
        RefreshCell(index);
        return c;
    }

    private bool __RemoveCell()
    {
        if (controlList.Count == 0)
            return false;
        //因为RemoveCell里会减去maxCount，这里先加1
        maxCount += 1;
        return RemoveCell(controlList.Count - 1);
    }

    private void FastUpdateCells()
    {
        if (controlTemplate == null)
            return;

        int diff = MaxCount - controlList.Count;

        if (diff > 0)
        {
            int n = controlList.Count;

            //             for (int i = n; i < n + diff; ++i)
            //             {
            //                 AppendCell(i);
            //             }
            int tmpDiff = diff;
            if (m_NeedDynamicCreateCell)
            {
                //每次都延迟创建
                tmpDiff = Mathf.Min(diff, m_MaxCreateCountPerTime);
            }
            for (int i = n, count = n + tmpDiff; i < count; ++i)
                __AppendCell(i);
        }
        else if (diff < 0)
        {
            for (int i = 0; i < -diff; ++i)
                __RemoveCell();
        }
        else
            return;

        if (uiScrollBar != null)
            uiScrollBar.value = 0;

        repositionNow = true;
    }

    void LabelPosYReSet(Transform argTransform)
    {
        UILabel[] lab = argTransform.GetComponentsInChildren<UILabel>();
        for (int j = 0; j < lab.Length; j++)
        {
            lab[j].transform.localPosition = new Vector3(lab[j].transform.localPosition.x, (int)lab[j].transform.localPosition.y, lab[j].transform.localPosition.z);
        }
    }

    public void SetControlPosUseBounds()
    {
        isNotReposWhenStart = true;
        List<Vector3> boundList = new List<Vector3>();
        float height = 0.0f;
        for (int i = 0; i < maxCount; i++)
        {
            Bounds bunnd = NGUIMath.CalculateRelativeWidgetBounds(controlList[i].go.transform);
            height += bunnd.size.y;
            boundList.Add(bunnd.size);
        }

        float curHeight = 0.0f + height / 2.0f;
        for (int i = 0; i < maxCount; i++)
        {
            controlList[i].go.transform.localPosition = new Vector3(controlList[i].go.transform.localPosition.x, curHeight, controlList[i].go.transform.localPosition.z);
            curHeight -= boundList[i].y;
        }
    }

    private bool isNotReposWhenStart = false;
    public bool IsNotReposWhenStart
    {
        get { return isNotReposWhenStart; }
        set { isNotReposWhenStart = value; }
    }

    /// <summary>
    /// 计算模板边界数据
    /// </summary>
    private void __CalculateTemplateRelativeBounds()
    {
        UIPanel tmpPanel = Panel;
        GameObject tmpControlTemplate = controlTemplate;
        if (tmpPanel == null || controlTemplate == null)
            return;
        tmpControlTemplate.SetActive(true);
        m_TemplateRelativeBounds = NGUIMath.CalculateRelativeWidgetBounds(tmpPanel.transform, tmpControlTemplate.transform);
        tmpControlTemplate.SetActive(false);
    }
    /// <summary>
    /// 计算最多显示个数
    /// </summary>
    private void __CalculateMaxShowCount()
    {
        __CalculateTemplateRelativeBounds();

        UIPanel tmpPanel = Panel;
        if (tmpPanel == null || controlTemplate == null)
            return;
        Vector3[] tmpLocalCorners = tmpPanel.localCorners;
        Vector2 tmpClipSize = new Vector2(
            (tmpLocalCorners[1] - tmpLocalCorners[2]).magnitude,
            (tmpLocalCorners[0] - tmpLocalCorners[1]).magnitude);
        Vector2 tmpShowCount = new Vector2(
            tmpClipSize.x / (m_TemplateRelativeBounds.extents.x * 2.0f),
            tmpClipSize.y / (m_TemplateRelativeBounds.extents.y * 2.0f));
        m_MaxShowCount.x = Mathf.CeilToInt(tmpShowCount.x) + 1;
        m_MaxShowCount.y = Mathf.CeilToInt(tmpShowCount.y) + 1;
        if (arrangement == Arrangement.Vertical)
            m_MaxShowCount.x = Mathf.Max(Mathf.Min((int)m_MaxShowCount.x, maxPerLine), 1.0f);
        else
            m_MaxShowCount.y = Mathf.Max(Mathf.Min((int)m_MaxShowCount.y, maxPerLine), 1.0f);
    }
    /// <summary>
    /// 重置最大创建元素个数
    /// </summary>
    private void __ResetMaxCreateCount()
    {
        int tmpCount = (int)m_MaxShowCount.x * (int)m_MaxShowCount.y + m_EdgeShowLimitCount * 2;
        m_MaxCreateCountPerTime = tmpCount * m_AutoCreateMulti;
    }
    /// <summary>
    /// 更新第一个显示索引
    /// </summary>
    private void __UpdateFirstShowIndex()
    {
        int tmpCount = controlList.Count;
        if (tmpCount <= 0)
        {
            m_FirstShowIndex = -1;
            return;
        }

        PANEL_OUT_SIDE tmpFirstOutSide = __GetCellOutSideOfPanel(m_FirstShowIndex);
        //需要向前偏移的索引数量
        int tmpOffsetIndexCount = 0;
        if (arrangement == Arrangement.Vertical)
            tmpOffsetIndexCount = m_EdgeShowLimitCount * (maxPerLine < 0 ? maxCount : maxPerLine);
        else
            tmpOffsetIndexCount = m_EdgeShowLimitCount;
        switch (tmpFirstOutSide)
        {
            case PANEL_OUT_SIDE.UNKNOWN:
            case PANEL_OUT_SIDE.LEFT:
            case PANEL_OUT_SIDE.TOP:
                {
                    //向后找，更改m_FirstShowIndex
                    for (int i = m_FirstShowIndex; i < tmpCount; i++)
                    {
                        if (__GetCellOutSideOfPanel(i) == PANEL_OUT_SIDE.INSIDE)
                        {
                            m_FirstShowIndex = i - tmpOffsetIndexCount;
                            break;
                        }
                    }
                } break;
            case PANEL_OUT_SIDE.RIGHT:
            case PANEL_OUT_SIDE.BOTTOM:
                {
                    //向前找，更改m_FirstShowIndex
                    for (int i = m_FirstShowIndex; i >= 0; i--)
                    {
                        PANEL_OUT_SIDE tmpSide = __GetCellOutSideOfPanel(i);
                        if (tmpSide == PANEL_OUT_SIDE.TOP ||
                            tmpSide == PANEL_OUT_SIDE.LEFT)
                        {
                            m_FirstShowIndex = i + 1 - tmpOffsetIndexCount;
                            break;
                        }
                    }
                } break;
            case PANEL_OUT_SIDE.INSIDE:
                {
                    //向前找，更改m_FirstShowIndex
                    int i = m_FirstShowIndex;
                    for (; i >= 0; i--)
                    {
                        PANEL_OUT_SIDE tmpSide = __GetCellOutSideOfPanel(i);
                        if (tmpSide == PANEL_OUT_SIDE.TOP ||
                            tmpSide == PANEL_OUT_SIDE.LEFT)
                        {
                            m_FirstShowIndex = i + 1 - tmpOffsetIndexCount;
                            break;
                        }
                    }
                    if (i == -1)
                        m_FirstShowIndex = 0;
                } break;
        }
        m_FirstShowIndex = Mathf.Max(Mathf.Min(m_FirstShowIndex, maxCount - 1), 0);
    }
    /// <summary>
    /// 更新最后一个显示索引
    /// </summary>
    private void __UpdateLastShowIndex()
    {
        int tmpCount = controlList.Count;
        if (tmpCount <= 0)
        {
            m_LastShowIndex = -1;
            return;
        }

        PANEL_OUT_SIDE tmpLastOutSide = __GetCellOutSideOfPanel(m_LastShowIndex);
        //需要向后偏移的索引数量
        int tmpOffsetIndexCount = 0;
        if (arrangement == Arrangement.Vertical)
            tmpOffsetIndexCount = m_EdgeShowLimitCount * (maxPerLine < 0 ? maxCount : maxPerLine);
        else
            tmpOffsetIndexCount = m_EdgeShowLimitCount;
        switch (tmpLastOutSide)
        {
            case PANEL_OUT_SIDE.UNKNOWN:
            case PANEL_OUT_SIDE.LEFT:
            case PANEL_OUT_SIDE.TOP:
                {
                    //向后找，更改m_LastShowIndex
                    for (int i = m_LastShowIndex; i < tmpCount; i++)
                    {
                        PANEL_OUT_SIDE tmpSide = __GetCellOutSideOfPanel(i);
                        if (tmpSide == PANEL_OUT_SIDE.BOTTOM ||
                            tmpSide == PANEL_OUT_SIDE.RIGHT)
                        {
                            m_LastShowIndex = i - 1 + tmpOffsetIndexCount;
                            break;
                        }
                    }
                } break;
            case PANEL_OUT_SIDE.RIGHT:
            case PANEL_OUT_SIDE.BOTTOM:
                {
                    //向前找，更改m_LastShowIndex
                    for (int i = m_LastShowIndex; i >= 0; i--)
                    {
                        if (__GetCellOutSideOfPanel(i) == PANEL_OUT_SIDE.INSIDE)
                        {
                            m_LastShowIndex = i + tmpOffsetIndexCount;
                            break;
                        }
                    }
                } break;
            case PANEL_OUT_SIDE.INSIDE:
                {
                    //向后找，更改m_LastShowIndex
                    int i = m_LastShowIndex;
                    for (; i < tmpCount; i++)
                    {
                        PANEL_OUT_SIDE tmpSide = __GetCellOutSideOfPanel(i);
                        if (tmpSide == PANEL_OUT_SIDE.BOTTOM ||
                            tmpSide == PANEL_OUT_SIDE.RIGHT)
                        {
                            m_LastShowIndex = i - 1 + tmpOffsetIndexCount;
                            break;
                        }
                    }
                    if (i == tmpCount)
                        m_LastShowIndex = tmpCount - 1;
                } break;
        }
        m_LastShowIndex = Mathf.Max(Mathf.Min(m_LastShowIndex, maxCount - 1), 0);
    }
    /// <summary>
    /// 根据最大显示数量调整显示索引
    /// </summary>
    private void __AdjustFirstAndLastShowIndex()
    {
        if (maxCount <= 0)
        {
            m_FirstShowIndex = -1;
            m_LastShowIndex = -1;
            return;
        }
        int tmpSpacingCount = (int)m_MaxShowCount.x * (int)m_MaxShowCount.y - 1;// + m_EdgeShowLimitCount * maxPerLine * 2 - 1;
        if (arrangement == Arrangement.Vertical)
            tmpSpacingCount += (m_EdgeShowLimitCount * (maxPerLine >= 0 ? maxPerLine : (int)m_MaxShowCount.x) * 2);
        else
            tmpSpacingCount += m_EdgeShowLimitCount * 2;
        if (m_FirstShowIndex == 0 || m_LastShowIndex != (maxCount - 1))
            m_LastShowIndex = m_FirstShowIndex + tmpSpacingCount;
        else if (m_LastShowIndex == maxCount - 1)
            m_FirstShowIndex = m_LastShowIndex - tmpSpacingCount;
        m_FirstShowIndex = Mathf.Max(Mathf.Min(m_FirstShowIndex, maxCount - 1), 0);
        m_LastShowIndex = Mathf.Max(Mathf.Min(m_LastShowIndex, maxCount - 1), 0);

        int tmpLastIndex = m_FirstShowIndex + tmpSpacingCount;
        if (tmpLastIndex >= maxCount)
        {
            m_LastShowIndex = Mathf.Max(maxCount - 1, 0);
            m_FirstShowIndex = Mathf.Max(m_LastShowIndex - tmpSpacingCount + 1, 0);
        }
        else
            m_LastShowIndex = Mathf.Max(m_LastShowIndex, tmpLastIndex);
    }
    /// <summary>
    /// 刷新当前First、Last索引之间的元素
    /// </summary>
    private void __RefreshFirstAndLastCells()
    {
        for (int i = m_FirstShowIndex; i <= m_LastShowIndex; i++)
            RefreshCell(i);
    }
    /// <summary>
    /// 更新所有元素的可视状态
    /// </summary>
    private void __UpdateAllCellVisible()
    {
//         for (int i = 0, count = controlList.Count; i < count; i++)
//         {
//             bool tmpVisible = (i >= m_FirstShowIndex && i <= m_LastShowIndex);
//             GridCell tmpCell = controlList[i];
//             if (tmpCell.go.activeSelf != tmpVisible)
//                 tmpCell.go.SetActive(tmpVisible);
//         }

        int tmpCount = controlList.Count;
        if (m_FirstShowIndex >= 0 && m_FirstShowIndex < tmpCount &&
            m_LastShowIndex >= 0 && m_LastShowIndex < tmpCount)
        {
            //将First、Last之间元素显示
            for (int i = m_FirstShowIndex; i <= m_LastShowIndex; i++)
            {
                GridCell tmpCell = controlList[i];
                if (!tmpCell.go.activeSelf)
                    tmpCell.go.SetActive(true);
            }
        }
        if (m_FirstShowIndex != -1 && m_LastShowIndex != -1)
        {
            //从First开始，将First之前元素隐藏，直到找到一个隐藏的元素为止
            for (int i = m_FirstShowIndex - 1; i >= 0; i--)
            {
                GridCell tmpCell = null;
                try
                {
                     tmpCell = controlList[i];
                }
                catch (System.Exception ex)
                {
                    int a;
                    a = 123;
                    DEBUG.Log("GridsContainer error - m_FirstShowIndex : " + m_FirstShowIndex + ", m_LastShowIndex : " + m_LastShowIndex + ", i : " + i + ", controlList.Count : " + controlList.Count + ", maxCount : " + maxCount + ", ex : " + ex);
                }
                if (!tmpCell.go.activeSelf)
                    break;
                tmpCell.go.SetActive(false);
            }
            //从Last开始，将Last之后元素隐藏，直到找到一个隐藏的元素为止
            for (int i = m_LastShowIndex + 1, count = controlList.Count; i < count; i++)
            {
                GridCell tmpCell = controlList[i];
                if (!tmpCell.go.activeSelf)
                    break;
                tmpCell.go.SetActive(false);
            }
        }
        else
        {
            for (int i = 0, count = controlList.Count; i < count; i++)
            {
                if (i >= m_FirstShowIndex && i <= m_LastShowIndex)
                    continue;
                GridCell tmpCell = controlList[i];
                if (tmpCell.go.activeSelf)
                    tmpCell.go.SetActive(false);
            }
        }
    }
    /// <summary>
    /// 检测元素是否可见
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private bool __IsCellVisible(int index)
    {
        if (controlList == null || controlList.Count <= 0)
            return false;
        if (index < 0 || index >= controlList.Count)
            return false;

        GameObject tmpItem = controlList[index].go;
        return tmpItem.activeInHierarchy;
    }
    /// <summary>
    /// 检测元素是否已经完全不在Panel剪裁范围内
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private PANEL_OUT_SIDE __GetCellOutSideOfPanel(int index)
    {
        if (controlList == null || controlList.Count <= 0)
            return PANEL_OUT_SIDE.UNKNOWN;
        if (index < 0 || index >= controlList.Count)
            return PANEL_OUT_SIDE.UNKNOWN;

        GridCell tmpGridCell = controlList[index];
        GameObject tmpItem = tmpGridCell.go;
        UIPanel tmpPanel = Panel;
        if (tmpPanel == null)
            return PANEL_OUT_SIDE.UNKNOWN;
        bool tmpItemActiveSelf = tmpItem.activeSelf;
        tmpItem.SetActive(true);
        if (tmpGridCell.NeedCalcRelativeBounds)
        {
            tmpGridCell.NeedCalcRelativeBounds = false;
            tmpGridCell.RelativeBounds = NGUIMath.CalculateRelativeWidgetBounds(tmpPanel.transform, tmpItem.transform);
        }
        Bounds tmpBounds = tmpGridCell.RelativeBounds;
        tmpItem.SetActive(tmpItemActiveSelf);
        Vector3 tmpOffset = tmpPanel.CalculateConstrainOffset(tmpBounds.min, tmpBounds.max);
        if (arrangement == Arrangement.Vertical && tmpOffset.y != 0.0f)
        {
            if (tmpBounds.extents.y * 2.0f < Mathf.Abs(tmpOffset.y))
            {
                if (tmpOffset.y < 0.0f)
                    return PANEL_OUT_SIDE.TOP;
                else
                    return PANEL_OUT_SIDE.BOTTOM;
            }
        }
        else if (arrangement == Arrangement.Horizontal && tmpOffset.x != 0.0f)
        {
            if (tmpBounds.extents.x * 2.0f < Mathf.Abs(tmpOffset.x))
            {
                if (tmpOffset.x < 0.0f)
                    return PANEL_OUT_SIDE.RIGHT;
                else
                    return PANEL_OUT_SIDE.LEFT;
            }
        }
        return PANEL_OUT_SIDE.INSIDE;
    }
    /// <summary>
    /// 检查并且创建新的元素，当还剩m_NeedCreateNewItemLimit个元素将要显示到最后时，需要重新创建
    /// </summary>
    private void __CheckAndRecreateCells()
    {
        if (!m_NeedDynamicCreateCell)
            return;
        int tmpNowControlCount = controlList.Count;
        if (tmpNowControlCount - (m_LastShowIndex + 1) >= m_NeedCreateNewItemLimit)
            return;
        FastUpdateCells();
    }

    /// <summary>
    /// 移除指定索引处元素
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool RemoveCell(int index)
    {
        if (index < 0 || index >= controlList.Count)
            return false;

        GridCell c = controlList[index];

        if (c != null && c.go != null)
            c.go.SetActive(false);

        buffer.Add(c);
        controlList.RemoveAt(index);
        repositionNow = true;
        maxCount -= 1;
        return true;
    }
    /// <summary>
    /// 刷新指定索引处元素
    /// </summary>
    /// <param name="index"></param>
    public void RefreshCell(int index)
    {
        if (index < 0 || index >= controlList.Count)
            return;

        GridCell tmpCell = controlList[index];
        if (tmpCell != null && tmpCell.IsNeedRefresh)
        {
            if (m_FuncRefreshCell != null)
                m_FuncRefreshCell(index, tmpCell.go);
            tmpCell.IsNeedRefresh = false;
        }
    }
    /// <summary>
    /// 刷新所有元素
    /// </summary>
    public void RefreshAllCell()
    {
        if (controlList.Count <= 0)
            return;

        for (int i = 0, count = controlList.Count; i < count; i++)
        {
            GridCell tmpCell = controlList[i];
            tmpCell.IsNeedRefresh = true;
        }
    }
}
