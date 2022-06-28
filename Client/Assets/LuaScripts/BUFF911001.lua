



P = Buff()



function Init()
	
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var1 + var2*lv
	title = "精力 Lv."..lv
	describe = "\n\n符灵在被召唤时恢复主角法力"..rmp.."点"
end


function Start()
	
	print("精力被动，ID=911001")
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var1 + var2*lv	
	print("1="..lv..";2="..rmp)
	
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and tostring(obj:GetConfig("CLASS")) == "CHAR" then

		else
		obj:ChangeMP(rmp)
		
		end	


	end	
end


return P