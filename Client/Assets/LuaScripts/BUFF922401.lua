P = Buff()


function Init()
	title = "神法 Lv."..var10
	describe = "\n\n被动提升主角"..var1.."点法力上限，被召唤时减少主角所有技能"..var2.."秒的冷却时间（下一次释放时有效）"
end


function Start()
	for _, obj in pairs(Objects) do
		if not owner:IsEnemy(obj) and tostring (obj:GetConfig("CLASS")) == "CHAR" then

			
			ApplyBuff(obj, var3)
			
		end
	end
	
end

return P