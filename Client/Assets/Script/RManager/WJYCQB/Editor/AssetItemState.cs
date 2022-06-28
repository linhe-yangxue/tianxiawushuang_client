using System;
public class AssetItemState
{
    public AssetItemState(string sceneName, string para, string para1)
    {
        SceneName = sceneName;
        Enable = string.Compare(para, "1") == 0;
        State = byte.Parse(para1);
    }
    public string SceneName = "";
    public bool Enable = true;
    public byte State = 0;//0:就绪;1:失败;2:成功
}