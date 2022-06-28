
P = Buff()



function Init()
	
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var3 + lv
	slow = var4 + (var5 * lv)
	title = "击退 Lv."..lv
	describe = "\n\n符灵在攻击时，将使自身"..var1.."米范围内的敌人减速"..slow.."%，持续"..var2.."秒"
end


function OnHit(target,damage)
	
	print("击退被动，ID=912301")
	print("1="..lv..";2="..rmp)
	
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,10) then

			ApplyBuff(obj, rmp)
				print("附加成功")
		end	


	end	
end


return P