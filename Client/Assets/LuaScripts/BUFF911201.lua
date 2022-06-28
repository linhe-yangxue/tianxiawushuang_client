



P = Buff()


function Init()
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var1 + var2*lv
	title = "鼓舞 Lv."..lv	
	describe = "\n\n符灵在被召唤时增加所有友方攻击力"..rmp.."点"
end


function Start()
	print("鼓舞被动，ID=911201")
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var1 + var2*lv
	print("1="..lv..";2="..rmp)
	
	for _, obj in pairs(Objects) do
		if not owner:IsEnemy(obj) then
			
		return {ATTACK = rmp}
		
		end
	end	
end


return P