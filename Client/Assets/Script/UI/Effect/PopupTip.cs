using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utilities.Routines;


public class PopupTip
{
    private class TipContent
    {
        public string text = "";
        public int priority = 0;

        public TipContent(string text, int priority)
        {
            this.text = text;
            this.priority = priority;
        }
    }

    private List<TipContent> queue = new List<TipContent>();
    private IRoutine tipRoutine = null;

    /// <summary>
    /// 队列中tips的播放间隔 
    /// </summary>
    public float tipRate { get; set; }

    /// <summary>
    /// 单个tip的持续时间
    /// </summary>
    public float tipTime { get; set; }

    /// <summary>
    /// tip模板，在其上挂载Tween脚本控制播放动画
    /// </summary>
    public GameObject template { get; set; }

    /// <summary>
    /// tip父节点
    /// </summary>
    public GameObject parent { get; set; }

    /// <summary>
    /// Tip初始相对偏移量
    /// </summary>
    public Vector3 offset { get; set; }

    /// <summary>
    /// 具有默认优先级0的tip入队，如果当前还有其他tip未播放完，则在队列中等待
    /// </summary>
    /// <param name="text"> tip内容 </param>
    public void Enqueue(string text)
    {
        Enqueue(text, 0);
    }

    /// <summary>
    /// 具有指定优先级的tip入队，如果当前还有其他tip未播放完，则在队列中等待
    /// </summary>
    /// <param name="text"> tip内容 </param>
    /// <param name="priority"> tip优先级，值越高越早播放 </param>
    public void Enqueue(string text, int priority)
    {
        bool inserted = false;

        for (int i = queue.Count - 1; i >= 0; --i)
        {
            if (priority <= queue[i].priority)
            {
                inserted = true;
                queue.Insert(i + 1, new TipContent(text, priority));
                break;
            }
        }

        if (!inserted)
        {
            queue.Insert(0, new TipContent(text, priority));
        }

        if (!Routine.IsActive(tipRoutine) && parent != null && parent.activeInHierarchy)
        {
            tipRoutine = parent.StartRoutine(DoQueue());
        }
    }

    /// <summary>
    /// 清空队列
    /// </summary>
    public void Clear()
    {
        queue.Clear();
    }

    private IEnumerator DoQueue()
    {
        yield return null;

        while (queue.Count > 0)
        {
            string text = queue[0].text;
            queue.RemoveAt(0);

            if (template != null)
            {
                GameObject tipObj = GameObject.Instantiate(template) as GameObject;
                tipObj.name = template.name + "(Clone)";
                tipObj.transform.parent = parent.transform;
                tipObj.transform.localPosition = offset;
                tipObj.transform.localScale = new Vector3(1, 1, 1);
                tipObj.SetActive(true);
                UILabel label = tipObj.GetComponent<UILabel>();

                if (label != null)
                    label.text = text;

                tipObj.AddComponent<DestroyOnDisable>();
                GameObject.Destroy(tipObj, tipTime < 0.001f ? 0.3f : tipTime);
            }

            yield return new Delay(tipRate);
        }
    }
}