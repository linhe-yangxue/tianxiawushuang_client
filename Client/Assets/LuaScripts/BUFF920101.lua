P = Buff()


function Init()
	title = "狂暴 Lv."..var10
	describe = "\n\n自身每减少"..var1.."%生命值就提升"..var2.."%攻击力"
end

function Start()
	local atk = owner:GetStatic("ATTACK")
	local MAXHP = owner:GetFinal("HP_MAX")		
	local HP = owner:GetHP()
		
	local HP_DMG_R = (MAXHP - HP) / MAXHP * 100
	local ATKUP_R = HP_DMG_R / var1
	ATKUP_R = math.modf(ATKUP_R)
	
	if ATKUP_R ~= 0 then
		eff = owner:PlayEffect(1003)
	end
	
	atk = atk * (var2 / 100) * ATKUP_R

	
	return {ATTACK = atk}

end

function OnDamage(ATKER, DMG)
	local atk = owner:GetStatic("ATTACK")
	local MAXHP = owner:GetFinal("HP_MAX")		
	local HP = owner:GetHP()
	
	
	local HP_DMG_R = (MAXHP - HP) / MAXHP * 100
	local ATKUP_R = HP_DMG_R / var1
	ATKUP_R = math.modf(ATKUP_R)
	
	if ATKUP_R ~= 0 then
		eff = owner:PlayEffect(1003)
	end
	
	atk = atk * (var2 / 100) * ATKUP_R
	
	return {ATTACK = atk}

end



function OnRemove()
	if eff then
		eff:Finish()
	end
end
return P