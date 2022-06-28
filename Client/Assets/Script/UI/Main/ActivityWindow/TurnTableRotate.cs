using UnityEngine;
using System.Collections;

public class TurnTableRotate : MonoBehaviour
{
    [SerializeField]
    public AnimationCurve mAnimCurve;   //> 旋转曲线
    private float mFromRotSpeed;             //> 旋转起始角度
    [SerializeField]
    public float mfDuration;                //> 旋转持续时间
    [SerializeField]
    public float mfRotSpeed;               //> 旋转速度
    private bool mbRotate = false;          //> 是否开始旋转
    private bool mbStartSlowDown = false;   //> 是否开始减速 
    private float mfCurRotateTime = 0f;     //> 当前已经旋转的时间
    public System.Action OnRotationCompleted = () => {};    //> 旋转结束后的回调方法
    [SerializeField]
    public float WaitTime = 0.5f;   //> 转盘匀速旋转多久后角色播放开枪动画
    void Start() 
    {
        //mfRotSpeed = Mathf.Abs(mAnimCurve.Evaluate(0.01f) * mTo.z - mTo.z) / 3f;
        mFromRotSpeed = mfRotSpeed;
    }

    private float mfAnimPercent = 0f;   //> 当前AnimCurve的采样位置
    private float tmpRotSpeed = 0f;
	// Update is called once per frame
	void Update () 
    {
        if (!mbRotate)
            return;
        // 匀速阶段
        if (!mbStartSlowDown) 
        {
            transform.Rotate(Vector3.forward,-mfRotSpeed * Time.deltaTime);
            return;
        }

         mfCurRotateTime += Time.deltaTime;
        if (mfCurRotateTime < mfDuration)
        {
            mfAnimPercent = mfCurRotateTime / mfDuration;
            mfAnimPercent = mAnimCurve.Evaluate(mfAnimPercent);
            tmpRotSpeed = Mathf.Lerp(mFromRotSpeed, 0f, mfAnimPercent);
        }
        else 
        {
            mbRotate = false;
            mbStartSlowDown = false;
            mfCurRotateTime = 0f;
            tmpRotSpeed = 0f;
            //执行旋转结束后的委托
            OnRotationCompleted();
        }
        transform.Rotate(Vector3.forward, -tmpRotSpeed * Time.deltaTime);
	}

    public void StartRotate() 
    {
        mbRotate = true;
    }

    public void StartSlowDown() 
    {
        //mFrom.z = transform.localRotation.z;
        mbStartSlowDown = true;
    }

    void OnDisable() 
    {
        mbRotate = false;
        mbStartSlowDown = false;
        mfCurRotateTime = 0f;
        tmpRotSpeed = 0f;
    }
}
