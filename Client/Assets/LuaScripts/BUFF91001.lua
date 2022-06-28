P = Buff()


title = "召唤"

function Init()
	describe = "被召唤时为主角回复"..var1.."点生命值"
end


function Start()
	
	for _, obj in pairs(Objects) do
		if tostring (obj:GetConfig("CLASS")) == "CHAR" then
			obj:ChangeHP(var1)
						
		end
	
	end

	
end


return P