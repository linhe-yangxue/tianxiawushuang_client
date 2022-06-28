P = Buff()


function Init()
	title = "金盾 Lv."..var10

	describe = "\n\n召唤时增加自身范围"..var1.."码内全体友方角色"..var2.."%防御力，增加"..var3.."秒存活时间"
end

function Start()
	
	for _, obj in pairs(Objects) do
		if not owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			ApplyBuff(obj, var4)
			
		end
	
	end
	return {LIFE_TIME = var3}
	
end


return P