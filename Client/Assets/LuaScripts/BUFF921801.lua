P = Buff()

function Init()
	title = "暴戾 Lv."..var10
	describe = "\n\n被动提升自身"..var1.."点暴击伤害，所受到的伤害减少"..var2.."点"
end


function Start()
	
	return {HIT_CRITICAL_STRIKE_RATE = var1}
	
end

function FinalDamage(atker, DMG)
		DMG = DMG + var2
		if DMG < 0 then
			DMG = 0
		return DMG

	end

end

return P