P = Buff()


function Init()
	title = "延威 Lv."..var10

	describe = "\n\n降低"..var2.."%主角法术冷却时间，增加"..var3.."秒存活时间"
end


function Start()
	
	for _, obj in pairs(Objects) do
		if not owner:IsEnemy(obj) and tostring (obj:GetConfig("CLASS")) == "CHAR" then

			ApplyBuff(obj, var4)
			
		end
	
	end

	return {LIFE_TIME = var3}
end

function Update()
	
	for _, obj in pairs(Objects) do
		if not owner:IsEnemy(obj) and tostring (obj:GetConfig("CLASS")) == "CHAR" then

			ApplyBuff(obj, var4)
			
		end
	
	end

	
end


return P