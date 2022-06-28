
Configs = { }

Objects = { }

globalTime = 0

math.randomseed(os.time())


function Instance()
	local P = {}
	setmetatable(P, {__index = _G})
	setfenv(2, P)
	return P
end


function Behaviour()
	local P = {}
	setmetatable(P, {__index = _G})
	setfenv(2, P)
		
	P.deltaTime = 1
	P.elapsedTime = 0
	
	return P
end


function Timer()
	local P = {}
	setmetatable(P, {__index = _G})
	setfenv(2, P)
		
	P.deltaTime = 1
	P.elapsedTime = 0
	P.totalTime = 0
	
	return P
end


function Inherited(script)
	local P = LuaBehaviourWrapper.CreateGlobalBehaviourTable(script)
	local b = {}
	
	for k, v in pairs(P) do
		b[k] = v
	end

	setfenv(2, P)
	return P, b
end


function Buff()
	local P = {}
	setmetatable(P, {__index = _G})
	setfenv(2, P)
		
	P.totalTime = 0
	P.deltaTime = 1
	P.elapsedTime = 0
	P.owner = 0
	P.index = 0
	P.hideCD = false
	P.title = ""
	P.describe = ""
		
	return P
end


function Skill()
	local P = {}
	setmetatable(P, {__index = _G})
	setfenv(2, P)
	
	P.deltaTime = 1
	P.elapsedTime = 0
	P.totalTime = 0.01	
	P.owner = 0
	P.index = 0
	P.skillCD = 0
	P.needMP = 0
	P.distance = 999
	P.title = ""
	P.describe = ""
		
	return P
end


function NewInstance()
	local P = Instance()
	return P
end

function EmptyFunc() end

function CreateBehaviour(b)
	if b then
		return LuaBehaviourWrapper.CreateGlobalBehaviour(b)
	else
		return LuaBehaviourWrapper.CreateGlobalBehaviour()
	end
end

function CreateTimer(b)
	if b then
		return LuaBehaviourWrapper.CreateGlobalTimer(b)
	else
		return LuaBehaviourWrapper.CreateGlobalTimer()
	end
end

function StartCoroutine(f, ...)
	local p = getfenv(2)
	return p.this:StartCoroutine(f, ...)
end

function StopAllCoroutines()
	local p = getfenv(2)
	p.this:StopAllCoroutines()
end

function Wait(block, ...)
	if type(block) == "function" then
		local P = getfenv(2)
		LuaBehaviourWrapper.PushBlock(P.this:StartCoroutine(block, ...))
	else
		LuaBehaviourWrapper.PushBlock(block)
	end
	
	coroutine.yield()
end

function ApplyBuff(target, index)
	local p = getfenv(2)
	return target:StartAffect(p.owner, index)
end

function StopBuff(target, index)
	target:StopAffect(index)
end

function Vector3:RotateAround(axis, angle)
	q = Quaternion.AngleAxis(angle, axis)
	self:MulQuat(q)
	return self
end

function Vector3:Rotate(angle)
	self:RotateAround(Vector3.up, angle)
end

function FormatString(str, ...)
	return CommonWrapper.FormatString(str, ...)
end