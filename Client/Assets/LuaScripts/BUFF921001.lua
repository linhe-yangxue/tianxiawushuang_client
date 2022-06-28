P = Buff()




function Init()
	title = "链接 Lv."..var10
	describe = "\n\n被动减免自身受到的伤害"..var1.."点，若场上有分身，则将受到的"..var2.."%的伤害分摊与分身"
end


function Start()
	local DEF = owner:GetFinal("DEFENSE")

	return {DEFENSE = var1}
end


function FinalDamage(atker, DMG)

	for _, obj in pairs(Objects) do
		if not owner:IsEnemy(obj) and obj:IsMonster() then
			local SUMMER = tostring obj:GetOwner()
			if owner == SUMMER then
				DMG = DMG * var2 / 100
				obj:ChangeHP(DMG)

				return DMG
			end
		end
	
	end

end

return P