using UnityEngine;
using System.Collections;


public class USLoadScene : MonoBehaviour
{
    public string sceneName = "Prologue";
    public float delay = 1f;

    private void Start()
    {
        StartCoroutine(WaitForLoad(sceneName, delay));
    }

    private IEnumerator WaitForLoad(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        GlobalModule.ClearAllWindow();
        MainProcess.ClearBattle();
        GlobalModule.Instance.LoadScene(sceneName, false);
    }
}