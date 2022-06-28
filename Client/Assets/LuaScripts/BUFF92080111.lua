P = Buff()

totaltime = var2
local skillID = 0
local skillLV = 1
title = "虚体"

function Init()
	describe = "\n\n攻击命中时降低目标"..var1.."点攻击力，当场上有【白无常】时，效果为n倍"
end

function OnDamage(ATKER, DMG)

	local ID = tostring(ATKER:GetConfig("INDEX"))
	if ID == tostring (var3) then
		skillID = tostring(ATKER:GetConfig("AFFECT_BUFFER_1"))
		skillLV = tonumber(skillID) % 100
	end
	
end

function Start()
	eff = owner:PlayEffect(1005)
	return {ATTACK = var1 * skillLV}
	
end

function Update()
	return {ATTACK = var1 * skillLV}
end


function OnRemove()
	if eff then
		eff:Finish()
	end
end
return P