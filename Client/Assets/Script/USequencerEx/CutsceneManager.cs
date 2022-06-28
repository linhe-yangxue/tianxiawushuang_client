using UnityEngine;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Self;

    private GameObject cutsceneInstance;

    public static void StartBattle()
    {
        if (Self != null)
        {
            Self.UnloadCutscene();
            Self.OpenMainCamera();
            Self.gameObject.AddComponent<MainProcess>();
            Destroy(Self);
        }
    }

    private void Awake()
    {
        Self = this;
    }

    private void Start()
    {
        int stage = DataCenter.Get("CURRENT_STAGE");
        string cutscenePath = TableCommon.GetStringFromStageConfig(stage, "CUTSCENE");

        if (stage > 0 && !string.IsNullOrEmpty(cutscenePath))// && StageProperty.IsFirstFight(stage))
        {
            cutscenePath = "Cutscenes/" + cutscenePath;
            CloseMainCamera();

            if (LoadCutscene(cutscenePath))
            {
                if (StageProperty.GetStageType(stage) == STAGE_TYPE.CHAOS)
                {
                    DataCenter.SetData("BOSS_BATTLE_LOADING_WINDOW", "CONTINUE", true);
                }
                else 
                {
                    DataCenter.SetData("BATTLE_LOADING_WINDOW", "CONTINUE", true);
                }
            }
            else
            {
                OpenMainCamera();
                gameObject.AddComponent<MainProcess>();
                Destroy(this);
            }
        }
        else 
        {
            gameObject.AddComponent<MainProcess>();
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        Self = null;
    }

    private void OpenMainCamera()
    {
        GameObject tempObj = GameCommon.FindObject(gameObject, "Camera");

        if (tempObj != null)
            tempObj.SetActive(true);

        GameObject worldCenter = GameCommon.GetMainCameraObj();

        if (worldCenter != null)
            worldCenter.SetActive(true);
    }

    private bool LoadCutscene(string cutscenePath)
    {
        GameObject cutscenePrefab = GameCommon.LoadPrefabs(cutscenePath, "CUTSCENE");//GameCommon.mResources.LoadPrefab(cutscenePath, "CUTSCENE");//Resources.Load<GameObject>(cutscenePath);

        if (cutscenePrefab == null)
            return false;

        cutsceneInstance = Instantiate(cutscenePrefab) as GameObject;
        return true;
    }

    private void UnloadCutscene()
    {
        Destroy(cutsceneInstance);
    }

    private void CloseMainCamera()
    {
        GameObject tempObj = GameCommon.FindObject(gameObject, "Camera");

        if (tempObj != null)
            tempObj.SetActive(false);

        GameObject worldCenter = GameCommon.GetMainCameraObj();

        if (worldCenter != null)
            worldCenter.SetActive(false);
    }
}