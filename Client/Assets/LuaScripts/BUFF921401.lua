P = Buff()

local i = 0
 
function Init()
	title = "虎壁 Lv."..var10
	describe = "\n\n被动使全体友方角色提升"..var1.."点防御力，当场上有【山野之王】时，提升召唤持续时间"..var2.."秒"
end


function Start()

	for _, obj in pairs(Objects) do
		local ID = tostring (obj:GetConfig("INDEX"))
		if not owner:IsEnemy(obj) then
			
			ApplyBuff(obj, var4)
			if i < 1 then
				if ID == tostring (var3)  then
					i = i + 1
					
					return {DEFENSE = var1, LIFE_TIME = var2}
				end
			end
		end
	end
	
	return {DEFENSE = var1}
end

function Update()

	for _, obj in pairs(Objects) do
		local ID = tostring (obj:GetConfig("INDEX"))
		if not owner:IsEnemy(obj) then
			
			ApplyBuff(obj, var4)
			if i < 1 then
				if ID == tostring (var3)  then
					i = i + 1
					
					return {LIFE_TIME = var2}
				end
			end
		end
	end
	
end


return P