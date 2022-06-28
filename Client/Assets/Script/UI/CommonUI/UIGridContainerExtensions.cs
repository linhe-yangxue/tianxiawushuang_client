using UnityEngine;
using System;
using System.Collections;
using Utilities;

public static class UIGridContainerExtensions
{
    // 异步设置UIGridContainer的MaxCount要求GridContainer的fastResize选项为true
    // 调用此方法之前请确保未通过此UIGridContainer开启其他协程，否则其他协程会被强行终止
    public static void SetMaxCountAsync(this UIGridContainer container, int maxCount, int cellPerFrame, Action<int> onInitCell, Action onFinish)
    {
        container.fastResize = true;
        container.StopAllCoroutines();
        container.StartCoroutine(SetMaxCount(container, maxCount, cellPerFrame, onInitCell, onFinish));
    }

    private static IEnumerator SetMaxCount(UIGridContainer container, int maxCount, int cellPerFrame, Action<int> onInitCell, Action onFinish)
    {
        container.MaxCount = 0;
        cellPerFrame = Mathf.Max(1, cellPerFrame);

        while (container.MaxCount < maxCount)
        {
            int current = container.MaxCount;
            int next = Mathf.Min(container.MaxCount + cellPerFrame, maxCount);
            container.MaxCount = next;

            if (onInitCell != null)
            {
                for (int i = current; i < next; ++i)
                {
                    onInitCell(i);
                }
            }

            yield return new WaitForEndOfFrame();
        }

        if (onFinish != null)
        {
            onFinish();
        }
    }
}