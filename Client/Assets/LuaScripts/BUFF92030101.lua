P = Buff()

totaltime = 0
deltatime = 0

title = "威压"

function Init()
	describe = "如果目标生命低于"..var1.."%时，将对其造成"..var2.."%的额外伤害"
end


function OnDamage(ATKER, DMG)
	local atk = ATKER:GetFinal("ATTACK")
	local ID = tostring(ATKER:GetConfig("INDEX"))
	local MAXHP = owner:GetFinal("HP_MAX")		
	local HP = owner:GetHP()
	
	if ID == tostring (var3) then
		local HP_REM_R = HP / MAXHP * 100  --取得剩余血量
		if HP_REM_R <= var1 then
			
			eff = owner:PlayEffect(1013)

			owner:ChangeHP(-1 * atk * var2 / 100 )
			
		end
	end
end


function OnRemove()
	if eff then
		eff:Finish()
	end
end

return P