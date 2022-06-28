P = Buff()

totaltime = 0
deltatime = 0


function Init()
	title = "威压 Lv."..var10
	describe = "\n\n当目标生命低于"..var1.."%时，将对其造成一次额外"..var2.."%攻击力的伤害"
end


function OnAttack(e)
	
	ApplyBuff(e, var3)
end


return P