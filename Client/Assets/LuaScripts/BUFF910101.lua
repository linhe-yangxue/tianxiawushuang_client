P = Buff()




function Init()
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var1 + var2*lv
	title = "冰爆 Lv."..lv
	describe = "\n\n死亡时对自身范围"..var1.."码内的敌人造成"..var2.."点伤害，并使其冰冻"..var3.."秒"
end
function Start()
	print("冰爆被动，ID=910101")
end

function OnDead()
	print("进入死亡遍历流程")
	for _, obj in pairs(Objects) do
		print("遍历完毕，进入判断流程")
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			print("判断完毕，进入掉血加buff流程")
			obj:ChangeHP(var2 * -1)
			ApplyBuff(obj, rmp)
		end
	
	end
	
end


return P