

P = Buff()


function Init()
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var1 + var2*lv
	title = "散毒 Lv."..lv
	describe = "\n\n死亡时使自身范围"..var1.."码内的敌人中毒"..var2.."秒"
end
function Start()
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var3 + lv		
	print("散毒被动，ID=911601")

end

function OnDead()

	print("进入死亡遍历流程")
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then

			ApplyBuff(obj, rmp)
			
		end	
	end	
end


return P