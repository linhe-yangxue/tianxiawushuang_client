



P = Buff()



function Init()
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var1 + var2*lv		
	title = "盛怒 Lv."..lv
	describe = "\n\n被召唤时增加所有友方暴击伤害"..rmp.."%"
end


function Start()

	print("盛怒被动，ID=911401")
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var1 + var2*lv 
	print("1="..lv..";2="..rmp)
	
	for _, obj in pairs(Objects) do
		if not owner:IsEnemy(obj) then
			
		return {HIT_CRITICAL_STRIKE_RATE = rmp}
		
		end
	end	
end

return P