P = Buff()



function Init()
	title = "猛攻 Lv."..var10
	var2 = var2 / 100
	describe = "\n\n被动提升自身"..var1.."点攻击力，当场上有【斩罪人·丑】时此效果变为"..var2.."倍"
end


function Start()
	local atk = 0
	atk = atk + var1
	for _, obj in pairs(Objects) do
		local ID = tostring (obj:GetConfig("INDEX"))
		if not owner:IsEnemy(obj) then
			if ID == tostring (var3)  then
				atk = atk * var2
			end
		end
	end
	
	return {ATTACK = atk}
	
end

function Update()
	local atk = 0
	atk = atk + var1
	for _, obj in pairs(Objects) do
		local ID = tostring (obj:GetConfig("INDEX"))
		if not owner:IsEnemy(obj) then
			if ID == tostring (var3)  then
			atk = atk * var2

			end
		end
	end
	
	return {ATTACK = atk}
end

return P