P = Buff()

local attackbuff
local lv
local attackup

function Init()
	
	lv = tonumber(string.sub(index,tonumber(string.len(index))))
	attackup = var1 + (var2*(lv-1))
	attackbuff = var4 + lv 
	
	title = "得力 Lv."..lv
	describe = "\n\n攻击时使自身攻击力提升"..attackup.."点，持续"..var3.."秒"
	
end

function Start()
	print("得力被动，ID=912701")
end

function OnHit(target,damage)

	StartCoroutine(Co)
	print("ID ="..attackbuff)		
end

function Co()
	Wait()
	ApplyBuff(owner,70401)
end


return P