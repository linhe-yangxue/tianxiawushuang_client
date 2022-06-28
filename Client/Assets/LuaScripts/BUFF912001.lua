
P = Buff()

deltaTime = 0.5

function Init()
	
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var2 + var3*lv
	title = "续法 Lv."..lv
	describe = "\n\n符灵"..var1.."米范围内的敌人死亡将给主角恢复"..rmp.."点法力值"
end


function Start()
	
	print("续法被动，ID=912001")
	
	print("1="..lv..";2="..rmp)
		
end

function Update()
	for _, obj in pairs(Objects) do
	
		if owner:IsEnemy(obj) and owner:InRange(obj,10) then
			local b = ApplyBuff(obj,91200101)
			b:Set("manarestore",rmp)
		end
		
	end
end


return P