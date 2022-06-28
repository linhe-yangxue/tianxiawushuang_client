P = Buff()

local totalDMG = 0
local DOTDMG = -100

function Init()
	totalTime = var1  --通过scriptbase表内设置的变量var1来取得buff的持续时间
	deltaTime = var2  --通过scriptbase表内设置的变量var2来取得buff的刷新时间
end


function Start()
	
end


function Update()
	
	owner:ChangeHP(DOTDMG * var3)
	totalDMG = totalDMG + (-1) * DOTDMG * var3
	var3 = var3 + 1

	
end


--function OnDamage(attacker)
	--attacker:ChangeHP(totalDMG * 0.5)
--end


--function OnAttack(target)
	
--end


return P