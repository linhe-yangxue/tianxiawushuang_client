P = Buff()

local i = 0
 
function Init()
	totalTime = 0
	deltaTime = var3
	title = "玄威 Lv."..var10
	describe = "\n\n被召唤时对自身范围"..var1.."码内敌人造成"..var2.."点伤害，同时对范围敌人造成每"..var3.."秒"..var4.."点的持续伤害，当场上有【北冥神】时被召唤，则提升召唤持续时间"..var5.."秒"
end


function Start()

	for _, obj in pairs(Objects) do

		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			obj:ChangeHP(var2)
			eff = obj:PlayEffect(8027)
			StartCoroutine(CO,1)
		end
		
		if not owner:IsEnemy(obj) then
			local ID = tostring (obj:GetConfig("INDEX"))
			if i < 1 then
				if ID == tostring (var6)  then
					i = i + 1
					
					return {LIFE_TIME = var5}
				end
			end
		end
		
		
	end

end

function Update()

	for _, obj in pairs(Objects) do

		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			obj:ChangeHP(var4)
			eff = obj:PlayEffect(8027)
			StartCoroutine(CO,1)
		end
		
		if not owner:IsEnemy(obj) then
			local ID = tostring (obj:GetConfig("INDEX"))
			if i < 1 then
				if ID == tostring (var6)  then
					i = i + 1

					return {LIFE_TIME = var5}
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