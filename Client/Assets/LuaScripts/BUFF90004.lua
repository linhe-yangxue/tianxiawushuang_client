P = Buff()

totalTime = 50
deltaTime = 1
title = "虚弱气场"

function Init()
	describe = "对自身范围"..var1.."码内的敌人造成攻击力下降"..var2.."%的虚弱效果"
end


function Update()
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
				ApplyBuff(obj,90005)
		end
	
	end

	
end


return P