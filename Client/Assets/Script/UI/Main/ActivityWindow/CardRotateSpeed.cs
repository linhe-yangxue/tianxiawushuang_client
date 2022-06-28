using UnityEngine;
using System.Collections;

public class CardRotateSpeed : MonoBehaviour
{
    [SerializeField]
    public float mfRotSpeed = 0f;   //> 卡牌旋转速度
    [SerializeField]
    public float mfMoveSpeed = 0f;   //> 卡牌交换的移动速度
    [SerializeField]
    public int mnExchangeTimes = 5;  //> 交换卡牌的次数
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
