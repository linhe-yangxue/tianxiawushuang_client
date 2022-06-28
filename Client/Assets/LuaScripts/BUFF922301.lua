P = Buff()

function Init()
	title = "破防 Lv."..var10
	describe = "\n\n被动降低自身"..var1.."码范围内敌人"..var2.."点防御力，被召唤时增加所有己方符灵"..var3.."秒召唤持续时间"
end


function Start()
	
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			ApplyBuff(obj, var4)
			
		end
		
		if not owner:IsEnemy(obj) and obj:IsPet() then
			local Is_have_buff obj:GetScriptBuff(var5)
			if Is_have_buff == nil	then

				ApplyBuff(obj, var5)
			end
		end
	end

	
end

function Update()
	
	for _, obj in pairs(Objects) do
		if owner:IsEnemy(obj) and owner:InRange(obj,var1) then
			ApplyBuff(obj, var4)
			
		end
		
		if not owner:IsEnemy(obj) and obj:IsPet() then
			local Is_have_buff obj:GetScriptBuff(var5)
			if Is_have_buff == nil	then

				ApplyBuff(obj, var5)
			end
		end
	end

	
end


return P