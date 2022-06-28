
P = Buff()

deltaTime = 0.5

function Init()
	
	
	title = "取攻被动附加"
	describe = "\n\n赤雷被动附加，ID=91220101"

end


function Start()
	
	addattack = 100
	print("赤雷被动附加，ID=91220101")
	print("增加攻击="..addattack)
	
end

function OnDead()
	print("怪物死亡")
	for _, obj in pairs(Objects) do

	if owner:IsEnemy(obj) and tostring(obj:GetConfig("CLASS")) == "CHAR" then
		
		print("开始增加主角攻击")
		return {ATTACK = addattack}
		
	end
		
	end
end


return P