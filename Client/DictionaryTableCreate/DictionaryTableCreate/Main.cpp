#include <iostream>
#include <vector>
#include <windows.h>
#include <fstream>
#include <conio.h>
#include <string>

void CreateByteDictionary(int length, const char *fileName);
void CreateCharDictionary(int length, const char *fileName);

int main(int argc, char *args[])
{
	CreateByteDictionary(256, "./BytesDictionary");
	CreateCharDictionary(256, "./CharDictoinary");

	return 0;
}

void CreateByteDictionary(int length, const char *fileName)
{
	srand(::GetTickCount());
	std::vector<unsigned char> tmpVec;
	for(int i = 0; i < length; i++)
	{
		unsigned char tmpByte = rand() % 256;
		tmpVec.push_back(tmpByte);
	}

	std::ofstream tmpOF(fileName, std::ios_base::out);
	if(!tmpOF.is_open())
		return;
	std::string tmpStr = "";
	for(std::vector<unsigned char>::iterator tmpIter = tmpVec.begin(), tmpIterEnd = tmpVec.end();
		tmpIter != tmpIterEnd; tmpIter++)
	{
		char tmpSzStr[16] = "";
		sprintf(tmpSzStr, "%d,", *tmpIter);
		tmpStr.append(tmpSzStr);
	}
	tmpOF.write(tmpStr.c_str(), tmpStr.length());
	tmpOF.close();
}
void CreateCharDictionary(int length, const char *fileName)
{
	srand(::GetTickCount());
	std::vector<char> tmpVec;
	for(int i = 0; i < length; i++)
	{
		unsigned char tmpByte = rand() % 256;
		tmpVec.push_back(tmpByte);
	}

	std::ofstream tmpOF(fileName, std::ios_base::out);
	if(!tmpOF.is_open())
		return;
	std::string tmpStr = "";
	for(std::vector<char>::iterator tmpIter = tmpVec.begin(), tmpIterEnd = tmpVec.end();
		tmpIter != tmpIterEnd; tmpIter++)
	{
		char tmpSzStr[16] = "";
		sprintf(tmpSzStr, "%c,", *tmpIter);
		tmpStr.append(tmpSzStr);
	}
	tmpOF.write(tmpStr.c_str(), tmpStr.length());
	tmpOF.close();
}
