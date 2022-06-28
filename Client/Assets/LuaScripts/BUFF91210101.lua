
P = Buff()

deltaTime = 0.5

function Init()
	
	
	title = "固体被动附加"
	describe = "\n\n至强被动附加，ID=91300101"

end


function Start()
	
	adddefense = 100
	print("瞬步被动附加，ID=91210101")
	print("增加防御="..adddefense)
	
end

function OnDead()
	print("怪物死亡")
	for _, obj in pairs(Objects) do

	if owner:IsEnemy(obj) and tostring(obj:GetConfig("CLASS")) == "CHAR" then
		
		print("开始增加主角防御")
		return {DEFENSE = adddefense}
		
	end
		
	end
end


return P