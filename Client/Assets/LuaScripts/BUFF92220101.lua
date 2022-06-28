P = Buff()

local i = 0
 
function Init()
	title = "斗志 Lv."..var10
	describe = "\n\n被召唤时减少所有符灵"..var1.."秒的召唤间隔时间"
end

function Awake()
	
	return {SUMMON_CD = var1}
	
end


return P