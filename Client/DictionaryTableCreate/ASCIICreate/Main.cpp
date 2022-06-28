#include <iostream>
#include <string>
#include <windows.h>
#include <fstream>

void ASCIICreate(int length, const char *szFile);

int main(int argc, char *args[])
{
	ASCIICreate(7, "./ASCII");

	return 0;
}

void ASCIICreate(int length, const char *szFile)
{
	std::string tmpStr = "";
	int tmpStart = (int)'A', tmpStop = (int)'Z';
	srand(::GetTickCount());
	for(int i = 0; i < length; i++)
	{
		int tmp = tmpStart + rand() % 26;
		char tmpSzStr[16] = "";
		sprintf(tmpSzStr, "%c", tmp);
		tmpStr += tmpSzStr;
	}

	std::ofstream tmpOF(szFile);
	if(!tmpOF.is_open())
		return;
	tmpOF << tmpStr;
	tmpOF.close();
}
