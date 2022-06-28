



P = Buff()



function Init()
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var1 + var2*lv	
	title = "迅捷 Lv."..lv
	describe = "\n\n被召唤时增加所有友方闪避"..rmp.."%"
end


function Start()
	print("迅捷被动，ID=911301")

	print("1="..lv..";2="..rmp)
	
	for _, obj in pairs(Objects) do
		if not owner:IsEnemy(obj) then
			
		return {DODGE_RATE = rmp}
		
		end
	end	
end

return P