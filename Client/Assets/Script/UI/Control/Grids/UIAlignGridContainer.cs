using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class UIAlignGridContainer : UIGridContainer
{
    [HideInInspector][SerializeField]
    protected bool mInverseShow = false;        //颠倒显示顺序，默认从左到右
    [ExposeProperty]
    public bool InverseShow
    {
        set
        {
            mInverseShow = value;
            Reposition();
        }
        get { return mInverseShow; }
    }

    public override void Reposition()
    {
        if (IsNotReposWhenStart)
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

            for (int i = 0; i < myTrans.childCount; ++i)
            {
                Transform t = myTrans.GetChild(i);
                if (t && (!hideInactive || NGUITools.GetActive(t.gameObject))) list.Add(t);
            }
            list.Sort(SortByName);

            int tmpStartX = 0;
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                Transform t = list[i];

                if (!NGUITools.GetActive(t.gameObject) && hideInactive) continue;

                float depth = t.localPosition.z;
                int tmpCellSize = (arrangement == Arrangement.Horizontal) ? (int)CellWidth : (int)CellHeight;
                if (mInverseShow && x == 0)
                {
                    //计算剩余元素数量，如果过少，开始颠倒计算
                    int tmpLeftCount = imax - i;
                    if (tmpLeftCount < MaxPerLine)
                        tmpStartX = tmpCellSize * (MaxPerLine - tmpLeftCount);
                }
                t.localPosition = (arrangement == Arrangement.Horizontal) ?
                    new Vector3(tmpStartX + (int)CellWidth * x, -(int)CellHeight * y, depth) :
                    new Vector3((int)CellWidth * y, -(tmpStartX + (int)CellHeight * x), depth);

                if (++x >= MaxPerLine && MaxPerLine > 0)
                {
                    x = 0;
                    ++y;
                }
            }
        }
        else
        {
            int tmpMaxCount = 0;
            for (int i = 0; i < myTrans.childCount; ++i)
            {
                Transform t = myTrans.GetChild(i);
                if (t && (!hideInactive || NGUITools.GetActive(t.gameObject)))
                    tmpMaxCount += 1;
            }

            int tmpStartX = 0;
            int tmpShowCount = 0;       //已显示的元素个数
            for (int i = 0; i < myTrans.childCount; ++i)
            {
                Transform t = myTrans.GetChild(i);

                if (!NGUITools.GetActive(t.gameObject) && hideInactive) continue;

                tmpShowCount += 1;

                float depth = t.localPosition.z;
                int tmpCellSize = (arrangement == Arrangement.Horizontal) ? (int)CellWidth : (int)CellHeight;
                if (mInverseShow && x == 0)
                {
                    //计算剩余元素数量，如果过少，开始颠倒计算
                    int tmpLeftCount = tmpMaxCount - tmpShowCount + 1;
                    if (tmpLeftCount < MaxPerLine)
                        tmpStartX = tmpCellSize * (MaxPerLine - tmpLeftCount);
                }
                t.localPosition = (arrangement == Arrangement.Horizontal) ?
                    new Vector3(tmpStartX + (int)CellWidth * x, scrollDetla - (int)CellHeight * y, depth) :
                    new Vector3((int)CellWidth * y, scrollDetla - (tmpStartX + (int)CellHeight * x), depth);

                if (++x >= MaxPerLine && MaxPerLine > 0)
                {
                    x = 0;
                    ++y;
                }
            }

            LabelPosYReSet(myTrans);
        }
    }
}
