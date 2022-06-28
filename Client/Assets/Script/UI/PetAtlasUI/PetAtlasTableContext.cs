using System.Collections.Generic;
using Logic;
using UnityEngine;
using System.Collections;

public class PetAtlasTableContext : PetAtlasBehaviourBase
{
    public UILabel HasCount;
    public UISprite TitleTypeFlag;
    public Transform ThisParentTrans;
    [HideInInspector]
    public int ElementCount;
    public GameObject EleProfab;

    private string HasTemp = "[ffc579]已收集{0}系侠客[-]{1}/{2}";


    public void Init(List<KnightData> knightData, List<KnightData> localdata, string element)
    { 
        int elemnt = -1;
        string TypeFlag = "";
        if (element.Equals("火"))
        {
            TypeFlag = "ui_firetoptxt";
            elemnt = 0;
        }
        else if (element.Equals("水"))
        {
            TypeFlag = "ui_watertoptxt";
            elemnt = 1;
        }
        else if (element.Equals("阳"))
        {
            TypeFlag = "ui_goldtoptxt";
            elemnt = 2;
        }
        else if (element.Equals("木"))
        {
            TypeFlag = "ui_woodtoptxt";
            elemnt = 3;
        }
        else if (element.Equals("阴"))
        {
            TypeFlag = "ui_shadowtoptxt";
            elemnt = 4;
        }

        ThisParentTrans.GetComponent<UITable>().keepWithinPanel = false;
        TitleTypeFlag.spriteName = TypeFlag;
        HasCount.text = string.Format(HasTemp, element, knightData.Count, localdata.Count);
        //根据协议实体数据 创建表格元素
        StartCoroutine(CreateTableElement(knightData, localdata, elemnt));
    }

    //创建单个表格元素
    IEnumerator CreateTableElement(List<KnightData> knightData, List<KnightData> localdata, int element)
    {
        for (int i = 0; i < localdata.Count; i++)
        {
            GameObject subCell = Instantiate(EleProfab) as GameObject;
            subCell.transform.parent = ThisParentTrans;
            subCell.transform.localPosition = Vector3.zero;
            subCell.transform.localScale = Vector3.one;

			if (knightData.Find(k => k.PetId == localdata[i].PetId) != null)
			{
				KnightData tempKnightData = knightData.Find(k => k.PetId == localdata[i].PetId);
				subCell.GetComponent<PetAtlasTableCellInfo>().InitCellInfo(tempKnightData);
			}
			else
                subCell.GetComponent<PetAtlasTableCellInfo>().InitCellInfo(localdata[i]);

            if (i % 8 == 0)
            {
                subCell.GetComponent<PetAtlasTableCellInfo>().SetDepth(i);
                yield return new WaitForEndOfFrame();
            }

        }

        //释放Resources加载的镜像文件     如果考虑内存就使用
        //EleProfab = null;
        //Resources.UnloadUnusedAssets();
        ThisParentTrans.GetComponent<UITable>().Reposition();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

}


