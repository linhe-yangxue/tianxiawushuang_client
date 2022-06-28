using UnityEngine;
using System;
using System.Collections.Generic;
using DataTable;


class NiceFont
{
    public NiceSprite[] mShowWord;
    public GameObject mMainObject;
    public NiceTable mFontTable;

    public void SetText(string text)
    {
        foreach (char c in text)
        {
			_AppendWord(c);
        }       
    }

    public void _AppendWord(char c)
    {
		
		string k = new string (c, 1);

    }

}

