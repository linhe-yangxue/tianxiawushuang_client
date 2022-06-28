using UnityEngine;
using System;
public static class ArrangeHelper {
    public static void SetNextTFPos(Transform preTransform,Transform nextTransform,float spacing,bool isVertical=true) {
        spacing=spacing*preTransform.lossyScale.y;
        Bounds preBounds=NGUIMath.CalculateAbsoluteWidgetBounds(preTransform);
        Bounds nextBounds=NGUIMath.CalculateAbsoluteWidgetBounds(nextTransform);
        float startPos;
        float preBoundsSize;
        if(isVertical) {
            startPos=preBounds.min.y;
            preBoundsSize=preBounds.size.y;
        } else {
            startPos=preBounds.min.x;
            preBoundsSize=preBounds.size.x;
        }
        startPos-=preBounds.min.y+spacing;
        
        float offset=startPos-nextBounds.max.y;
        Vector3 offsetV=(isVertical)?new Vector3(0,offset,0):new Vector3(offset,0,0);
        nextTransform.position=preTransform.position+offsetV;
    }

    public static Action<int,int> GetSetDimensionFunc(int increaseRate,bool isVertical,params UISprite[] spriteArr) {
        return (initValue,count) => {
            if(isVertical) spriteArr.Foreach(sprite => sprite.height=initValue+increaseRate*count);
            else spriteArr.Foreach(sprite => sprite.width=initValue+increaseRate*count);
        };
    }
}