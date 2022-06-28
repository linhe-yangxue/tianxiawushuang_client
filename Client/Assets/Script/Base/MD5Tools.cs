using UnityEngine;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;


public class MD5Tools
{
    private static readonly MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();

    /// <summary>
    /// 获取整型值的MD5值
    /// </summary>
    /// <param name="value"> 整型值 </param>
    /// <returns> MD5值 </returns>
    public static byte[] GetMD5(int value)
    {
        return md5Provider.ComputeHash(BitConverter.GetBytes(value));
    }

    /// <summary>
    /// 比较整型值和MD5值是否一致
    /// </summary>
    /// <param name="value"> 整型值 </param>
    /// <param name="hash"> MD5值 </param>
    /// <returns> 是否一致 </returns>
    public static bool IsMD5(int value, byte[] hash)
    {
        return hash != null && GetMD5(value).SequenceEqual(hash);
    }
}



///// <summary>
///// 可验证对象的接口
///// </summary>
//public interface IVerifiable
//{
//    /// <summary>
//    /// 进行验证
//    /// </summary>
//    /// <returns> 验证结果，1表示验证通过，0表示未验证，其他值表示验证不通过 </returns>
//    int Verify();
//}


///// <summary>
///// 随机时间验证器
///// </summary>
//public class RandTimeVerifier
//{
//    private class VerifyInfo
//    {
//        private WeakReference weakRef;
//        private float minInterval = 1f;
//        private float maxInterval = 3f;
//        private float nextVerifyTime = 0f;

//        public bool invalid { get; private set; }

//        public VerifyInfo(IVerifiable target, float minInterval, float maxInterval)
//        {
//            this.weakRef = new WeakReference(target);
//            this.invalid = false;
//            this.minInterval = minInterval;
//            this.maxInterval = maxInterval;
//        }

//        public bool IsTarget(object target)
//        {
//            return weakRef.Target == target;
//        }

//        public int Verify()
//        {
//            var target = weakRef.Target as IVerifiable;

//            if (target == null)
//            {
//                invalid = true;
//                return 0;
//            }
//            else
//            {
//                int result = 0;

//                if (Time.time >= nextVerifyTime)
//                {
//                    result = target.Verify();
//                }

//                if (result != 0)
//                {
//                    nextVerifyTime = Time.time + UnityEngine.Random.Range(minInterval, maxInterval);
//                }

//                return result;
//            }
//        }
//    }

//    private List<VerifyInfo> verifyList = new List<VerifyInfo>();

//    /// <summary>
//    /// 添加待验证对象
//    /// </summary>
//    /// <param name="target"> 添加的对象 </param>
//    /// <param name="minInterval"> 最小时间间隔 </param>
//    /// <param name="maxInterval"> 最大时间间隔 </param>
//    public void Add(IVerifiable target, float minInterval, float maxInterval)
//    {
//        if (target != null && !verifyList.Exists(x => x.IsTarget(target)))
//        {
//            verifyList.Add(new VerifyInfo(target, minInterval, maxInterval));
//        }
//    }

//    /// <summary>
//    /// 移除待验证对象
//    /// </summary>
//    /// <param name="target"> 移除的对象 </param>
//    public void Remove(IVerifiable target)
//    {
//        int index = verifyList.FindIndex(x => x.IsTarget(target));

//        if (index >= 0)
//        {
//            verifyList.RemoveAt(index);
//        }
//    }

//    /// <summary>
//    /// 验证器的刷新函数，在游戏主循环中调用
//    /// </summary>
//    public void Update()
//    {
//        foreach (var info in verifyList)
//        {
//            int result = info.Verify();

//            if (result != 0 && result != 1)
//            {
//                OnVerifyFailed(result);
//                break;
//            }
//        }

//        verifyList.RemoveAll(x => x.invalid);
//    }

//    /// <summary>
//    /// 验证不通过时的回调函数
//    /// </summary>
//    /// <param name="verifyResult"> 验证结果 </param>
//    public virtual void OnVerifyFailed(int verifyResult)
//    { }
//}
