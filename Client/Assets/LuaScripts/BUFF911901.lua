	



P = Buff()


function Init()
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	title = "定身 Lv."..lv
	describe = "\n\n被召唤时对自身范围"..var1.."码内的敌人造成定身"..lv.."秒的效果"
end


function Start()
	
	lv1 = var2 + lv
		
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			ApplyBuff(obj,lv1)
		end	
	end	
end


return P