//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright ?2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// All children added to the game object with this script will be repositioned to be on a grid of specified dimensions.
/// If you want the cells to automatically set their scale based on the dimensions of their content, take a look at UITable.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/GridContainer")]
public class UIGridContainer : MonoBehaviour
{
    public enum Arrangement
    {
        Horizontal,
        Vertical,
    }

    public Arrangement arrangement = Arrangement.Horizontal;
    [HideInInspector][SerializeField]private int maxPerLine = 0;
    [ExposeProperty]
    public int MaxPerLine
    {
        get { return maxPerLine; }
        set
        {
            if(maxPerLine==value)
                return;
            maxPerLine = value;
            Reposition();
        }
    }
    [HideInInspector][SerializeField]private float cellWidth = 200f;
    [ExposeProperty]
    public float CellWidth
    {
        get { return cellWidth; }
        set
        {
            if(cellWidth==value)
                return;
            cellWidth = (int)value;
            Reposition();
        }
    }
    
    [HideInInspector][SerializeField]private float cellHeight = 200f;
    [ExposeProperty]
    public float CellHeight
    {
        get { return cellHeight; }
        set
        {
           if(cellHeight == value)
               return;
            cellHeight = (int)value;
            Reposition();
        }
    }

    [HideInInspector] [SerializeField] private bool isSrollEnabled = false;
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

    private UIPanel panel;
    [HideInInspector]
    public UIPanel Panel
    {
        get
        {
            if (panel == null)
                panel = GetComponent<UIPanel>();

            return panel;
        }
    }
    

    //by chenliang
    //begin

//    private float scrollDetla = 0.0f;
//-----------
    //让子类访问
    protected float scrollDetla = 0.0f;

    //end

    public bool repositionNow = false;
    public bool sorted = false;
    public bool hideInactive = true;
    public bool fastResize = true;

    //by chenliang
    //begin

//    bool mStarted = false;
//-------------
    //让子类访问
    protected bool mStarted = false;

    //end

    void Start()
    {
        mStarted = true;
        Reposition();
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
        //added by xuke
        OnUpdate();
        //end
    }

    //added by xuke
#region 动态加载相关
    [SerializeField] public bool needDynamicLoad = false;    //> 是否需要动态加载
    [SerializeField] public int unitLoadCount = 0;           //> 每次动态加载的数量
    private float mLoadOffsetY = 100f;                       //> 偏移多少距离后才触发刷新   
    public bool mIsAllLoaded = false;                        //> 是否所有的都加载完了     
    public Action mRequestLoadHandler = null;   

    private bool mIsLoading = false;                         //> 当前是否正在加载
    public bool IsLoading 
    {
        set { mIsLoading = value; }
        get { return mIsLoading; }
    }
    public UIPanel mRestrictPanel = null;
    public UIPanel RestrictPanel 
    {
        set { mRestrictPanel = value; }
        get
        {
            if (mRestrictPanel == null)
            {
                mRestrictPanel = GetComponent<UIPanel>();
                if (mRestrictPanel == null)
                {
                    Transform tmpTrans = transform.parent;
                    while (tmpTrans != null)
                    {
                        mRestrictPanel = tmpTrans.gameObject.GetComponent<UIPanel>();
                        if (panel != null)
                            break;
                        tmpTrans = tmpTrans.transform.parent;
                    }
                }
            }
            return mRestrictPanel;
        }
    }


    /// <summary>
    /// 判断是否达到了刷新的偏移量
    /// </summary>
    /// <returns></returns>
    private bool __CheckReachLoadOffset() 
    {
        if (MaxCount == 0)
            return false;
        if (RestrictPanel == null)
            return false;
        GameObject _obj = controlList[MaxCount - 1];
        if(_obj == null)
            return false;
        Bounds _relativeBounds = NGUIMath.CalculateRelativeWidgetBounds(RestrictPanel.transform, _obj.transform);
        Vector2 _relativePos_1 = new Vector2(_relativeBounds.min.x, _relativeBounds.min.y - mLoadOffsetY);
        Vector3 _offset = RestrictPanel.CalculateConstrainOffset(_relativePos_1, _relativeBounds.max);
        return _offset.y == 0;
    }

    private bool __CheckNeedLoad() 
    {
        return (!mIsAllLoaded && __CheckReachLoadOffset());
    }

    public void RegisterDynamicLoadHandler(System.Action kActoin) 
    {
        if (!needDynamicLoad)
            return;
        mRequestLoadHandler = kActoin;
    }

    private void OnUpdate() 
    {
        if (!needDynamicLoad)
            return;
        if (!IsLoading && __CheckNeedLoad())
        {
            if (mRequestLoadHandler != null) 
            {
                IsLoading = true;
                mRequestLoadHandler();
            }
        }   
    }
#endregion
    //end

    static public int SortByName(Transform a, Transform b) { return string.Compare(a.name, b.name); }

    /// <summary>
    /// Recalculate the position of all elements within the grid, sorting them alphabetically if necessary.
    /// </summary>

    //by chenliang
    //begin

//    public void Reposition()
//----------------
    //让子类能够重载
    public virtual void Reposition()

    //end
    {
        if(isNotReposWhenStart)
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
        //if(!Application.isPlaying)
        //{
        //    controlTemplate.SetActive(true);
        //}
        //else
        //{
        //    controlTemplate.SetActive(false);
        //}


        if (sorted)
        {
            List<Transform> list = new List<Transform>();

            for (int i = 0; i < myTrans.childCount; ++i)
            {
                Transform t = myTrans.GetChild(i);
                //if (controlTemplate == t.gameObject)
                //    continue;
                //if(controlList[0] == t.gameObject)
                //{
                //    if(!Application.isPlaying)
                //    {
                //        t.gameObject.SetActive(false);
                //    }
                //    else
                //    {
                //        t.gameObject.SetActive(true);
                //    }
                //}

                if (t && (!hideInactive || NGUITools.GetActive(t.gameObject))) list.Add(t);
            }
            list.Sort(SortByName);

            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                Transform t = list[i];

                if (!NGUITools.GetActive(t.gameObject) && hideInactive) continue;

                float depth = t.localPosition.z;
                t.localPosition = (arrangement == Arrangement.Horizontal) ?
                    new Vector3((int)cellWidth * x, -(int)cellHeight * y, depth) :
                    new Vector3((int)cellWidth * y, -(int)cellHeight * x, depth);

                if (++x >= maxPerLine && maxPerLine > 0)
                {
                    x = 0;
                    ++y;
                }
            }
        }
        else
        {
            for (int i = 0; i < myTrans.childCount; ++i)
            {
                Transform t = myTrans.GetChild(i);

                if (!NGUITools.GetActive(t.gameObject) && hideInactive) continue;

                float depth = t.localPosition.z;
                t.localPosition = (arrangement == Arrangement.Horizontal) ?
                    new Vector3((int)cellWidth * x, scrollDetla - (int)cellHeight * y, depth) :
                    new Vector3((int)cellWidth * y, scrollDetla - (int)cellHeight * x, depth);

                if (++x >= maxPerLine && maxPerLine > 0)
                {
                    x = 0;
                    ++y;
                }
            }

            LabelPosYReSet(myTrans);
        }

        //UIDraggablePanel drag = NGUITools.FindInParents<UIDraggablePanel>(gameObject);
        //if (drag != null) drag.UpdateScrollbars(true);
    }

    [HideInInspector]
    public List<GameObject> buffer = new List<GameObject>();

    public List<GameObject> controlList = new List<GameObject>();
    //private int lastControlListCount = 0;
    //public List<GameObject> ControlList
    //{
    //    get { return controlList; }
    //    set
    //    {
    //        if (lastControlListCount == controlList.Count)
    //            return;
    //        List<GameObject> goList = new List<GameObject>();
    //        foreach (Transform child in transform)
    //        {
    //            if(child.parent==transform&&child.name.Contains("Clone"))
    //            {
    //                goList.Add(child.gameObject);
                    
    //            }
    //        }

    //        foreach (GameObject go in goList)
    //        {
    //            DestroyImmediate(go);
    //        }
    //        goList.Clear();
    //        lastControlListCount = value.Count;
    //        controlList.Clear();
    //        if (lastControlListCount > 0)
    //            controlList.Add(controlTemplate);
            
    //        for (int i = 0; i < lastControlListCount - 1; i++)
    //        {
    //            if (controlTemplate == null)
    //                return;
    //            GameObject go = UnityEngine.Object.Instantiate(controlTemplate) as GameObject;
    //            go.transform.parent = transform;
    //            go.transform.localPosition = Vector3.zero;
    //            go.transform.localRotation = Quaternion.identity;
    //            go.transform.localScale = Vector3.one;
    //            controlList.Add(go);
    //        }
    //        Reposition();
    //    }
    //}

    public GameObject controlTemplate;
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
            //added by xuke
            IsLoading = false;
            //end
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
                        if(b != null)
                            DestroyImmediate(b);
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
        if (this == null)
            return;
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

        foreach (GameObject c in controlList)
        {
            if (c == null)
            {
                continue;
            }

            //if(Application.isPlaying)
              //  Destroy(c);
            //else
				DestroyImmediate(c);
        }

        controlList.Clear();

        int rows = maxPerLine == 0 ? maxCount : maxPerLine;
        int cols = maxPerLine == 0 ? 1 : maxCount % maxPerLine == 0 ? maxCount / maxPerLine : (int)(maxCount / maxPerLine) + 1;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (controlList.Count >= maxCount)
                {
                    break;
                }

                GameObject c = UnityEngine.Object.Instantiate(controlTemplate) as GameObject;
                c.name += "_" + (i * cols + j).ToString();
                c.transform.parent = this.transform;

                c.transform.localScale = Vector3.one;

                if (c.GetComponent<UILabel>() != null)
                {
                    c.transform.localScale = new Vector3(19, 19, 1);
                }

                //c.transform.localPosition = new Vector3(0, 0, c.transform.position.z);

                c.transform.localPosition = Vector3.zero;

                LabelPosYReSet(c.transform);

                //c.active = true;
                c.SetActive(true);

                controlList.Add(c);
            }
        }

        if (uiScrollBar != null)
            uiScrollBar.scrollValue = 0;

        repositionNow = true;
    }

    public void UpdateCells()
    {
        if (controlTemplate == null)
            return;

        RebuildCells();
    }

    private GameObject AppendCell(int index)
    {
        GameObject c;

        if (buffer.Count > 0)
        {
            c = buffer[buffer.Count - 1];
            buffer.RemoveAt(buffer.Count - 1);

            if (c != null)
            {
                c.SetActive(true);
                controlList.Add(c);                
                return c;
            }
        }

        c = UnityEngine.Object.Instantiate(controlTemplate) as GameObject;
        c.name += "_" + index.ToString();
        c.transform.parent = this.transform;
        c.transform.localScale = Vector3.one;

        if (c.GetComponent<UILabel>() != null)
        {
            c.transform.localScale = new Vector3(19, 19, 1);
        }

        c.transform.localPosition = Vector3.zero;
        LabelPosYReSet(c.transform);
        c.SetActive(true);
        controlList.Add(c);
        return c;
    }

    private bool RemoveCell()
    {
        if (controlList.Count == 0)
            return false;

        GameObject c = controlList[controlList.Count - 1];

        if (c != null)
        {
            c.SetActive(false);
        }

        buffer.Add(c);
        controlList.RemoveAt(controlList.Count - 1);
        return true;
    }

    private void FastUpdateCells()
    {
        if (controlTemplate == null)
            return;

        int diff = MaxCount - controlList.Count;

        if (diff > 0)
        {
            int n = controlList.Count;

            for (int i = n; i < n + diff; ++i)
            {
                AppendCell(i);
            }
        }
        else if(diff < 0) 
        {
            for (int i = 0; i < -diff; ++i)
            {
                RemoveCell();
            }
        }
        
        if (uiScrollBar != null)
            uiScrollBar.scrollValue = 0;

        repositionNow = true;
    }

    //by chenliang
    //begin

//    void LabelPosYReSet(Transform argTransform)
//---------------
    //让子类访问
    protected void LabelPosYReSet(Transform argTransform)

    //end
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
            Bounds bunnd = NGUIMath.CalculateRelativeWidgetBounds(controlList[i].transform);
            height += bunnd.size.y;
            boundList.Add(bunnd.size);
        }

        float curHeight = 0.0f + height / 2.0f;
        for (int i = 0; i < maxCount; i++)
        {
            controlList[i].transform.localPosition = new Vector3(controlList[i].transform.localPosition.x, curHeight, controlList[i].transform.localPosition.z);
            curHeight -= boundList[i].y;
        }
    }

    private bool isNotReposWhenStart = false;
    public bool IsNotReposWhenStart
    {
        get { return isNotReposWhenStart; }
        set { isNotReposWhenStart = value; }
    }
}