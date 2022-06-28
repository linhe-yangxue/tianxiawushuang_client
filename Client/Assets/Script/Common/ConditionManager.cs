using UnityEngine;
using System.Collections;
using System;


public class BagShowConditionAction<T> where T:ItemDataBase
{
    public readonly Func<T,bool> bagShowCondition;//返回true才能在列表显示
    public readonly Action okBtnAction;

    public BagShowConditionAction(Func<T, bool> bagShowCondition, Action okBtnAction)
    {
        this.bagShowCondition = bagShowCondition;
        this.okBtnAction = okBtnAction;
    }
}
