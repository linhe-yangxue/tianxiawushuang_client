
P = Buff()

deltaTime = 0.5

function Init()
	
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var2 * lv
	title = "固体 Lv."..lv
	describe = "\n\n符灵"..var1.."米范围内的敌人死亡将降低主角受到的伤害"..rmp.."点"
end


function Start()
	
	print("固体被动，ID=912101")
	
	print("1="..lv..";2="..rmp)
		
end

function Update()
	for _, obj in pairs(Objects) do
	
		if owner:IsEnemy(obj) and owner:InRange(obj,10) then
			local b = ApplyBuff(obj,91210101)
			b:Set("adddefense",rmp)
		end
		
	end
end


return P