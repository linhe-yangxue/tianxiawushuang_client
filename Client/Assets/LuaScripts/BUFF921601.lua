P = Buff()

local i = 0
 
function Init()
	title = "龙腾 Lv."..var10
	describe = "\n\n提升自身"..var1.."点闪避，当场上有青龙时，提升召唤持续时间"..var2.."秒"
end


function Start()

	for _, obj in pairs(Objects) do
		local ID = tostring (obj:GetConfig("INDEX"))
		if not owner:IsEnemy(obj) then
			if i < 1 then
				if ID == tostring (var3)  then
					i = i + 1
					
					return {DODGE = var1 , LIFE_TIME = var2}
				end
			end
		end
	end
	
	return {DODGE = var1}
end

function Update()
	for _, obj in pairs(Objects) do
		local ID = tostring (obj:GetConfig("INDEX"))
		if not owner:IsEnemy(obj) then

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