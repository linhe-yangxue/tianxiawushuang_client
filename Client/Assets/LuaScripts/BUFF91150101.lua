P = Buff()


title = "沉默状态"

function Start()
	
	print("添加buff")
	
	eff = owner:PlayEffect(8020)
	
	return{SILENCE = 1}
	
	
end

function OnRemove()
	
	if eff then eff:Finish() end
		print("移除buff")
	return{SILENCE = 0}
	
end

return P