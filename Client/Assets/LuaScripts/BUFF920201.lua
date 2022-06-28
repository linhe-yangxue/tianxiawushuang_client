P = Buff()


function Init()
	title = "长存 Lv."..var10	
	describe = "\n\n被动增加召唤持续时间"..var1.."秒"
end


function Awake()
	return { LIFE_TIME = var1}
end


return P