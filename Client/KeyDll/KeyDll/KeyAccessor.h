#ifndef _KEY_ACCESSOR_H
#define _KEY_ACCESSOR_H


class CKeyAccessor
{
public:
	CKeyAccessor();
	~CKeyAccessor();

public:
	void GetGameStringValueBySign(char *sz, char *sz2, int count);
	void GetGameBytesValueBySign(char *sz, unsigned char *sz2, int count);
	void GetGameBytesValueByIndex(int *sz, unsigned char *sz2, int count);

private:
	std::map<char *, char *> m_mapValue;
};

KEY_API_EXTERN_C void GetGameStringValueBySign(char *sz, char *sz2, int count);
KEY_API_EXTERN_C void GetGameBytesValueBySign(char *sz, unsigned char *sz2, int count);
KEY_API_EXTERN_C void GetGameBytesValueByIndex(int *sz, unsigned char *sz2, int count);


#endif
