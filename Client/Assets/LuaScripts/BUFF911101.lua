



P = Buff()



function Init()
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var1 + var2*lv
	title = "毒气 Lv."..lv
	describe = "\n\n符灵在被召唤时对"..var1.."米范围内敌人附加中毒状态"
end


function Start()
	
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var2 + lv
	print(rmp)
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			ApplyBuff(obj,rmp)
		end	
	end
end


return P