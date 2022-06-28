P = Buff()


function Init()
	title = "灵溢 Lv."..var10
	describe = "\n\n被动提升主角"..var1.."点法力上限，被召唤时减少主角所有技能"..var2.."秒的冷却时间（下一次释放时有效）"
end

function Awake()
	
	return {SKILL_CD = var2}
	
end

function Start()
	
	return {MP_MAX = var1, MP = var1}
	
end

return P