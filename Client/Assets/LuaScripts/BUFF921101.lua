P = Buff()


function Init()
	title = "挫勇 Lv."..var10
	describe = "\n\n自身每降低"..var1.."%血量，减少受到的"..var2.."%伤害"
end


function Update()
	local MAXHP = owner:GetFinal("HP_MAX")		
	local HP = owner:GetHP()
	
	local HP_REM_R = HP / MAXHP * 100  --取得剩余血量
	HP_REM_R = math.modf(HP_REM_R/var1)
	local DMG_R = var2 * HP_REM_R
	
	return {DEFENSE_HIT_RATE = DMG_R}

end


return P