using UnityEngine;
using System.Collections;
using Logic;

class GameTimeManager : Logic.TimeEventManager
{
    protected float mLastTime = Time.time;

	public override float GetNowTime()
	{
        return Time.time;
	}       
}


class GameEvent
{
	static public void RegisterAIEvent()
	{
        EventCenter.Self.RegisterEvent("AI_CharReadyIdle", new DefineFactoryLog<AI_CharReadyIdle>());
		//EventCenter.Self.RegisterEvent("AI_Attack", new DefineFactory<AI_Attack>());
        EventCenter.Self.RegisterEvent("AI_MoveTo", new DefineFactoryLog<AI_MoveTo>());
		EventCenter.Self.RegisterEvent("AI_MainCharMove", new DefineFactoryLog<AI_MainCharMove>());
        EventCenter.Self.RegisterEvent("AI_CoolDownAttack", new DefineFactoryLog<AI_CoolDownAttack>());
        
        EventCenter.Self.RegisterEvent("TM_RestartAI", new DefineFactoryLog<TM_RestartAI>());
        EventCenter.Self.RegisterEvent("AI_FriendReadyIdle", new DefineFactoryLog<AI_FriendReadyIdle>());
        EventCenter.Self.RegisterEvent("AI_FriendFollowIdle", new DefineFactoryLog<AI_FriendFollowIdle>());
        EventCenter.Self.RegisterEvent("AI_FriendCheckMainRaleEnemy", new DefineFactoryLog<AI_FriendCheckMainRaleEnemy>());
        EventCenter.Self.RegisterEvent("AI_FriendFollowMove", new DefineFactoryLog<AI_FriendFollowMove>());
        EventCenter.Self.RegisterEvent("AI_FriendReadyIdleInAutoBattle", new DefineFactoryLog<AI_FriendReadyIdleInAutoBattle>());
        EventCenter.Self.RegisterEvent("AI_FriendWaitAttack", new DefineFactoryLog<AI_FriendWaitAttack>());
        
        
        EventCenter.Self.RegisterEvent("AI_MonsterReadyIdle", new DefineFactoryLog<AI_MonsterReadyIdle>());
		EventCenter.Self.RegisterEvent("AI_PatrolReadyIdle", new DefineFactory<AI_PatrolReadyIdle>());        
        EventCenter.Self.RegisterEvent("AI_MoveToTargetThenAttack", new DefineFactoryLog<AI_MoveToTargetThenAttack>());
        EventCenter.Self.RegisterEvent("AI_MoveToTargetThenSkill", new DefineFactory<AI_MoveToTargetThenSkill>());

        EventCenter.Self.RegisterEvent("AI_MonsterRelive", new DefineFactoryLog<AI_MonsterRelive>());
        
		EventCenter.Self.RegisterEvent("AI_BeatBackMove", new DefineFactory<AI_BeatBackMove>());
        EventCenter.Self.RegisterEvent("AI_FriendCheckFollow", new DefineFactory<AI_FriendCheckFollow>());
		EventCenter.Self.RegisterEvent("AI_CharMoveToEnemy", new DefineFactory<AI_CharMoveToEnemy>());
        EventCenter.Self.RegisterEvent("AI_HoldAI", new DefineFactory<AI_HoldAI>());
        
	}

	static public void RegisterTimeEvent()
	{
		EventCenter.Self.RegisterEvent("TM_CreateActiveObject", new DefineFactory<TM_CreateActiveObject>());
        EventCenter.Self.RegisterEvent("TM_WaitDestory", new DefineFactory<TM_WaitDestory>());
        EventCenter.Self.RegisterEvent("TM_WaitToBeginOutShow", new DefineFactory<TM_WaitToBeginOutShow>());
        EventCenter.Self.RegisterEvent("TM_SkillButtonData", new DefineFactory<TM_SkillButtonData>());
        EventCenter.Self.RegisterEvent("TM_SummonCD", new DefineFactory<TM_SummonCD>());
        EventCenter.Self.RegisterEvent("TM_PetLifeTime", new DefineFactory<TM_PetLifeTime>());
        
        EventCenter.Self.RegisterEvent("TM_SkillCD", new DefineFactory<TM_SkillCD>());
        EventCenter.Self.RegisterEvent("TM_WaitPlaySound", new DefineFactory<TM_WaitPlaySound>());
        
        EventCenter.Self.RegisterEvent("TM_WaitToBattleResult", new DefineFactory<TM_WaitToBattleResult>());
        EventCenter.Self.RegisterEvent("TM_WaitToBossBattleResult", new DefineFactory<TM_WaitToBossBattleResult>());
		EventCenter.Self.RegisterEvent("TM_WaitNetEvent", new DefineFactory<TM_WaitNetEvent>());

		EventCenter.Self.RegisterEvent("TM_RoleBufferData", new DefineFactory<TM_RoleBufferData>());
        EventCenter.Self.RegisterEvent("TM_BuffCD", new DefineFactory<TM_BuffCD>());

        // Tweener
        EventCenter.Self.RegisterEvent("TM_Tweener", new DefineFactory<TM_Tweener>());
        EventCenter.Self.RegisterEvent("TM_FadeIn", new DefineFactory<TM_FadeIn>());
        EventCenter.Self.RegisterEvent("TM_FadeOut", new DefineFactory<TM_FadeOut>());
        EventCenter.Self.RegisterEvent("TM_FadeInOut", new DefineFactory<TM_FadeInOut>());
        EventCenter.Self.RegisterEvent("TM_TweenPosition", new DefineFactory<TM_TweenPosition>());
        EventCenter.Self.RegisterEvent("TM_TweenScale", new DefineFactory<TM_TweenScale>());

        // Tools
        EventCenter.Self.RegisterEvent("TM_UpdateEvent", new DefineFactory<TM_UpdateEvent>());
        EventCenter.Self.RegisterEvent("TM_DelayEvent", new DefineFactory<TM_DelayEvent>());

		//pev text effect
		EventCenter.Self.RegisterEvent("TM_WaitToPlayPveBeginTextEffect", new DefineFactory<TM_WaitToPlayPveBeginTextEffect>());
		EventCenter.Self.RegisterEvent ("TM_WaitToShowAutoBattleButton", new DefineFactory<TM_WaitToShowAutoBattleButton>());
		EventCenter.Self.RegisterEvent ("TM_WaitToPlayPveEndTextEffect", new DefineFactory<TM_WaitToPlayPveEndTextEffect>());
		EventCenter.Self.RegisterEvent ("TM_WaitToPetFollowRole", new DefineFactory<TM_WaitToPetFollowRole>());
	}

	static public void RegisterEffect()
	{
        EventCenter.Self.RegisterEvent("Object_HeightLight", new DefineFactory<Object_HeightLight>());
        EventCenter.Self.RegisterEvent("Object_OutLine", new DefineFactory<Object_OutLine>());
        EventCenter.Self.RegisterEvent("UI_PaoPaoText", new DefineFactory<UI_PaoPaoText>());

        EventCenter.Self.RegisterEvent("Effect_MoveText", new DefineFactory<Effect_MoveText>());
		EventCenter.Self.RegisterEvent("Object_FadeInOrOut", new DefineFactory<Object_FadeInOrOut>());
        EventCenter.Self.RegisterEvent("Object_Transparent", new DefineFactory<Object_Transparent>());
		EventCenter.Self.RegisterEvent("Effect_ShakeCamera", new DefineFactory<Effect_ShakeCamera>());
        EventCenter.Self.RegisterEvent("CameraMoveEvent", new DefineFactory<CameraMoveEvent>());
        //EventCenter.Self.RegisterEvent("CameraRestoreHeightEvent", new DefineFactory<CameraRestoreHeightEvent>());

        EventCenter.Self.RegisterEvent("BaseEffect", new DefineFactory<BaseEffect>());
		EventCenter.Self.RegisterEvent("Effect", new DefineFactory<Effect>());
        EventCenter.Self.RegisterEvent("DropEffect", new DefineFactory<DropEffect>());
		EventCenter.Self.RegisterEvent("SoundEffect", new DefineFactory<SoundEffect>());
        EventCenter.Self.RegisterEvent("MouseCoord", new DefineFactory<MouseCoord>());
        EventCenter.Self.RegisterEvent("WarnEffect", new DefineFactory<WarnEffect>());
        
        
        EventCenter.Self.RegisterEvent("Skill", new DefineFactory<BaseSkill>());
		EventCenter.Self.RegisterEvent("Bullet", new DefineFactory<BulletSkill>());
        EventCenter.Self.RegisterEvent("FlySkill", new DefineFactory<FlySkill>());
        EventCenter.Self.RegisterEvent("RandAttack", new DefineFactory<RandAttackSkill>());
        EventCenter.Self.RegisterEvent("AttackSkill", new DefineFactoryLog<AttackSkill>());
        EventCenter.Self.RegisterEvent("RandAllSkill", new DefineFactoryLog<RandAllSkill>());
        EventCenter.Self.RegisterEvent("AOESkill", new DefineFactoryLog<AOESkill>());
		EventCenter.Self.RegisterEvent("LurkerSkill", new DefineFactoryLog<LurkerSkill>());
		EventCenter.Self.RegisterEvent("TwisterSkill", new DefineFactoryLog<TwisterSkill>());
        EventCenter.Self.RegisterEvent("ThreeBulletSkill", new DefineFactoryLog<ThreeBulletSkill>());
        EventCenter.Self.RegisterEvent("Summon", new DefineFactoryLog<SummonSkill>());
        EventCenter.Self.RegisterEvent("ScriptSkill", new DefineFactoryLog<ScriptSkill>());
        
		EventCenter.Self.RegisterEvent("WoozyBuffer", new DefineFactoryLog<HoldBuffer>());
		EventCenter.Self.RegisterEvent("HoldBuffer", new DefineFactoryLog<HoldBuffer>());
        EventCenter.Self.RegisterEvent("CharmBuffer", new DefineFactoryLog<CharmBuffer>());
        EventCenter.Self.RegisterEvent("FearBuffer", new DefineFactoryLog<FearBuffer>());
        EventCenter.Self.RegisterEvent("AffectValueBuffer", new DefineFactoryLog<AffectValueBuffer>());
        EventCenter.Self.RegisterEvent("ChangeValueBuffer", new DefineFactoryLog<ChangeValueBuffer>());
        EventCenter.Self.RegisterEvent("ChangeModelBuffer", new DefineFactoryLog<ChangeModelBuffer>());
        EventCenter.Self.RegisterEvent("ScriptBuffer", new DefineFactoryLog<ScriptBuffer>());
	}

	static public void RegisterUIEvent()
	{
        EventCenter.Self.RegisterEvent("Button_do_skill_0", new DefineFactory<Button_DoSkill>());
        EventCenter.Self.RegisterEvent("Button_do_skill_1", new DefineFactory<Button_DoSkill>());
        EventCenter.Self.RegisterEvent("Button_do_skill_2", new DefineFactory<Button_DoSkill>());
        EventCenter.Self.RegisterEvent("Button_do_skill_3", new DefineFactory<Button_DoSkill>());
        EventCenter.Self.RegisterEvent("Button_do_skill_4", new DefineFactory<Button_DoSkill>());
        
		EventCenter.Self.RegisterEvent("Button_Again", new DefineFactory<Button_Again>());
		EventCenter.Self.RegisterEvent("Button_BattleBack", new DefineFactory<Button_BattleBack>());
		EventCenter.Self.RegisterEvent("Button_AgainAndExitCloseBtn", new DefineFactory<Button_AgainAndExitCloseBtn>());
		EventCenter.Self.RegisterEvent("Button_Exit", new DefineFactory<Button_Exit>());
		EventCenter.Self.RegisterEvent("Button_Pause", new DefineFactory<Button_Pause>());

        //EventCenter.Self.RegisterEvent("Button_auto_fight", new DefineFactory<Button_auto_fight>());
        EventCenter.Self.RegisterEvent("AutoBattleAI", new DefineFactory<AutoBattleAI>());

		//----------------------------------------------------------------------------------
		//LandingUI
		EventCenter.Self.RegisterEvent("Button_LandingPushGoinButton", new DefineFactory<Button_LandingPushGoinButton>());
		EventCenter.Self.RegisterEvent("Button_UIForMoMoButton", new DefineFactory<Button_UIForMoMoButton>());
		EventCenter.Self.RegisterEvent("Button_DemoPlayButton", new DefineFactory<Button_DemoPlayButton>());

        
		//----------------------------------------------------------------------------------
		//NameUI
//		EventCenter.Self.RegisterEvent("Button_InputNameOKButton", new DefineFactory<Button_InputNameOKButton>());
		EventCenter.Self.RegisterEvent("Button_InputNameRandomButton", new DefineFactory<Button_InputNameRandomButton>());
		EventCenter.Self.RegisterEvent("Button_MessageOKButton", new DefineFactory<Button_MessageOKButton>());

		//----------------------------------------------------------------------------------
		//SelectRoleUI
        EventCenter.Self.RegisterEvent("Button_ChangeRole", new DefineFactory<Button_ChangeRole>());
        //EventCenter.Self.RegisterEvent("Button_Role01", new DefineFactory<Button_ClickRoleButton>());
        //EventCenter.Self.RegisterEvent("Button_Role02", new DefineFactory<Button_ClickRoleButton>());
        //EventCenter.Self.RegisterEvent("Button_Role03", new DefineFactory<Button_ClickRoleButton>());
        EventCenter.Self.RegisterEvent("Button_Role", new DefineFactory<Button_ClickRoleButton>());
        EventCenter.Self.RegisterEvent("Button_RoleSkinExit", new DefineFactory<Button_RoleSkinExit>());

        
		//----------------------------------------------------------------------------------
		// MainUI
		EventCenter.Self.RegisterEvent("Button_AddStaminaBtn", new DefineFactory<Button_AddStaminaBtn>());		
		EventCenter.Self.RegisterEvent("Button_AddGoldBtn", new DefineFactory<Button_AddGoldBtn>());
		EventCenter.Self.RegisterEvent("Button_AddDiamondBtn", new DefineFactory<Button_AddDiamondBtn>());
		EventCenter.Self.RegisterEvent("Button_TalkBtn", new DefineFactory<Button_TalkBtn>());
		EventCenter.Self.RegisterEvent("Button_Talk", new DefineFactory<Button_TalkBtn>());
		EventCenter.Self.RegisterEvent("Button_SettingBtn", new DefineFactory<Button_SettingBtn>());
        EventCenter.Self.RegisterEvent("Button_battle_settings", new DefineFactory<Button_battle_settings>());
        EventCenter.Self.RegisterEvent("Button_set_button", new DefineFactory<Button_battle_settings>());
		EventCenter.Self.RegisterEvent("Button_on_hook_btn", new DefineFactory<Button_on_hook_btn>());

		EventCenter.Self.RegisterEvent("Button_NoticeBtn", new DefineFactory<Button_NoticeBtn>());
		EventCenter.Self.RegisterEvent("Button_MailBtn", new DefineFactory<Button_MailBtn>());
		EventCenter.Self.RegisterEvent("Button_ChatBtn", new DefineFactory<Button_ChatBtn>());
		EventCenter.Self.RegisterEvent("Button_TaskBtn", new DefineFactory<Button_TaskBtn>());
		EventCenter.Self.RegisterEvent("Button_DailySignBtn", new DefineFactory<Button_DailySignBtn>());
		EventCenter.Self.RegisterEvent("Button_first_pay_button", new DefineFactory<Button_first_pay_button>());
		EventCenter.Self.RegisterEvent ("Button_morrow_land_button", new DefineFactory<Button_morrow_land_button>());
		EventCenter.Self.RegisterEvent ("Button_seven_days_carnival_button", new DefineFactory<Button_seven_days_carnival_button>());

		EventCenter.Self.RegisterEvent("Button_shop_button", new DefineFactory<Button_ShopBtn>());
		EventCenter.Self.RegisterEvent("Button_ShopBtn", new DefineFactory<Button_ShopBtn>());
		EventCenter.Self.RegisterEvent("Button_PVEBtn", new DefineFactory<Button_PVEBtn>());
		EventCenter.Self.RegisterEvent("Button_PVPBtn", new DefineFactory<Button_PVPBtn>());
        EventCenter.Self.RegisterEvent("Button_lilian_btn", new DefineFactory<Button_LilianBtn>());
        EventCenter.Self.RegisterEvent("Button_vip_exclusive_btn",new DefineFactory<Button_vip_exclusive_btn>());
        EventCenter.Self.RegisterEvent("Button_trial_window_back_btn", new DefineFactory<Button_TrialWindowBackBtn>());
		EventCenter.Self.RegisterEvent("Button_five_copy_level_button", new DefineFactory<Button_five_copy_level_button>());
		EventCenter.Self.RegisterEvent("Button_BecomeStrongButton", new DefineFactory<Button_BecomeStrongButton>());
		//ToDo
		EventCenter.Self.RegisterEvent("Button_PointStarBtn", new DefineFactory<Button_PointStarBtn>());
		EventCenter.Self.RegisterEvent("Button_SendMailBtn", new DefineFactory<Button_SendMailBtn>());
        EventCenter.Self.RegisterEvent("Button_RecoverBtn", new DefineFactory<Button_RecoverBtn>());
        EventCenter.Self.RegisterEvent("Button_ToUnionBtn", new DefineFactory<Button_ToUnionBtn>());
        EventCenter.Self.RegisterEvent("Button_ToAlbum",new DefineFactory<Button_ToAlbum>());
		EventCenter.Self.RegisterEvent("Button_SentGuildExpNumBtn", new DefineFactory<Button_SentGuildExpNumBtn>());

        EventCenter.Self.RegisterEvent("Button_OpenRoleDetail", new DefineFactory<Button_OpenRoleDetail>());

		EventCenter.Self.RegisterEvent("Button_IllustratedHandbookBtn", new DefineFactory<Button_IllustratedHandbookBtn>());
		EventCenter.Self.RegisterEvent("Button_FriendBtn", new DefineFactory<Button_FriendBtn>());
		EventCenter.Self.RegisterEvent("Button_PetBtn", new DefineFactory<Button_PetBtn>());
		EventCenter.Self.RegisterEvent("Button_CharacterBtn", new DefineFactory<Button_CharacterBtn>());
		EventCenter.Self.RegisterEvent("Button_UnionBtn", new DefineFactory<Button_UnionBtn>());
        EventCenter.Self.RegisterEvent("Button_ActivityRankBtn", new DefineFactory<Button_ActivityRankBtn>());
        EventCenter.Self.RegisterEvent("Button_flash_sale_single_button", new DefineFactory<Button_flash_sale_single_button>());
        EventCenter.Self.RegisterEvent("Button_flash_sale_multi_button", new DefineFactory<Button_flash_sale_multi_button>());
        
        EventCenter.Self.RegisterEvent("Button_RankBtn", new DefineFactory<Button_RankBtn>());
		EventCenter.Self.RegisterEvent("Button_close_menu_button", new DefineFactory<Button_close_menu_button>());
		EventCenter.Self.RegisterEvent("Button_open_menu_button", new DefineFactory<Button_open_menu_button>());

		EventCenter.Self.RegisterEvent("RoleSelUI_PlayIdleEvent", new DefineFactory<RoleSelUI_PlayIdleEvent>());
		EventCenter.Self.RegisterEvent("PetSelUI_PlayAttackEvent", new DefineFactory<PetSelUI_PlayAttackEvent>());
        //by chenliang
        //begin

////		EventCenter.Self.RegisterEvent("ChouKaAnimitorEnd", new DefineFactory<ChouKaAnimitorEnd>());
//----------------
        //添加该消息
        EventCenter.Self.RegisterEvent("ChouKaAnimitorEnd", new DefineFactory<ChouKaAnimitorEnd>());
        EventCenter.Self.RegisterEvent("Button_add_refine_btn", new DefineFactory<Button_add_refine_btn>());

        //end
//		EventCenter.Self.RegisterEvent("FuChouKaAnimitorEnd", new DefineFactory<FuChouKaAnimitorEnd>());

//		EventCenter.Self.RegisterEvent("Button_invite_code_window_button", new DefineFactory<Button_invite_code_window_button>());
		EventCenter.Self.RegisterEvent("Button_vip_details_button", new DefineFactory<Button_vip_details_button>());
		//----------------------------------------------------------------------------------
		// SelectLevelUI
		//EventCenter.Self.RegisterEvent("Button_select_level_back", new DefineFactory<Button_select_level_back>());
//		EventCenter.Self.RegisterEvent("Button_button_fight", new DefineFactory<Button_button_fight>());
		//EventCenter.Self.RegisterEvent("Button_level_back_button", new DefineFactory<Button_level_back_button>());
		//EventCenter.Self.RegisterEvent("Button_level_forward_button", new DefineFactory<Button_level_forward_button>());

		//EventCenter.Self.RegisterEvent("Button_common_level_button", new DefineFactory<Button_common_level_button>());
		//EventCenter.Self.RegisterEvent("Button_gao_shou_level_button", new DefineFactory<Button_gao_shou_level_button>());
		//EventCenter.Self.RegisterEvent("Button_master_level_button", new DefineFactory<Button_master_level_button>());
		//EventCenter.Self.RegisterEvent("Button_button_level_info", new DefineFactory<Button_button_level_info>());
		EventCenter.Self.RegisterEvent("Button_friend_help_back_button", new DefineFactory<Button_friend_help_back_button>());
		EventCenter.Self.RegisterEvent("Button_friend_help_start_button", new DefineFactory<Button_friend_help_start_button>());
		EventCenter.Self.RegisterEvent("Button_friend_help_select_button", new DefineFactory<Button_friend_help_select_button>());
		EventCenter.Self.RegisterEvent("Button_friend_help_cancel_button", new DefineFactory<Button_friend_help_cancel_button>());
		EventCenter.Self.RegisterEvent("Button_friend_help_element_ranking_button", new DefineFactory<Button_friend_help_element_ranking_button>());
		EventCenter.Self.RegisterEvent("Button_friend_help_level_ranking_button", new DefineFactory<Button_friend_help_level_ranking_button>());
		EventCenter.Self.RegisterEvent("Button_friend_help_star_ranking_button", new DefineFactory<Button_friend_help_star_ranking_button>());
		EventCenter.Self.RegisterEvent("Button_friend_help_sifting_button", new DefineFactory<Button_friend_help_sifting_button>());


		EventCenter.Self.RegisterEvent("Button_fa_bao_button", new DefineFactory<Button_fa_bao_button>());
		EventCenter.Self.RegisterEvent("Button_switch_button", new DefineFactory<Button_CharacterBtn>());
		EventCenter.Self.RegisterEvent("Button_change_team_button", new DefineFactory<Button_ChangeTeamBtn>());
        EventCenter.Self.RegisterEvent("Button_change_role_button", new DefineFactory<Button_ChangeRoleBtn>());

		//----------------------------------------------------------------------------------
		// AllRoleAttributeInfoUI
		EventCenter.Self.RegisterEvent("Button_AllRoleAttributeInfoBack", new DefineFactory<Button_AllRoleAttributeInfoBack>());
		EventCenter.Self.RegisterEvent("Button_RoleInfoBtn", new DefineFactory<Button_RoleInfoBtn>());
        EventCenter.Self.RegisterEvent("Button_RoleEvolutionBtn", new DefineFactory<Button_RoleEvolutionBtn>());
        EventCenter.Self.RegisterEvent("Button_RoleEquipCultivateBtn", new DefineFactory<Button_RoleEquipCultivateBtn>());
        EventCenter.Self.RegisterEvent("Button_RoleEquipCompositionBtn", new DefineFactory<Button_RoleEquipCompositionBtn>());

        EventCenter.Self.RegisterEvent("Button_RoleEquipStrengthenBtn", new DefineFactory<Button_RoleEquipStrengthenBtn>());
        //EventCenter.Self.RegisterEvent("Button_RoleEquipEvolutionBtn", new DefineFactory<Button_RoleEquipEvolutionBtn>());
        EventCenter.Self.RegisterEvent("Button_RoleEquipResetBtn", new DefineFactory<Button_RoleEquipResetBtn>());

        EventCenter.Self.RegisterEvent("Button_role_upgrade_auto_add_btn", new DefineFactory<Button_RoleEquipStrengthenAutoAddBtn>());

		EventCenter.Self.RegisterEvent("Button_RoleEquipStrengthenCloseBtn", new DefineFactory<Button_RoleEquipExpansionInfoWindowCloseBtn>());
		EventCenter.Self.RegisterEvent("Button_RoleEquipEvolutionCloseBtn", new DefineFactory<Button_RoleEquipExpansionInfoWindowCloseBtn>());
		EventCenter.Self.RegisterEvent("Button_RoleEquipResetCloseBtn", new DefineFactory<Button_RoleEquipExpansionInfoWindowCloseBtn>());

		EventCenter.Self.RegisterEvent("Button_RoleEquipStrengthenOKBtn", new DefineFactory<Button_RoleEquipStrengthenOKBtn>());
		//EventCenter.Self.RegisterEvent("Button_RoleEquipEvolutionOKBtn", new DefineFactory<Button_RoleEquipEvolutionOKBtn>());
		EventCenter.Self.RegisterEvent("Button_RoleEquipResetOKBtn", new DefineFactory<Button_RoleEquipResetOKBtn>());

		EventCenter.Self.RegisterEvent("Button_RoleEquipUseBtn", new DefineFactory<Button_RoleEquipUseBtn>());
		EventCenter.Self.RegisterEvent("Button_RoleEquipUnUseBtn", new DefineFactory<Button_RoleEquipUnUseBtn>());
		EventCenter.Self.RegisterEvent("Button_RoleEquipSaleBtn", new DefineFactory<Button_RoleEquipSaleBtn>());

		EventCenter.Self.RegisterEvent("Button_had_locked_button", new DefineFactory<Button_UnlockButton>());
		EventCenter.Self.RegisterEvent("Button_lock_button", new DefineFactory<Button_LockButton>());
		//----------------------------------------------------------------------------------
		// AllPetAttributeInfoUI
		EventCenter.Self.RegisterEvent("Button_AllPetAttributeInfoBack", new DefineFactory<Button_AllPetAttributeInfoBack>());
		EventCenter.Self.RegisterEvent("Button_PetInfoBtn", new DefineFactory<Button_PetInfoBtn>());
		EventCenter.Self.RegisterEvent("Button_PetUpgradeBtn", new DefineFactory<Button_PetUpgradeBtn>());
        EventCenter.Self.RegisterEvent("Button_PetSkillBtn", new DefineFactory<Button_PetSkillBtn>());
		EventCenter.Self.RegisterEvent("Button_PetDecomposeBtn", new DefineFactory<Button_PetDecomposeBtn>());
		EventCenter.Self.RegisterEvent("Button_PetEvolutionBtn", new DefineFactory<Button_PetEvolutionBtn>());
		EventCenter.Self.RegisterEvent("Button_pet_sale_button", new DefineFactory<Button_PetSaleButton>());
		EventCenter.Self.RegisterEvent("Button_pet_info_auto_join_button", new DefineFactory<Button_AutoJoinButton>());
		EventCenter.Self.RegisterEvent("Button_tactical_formation_button", new DefineFactory<TacticalFormationOpenButton>());
		
		EventCenter.Self.RegisterEvent("Button_pet_play_check_box_btn", new DefineFactory<Button_pet_play_check_box_btn>());
		EventCenter.Self.RegisterEvent("Button_pet_play_flag_btn", new DefineFactory<Button_pet_play_flag_btn>());
		
		EventCenter.Self.RegisterEvent("Button_Upgrade", new DefineFactory<Button_Upgrade>());
		EventCenter.Self.RegisterEvent("Button_Upgrade_JinHua", new DefineFactory<Button_Upgrade_JinHua>());
		//EventCenter.Self.RegisterEvent("Button_Upgrade_QiangHua", new DefineFactory<Button_Upgrade_QiangHua>());
		EventCenter.Self.RegisterEvent("Button_Upgrade_unuse", new DefineFactory<Button_Upgrade_Unuse>());
        EventCenter.Self.RegisterEvent("Button_Upgrade_Skill", new DefineFactory<Button_Upgrade_Skill>());

//		EventCenter.Self.RegisterEvent("Button_pet_info_team_forward_button", new DefineFactory<Button_pet_info_team_forward_button>());
//		EventCenter.Self.RegisterEvent("Button_pet_info_team_back_button", new DefineFactory<Button_pet_info_team_back_button>());

		EventCenter.Self.RegisterEvent("Button_PetAndStoneSelBtn", new DefineFactory<Button_PetAndStoneSelBtn>());
		
		EventCenter.Self.RegisterEvent("Button_ChoosePet", new DefineFactory<Button_ChoosePet>());
		
		EventCenter.Self.RegisterEvent("Button_PetUpgradeBtnOK", new DefineFactory<Button_PetUpgradeBtnOK>());
		EventCenter.Self.RegisterEvent("Button_PetStrengthenOK", new DefineFactory<Button_PetStrengthenOK>());
		EventCenter.Self.RegisterEvent("Button_PetEvolutionBtnOK", new DefineFactory<Button_PetEvolutionBtnOK>());

		EventCenter.Self.RegisterEvent("Button_evolution_gain_pet_info_window_close_btn", new DefineFactory<Button_evolution_gain_pet_info_window_close_btn>());
		
		EventCenter.Self.RegisterEvent("Button_UpGradeAndStrengthenWindowOKBtn", new DefineFactory<Button_UpGradeAndStrengthenWindowCloseBtn>());

		EventCenter.Self.RegisterEvent("Button_pet_upgrade_auto_add_pet_btn", new DefineFactory<Button_PetUpgradeAutoAddPetBtn>());

		// bag
		EventCenter.Self.RegisterEvent("Button_pet_icon_check_btn", new DefineFactory<Button_pet_icon_check_btn>());
		EventCenter.Self.RegisterEvent("Button_pet_icon_upgrade_btn", new DefineFactory<Button_pet_icon_upgrade_btn>());
		EventCenter.Self.RegisterEvent("Button_stone_icon_sel_btn(Clone)", new DefineFactory<Button_stone_icon_sel_btn>());
		EventCenter.Self.RegisterEvent("Button_role_equip_icon_btn", new DefineFactory<ButtonRoleEquipIconBtn>());
		EventCenter.Self.RegisterEvent("Button_role_equip_strengthen_icon_btn", new DefineFactory<ButtonRoleEquipStrengthenIconBtn>());

		EventCenter.Self.RegisterEvent("Button_pet_group_sifting_button", new DefineFactory<Button_pet_group_sifting_button>());
		EventCenter.Self.RegisterEvent("Button_pet_star_btn", new DefineFactory<Button_PetStarBtn>());
		EventCenter.Self.RegisterEvent("Button_pet_level_btn", new DefineFactory<Button_PetLevelBtn>());
		EventCenter.Self.RegisterEvent("Button_pet_attribute_btn", new DefineFactory<Button_PetAttributeBtn>());

		EventCenter.Self.RegisterEvent("Button_role_equip_star_btn", new DefineFactory<Button_RoleEquipStarBtn>());
		EventCenter.Self.RegisterEvent("Button_role_equip_strengthen_level_btn", new DefineFactory<Button_RoleEquipStrengthenLevelBtn>());
		EventCenter.Self.RegisterEvent("Button_role_equip_attribute_btn", new DefineFactory<Button_RoleEquipAttributeBtn>());


		//----------------------------------------------------------------------------------
		// common ui
		EventCenter.Self.RegisterEvent("Button_PetInfoSingleWindowCloseBtn", new DefineFactory<Button_PetInfoSingleCloseBtn>());
		EventCenter.Self.RegisterEvent("Button_PetInfoSingleOKBtn", new DefineFactory<Button_PetInfoSingleOKBtn>());
		EventCenter.Self.RegisterEvent("Button_PetInfoSingleSaleBtn", new DefineFactory<Button_PetInfoSingleSaleBtn>());

		EventCenter.Self.RegisterEvent("Button_PetInfoDescriptionWindowCloseBtn", new DefineFactory<Button_PetInfoDescriptionWindowCloseBtn>());

		EventCenter.Self.RegisterEvent("Button_team_forward_button", new DefineFactory<Button_team_forward_button>());
		EventCenter.Self.RegisterEvent("Button_team_back_button", new DefineFactory<Button_team_back_button>());

		EventCenter.Self.RegisterEvent("Button_Button1", new DefineFactory<Button_TipButton>());
		EventCenter.Self.RegisterEvent("Button_Button2", new DefineFactory<Button_TipButton>());
		EventCenter.Self.RegisterEvent("Button_Button3", new DefineFactory<Button_TipButton>());
		EventCenter.Self.RegisterEvent("Button_icon_tips_btn", new DefineFactory<Button_TipButton>());

		EventCenter.Self.RegisterEvent("Button_tip_background", new DefineFactory<Button_TipBackground>());
		EventCenter.Self.RegisterEvent("Button_skill_tip", new DefineFactory<Button_SkillTip>());
		EventCenter.Self.RegisterEvent("Button_skill", new DefineFactory<Button_SkillTip>());
        EventCenter.Self.RegisterEvent("Button_tab_btn", new DefineFactory<Button_tab_btn>());

		EventCenter.Self.RegisterEvent("Button_tip_btn", new DefineFactory<Button_TipButton>());

		//----------------------------------------------------------------------------------
        //BattleEndUI
        EventCenter.Self.RegisterEvent("Button_but_again", new DefineFactory<Button_Again>());
		EventCenter.Self.RegisterEvent("Button_but_next", new DefineFactory<Button_but_next>());
		EventCenter.Self.RegisterEvent("Button_but_select", new DefineFactory<Button_but_select>());
        EventCenter.Self.RegisterEvent("Button_but_clean", new DefineFactory<Button_but_clean>());
        EventCenter.Self.RegisterEvent("Button_but_close", new DefineFactory<Button_but_close>());
        EventCenter.Self.RegisterEvent("Button_quit_boss_battle", new DefineFactory<Button_quit_boss_battle>());
		EventCenter.Self.RegisterEvent("Button_boss_hurt_rank", new DefineFactory<Button_damage_button>());//boss damage rank button
        EventCenter.Self.RegisterEvent("Button_again_boss_battle", new DefineFactory<Button_again_boss_battle>());
		EventCenter.Self.RegisterEvent("Button_world_map_button", new DefineFactory<Button_world_map_button>());
		EventCenter.Self.RegisterEvent("Button_but_back_home_page", new DefineFactory<Button_but_back_home_page>());

        //----------------------------------------------------------------------------------
        //PetAtlasUI
        EventCenter.Self.RegisterEvent("Button_button_type_0", new DefineFactory<ButtonTypeEvent>());
        EventCenter.Self.RegisterEvent("Button_button_type_1", new DefineFactory<ButtonTypeEvent>());
        EventCenter.Self.RegisterEvent("Button_button_type_2", new DefineFactory<ButtonTypeEvent>());
        EventCenter.Self.RegisterEvent("Button_button_type_3", new DefineFactory<ButtonTypeEvent>());
        EventCenter.Self.RegisterEvent("Button_button_type_4", new DefineFactory<ButtonTypeEvent>());
        EventCenter.Self.RegisterEvent("Button_button_type_11", new DefineFactory<ButtonTypeEvent>());
        EventCenter.Self.RegisterEvent("Button_SubCell(Clone)", new DefineFactory<SubCellClick>());
        EventCenter.Self.RegisterEvent("Button_PetAtlasDetailsClose", new DefineFactory<PetAtlasDetailClose>());
        EventCenter.Self.RegisterEvent("Button_PetAtlasReturnBack", new DefineFactory<PetAtlasReturnBack>());

        EventCenter.Self.RegisterEvent("Button_BtnKnightPanelClose", new DefineFactory<BtnKnightPanelClose>());
        EventCenter.Self.RegisterEvent("Button_ButtonPageLeft", new DefineFactory<ButtonPageLeft>());
        EventCenter.Self.RegisterEvent("Button_ButtonPageRight", new DefineFactory<ButtonPageRight>());
        //EventCenter.Self.RegisterEvent("Button_ButtonPageRight", new DefineFactory<ButtonPageRight>());
        
		//----------------------------------------------------------------------------------
		// MissionWindowUI
        EventCenter.Self.RegisterEvent("Button_MissionWindowBack", new DefineFactory<Button_MissionWindowBack>());
        EventCenter.Self.RegisterEvent("Button_daily_tasks_btn", new DefineFactory<Button_daily_tasks_btn>());
        EventCenter.Self.RegisterEvent("Button_achievment_tasks_btn", new DefineFactory<Button_achievment_tasks_btn>());
        EventCenter.Self.RegisterEvent("Button_weekly_tasks_btn", new DefineFactory<Button_weekly_tasks_btn>());
        EventCenter.Self.RegisterEvent("Button_activity_tasks_btn", new DefineFactory<Button_activity_tasks_btn>());
        EventCenter.Self.RegisterEvent("Button_but_get_task_award", new DefineFactory<Button_but_get_task_award>());
		EventCenter.Self.RegisterEvent("Button_go_task_btn", new DefineFactory<Button_go_task_btn>());

        //----------------------------------------------------------------------------------
        // ShopWindowUI

		EventCenter.Self.RegisterEvent("Button_shop_window_back", new DefineFactory<Button_shop_window_back>());
		/*
		EventCenter.Self.RegisterEvent("Button_pet_shop_btn", new DefineFactory<Button_pet_shop_btn>());
        EventCenter.Self.RegisterEvent("Button_tool_shop_btn", new DefineFactory<Button_tool_shop_btn>());
        EventCenter.Self.RegisterEvent("Button_character_skin_shop_btn", new DefineFactory<Button_character_skin_shop_btn>());
        EventCenter.Self.RegisterEvent("Button_gold_shop_btn", new DefineFactory<Button_gold_shop_btn>());
        EventCenter.Self.RegisterEvent("Button_diamond_shop_btn", new DefineFactory<Button_diamond_shop_btn>());
		EventCenter.Self.RegisterEvent("Button_mysterious_shop_btn", new DefineFactory<Button_mysterious_shop_btn>());
		*/
        EventCenter.Self.RegisterEvent("Button_jump_animator_button", new DefineFactory<Button_jump_animator_button>());

		EventCenter.Self.RegisterEvent("Button_shop_gain_pet_window_close_btn", new DefineFactory<Button_shop_gain_pet_window_close_btn>());
		EventCenter.Self.RegisterEvent("Button_shop_gain_pet_window_details_btn", new DefineFactory<Button_shop_gain_pet_window_details_btn>());

		EventCenter.Self.RegisterEvent("Button_shop_gain_item_window_close_btn", new DefineFactory<Button_shop_gain_item_window_close_btn>());

		EventCenter.Self.RegisterEvent("Button_shop_gain_pet_and_info_window_close_btn", new DefineFactory<Button_shop_gain_pet_and_info_window_close_btn>());

		EventCenter.Self.RegisterEvent("Button_but_shop_buy", new DefineFactory<Button_but_shop_buy>());
        EventCenter.Self.RegisterEvent("Button_but_shop_buy_disable", new DefineFactory<Button_but_shop_buy_disable>());
        //by chenliang
        //begin

//		EventCenter.Self.RegisterEvent("Button_but_shop_buy_again", new DefineFactory<Button_but_shop_buy_again>());
//----------------
        EventCenter.Self.RegisterEvent("Button_but_shop_buy_again", new DefineFactoryLog<Button_Chouka_Buy_Again>());

        //end

        // Rank UI
        EventCenter.Self.RegisterEvent("Button_rank_window_back", new DefineFactory<Button_rank_window_back>());
        EventCenter.Self.RegisterEvent("Button_arena_rank_btn", new DefineFactory<Button_arena_rank_btn>());
        EventCenter.Self.RegisterEvent("Button_element_rank_btn", new DefineFactory<Button_element_rank_btn>());
        EventCenter.Self.RegisterEvent("Button_role_fight_rank_btn", new DefineFactory<Button_role_fight_rank_btn>());
        EventCenter.Self.RegisterEvent("Button_union_fight_rank_btn", new DefineFactory<Button_union_fight_rank_btn>());
        EventCenter.Self.RegisterEvent("Button_union_battle_rank_btn", new DefineFactory<Button_union_battle_rank_btn>());

        // Help Tips
        EventCenter.Self.RegisterEvent("Button_help_tip_1", new DefineFactory<Button_help_tip>());
        EventCenter.Self.RegisterEvent("Button_help_tip_2", new DefineFactory<Button_help_tip>());
        EventCenter.Self.RegisterEvent("Button_help_tip_3", new DefineFactory<Button_help_tip>());
        EventCenter.Self.RegisterEvent("Button_help_tip_4", new DefineFactory<Button_help_tip>());
        EventCenter.Self.RegisterEvent("Button_help_tip_5", new DefineFactory<Button_help_tip>());
        EventCenter.Self.RegisterEvent("Button_help_tip_6", new DefineFactory<Button_help_tip>());
        EventCenter.Self.RegisterEvent("Button_help_tip_7", new DefineFactory<Button_help_tip>());
        EventCenter.Self.RegisterEvent("Button_help_tip_8", new DefineFactory<Button_help_tip>());
        EventCenter.Self.RegisterEvent("Button_help_tip_9", new DefineFactory<Button_help_tip>());
        EventCenter.Self.RegisterEvent("Button_help_tip_10", new DefineFactory<Button_help_tip>());
        EventCenter.Self.RegisterEvent("Button_help_tip_11", new DefineFactory<Button_help_tip>());
        EventCenter.Self.RegisterEvent("Button_help_tip_12", new DefineFactory<Button_help_tip>());
        EventCenter.Self.RegisterEvent("Button_help_tip_13", new DefineFactory<Button_help_tip>());
        EventCenter.Self.RegisterEvent("Button_help_tip_14", new DefineFactory<Button_help_tip>());

		//button_gary
		EventCenter.Self.RegisterEvent("Button_Upgrade_gray", new DefineFactory<Button_gary>());
		EventCenter.Self.RegisterEvent("Button_Upgrade_JinHua_gray", new DefineFactory<Button_gary>());
		//EventCenter.Self.RegisterEvent("Button_Upgrade_QiangHua_gray", new DefineFactory<Button_gary>());
		EventCenter.Self.RegisterEvent("Button_Upgrade_Skill_gray", new DefineFactory<Button_gary>());
		EventCenter.Self.RegisterEvent("Button_FriendBtn_gray", new DefineFactory<Button_gary>());
		EventCenter.Self.RegisterEvent("Button_PVPBtn_gray", new DefineFactory<Button_gary>());
		EventCenter.Self.RegisterEvent("Button_fa_bao_button_gray", new DefineFactory<Button_gary>());
		EventCenter.Self.RegisterEvent("Button_UnionBtn_gray", new DefineFactory<Button_gary>());
		EventCenter.Self.RegisterEvent("Button_stage_info_clean_gray", new DefineFactory<Button_gary>());
		EventCenter.Self.RegisterEvent("Button_RoleEquipStrengthenBtn_gray", new DefineFactory<Button_gary>());
		EventCenter.Self.RegisterEvent("Button_RoleEquipResetBtn_gray", new DefineFactory<Button_gary>());
		EventCenter.Self.RegisterEvent("Button_explore_land_button_gray", new DefineFactory<Button_gary>());
		EventCenter.Self.RegisterEvent("Button_EnterRoleEvolutionPageBtn_gray", new DefineFactory<Button_gary>());
		EventCenter.Self.RegisterEvent("Button_role_equip_info_reset_button_gray", new DefineFactory<Button_gary>());
		EventCenter.Self.RegisterEvent("Button_role_equip_info_strengthen_button_gray", new DefineFactory<Button_gary>());

        EventCenter.Self.RegisterEvent("Button_skip_prologue", new DefineFactory<Button_skip_prologue>());

		EventCenter.Self.RegisterEvent("Button_order_button", new DefineFactory<Button_order_button>());

		EventCenter.Register ("Button_active_list_button", new DefineFactory<Button_active_list_button>());
		EventCenter.Register ("Button_open_fund_button", new DefineFactory<Button_open_fund_button>());
		EventCenter.Register ("Button_first_recharge_button", new DefineFactory<Button_first_recharge_button>());

        EventCenter.Register("Button_PackageEquipBtn", new DefineFactory<Button_PackageEquipBtn>());
        EventCenter.Register("Button_PackageConsumeBtn", new DefineFactory<Button_PackageConsumeBtn>());
        EventCenter.Register("Button_close_owner_window", new DefineFactory<Button_close_owner_window>());

        //by chenliang
        //begin

        EventCenter.Self.RegisterEvent("Button_map_boss_list", new DefineFactory<Button_map_boss_list>());

        //end

		EventCenter.Self.RegisterEvent("Button_go_to_get_btn", new DefineFactory<Button_go_to_get_btn>());

        // added by xuke
        // 射手乐园
        EventCenter.Self.RegisterEvent("ShootPark_RoleShoot",new DefineFactory<ShooterPark_RoleShoot>());
        // end

        EventCenter.Self.RegisterEvent("Button_skip_battle", new DefineFactory<Button_skip_battle>());      
    }
}

