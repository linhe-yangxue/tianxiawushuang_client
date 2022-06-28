	



P = Buff()


function Init()
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var1 + var2*lv
	title = "闪爆 Lv."..lv
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	describe = "\n\n被召唤时对自身范围"..var1.."码内的敌人造成眩晕"..var2*lv.."秒的效果"
end


function Start()
	
	lv = var4 + tonumber(string.sub(index,tonumber(string.len(index))))
		
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			ApplyBuff(obj,lv)
		end	
	end	
end


return P