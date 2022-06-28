



P = Buff()


function Init()
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var1 + var2*lv	
	title = "恢复 Lv."..lv
	describe = "\n\n被召唤时增加所有友方当前生命"..rmp.."点"
end


function Start()
	
	print("恢复被动，ID=911801")
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var1 + var2*lv 
	print("等级="..lv..";效果="..rmp)
	
	for _, obj in pairs(Objects) do
		if not owner:IsEnemy(obj) then
			
		obj:ChangeHP(rmp)
		
		end
	end	
end

return P