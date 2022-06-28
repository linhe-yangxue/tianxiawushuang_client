#ifndef _DFEINE_H
#define _DFEINE_H


#if defined(KEYDLL_EXPORTS)
#define KEY_API _declspec(dllexport)
#else
#define KEY_API _descspec(dllimport)
#endif

#define KEY_API_EXTERN_C extern "C" KEY_API


#endif
