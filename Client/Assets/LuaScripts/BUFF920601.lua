P = Buff()




function Init()
	var2 = var2 / 100
	title = "耐受 Lv."..var10
	describe = "\n\n被动提升自身"..var1.."点防御力，当场上有【腾骁单骑】时此效果变为"..var2.."倍"
end


function Start()
	local def = 0
	def = def + var1
	for _, obj in pairs(Objects) do
		local ID = tostring (obj:GetConfig("INDEX"))
		if not owner:IsEnemy(obj) then
			if ID == tostring (var3)  then
				def = def * var2
			end
		end
	end
	
	return {DEFENSE = def}
	
end

function Update()
	local def = 0
	def = def + var1
	for _, obj in pairs(Objects) do
		local ID = tostring (obj:GetConfig("INDEX"))
		if not owner:IsEnemy(obj) then
			if ID == tostring (var3)  then
			def = def * var2
			
			end
		end
	end

	return {DEFENSE = def}
end

return P