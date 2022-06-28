P = Buff()

function Init()
	title = "冰壁 Lv."..var10
	describe = "\n\n死亡时使自身范围"..var1.."码内的敌人冰冻"..var2.."秒"
end


function OnDead()
	
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then

			
			ApplyBuff(obj, var3 + var10)
			
		end
	
	end

	
end


return P