﻿@Switch_YuanBao_Icon yuanBaoCost > 0

@Set_YuanBao_Cost
yuanBaoCost>0 is yuanBaoCost
else is empty

@Switch_Contri_Icon contriCost>0

@Set_Contri_Cost
contriCost>0 is contriCost
else is empty

@Switch_CanBuy
limitEach-boughtNumEach>0&&guildLevel-guildLimitLevel>=0||limitEach==0 is "购买"
else is "不可购买"#BLACK

@Switch_CanBuy_Btn
limitEach-boughtNumEach>0 &&guildLevel-guildLimitLevel>=0||limitEach==0

@Set_Name tid
@Set_UI_Sprite tid

@Set_Remain_Buy_Count_Each
limitEach==0 is empty
limitEach-boughtNumEach>0 is "可购买{0}次"$(limitEach-boughtNumEach)
else is "已达购买上限"

@Set_Remain_Buy_Count_Guild
limitGuild-boughtNumGuild>0 is "总会剩余{0}件"$(limitGuild-boughtNumGuild)
else is "已卖光"

@Set_Each_Deal_Num
eachDealNum>1 is "×"+eachDealNum
else is empty
