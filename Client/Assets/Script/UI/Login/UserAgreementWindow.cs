using UnityEngine;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class UserAgreementWindow : tWindow 
{
	private GameObject mark;//对号 
	private int curPage;
	private UILabel pageLabel;
	private UILabel agreementLabel;
	private int pageCount;

	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_close", new DefineFactory<Button_close>());
		EventCenter.Self.RegisterEvent("Button_next_page", new DefineFactory<Button_next_page>());
		EventCenter.Register ("Button_last_page", new DefineFactory<Button_last_page>());
		EventCenter.Register ("Button_Agree", new DefineFactory<Button_Agree>());
	}

	protected override void OpenInit ()
	{
		mark = GameCommon.FindObject(mGameObjUI,"Mark");
		pageLabel = GameCommon.FindObject (mGameObjUI,"PageLabel").GetComponent<UILabel>();
		Dictionary<int, DataRecord> allAgreement = DataCenter.mAgreementTable.GetAllRecord ();
		pageCount = allAgreement.Count;
		agreementLabel = GameCommon.FindObject (mGameObjUI,"annouceInfo").GetComponent<UILabel>();

	}
	public override void Open (object param)
	{
		base.Open (param);
		SetMarkVisible (CommonParam.isUserAgree);
		curPage = 1;
		pageLabel.text = curPage+"/"+pageCount;
		string tText = DataCenter.mAgreementTable.GetRecord(curPage.ToString()).get("TEXT");
		agreementLabel.text = GameCommon.ReplaceWithSpace(tText);
		GameCommon.SetTextColor(agreementLabel.gameObject,"[69503E]");
	}
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "AGREE")
		{
			CommonParam.isUserAgree = !CommonParam.isUserAgree;
			SetMarkVisible(CommonParam.isUserAgree);
			DataCenter.SetData("LANDING_WINDOW","AGREE",true);
		}
		else if(keyIndex == "NEXTPAGE")
		{
			if(curPage<pageCount)
			{
				curPage++;
				pageLabel.text = curPage+"/"+pageCount;
				string tText = DataCenter.mAgreementTable.GetRecord(curPage.ToString()).get("TEXT");
				agreementLabel.text = GameCommon.ReplaceWithSpace(tText);
				GameCommon.SetTextColor(agreementLabel.gameObject,"[69503E]");

				GameCommon.FindObject(mGameObjUI,"view").GetComponent<UIScrollView>().ResetPosition();
			}
		}
		else if(keyIndex == "LASTPAGE")
		{
			if(curPage>1)
			{
				curPage--;
				pageLabel.text = curPage+"/"+pageCount;
				string tText = DataCenter.mAgreementTable.GetRecord(curPage.ToString()).get("TEXT");
				agreementLabel.text = GameCommon.ReplaceWithSpace(tText);
				GameCommon.SetTextColor(agreementLabel.gameObject,"[69503E]");
				GameCommon.FindObject(mGameObjUI,"view").GetComponent<UIScrollView>().ResetPosition();
			}
		}
	}

	void SetMarkVisible(bool isVisiable)
	{
		mark.SetActive (isVisiable);
	}

}

public class Button_close : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("USER_AGREEMENT_WINDOW", "CLOSE", true);
		return true;
	}
}

public class Button_next_page : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("USER_AGREEMENT_WINDOW", "NEXTPAGE", true);
		return true;
	}
}

public class Button_last_page : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("USER_AGREEMENT_WINDOW", "LASTPAGE", true);
		return true;
	}
}

public class Button_Agree : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("USER_AGREEMENT_WINDOW", "AGREE", true);
		return true;
	}
}
