P = Buff()


title = "自爆"

function Init()
	describe = "死亡时对自身范围"..var1.."码内的敌人造成"..var2.."点伤害，并使其附带灼烧效果"
end


function OnDead()
	
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			obj:ChangeHP(var2 * -50)
			ApplyBuff(obj, 90002)
			
		end
	
	end

	
end


return P