using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// This script makes it possible for a scroll view to wrap its content, creating endless scroll views.
/// Usage: simply attach this script underneath your scroll view where you would normally place a UIGrid:
/// 
/// + Scroll View
/// |- UIWrappedContent
/// |-- Item 1
/// |-- Item 2
/// |-- Item 3
/// </summary>

[ExecuteInEditMode]
public class UIWrapGrid : MonoBehaviour
{
    public delegate void OnInitializeItem(GameObject go, int wrapIndex, int realIndex);

    /// <summary>
    /// Width of the child items for positioning purposes.
    /// </summary>
    [SerializeField]
    [HideInInspector]
    protected int mItemWidth = 100;
    [ExposeProperty]
    public int ItemWidth
    {
        set
        {
            if (value <= 0)
                return;
            mItemWidth = value;
        }
        get { return mItemWidth; }
    }
    /// <summary>
    /// 网格元素宽度
    /// </summary>
    protected int _GridWidthCount
    {
        get
        {
            int tmpChildCount = mChildren.Count;
            int tmpWidth = 0;
            if (mHorizontal)
            {
                if (mPerLineCount == 0)
                    tmpWidth = 1;
                else
                {
                    tmpWidth = tmpChildCount / mPerLineCount;
                    tmpWidth += ((tmpChildCount % mPerLineCount == 0) ? 0 : 1);
                }
            }
            else
            {
                if (mPerLineCount == 0)
                    tmpWidth = tmpChildCount;
                else
                    tmpWidth = mPerLineCount;
            }
            return tmpWidth;
        }
    }
    /// <summary>
    /// 完整网格元素宽度
    /// </summary>
    protected int _FullGridWidthCount
    {
        get
        {
            int tmpChildCount = maxIndex - minIndex + 1;
            int tmpWidth = 0;
            if (mHorizontal)
            {
                if (mPerLineCount == 0)
                    tmpWidth = 1;
                else
                {
                    tmpWidth = tmpChildCount / mPerLineCount;
                    tmpWidth += ((tmpChildCount % mPerLineCount == 0) ? 0 : 1);
                }
            }
            else
            {
                if (mPerLineCount == 0)
                    tmpWidth = tmpChildCount;
                else
                    tmpWidth = mPerLineCount;
            }
            return tmpWidth;
        }
    }
    /// <summary>
    /// Height of the child items for positioning purposes.
    /// </summary>
    [SerializeField]
    [HideInInspector]
    protected int mItemHeight = 100;
    [ExposeProperty]
    public int ItemHeight
    {
        set
        {
            if (value <= 0)
                return;
            mItemHeight = value;
        }
        get { return mItemHeight; }
    }
    /// <summary>
    /// 网格元素高度
    /// </summary>
    protected int _GridHeightCount
    {
        get
        {
            if (mIsUnlimitedRange)
                return -1;

            int tmpChildCount = mChildren.Count;
            int tmpHeight = 0;
            if (mHorizontal)
            {
                if (mPerLineCount == 0)
                    tmpHeight = tmpChildCount;
                else
                    tmpHeight = mPerLineCount;
            }
            else
            {
                if (mPerLineCount == 0)
                    tmpHeight = 1;
                else
                {
                    tmpHeight = tmpChildCount / mPerLineCount;
                    tmpHeight += ((tmpChildCount % mPerLineCount == 0) ? 0 : 1);
                }
            }
            return tmpHeight;
        }
    }
    /// <summary>
    /// 完整网格元素高度
    /// </summary>
    protected int _FullGridHeightCount
    {
        get
        {
            if (mIsUnlimitedRange)
                return -1;

            int tmpChildCount = maxIndex - minIndex + 1;
            int tmpHeight = 0;
            if (mHorizontal)
            {
                if (mPerLineCount == 0)
                    tmpHeight = tmpChildCount;
                else
                    tmpHeight = mPerLineCount;
            }
            else
            {
                if (mPerLineCount == 0)
                    tmpHeight = 1;
                else
                {
                    tmpHeight = tmpChildCount / mPerLineCount;
                    tmpHeight += ((tmpChildCount % mPerLineCount == 0) ? 0 : 1);
                }
            }
            return tmpHeight;
        }
    }

    /// <summary>
    /// 元素模板
    /// </summary>
    [SerializeField]
    [HideInInspector]
    private GameObject mItemTemplate;
    [ExposeProperty]
    public GameObject ItemTemplate
    {
        set
        {
            if (mItemTemplate != null &&
                mItemTemplate.transform.parent == mTrans)
                return;
            mItemTemplate = value;
        }
        get { return mItemTemplate; }
    }
    /// <summary>
    /// 元素个数
    /// </summary>
    [SerializeField]
    [HideInInspector]
    private int mItemsCount = 0;
    [ExposeProperty]
    public int ItemsCount
    {
        set
        {
            if (mItemsCount != value)
            {
                mItemsCount = value;
                mIsResetItemsCount = true;
                mIsUpdatePosition = true;
            }
            //如果设置了数量，就刷新界面
            mIsUpdateAllItems = true;
        }
        get { return mItemsCount; }
    }

    /// <summary>
    /// 每行元素个数
    /// </summary>
    [SerializeField]
    [HideInInspector]
    protected int mPerLineCount = 1;
    [ExposeProperty]
    public int PerLineCount
    {
        set
        {
            if (value <= 0)
                return;
            mPerLineCount = value;
            mIsUpdatePosition = true;
            mIsUpdateAllItems = true;
        }
        get { return mPerLineCount; }
    }

    [SerializeField]
    [HideInInspector]
    protected bool mArrangementInverse = false;
    [ExposeProperty]
    public bool ArrangementInverse
    {
        set
        {
            if (Application.isPlaying)
                return;
            mArrangementInverse = value;
        }
        get { return mArrangementInverse; }
    }

    private bool mIsResetItemsCount = false;
    private bool mIsUpdatePosition = false;         //更新griditem位置
    private bool mIsUpdateAllItems = false;
    public bool IsUpdateAllItems
    {
        set { mIsUpdateAllItems = value; }
        get { return mIsUpdateAllItems; }
    }

    /// <summary>
    /// Whether the content will be automatically culled. Enabling this will improve performance in scroll views that contain a lot of items.
    /// </summary>

    public bool cullContent = true;

    /// <summary>
    /// Minimum allowed index for items. If "min" is equal to "max" then there is no limit.
    /// For vertical scroll views indices increment with the Y position (towards top of the screen).
    /// </summary>

    public int minIndex = 0;

    /// <summary>
    /// Maximum allowed index for items. If "min" is equal to "max" then there is no limit.
    /// For vertical scroll views indices increment with the Y position (towards top of the screen).
    /// </summary>

    public int maxIndex = 0;

    /// <summary>
    /// 元素总个数
    /// </summary>
    public int TotalCount
    {
        get
        {
            if (mIsUnlimitedRange || minIndex > maxIndex)
                return -1;
            return (maxIndex - minIndex + 1);
        }
    }

    [SerializeField]
    [HideInInspector]
    protected bool mIsUnlimitedRange = false;
    [ExposeProperty]
    public bool IsUnlimitedRange
    {
        set { mIsUnlimitedRange = value; }
        get { return mIsUnlimitedRange; }
    }

    /// <summary>
    /// Callback that will be called every time an item needs to have its content updated.
    /// The 'wrapIndex' is the index within the child list, and 'realIndex' is the index using position logic.
    /// </summary>

    public OnInitializeItem onInitializeItem;

    protected Transform mTrans;
    protected UIPanel mPanel;
    protected UIScrollView mScroll;
    protected bool mHorizontal = false;
    protected bool mFirstTime = true;
    protected List<Transform> mChildren = new List<Transform>();

    /// <summary>
    /// Initialize everything and register a callback with the UIPanel to be notified when the clipping region moves.
    /// </summary>

    protected virtual void Start()
    {
        SortBasedOnScrollMovement();
        WrapContent();
        if (mScroll != null) mScroll.GetComponent<UIPanel>().onClipMove = OnMove;
        mFirstTime = false;

        //设置元素数量，更新元素显示
        mIsResetItemsCount = true;
        mIsUpdatePosition = true;
        mIsUpdateAllItems = true;
    }

    void Update()
    {
        if (mIsResetItemsCount)
        {
            mIsResetItemsCount = false;
            _ResetItemsCount();
        }

        if (mIsUpdatePosition)
        {
            mIsUpdatePosition = false;
            ResetChildPositions();
            WrapContent();
        }

        if (mIsUpdateAllItems)
        {
            mIsUpdateAllItems = false;
            WrapContent();
            UpdateAllItems(true, 0, RestrictScrollView);
        }
    }

    /// <summary>
    /// Callback triggered by the UIPanel when its clipping region moves (for example when it's being scrolled).
    /// </summary>

    protected virtual void OnMove(UIPanel panel) { WrapContent(); }

    /// <summary>
    /// Immediately reposition all children.
    /// </summary>

    [ContextMenu("Sort Based on Scroll Movement")]
    public void SortBasedOnScrollMovement()
    {
        if (!CacheScrollView()) return;

        // Cache all children and place them in order
        mChildren.Clear();
        for (int i = 0; i < mTrans.childCount; ++i)
            mChildren.Add(mTrans.GetChild(i));

        // Sort the list of children so that they are in order
        if (mHorizontal) mChildren.Sort(UIGrid.SortHorizontal);
        else mChildren.Sort(UIGrid.SortVertical);
        ResetChildPositions();
    }

    /// <summary>
    /// Immediately reposition all children, sorting them alphabetically.
    /// </summary>

    [ContextMenu("Sort Alphabetically")]
    public void SortAlphabetically()
    {
        if (!CacheScrollView()) return;

        // Cache all children and place them in order
        mChildren.Clear();
        for (int i = 0; i < mTrans.childCount; ++i)
            mChildren.Add(mTrans.GetChild(i));

        // Sort the list of children so that they are in order
        mChildren.Sort(UIGrid.SortByName);
        ResetChildPositions();
    }

    /// <summary>
    /// Cache the scroll view and return 'false' if the scroll view is not found.
    /// </summary>

    protected bool CacheScrollView()
    {
        mTrans = transform;
        mPanel = NGUITools.FindInParents<UIPanel>(gameObject);
        mScroll = mPanel.GetComponent<UIScrollView>();
        if (mScroll == null) return false;
        if (mScroll.movement == UIScrollView.Movement.Horizontal) mHorizontal = true;
        else if (mScroll.movement == UIScrollView.Movement.Vertical) mHorizontal = false;
        else return false;
        return true;
    }

    /// <summary>
    /// Helper function that resets the position of all the children.
    /// </summary>

    protected virtual void ResetChildPositions()
    {
        for (int i = 0, imax = mChildren.Count; i < imax; ++i)
        {
            Transform t = mChildren[i];
            int tmpX = mHorizontal ? ((mPerLineCount == 0) ? 0 : (i / mPerLineCount)) : ((mPerLineCount == 0) ? i : (i % mPerLineCount));
            int tmpY = mHorizontal ? ((mPerLineCount == 0) ? i : (i % mPerLineCount)) : ((mPerLineCount == 0) ? 0 : (i / mPerLineCount));
            t.localPosition = new Vector3(tmpX * mItemWidth, (mArrangementInverse ? -1 : 1) * -tmpY * mItemHeight, 0f);
            int tmpRealIndex = _GetRealIndex(t.localPosition);
            if (mIsUnlimitedRange || (minIndex <= tmpRealIndex && tmpRealIndex <= maxIndex))
                UpdateItem(t, i);
        }
    }

    /// <summary>
    /// Wrap all content, repositioning all children as needed.
    /// </summary>

    public void WrapContent()
    {
        Vector3[] corners = mPanel.worldCorners;

        for (int i = 0; i < 4; ++i)
        {
            Vector3 v = corners[i];
            v = mTrans.InverseTransformPoint(v);
            corners[i] = v;
        }

        Vector3 center = Vector3.Lerp(corners[0], corners[2], 0.5f);
        bool allWithinRange = true;

        if (mHorizontal)
        {
            float extents = _GridWidthCount * mItemWidth * 0.5f;
            float ext2 = extents * 2f;
            float min = corners[0].x - mItemWidth;
            float max = corners[2].x + mItemWidth;

            for (int i = 0, imax = mChildren.Count; i < imax; ++i)
            {
                Transform t = mChildren[i];
                float distance = t.localPosition.x - center.x;
                int tmpRealIndex = int.MinValue;

                if (distance < -extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.x += ext2;
                    distance = pos.x - center.x;
                    tmpRealIndex = _GetRealIndex(pos);

                    if (mIsUnlimitedRange || (minIndex <= tmpRealIndex && tmpRealIndex <= maxIndex))
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                    else allWithinRange = false;
                }
                else if (distance > extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.x -= ext2;
                    distance = pos.x - center.x;
                    tmpRealIndex = _GetRealIndex(pos);

                    if (mIsUnlimitedRange || (minIndex <= tmpRealIndex && tmpRealIndex <= maxIndex))
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                    else allWithinRange = false;
                }
                else if (mFirstTime) UpdateItem(t, i);

                if (cullContent)
                {
                    distance += mPanel.clipOffset.x - mTrans.localPosition.x;
                    if (!_IsItemPressed(t.gameObject))//!UICamera.IsPressed(t.gameObject))
                    {
                        bool tmpIsVisible = false;
                        tmpRealIndex = _GetRealIndex(t.localPosition);
                        if (!mIsUnlimitedRange &&
                            minIndex <= maxIndex &&
                            (tmpRealIndex == minIndex || tmpRealIndex == maxIndex ||          //两端元素必须显示
                            tmpRealIndex == Mathf.Min(minIndex + ItemsCount - 1, maxIndex) || tmpRealIndex == Mathf.Max(maxIndex - ItemsCount + 1, minIndex)))        //两端元素开始，grid个数为边界的元素必须显示
                        {
                            tmpIsVisible = true;
                        }
                        else
                        {
                            tmpIsVisible = (distance > min && distance < max);
                            if (tmpIsVisible)
                            {
                                //如果该元素需要显示，判断该元素是否在指定的索引范围内
//                                if (t.localPosition.y > min && t.localPosition.y < max)
                                {
                                    if (minIndex <= maxIndex)
                                        tmpIsVisible &= (minIndex <= tmpRealIndex && tmpRealIndex <= maxIndex);
                                    else
                                        tmpIsVisible = false;
                                }
                            }
                        }
                        NGUITools.SetActive(t.gameObject, tmpIsVisible, false);
                    }
                }
            }
        }
        else
        {
            float extents = _GridHeightCount * mItemHeight * 0.5f;
            float ext2 = extents * 2f;
            float min = corners[0].y - mItemHeight;
            float max = corners[2].y + mItemHeight;

            for (int i = 0, imax = mChildren.Count; i < imax; ++i)
            {
                Transform t = mChildren[i];
                float distance = t.localPosition.y - center.y;
                int tmpRealIndex = int.MinValue;

                if (distance < -extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.y += ext2;
                    distance = pos.y - center.y;
                    tmpRealIndex = _GetRealIndex(pos);

                    if (mIsUnlimitedRange || (minIndex <= tmpRealIndex && tmpRealIndex <= maxIndex))
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                    else allWithinRange = false;
                }
                else if (distance > extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.y -= ext2;
                    distance = pos.y - center.y;
                    tmpRealIndex = _GetRealIndex(pos);

                    if (mIsUnlimitedRange || (minIndex <= tmpRealIndex && tmpRealIndex <= maxIndex))
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                    else allWithinRange = false;
                }
                else if (mFirstTime) UpdateItem(t, i);

                if (cullContent)
                {
                    distance += mPanel.clipOffset.y - mTrans.localPosition.y;
                    if (!_IsItemPressed(t.gameObject))//!UICamera.IsPressed(t.gameObject))
                    {
                        bool tmpIsVisible = false;
                        tmpRealIndex = _GetRealIndex(t.localPosition);
                        if (!mIsUnlimitedRange &&
                            minIndex <= maxIndex &&
                            (tmpRealIndex == minIndex || tmpRealIndex == maxIndex   ||          //两端元素必须显示
                            tmpRealIndex == Mathf.Min(minIndex + ItemsCount - 1, maxIndex) || tmpRealIndex == Mathf.Max(maxIndex - ItemsCount + 1, minIndex)))        //两端元素开始，grid个数为边界的元素必须显示
                        {
                            tmpIsVisible = true;
                        }
                        else
                        {
                            tmpIsVisible = (distance > min && distance < max);
                            if (tmpIsVisible)
                            {
                                //如果该元素需要显示，判断该元素是否在指定的索引范围内
//                                if (t.localPosition.y > min && t.localPosition.y < max)
                                {
                                    if (minIndex <= maxIndex)
                                        tmpIsVisible &= (minIndex <= tmpRealIndex && tmpRealIndex <= maxIndex);
                                    else
                                        tmpIsVisible = false;
                                }
                            }
                        }
                        NGUITools.SetActive(t.gameObject, tmpIsVisible, false);
                    }
                }
            }
        }
        mScroll.restrictWithinPanel = !allWithinRange;
    }

    /// <summary>
    /// 指定元素是否被按下
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected bool _IsItemPressed(GameObject item)
    {
        if (item == null)
            return false;

        Collider[] tmpCollider = item.GetComponentsInChildren<Collider>();
        for (int i = 0, count = tmpCollider.Length; i < count; i++)
        {
            if (UICamera.IsPressed(tmpCollider[i].gameObject))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 是否所有的网格元素都在隐藏状态
    /// </summary>
    /// <returns></returns>
    protected bool _IsAllItemsHiding()
    {
        if (mIsUnlimitedRange ||
            (minIndex > maxIndex))
            return false;

        Vector3[] tmpCorners = mPanel.worldCorners;

        for (int i = 0; i < 4; ++i)
        {
            Vector3 v = tmpCorners[i];
            v = mTrans.InverseTransformPoint(v);
            tmpCorners[i] = v;
        }

        Vector3 tmpMin = tmpCorners[0];// -new Vector3(mItemWidth, mItemHeight);
        Vector3 tmpMax = tmpCorners[2];// +new Vector3(mItemWidth, mItemHeight);
        Rect tmpRect = new Rect(
            tmpMin.x, tmpMin.y,
            tmpMax.x - tmpMin.x, tmpMax.y - tmpMin.y);

        for (int i = 0, count = mChildren.Count; i < count; i++)
        {
            Transform tmpItemTrans = mChildren[i];
            if (tmpItemTrans.gameObject.activeSelf && tmpRect.Contains(tmpItemTrans.localPosition))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Sanity checks.
    /// </summary>

    void OnValidate()
    {
        if (maxIndex < minIndex)
            maxIndex = minIndex;
        if (minIndex > maxIndex)
            maxIndex = minIndex;
    }

    public GameObject GetItemAt(int index)
    {
        if (mChildren == null || mChildren.Count <= 0)
            return null;
        if (index < 0 || index >= mChildren.Count)
            return null;

        return mChildren[index].gameObject;
    }
    public int GetItemRealIndex(GameObject item)
    {
        if (item == null)
            return -1;

        return _GetRealIndex(item.transform.localPosition);
    }

    /// <summary>
    /// 间隔一帧后刷新所有元素，并在刷新后将Grid移动到Panel范围内
    /// </summary>
    public void UpdateAllItems()
    {
        UpdateAllItems(true, 1, RestrictScrollView);
    }
    /// <summary>
    /// 更新所有元素
    /// </summary>
    /// <param name="checkActive">检查每个元素的激活状态</param>
    /// <param name="waitFrames">间隔指定帧数后刷新</param>
    /// <param name="updateCallback">Grid刷新元素结束后</param>
    public void UpdateAllItems(bool checkActive, int waitFrames, Action updateCallback)
    {
        StartCoroutine(_WaitForFrames(waitFrames, () =>
        {
            for (int i = 0, count = mChildren.Count; i < count; i++)
            {
                Transform tmpTrans = mChildren[i];
                int tmpRealIndex = _GetRealIndex(tmpTrans.localPosition);
                bool tmpUpdate =
                    (mIsUnlimitedRange ||
                    (minIndex <= tmpRealIndex && tmpRealIndex <= maxIndex));
                if (checkActive)
                    NGUITools.SetActive(tmpTrans.gameObject, tmpUpdate, false);
                if (tmpUpdate)
                    UpdateItem(mChildren[i], i);
            }

            StartCoroutine(_WaitForFrames(1, () =>
            {
                if (_IsAllItemsHiding())
                {
                    ResetChildPositions();
                    WrapContent();
                }

                StartCoroutine(_WaitForFrames(1, updateCallback));
            }));
        }));
    }
    public void RestrictScrollView()
    {
        UIScrollView tmpSV = GetComponentInParent<UIScrollView>();
        if (tmpSV == null)
            return;

        tmpSV.UpdateScrollbars(true);       //只是为了能重新计算scrollview的bounds
        tmpSV.RestrictWithinBounds(false);
    }

    protected void _ResetItemsCount()
    {
        if (!CacheScrollView())
            return;

        int tmpIgnoreCount = 0;
        if (mItemTemplate != null && mItemTemplate.transform.parent == mTrans)
            tmpIgnoreCount += 1;
        while (mTrans.childCount - tmpIgnoreCount > mItemsCount)
        {
            GameObject tmpGO = mTrans.GetChild(mTrans.childCount - 1).gameObject;
            if (tmpGO == mItemTemplate)
                continue;
            DestroyImmediate(tmpGO);
        }
        if (mItemTemplate != null)
        {
            while (mTrans.childCount - tmpIgnoreCount < mItemsCount)
            {
                GameObject tmpGO = Instantiate(mItemTemplate) as GameObject;
                tmpGO.name += ("_" + mTrans.childCount);
                tmpGO.transform.parent = mTrans;
                tmpGO.transform.localPosition = Vector3.zero;
                tmpGO.transform.localScale = Vector3.one;
                tmpGO.SetActive(false);
            }
        }

        // Cache all children and place them in order
        mChildren.Clear();
        for (int i = 0, count = mTrans.childCount; i < count; ++i)
        {
            GameObject tmpGO = mTrans.GetChild(i).gameObject;
            if (mItemTemplate != tmpGO)
                mChildren.Add(tmpGO.transform);
        }
    }

    /// <summary>
    /// Want to update the content of items as they are scrolled? Override this function.
    /// </summary>

    protected virtual void UpdateItem(Transform item, int index)
    {
        if (item != null && onInitializeItem != null)
        {
            int realIndex = _GetRealIndex(item.localPosition);
            onInitializeItem(item.gameObject, index, realIndex);
        }
    }

    /// <summary>
    /// 获取指定元素的真实索引
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected int _GetRealIndex(Vector3 localPosition)
    {
        int tmpRealIndex = 0;
        bool tmpIsHorizontal = (mScroll.movement == UIScrollView.Movement.Horizontal);
        if (tmpIsHorizontal)
        {
            if (mPerLineCount == 0)
                tmpRealIndex = Mathf.RoundToInt(-localPosition.y / mItemHeight);
            else
            {
                tmpRealIndex = Mathf.RoundToInt(
                    ((mArrangementInverse ? -1 : 1) * localPosition.x / mItemWidth) * mPerLineCount +
                    (-localPosition.y) / mItemHeight);
            }
        }
        else
        {
            if (mPerLineCount == 0)
                tmpRealIndex = Mathf.RoundToInt(localPosition.x / mItemWidth);
            else
            {
                tmpRealIndex = Mathf.RoundToInt(
                    ((mArrangementInverse ? -1 : 1) * (-localPosition.y) / mItemHeight) * mPerLineCount +
                    localPosition.x / mItemWidth);
            }
        }
        return tmpRealIndex;
    }

    /// <summary>
    /// 等待指定帧数
    /// </summary>
    /// <param name="frameCount"></param>
    /// <returns></returns>
    protected System.Collections.IEnumerator _WaitForFrames(int frameCount, Action callback)
    {
        for (int i = 0; i < frameCount; i++)
            yield return null;
        if (callback != null)
            callback();
    }
}
