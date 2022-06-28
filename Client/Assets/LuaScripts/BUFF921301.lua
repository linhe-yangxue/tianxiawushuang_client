P = Buff()


title = "归元"

function Init()
	title = "归元 Lv."..var10
	local HP_R = var2 + var3 * var10
	if HP_R > 100 then
		HP_R = 100
	end
	describe = "\n\n死亡时，使自身范围"..var1.."码内的全体友方角色回复"..HP_R.."%的生命值"
end


function OnDead()
	local skillID = tostring(owner:GetConfig("AFFECT_BUFFER_1"))
	local skillLV = tonumber(skillID) % 100
	
	HP_R = var2 + var3 * skillLV
	
	for _, obj in pairs(Objects) do
		if not owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			local M_HP = obj:GetFinal("HP_MAX")
			local HP = M_HP * HP_R / 100
			HP = math.modf(HP)
			obj:ChangeHP(HP)
			eff = obj:PlayEffect(3000)
			StartCoroutine(CO,1)
			
		end
	
	end

	
end

function OnRemove()
	if eff then
		eff:Finish()
	end
end

function CO(i)
	if eff then
		Wait(1)
		eff:Finish()
	end
end

return P