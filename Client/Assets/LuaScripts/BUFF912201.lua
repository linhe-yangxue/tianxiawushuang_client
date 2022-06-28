
P = Buff()

deltaTime = 0.5

function Init()
	
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var2 + var3 * lv
	title = "取攻 Lv."..lv
	describe = "\n\n符灵"..var1.."米范围内的敌人死亡将增加主角的攻击力"..rmp.."点"
end


function Start()
	
	print("取攻被动，ID=912201")
	
	print("1="..lv..";2="..rmp)
		
end

function Update()
	for _, obj in pairs(Objects) do
	
		if owner:IsEnemy(obj) and owner:InRange(obj,10) then
			local b = ApplyBuff(obj,91220101)
			b:Set("addattack",rmp)
		end
		
	end
end


return P