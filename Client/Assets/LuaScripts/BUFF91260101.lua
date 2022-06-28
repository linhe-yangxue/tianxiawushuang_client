P = Buff()


deltaTime = 1

function Init()

	title = "横扫附加流血"
end

function Start()
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	hpdown = 0
	basedamage1 = 1
	exdamage1 = 1
	print("鹰击附加流血，ID=91260101")
end

function Update()
	
	print("1="..lv..";2="..hpdown)
	if hpdown < var2 then
		hpdown = hpdown + 1
	else
		hpdown = var2
	end
	bleedbytime = basedamage1 + (exdamage1 * hpdown)
	
	owner:ChangeHP(-1 * bleedbytime)
		
end

function OnRemove()
	print("状态移除")
end

return P