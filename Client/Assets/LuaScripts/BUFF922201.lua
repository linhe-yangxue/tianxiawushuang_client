P = Buff()


function Init()
	title = "固攻 Lv."..var10
	describe = "\n\n被动提升自身"..var1.."点攻击力，被召唤时减少所有符灵"..var2.."秒的召唤间隔时间（下一次召唤时生效）"
end

function Start()
	for _, obj in pairs(Objects) do
		if not owner:IsEnemy(obj) and tostring (obj:GetConfig("CLASS")) == "CHAR" then

			ApplyBuff(obj, var3)
			
		end
	end
	return { ATTACK = var1}

end


return P