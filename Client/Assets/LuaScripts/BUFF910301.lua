P = Buff()





function Init()
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var1 + var2*lv
	title = "炎爆 Lv."..lv
	describe = "\n\n死亡时对自身范围"..var1.."码内的敌人造成"..var2.."点伤害，并使其灼烧，持续"..var3.."秒"
end

function Start()
	print("炎爆被动，ID=910301")
	
	
end

function OnDead()
	lv = var4 + tonumber(string.sub(index,tonumber(string.len(index))))
		print(lv)
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			obj:ChangeHP(var2 * -1)
			ApplyBuff(obj, lv)
			
		end	
	end	
end


return P