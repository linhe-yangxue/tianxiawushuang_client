using UnityEngine;

/// <summary>
/// Trivial script that fills the label's contents gradually, as if someone was typing.
/// </summary>

[RequireComponent(typeof(UILabel))]
[AddComponentMenu("NGUI/Examples/Typewriter Effect")]
public class TypewriterEffect : MonoBehaviour
{
	public int charsPerSecond = 20;
    public bool allText = false;

	UILabel mLabel;
	string mText;
	int mOffset = 0;
	float mNextChar = 0f;

    bool mIsShowColor = false;

	void Update ()
	{
		if (mLabel == null)
		{
			mLabel = GetComponent<UILabel>();
			mLabel.supportEncoding = true;
            mLabel.symbolStyle = NGUIText.SymbolStyle.Colored;
			mText = mLabel.processedText;
		}

		if (mOffset < mText.Length)
		{
			if (mNextChar <= RealTime.time)
			{
				charsPerSecond = Mathf.Max(1, charsPerSecond);

				// Periods and end-of-line characters should pause for a longer time.
				float delay = 1f / charsPerSecond;
				char c = mText[mOffset];
				if (c == '.' || c == '\n' || c == '!' || c == '?') delay *= 4f;

                if (c == '[')
                    mIsShowColor = true;

                while(mIsShowColor)
                {
                    char tempC = mText[++mOffset];
                    if (tempC == ']')
                        mIsShowColor = false;
                }
				mNextChar = RealTime.time + delay;
                if (allText)
                {
                    mLabel.text = mText.Substring(0, mText.Length);
                    Destroy(this);
                }
                else
                { 
				    mLabel.text = mText.Substring(0, ++mOffset);
                }
			}
		}
		else Destroy(this);
	}
}
