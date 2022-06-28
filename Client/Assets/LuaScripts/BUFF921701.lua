P = Buff()

local i = 0
 
function Init()
	title = "朱焰 Lv."..var10
	describe = "\n\n被动降低自身范围"..var1.."码内敌人"..var2.."点防御力，当场上有【朱雀】时被召唤，则提升召唤持续时间"..var3.."秒"
end

function Start()

	for _, obj in pairs(Objects) do

		if owner:IsEnemy(obj) and owner:InRange(obj,var1)  then
			
			ApplyBuff(obj, var4)
			eff = obj:PlayEffect(1108)
			StartCoroutine(CO,1)
		end
		
		if not owner:IsEnemy(obj) then
			local ID = tostring (obj:GetConfig("INDEX"))
			if i < 1 then
				if ID == tostring (var5)  then
					i = i + 1

					return {LIFE_TIME = var3}
				end
			end
		end
		
		
	end

end

function Update()

	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			
			ApplyBuff(obj, var4)
			eff = obj:PlayEffect(1108)
			StartCoroutine(CO,1)
		end
		
		if not owner:IsEnemy(obj) then
			local ID = tostring (obj:GetConfig("INDEX"))
			if i < 1 then
				if ID == tostring (var5)  then
					i = i + 1

					return {LIFE_TIME = var3}
				end
			end
		end
		
		
	end
	
end

function OnRemove()
	if eff then
		eff:Finish()
	end
end

function CO(j)
	if eff then
		Wait(1)
		eff:Finish()
	end
end


return P