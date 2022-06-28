P = Buff()




function Init()
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	rate = var2 + var3*lv	
	title = "无言 Lv."..lv
	
	describe = "\n\n攻击时有"..rate.."%几率对使目标沉默"..var1.."秒"
end

function Start()
	print("无言被动，ID=912501")
end

function OnHit(target,damage)
	
	print("1="..lv..";2="..rate.."%")

		if math.random() < (rate/100) then
			print("判断成功")
			buff = ApplyBuff (target,91150101)

			if buff then buff:StartCoroutine(Silent) end
		end
		print("判断不成功")

		
end

function Silent()
	Wait(var1)
	for _, obj in pairs(Objects) do
		print("成功进入")
		if owner:IsEnemy(obj) then
			StopBuff (obj,91150101)
			print("成功移除")
		end
	end	
end



return P