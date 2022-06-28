P = Buff()


function Init()
	
	title = "神怒 Lv."..var10
	describe = "\n\n攻击命中时使自身范围"..var1.."码内全体友方角色获得增强效果，同时提升"..var2.."%攻击力与"..var3.."%暴击率，持续"..var4.."秒"
end


function OnHit(target, damage)
	
	for _, obj in pairs(Objects) do
		if not owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			ApplyBuff(obj, var4)			
		end
	
	end

	
end


return P