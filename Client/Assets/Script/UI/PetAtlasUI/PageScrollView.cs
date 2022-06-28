using UnityEngine;



/// <summary>
/// 此脚本添加在UIgrid  GameObject节点上    作用是分页
/// 基于 UICenter on Child  改的
/// 替代 UICenter on Child  使用
/// </summary>
public class PageScrollView : MonoBehaviour
{

    public float springStrength = 8.0f;

    private UIScrollView scrollView;
    private int elementsPerPage;
    private int currentScrolledElements;
    private Vector3 startingScrollPosition;

    private UIGrid grid;

    void Start()
    {
        if (scrollView == null)
        {
            scrollView = gameObject.transform.parent.GetComponent<UIScrollView>();
            if (scrollView == null)
            {
                Debug.LogError(GetType() + " requires " + typeof(UIScrollView) + " object in order to work", this);
                enabled = false;
                return;
            }

            grid = this.GetComponent<UIGrid>();
            elementsPerPage = (int)(scrollView.panel.clipRange.z / grid.cellWidth);
            currentScrolledElements = 0;
            startingScrollPosition = scrollView.panel.cachedTransform.localPosition;

        }
    }


    /// <summary>
    /// 翻页main执行方法
    /// </summary>
    /// <param name="target"></param>
    void MoveBy(Vector3 target)
    {
        if (scrollView != null && scrollView.panel != null)
        {            
            SpringPanel.Begin(scrollView.panel.cachedGameObject, startingScrollPosition - target, springStrength);
        }
    }


    /// <summary>
    /// 下一页   
    /// </summary>
    public void NextPage()
    {
        if (scrollView != null && scrollView.panel != null)
        {
            currentScrolledElements += elementsPerPage;
            if (currentScrolledElements > (this.transform.childCount - elementsPerPage))
            {
                currentScrolledElements = (this.transform.childCount - elementsPerPage);
            }
            float nextScroll = grid.cellWidth * currentScrolledElements;
            Vector3 target = new Vector3(nextScroll, 0.0f, 0.0f);
            MoveBy(target);
        }
    }


    /// <summary>
    /// 上一页    
    /// </summary>
    public void PreviousPage()
    {
        if (scrollView != null && scrollView.panel != null)
        {
            currentScrolledElements -= elementsPerPage;
            if (currentScrolledElements < 0)
            {
                currentScrolledElements = 0;
            }
            float nextScroll = grid.cellWidth * currentScrolledElements;
            Vector3 target = new Vector3(nextScroll, 0.0f, 0.0f);
            MoveBy(target);
        }
    }   

}