P = Buff()




function Init()
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	basedamage = var1 + (var2 * lv)
	exdamage = var3 + (var4 * lv)
	title = "横扫 Lv."..lv
	
	describe = "\n\n普通攻击将对面前扇形范围内敌人产生伤害，同时使目标流血，每秒造成"..var1.."点伤害，每秒增加"..var2.."点"
end

function Start()
	print("横扫被动，ID=912601")
	print("基础伤害="..basedamage..";附加伤害="..exdamage)
end

function OnHit(target,damage)
	local buff = ApplyBuff (target,91260101)
	print("附加状态成功")
	if buff then buff:Set("basedamage1",basedamage) end
	print("设定基础伤害"..basedamage)
	if buff then buff:Set("exdamage1",exdamage)	end
	print("设定附加伤害"..exdamage)
end

return P