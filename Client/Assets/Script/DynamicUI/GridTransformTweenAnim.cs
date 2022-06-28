using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Transform_Direction 
{
    To_Left = 0,
    To_Right = 1,
}
public class GridTransformTweenAnim : MonoBehaviour 
{
    [SerializeField]
    public float mDelayStart = 0f;   //> 延迟开始播放时间
    [SerializeField]
    public float mStepDelay = 0f;    //> 每项延迟播放时间
    [SerializeField]
    public float mStepDuration = 1f; //> 每项持续时间
    private List<GameObject> mTweenChildren = new List<GameObject>();
    private List<TweenPosition> mTweenPos = new List<TweenPosition>();
    [SerializeField]
    public AnimationCurve mAnimCurve;
    [SerializeField]
    public Transform_Direction mDirection;
    [SerializeField]
    public Vector3 mOriginPos = Vector3.zero;   //> 动画播放起始位置
    private bool mPlayFlag = true;
    
    [ContextMenu("Set Origin Pos")]
    public void SetOriginPos() 
    {
        mOriginPos = this.transform.localPosition;
    }
	// Use this for initialization
    public void ResetPlayFlag() 
    {
        mPlayFlag = true;
    }
    
    public void PlayGridTween(object kGrid)
    {
        if (!CommonParam.mOpenDynamicUI)
            return;
        UIPanel _panel = null;
        if (!(kGrid != null && kGrid is UIGridContainer || kGrid is GridsContainer))
            return;
        if (!mPlayFlag)
            return;
        mPlayFlag = false;

        transform.localPosition = new Vector3(10000,0,0);
        //mGridContainer.Reposition();
        GlobalModule.DoLater(() =>
        {
            InitScriptInfo(kGrid);
            InitOriginPos();
        }, 0f);
     
        GlobalModule.DoLater(() => 
        {
            //int _noPlayIndex = __GetFirstNotPlayAnimIndex(kGrid);
            for (int i = 0, count = mTweenChildren.Count; i < count; i++)
            {
                if (mTweenPos[i] != null) 
                {
                  // if (i >= _noPlayIndex)
                  //  {
                  //      mTweenPos[i].transform.localPosition = mTweenPos[i].to;
                  //  }
                  //  else 
                  //  {
                        mTweenPos[i].PlayForward();
                  // }
                }                   
            }
        },mDelayStart);

        _panel = GetPanel(kGrid);
        if (_panel != null) 
        {
            GlobalModule.DoLater(() =>
            {
                _panel.SetDirty();
            }, 0);
        }     
    }

    //得到不需要播放tween动画的下标,优化效果
    private int __GetFirstNotPlayAnimIndex(object kGrid)
    {
        if (kGrid == null)
            return int.MaxValue;
        if (kGrid is UIGridContainer)
        {
            UIGridContainer _gridContainer = null;
            _gridContainer = (UIGridContainer)kGrid;
            //得到当前页面最多展示的个数
            UIPanel _panel = __GetRestrictPanel(_gridContainer.gameObject);
            if (_panel == null)
            {
                DEBUG.LogError("No Panel include this grid");
                return int.MaxValue;
            }
            else
            {
                int _maxRow = (int)(_panel.height / _gridContainer.CellHeight) + 1;
                int _maxCount = Mathf.Min(_gridContainer.MaxCount, _maxRow * _gridContainer.MaxPerLine);
                return Mathf.Max(0,_maxCount - 1);
            }
        }
        else if (kGrid is GridsContainer) 
        {
            GridsContainer _gridsContainer = null;
            _gridsContainer = (GridsContainer)kGrid;
            //得到当前页面最多展示的个数
            UIPanel _panel = __GetRestrictPanel(_gridsContainer.gameObject);
            if (_panel == null)
            {
                DEBUG.LogError("No Panel include this grid");
                return int.MaxValue;
            }
            else
            {
                int _maxRow = (int)(_panel.height / _gridsContainer.CellHeight) + 1;
                int _maxCount = Mathf.Min(_gridsContainer.MaxCount, _maxRow * _gridsContainer.MaxPerLine);
                return Mathf.Max(0, _maxCount - 1);
            }
        }
        return int.MaxValue;
    }

    private UIPanel __GetRestrictPanel(GameObject kObj) 
    {
        GameObject _obj = kObj;
        UIPanel _panel = null;
        while (true) 
        {
            if (_obj != null)
            {
                _panel = _obj.GetComponent<UIPanel>();
                if (_panel != null)
                {
                    return _panel;
                }
                else
                {
                    _obj = _obj.transform.parent.gameObject;
                }
            }
            else 
            {
                return null;
            }
        }
    }

    private UIPanel GetPanel(object kGrid)
    {
        if (kGrid is UIGridContainer)
            return ((UIGridContainer)kGrid).Panel;
        else if (kGrid is GridsContainer)
            return ((GridsContainer)kGrid).Panel;
        return null;
    }

    private float mStepDistance = 50f;  //> grid.CellWidth加上这个值就是总的偏移值
    private float mCellWidth = 0f;
    private int mColNum = 0;            //> grid的列数
    private int mMaxIndex = -1;
    private List<Vector3> mToPos = new List<Vector3>();
    private void InitScriptInfo(object kGrid) 
    {
        //mTweenChildren.Clear();
        mTweenPos.Clear();
        if (kGrid is UIGridContainer)
        {
            UIGridContainer _grid = (UIGridContainer)kGrid;
            mTweenChildren = _grid.controlList;
            mCellWidth = _grid.CellWidth;
            mColNum = _grid.MaxPerLine;
        }          
        else if (kGrid is GridsContainer)
        {
            mTweenChildren.Clear();
            GridsContainer _grids = (GridsContainer)kGrid;
            mCellWidth = _grids.CellWidth;
            mColNum = _grids.MaxPerLine;
            for (int i = 0, count = _grids.MaxCount; i < count; i++) 
            {
                mTweenChildren.Add(_grids.controlList[i].go);
            }
        }
        for (int i = 0, count = this.mTweenChildren.Count; i < count; i++)
        {
            mTweenPos.Add(InitCellInfo(i));
        }
    }
    private TweenPosition InitCellInfo(int kCellIndex)
    {
        TweenPosition _tweenPos = null;
        _tweenPos = mTweenChildren[kCellIndex].GetComponent<TweenPosition>();
        if (_tweenPos == null) 
        {
            _tweenPos = mTweenChildren[kCellIndex].gameObject.AddComponent<TweenPosition>();
            _tweenPos.enabled = false;        
        }

        if (mMaxIndex < kCellIndex)
        {
            _tweenPos.to = mTweenChildren[kCellIndex].transform.localPosition;
            mMaxIndex = kCellIndex;
            mToPos.Add(_tweenPos.to);
        }
        else 
        {
            _tweenPos.to = mToPos[kCellIndex];
        }
        if (mColNum == 1 || mColNum == 0)
        {
            if (mOriginPos == Vector3.zero)
            {
                _tweenPos.from = new Vector3(mDirection == Transform_Direction.To_Left ? _tweenPos.to.x + mCellWidth + mStepDistance : _tweenPos.to.x - mCellWidth - mStepDistance, _tweenPos.to.y, _tweenPos.to.z);
            }
            else 
            {
                _tweenPos.from = mOriginPos;
            }
            if (mStepDelay != 0)
            {
                _tweenPos.delay = kCellIndex * mStepDelay;
            }
        }
        else if (mColNum == 2)
        {
            if (kCellIndex % 2 == 0)
            {
                _tweenPos.from = new Vector3(_tweenPos.to.x - mCellWidth - mStepDistance, _tweenPos.to.y, _tweenPos.to.z);
            }
            else 
            {
                _tweenPos.from = new Vector3(_tweenPos.to.x + mCellWidth + mStepDistance, _tweenPos.to.y, _tweenPos.to.z);
            }
            if (mStepDelay != 0)
            {
                _tweenPos.delay = kCellIndex / 2 * mStepDelay;
            }
        }
        else 
        {
            DEBUG.LogError("没有这种列数设置");           
        }
        _tweenPos.duration = mStepDuration;
        _tweenPos.animationCurve = mAnimCurve;

        return _tweenPos;
    }
    private void InitOriginPos()
    {
        for (int i = 0, count = mTweenPos.Count; i < count; i++)
        {
            mTweenPos[i].ResetToBeginning();
            mTweenPos[i].enabled = false;
        }
        //added by xuke
        this.transform.localPosition = Vector3.zero;
        //end
    }
}
