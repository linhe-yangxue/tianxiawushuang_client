
P = Buff()

deltaTime = 0.5

function Init()
	
	
	title = "续法被动附加"
	describe = "\n\n岩壁被动附加，ID=91200101"

end


function Start()
	
	manarestore = 100
	print("岩壁被动附加，ID=91200101")
	print("回复量="..manarestore)
	
end

function OnDead()
	print("怪物死亡")
	for _, obj in pairs(Objects) do

	if owner:IsEnemy(obj) and tostring(obj:GetConfig("CLASS")) == "CHAR" then
		print("开始回复主角法力")
		obj:ChangeMP(manarestore)

	end
		
	end
end


return P