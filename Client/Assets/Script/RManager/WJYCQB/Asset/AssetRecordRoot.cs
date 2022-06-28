using System;
using UnityEngine;
using Object = UnityEngine.Object;
public class AssetRecordRoot : MonoBehaviour
{
    public string Type = null;
    public GameObject[] Objects;
    public AssetRecord[] Records;
    public MonoBehaviour[] DeferredExecScripts;
    public static Action<AssetRecordRoot> AddTaskHandle;
    public string Version = "";
    public byte Init = 0;

    public Action<AssetRecordRoot> OnRecordRootState;


    public void SetVersion()
    {
        Version = "1.0";
    }
    void Awake()
    {
        if (OnRecordRootState != null) OnRecordRootState(this);
        if (AddTaskHandle != null) AddTaskHandle(this);
        Init = 1;
    }
    void Start()
    {
        if (OnRecordRootState != null) OnRecordRootState(this);
        if (AddTaskHandle != null) AddTaskHandle(this);
        Init = 2;
    }
    void Update()
    {
        if (Init == 2)
        {
            if (OnRecordRootState != null) OnRecordRootState(this);
            if (AddTaskHandle != null) AddTaskHandle(this);
            Init = 3;
        }
    }
}