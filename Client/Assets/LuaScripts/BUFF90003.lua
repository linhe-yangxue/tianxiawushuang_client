P = Buff()


title = "眩晕"

function Init()
	describe = "被召唤时对自身范围"..var1.."码内的敌人造成眩晕"..var2.."秒的效果"
end


function Start()
	
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			ApplyBuff(obj,3005)
		end
	
	end

	
end


return P