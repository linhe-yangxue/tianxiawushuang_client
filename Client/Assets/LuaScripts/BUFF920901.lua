P = Buff()

totaltime = 0
deltatime = 0



function Init()
	title = "常弱 Lv."..var10
	var2 = var2 / 100
	describe = "\n\n攻击命中时降低目标"..var1.."点防御力，当场上有【勾魂使·墨】时，效果为"..var2.."倍"
end


function OnHit(target,damage)

	ApplyBuff(target, var4)

	for _, obj in pairs(Objects) do
		local ID = tostring (obj:GetConfig("INDEX"))
		if not owner:IsEnemy(obj) then
			if ID == tostring (var3)  then
				ApplyBuff(target, var5)

			end
		end
	end
	
end

return P