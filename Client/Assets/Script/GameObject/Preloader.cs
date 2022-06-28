using UnityEngine;
using DataTable;


public class Preloader
{
    public static void PreloadInStage(STAGE_TYPE battleType)
    {
        switch (battleType)
        {
            case STAGE_TYPE.MAIN_COMMON:
            case STAGE_TYPE.MAIN_ELITE:
            case STAGE_TYPE.MAIN_MASTER:
            case STAGE_TYPE.EXP:
            case STAGE_TYPE.MONEY:
            case STAGE_TYPE.TOWER:
                PreloadInPVE();
                break;

            case STAGE_TYPE.CHAOS:
                PreloadInBossBattle();
                break;

            case STAGE_TYPE.PVP4:
                PreloadInPVP4();
                break;
        }
    }

    public static void PreloadInBossBattle()
    {
        if (PetLogicData.Self != null)
        {
            for (int i = 1; i <= 3; ++i)
            {
                PetData d = PetLogicData.Self.GetPetDataByPos(i);

                if (d != null)
                {
                    PreloadActiveObject(d.tid);
                }
            }
        }
    }

    public static void PreloadInPVE()
    {
        if (PetLogicData.Self != null)
        {
            for (int i = 1; i <= 3; ++i)
            {
                PetData d = PetLogicData.Self.GetPetDataByPos(i);

                if (d != null)
                {
                    PreloadActiveObject(d.tid);
                }
            }

            if (PetLogicData.mFreindPetData != null)
            {
                PreloadActiveObject(PetLogicData.mFreindPetData.tid);
            }
        }
    }

    public static void PreloadInPVP6()
    {
        PVP_VictoryPredictWindow w = DataCenter.GetData("PVP_VICTORY_PREDICT_WINDOW") as PVP_VictoryPredictWindow;

        if (w != null)
        {
            foreach (var i in w.mPets)
            {
                if (i > 0)
                {
                    PreloadActiveObject(i);
                }
            }

            foreach (var i in w.yPets)
            {
                if (i > 0)
                {
                    PreloadActiveObject(i);
                }
            }
        }
    }

    public static void PreloadInPVP4()
    {
        PVP_FourVSFourVictoryPredictWindow w = DataCenter.GetData("PVP_FOUR_VS_FOUR_VICTORY_PREDICT_WINDOW") as PVP_FourVSFourVictoryPredictWindow;

        if (w != null)
        {
            foreach (var d in w.mPets)
            {
                if (d != null)
                {
                    PreloadActiveObject(d.tid);
                }
            }

            foreach (var d in w.yPets)
            {
                if (d != null)
                {
                    PreloadActiveObject(d.tid);
                }
            }
        }
    }

    public static void PreloadObject(int objectIndex)
    {
        OBJECT_TYPE type = ObjectManager.Self.GetObjectType(objectIndex);

        switch (type)
        {
            case OBJECT_TYPE.MONSTER:
                PreloadMonster(objectIndex);
                break;
            case OBJECT_TYPE.BIG_BOSS:
                PreloadBoss(objectIndex);
                break;
            default:
                PreloadActiveObject(objectIndex);
                break;
        }
    }

    public static void PreloadActiveObject(int petIndex)
    {
        if (petIndex > 0)
        {
            int model = TableCommon.GetNumberFromActiveCongfig(petIndex, "MODEL");

            if (model > 0)
            {
                PreloadModel(model);
            }
        }
    }

    public static void PreloadMonster(int monsterIndex)
    {
        if (monsterIndex > 0)
        {
            int model = DataCenter.mMonsterObject.GetData(monsterIndex, "MODEL");

            if (model > 0)
            {
                PreloadModel(model);
            }
        }
    }

    public static void PreloadBoss(int bossIndex)
    {
        if (bossIndex > 0)
        {
            int model = DataCenter.mBossConfig.GetData(bossIndex, "MODEL");

            if (model > 0)
            {
                PreloadModel(model);
            }
        }
    }

    public static void PreloadModel(int model)
    {
        DataRecord r = DataCenter.mModelTable.GetRecord(model);
        string modelRes = r["BODY"];
        string texRes = r["BODY_TEX_1"];
        string anim = r["ANIMATION"];

        if (!string.IsNullOrEmpty(modelRes))
        {
            GameCommon.mResources.LoadModel(modelRes);
        }

        if (!string.IsNullOrEmpty(texRes))
        {
            GameCommon.mResources.LoadTexture(texRes);
        }

        if (!string.IsNullOrEmpty(anim))
        {
            GameCommon.mResources.LoadController(anim);
        }
    }

    public static void PreloadWorldMap()
    {
        foreach (var pair in DataCenter.mStagePoint.GetAllRecord())
        {
            if (pair.Value["STAGETYPE"] == (int)MapPointType.Boss)
            {
                int bossID = TableCommon.GetNumberFromStageConfig((int)pair.Value["STAGEID"], "HEADICON");
                PreloadObject(bossID);
            }
        }
    }
}