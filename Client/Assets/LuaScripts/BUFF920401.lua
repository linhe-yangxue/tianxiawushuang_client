P = Buff()



function Init()
	title = "狂风 Lv."..var10
	describe = "\n\n被召唤时自动对自身范围"..var1.."码内的敌人释放一次飓风技能"
end


function Start()
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			local i = 0
			if i < 1 then
				owner:SetTarget(obj)
				owner:DoSkill(var2, obj)
				print(var2)
				i = i + 1
			end
			
		end
	
	end
end


return P