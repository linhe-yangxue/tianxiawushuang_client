



P = Buff()

--title = "无言"

function Init()
	--describe = "被召唤时对自身范围"..var1.."码内的敌人造成眩晕"..var2.."秒的效果"
end


function Start()
	lv1 = this:GetConfig("INDEX")
	lv = 1--tonumber(string.char(lv1,5))
		
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			ApplyBuff(obj,var4+lv)
		end	
	end	
end


return P