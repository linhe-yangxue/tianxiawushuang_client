P = Buff()



function Init()
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rmp = var2 + var3*lv	
	title = "沉默 Lv."..lv
	describe = "\n\n死亡时对自身范围"..var1.."码内的敌人施加沉默状态，持续"..rmp.."秒"
end

function Start()
	print("沉默被动，ID=911501")
end

function OnDead()
	
	print("1="..lv..";2="..rmp)
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			buff = ApplyBuff (obj,91150101)
			print("aaaab")
			buff:StartCoroutine(Silent)
		end

	end
		
end

function Silent()
	Wait(rmp)
	for _, obj in pairs(Objects) do
		print("成功进入")
		if owner:IsEnemy(obj) then
			StopBuff (obj,91150101)
			print("成功移除")
		end
	end	
end



return P