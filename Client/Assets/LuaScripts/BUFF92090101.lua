P = Buff()

totaltime = var2
local skillID = 0
local skillLV = 1
title = "弱甲"

function Init()
	describe = "\n\n攻击命中时降低目标"..var1.."点防御力，当场上有【黑无常】时，效果为1.5倍"
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
	return {DEFENSE = var1 * skillLV}
	
end

function Update()
	return {DEFENSE = var1 * skillLV}
end

function OnRemove()
	if eff then
		eff:Finish()
	end
end

return P