P = Buff()


function Init()
	title = "妄仇 Lv."..var10
	describe = "\n\n死亡时眩晕自身"..var1.."码范围内的全部敌人"..var2.."秒，将己方所有符灵的再次召唤间隔时间清零"
end



function OnDead()
	
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then

			
			ApplyBuff(obj, var3 + var10)
			
		end
		
		if not owner:IsEnemy(obj) and tostring (obj:GetConfig("CLASS")) == "CHAR" then

			
			obj:RemoveSummonCD()
			
		end
	end

	
end



return P