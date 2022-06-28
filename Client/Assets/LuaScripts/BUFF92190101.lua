P = Buff()


function Init()
	totalTime = var3
	title = "信念 Lv."..var10
	describe = "\n\n召唤时增加自身范围"..var1.."码内全体友方角色"..var2.."%防御力，增加"..var3.."秒存活时间"
end


function Start()
	local def = owner:GetFinal("DEFENSE")

	def = def * var1 / 100

	return {DEFENSE = def}
	
end


return P