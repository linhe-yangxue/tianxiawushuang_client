using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.RManager
{
    public enum ObjectSizeHint
    {
        SMALL       = 0,
        MEDIUM      = 5,
        LARGE       = 10,
        VERYLARGE   = 30
    }

    struct CacheEntry
    {
        public int      objUnitSize;
        public object   objectCached;
    }

    public class ObjectsSimpleCache
    {
        static int MaxObjectUnitOfVolatile = 0;
        static bool DisableCache = true;

        static ObjectsSimpleCache()
        {
            MaxObjectUnitOfVolatile = 512;
        }

        int objectUnitOfVolatile = 0;

        Dictionary<string, CacheEntry> dictionaryVolatile;
        Dictionary<string, CacheEntry> dictionaryPermenent;

        Dictionary<ObjectSizeHint, int> objectHintToUnit;

        List<string> volitalKeys;
        //st<string> permenentKeys;

        public ObjectsSimpleCache()
        {
            objectUnitOfVolatile = 0;
            dictionaryVolatile = new Dictionary<string,CacheEntry>();
            dictionaryPermenent = new Dictionary<string,CacheEntry>();

            objectHintToUnit = new Dictionary<ObjectSizeHint,int>();

            volitalKeys = new List<string>();
            //rmenentKeys = new List<string>();

            objectHintToUnit[ObjectSizeHint.SMALL] = 1;
            objectHintToUnit[ObjectSizeHint.MEDIUM] = 10;
            objectHintToUnit[ObjectSizeHint.LARGE] = 100;
            objectHintToUnit[ObjectSizeHint.VERYLARGE] = 1000;
        }

        public object GetObject(string key)
        {
            if(dictionaryVolatile.ContainsKey(key))
                return dictionaryVolatile[key].objectCached;

            if(dictionaryPermenent.ContainsKey(key))
                return dictionaryPermenent[key].objectCached;

            return null;
        }

        public void CacheIt(string key, object obj, ObjectSizeHint objSizeHint)
        {
            if(DisableCache)
                return;

            var unitC = objectHintToUnit[objSizeHint];

            if(objectUnitOfVolatile + unitC < MaxObjectUnitOfVolatile)
            {
                // if exist same key then move its key to the end of key list;
                // what exactly to do here is just remove the old key in list
                if(dictionaryVolatile.ContainsKey(key))
                {
                    var oldCache = dictionaryVolatile[key];
                    objectUnitOfVolatile -= oldCache.objUnitSize;
                    volitalKeys.Remove(key);
                }
            }
            else
            {
                // dispose the oldest key and object
                var disposingKey = volitalKeys[0];      // it can't be null
                var disposingCache = dictionaryVolatile[disposingKey];
                objectUnitOfVolatile -= disposingCache.objUnitSize;

                volitalKeys.RemoveAt(0);
                dictionaryVolatile.Remove(disposingKey);
            }

            volitalKeys.Add(key);
            dictionaryVolatile[key] = new CacheEntry{ objUnitSize = unitC, objectCached = obj };
            objectUnitOfVolatile += unitC;
            
        }

        public void CacheIt(string key, object obj, ObjectSizeHint objSizeHint, bool isPermenent)
        {
            if(!isPermenent)
            {
                CacheIt(key, obj, objSizeHint);
            }
            else
            {
                var unitC = objectHintToUnit[objSizeHint];
                dictionaryPermenent[key] = new CacheEntry{ objUnitSize = unitC, objectCached = obj };
            }
        }
    }
}
