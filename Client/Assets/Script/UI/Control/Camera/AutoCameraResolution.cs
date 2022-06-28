using UnityEngine;
using System.Collections;

public class AutoCameraResolution : MonoBehaviour
{
    private Camera m_Camera;
    private int mScrWidth = 0;
    private int mScrHeight = 0;

    void Awake()
    {
        m_Camera = GetComponent<Camera>();
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (mScrWidth != Screen.width || mScrHeight != Screen.height)
            __AdjustResolution(Screen.width, Screen.height);
    }

    private void __AdjustResolution(int width, int height)
    {
        GlobalModule.SetResolutionEx(m_Camera);
        mScrWidth = width;
        mScrHeight = height;
    }
}
