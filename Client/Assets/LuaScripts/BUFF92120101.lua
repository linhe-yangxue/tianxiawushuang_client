P = Buff()


function Init()
	totalTime = var3
	title = "御神 Lv."..var10
	describe = "\n\n攻击命中时使自身范围"..var1.."码内全体友方角色获得增强效果，同时提升"..var2.."%攻击力与"..var3.."%暴击率"
end


function Start()
	local ATK = owner:GetFinal("ATTACK")
	local CRT_R = owner:GetFinal("CRITICAL_STRIKE_RATE")
	ATK = ATK * var1 / 100
	CRT_R = CRT_R * var2 / 100
	return {ATTACK = ATK, CRITICAL_STRIKE_RATE = CRT_R}
	
end


return P