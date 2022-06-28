#include <iostream>
#include <windows.h>
#include <vector>
#include <math.h>
#include <string>
#include <fstream>

void CreateIndexRange(int length, int min, int max, const char *fileName);

int main(int argc, char *args[])
{
	CreateIndexRange(24, 0, 255, "./TripleDESKeyIndex");

	return 0;
}

void CreateIndexRange(int length, int min, int max, const char *fileName)
{
	srand(::GetTickCount());
	std::vector<int> tmpVec;
	int tmpDelta = max(0, max - min);
	for(int i = 0; i < length; i++)
	{
		int tmpIndex = min + rand() % tmpDelta;
		tmpVec.push_back(tmpIndex);
	}

	std::ofstream tmpOF(fileName);
	if(!tmpOF.is_open())
		return;
	std::string tmpStr = "";
	for(std::vector<int>::iterator tmpIter = tmpVec.begin(), tmpIterEnd = tmpVec.end();
		tmpIter != tmpIterEnd; tmpIter++)
	{
		char tmpSzStr[16] = "";
		sprintf(tmpSzStr, "%d#", *tmpIter);
		tmpStr += tmpSzStr;
	}
	tmpOF << tmpStr;
	tmpOF.close();
}
