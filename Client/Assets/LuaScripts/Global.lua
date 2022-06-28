
function GetNearestEnemy(owner)
	local nearestEnemy
	local sqrDistance = 1000000
	
	for _, o in pairs(Objects) do
		if owner:IsEnemy(o) then
			local d = (owner:GetPosition() - o:GetPosition()):SqrMagnitude()
			
			if d < sqrDistance then
				sqrDistance = d
				nearestEnemy = o
			end
		end
	end
	
	return nearestEnemy
end