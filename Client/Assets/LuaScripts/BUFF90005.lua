P = Buff()

totalTime = 1.1
deltaTime = 1
hideCD = true
title = "虚弱气场"

function Init()
	describe = "对自身范围"..var1.."码内的敌人造成攻击力下降"..var2.."%的虚弱效果"
end


function Start()
	
	local atk = owner:GetFinal("ATTACK")
	
	atk = atk * (-1 * var2/ 100)
	atk = math.modf(atk)

	return {ATTACK = atk}
	
	
	
end


return P