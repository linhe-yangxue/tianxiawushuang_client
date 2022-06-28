P = Buff()

title = "灼烧"

function Init()
	totalTime = var1  --通过scriptbase表内设置的变量var1来取得buff的持续时间
	deltaTime = var2  --通过scriptbase表内设置的变量var2来取得buff的刷新时间
	describe = "对目标每"..var2.."秒造成"..var3.."点伤害持续"..var1.."秒"
end


function Update()
	owner:ChangeHP(-1 * var3)
end


return P