#include <iostream>
#include <map>

#include "./Define.h"
#include "./KeyAccessor.h"

unsigned char g_BytesTable[] =
{
	53,223,177,252,67,167,191,222,187,
	39,199,112,75,229,133,140,55,162,
	7,22,161,141,18,212,7,182,195,128,
	13,197,114,28,125,236,219,134,140,
	26,128,76,95,114,73,193,205,251,110,
	81,103,153,150,17,181,60,65,154,195,
	87,99,24,220,148,81,158,150,198,225,
	189,15,30,205,84,117,163,89,169,204,
	220,54,185,235,207,165,205,200,48,
	224,78,246,210,181,218,108,97,116,
	151,136,83,11,200,84,28,112,158,7,
	161,63,80,207,240,165,110,205,46,124,
	114,97,208,183,152,169,15,1,239,68,
	149,161,176,92,125,161,207,227,122,
	47,209,28,86,67,221,94,159,130,21,21,
	157,99,39,9,134,144,30,227,245,143,
	127,237,153,161,145,25,42,234,252,69,
	162,211,149,189,169,173,121,1,81,151,
	89,201,3,161,20,71,186,49,139,172,110,
	166,177,237,212,60,225,199,68,47,117,
	1,250,36,148,241,130,196,75,65,110,
	170,223,243,74,127,98,163,210,99,133,
	12,98,143,173,206,173,57,73,111,178,
	184,98,159,236,233,115,192,201,209,
	124,165,94,132,82,154,89,69,57,165,
	55,238,180,12,183,147,156,23,142,98,112
};
static int g_BytesTableLength = 256;

class CBytes
{
public:
	CBytes()
		:m_Bytes(NULL),
		m_Length(0)
	{
	}
	CBytes(unsigned char bytes[], int length)
	{
		m_Bytes = bytes;
		m_Length = length;
	}
	~CBytes()
	{
	}

public:
	unsigned char *Bytes()
	{
		return m_Bytes;
	}
	int Length()
	{
		return m_Length;
	}

private:
	unsigned char *m_Bytes;
	int m_Length;
};
static std::map<const char *, CBytes> g_MapSignBytesValue;
static unsigned char g_ConfigAESCryptoKey[] =
{
	97,89,124,123,148,227,6,80,
	5,143,171,83,220,188,109,100,
	58,211,184,243,101,156,194,
	33,88,95,97,196,230,118,98,81
};

class CGlobalDataInit
{
public:
	CGlobalDataInit()
	{
		__InitSignBytesValue();
	}
	~CGlobalDataInit()
	{
	}

private:
	void __InitSignBytesValue()
	{
		g_MapSignBytesValue["6C8B466CBE0D53EC1C861516232971EC"] = CBytes(g_ConfigAESCryptoKey, 32);
	}
};
static CGlobalDataInit g_GlobalDataInit;

CKeyAccessor::CKeyAccessor()
{
}
CKeyAccessor::~CKeyAccessor()
{
}
void CKeyAccessor::GetGameStringValueBySign(char *sz, char *sz2, int count)
{
}
void CKeyAccessor::GetGameBytesValueBySign(char *sz, unsigned char *sz2, int count)
{
	std::map<const char *, CBytes>::iterator tmpIter, tmpIterEnd;
	for(tmpIter = g_MapSignBytesValue.begin(), tmpIterEnd = g_MapSignBytesValue.end();
		tmpIter != tmpIterEnd; tmpIter++)
	{
		if(strcmp((*tmpIter).first, sz) == 0)
		{
			CBytes &tmpBytes = (*tmpIter).second;
			for(int i = 0, count = tmpBytes.Length(); i < count; i++)
				sz2[i] = tmpBytes.Bytes()[i];
			return;
		}
	}
}
void CKeyAccessor::GetGameBytesValueByIndex(int *sz, unsigned char *sz2, int count)
{
	for(int i = 0; i < count; i++)
		sz2[i] = g_BytesTable[sz[i] % g_BytesTableLength];
}

static CKeyAccessor gs_KeyAccessor;

void GetGameStringValueBySign(char *sz, char *sz2, int count)
{
	return gs_KeyAccessor.GetGameStringValueBySign(sz, sz2, count);
}
void GetGameBytesValueBySign(char *sz, unsigned char *sz2, int count)
{
	return gs_KeyAccessor.GetGameBytesValueBySign(sz, sz2, count);
}
void GetGameBytesValueByIndex(int *sz, unsigned char *sz2, int count)
{
	return gs_KeyAccessor.GetGameBytesValueByIndex(sz, sz2, count);
}
