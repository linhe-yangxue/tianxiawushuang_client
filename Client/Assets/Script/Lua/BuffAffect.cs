using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using DataTable;


public class BuffAffect
{
    private BuffAffectPool pool;
    private List<KeyValuePair<string, double>> affect = new List<KeyValuePair<string, double>>();

    public BuffAffect(BuffAffectPool pool)
    {
        this.pool = pool;
        pool.AddAffect(this);
    }

    public void AddAffect(string key, double val)
    {
        int index = affect.FindIndex(pair => pair.Key == key);

        if (index >= 0)
        {
            KeyValuePair<string, double> newPair = new KeyValuePair<string, double>(key, affect[index].Value + val);
            affect[index] = newPair;
        }
        else
        {
            affect.Add(new KeyValuePair<string, double>(key, val));
        }

        pool.SetDirty();
    }

    public void SetAffect(string key, double val)
    {
        int index = affect.FindIndex(pair => pair.Key == key);

        if (index >= 0)
        {
            KeyValuePair<string, double> newPair = new KeyValuePair<string, double>(key, val);
            affect[index] = newPair;
        }
        else
        {
            affect.Add(new KeyValuePair<string, double>(key, val));
        }

        pool.SetDirty();
    }

    public void ResetAffect()
    {
        this.affect = new List<KeyValuePair<string, double>>();
        pool.SetDirty();
    }

    public void ResetAffect(IEnumerable<KeyValuePair<string, double>> affect)
    {
        this.affect = new List<KeyValuePair<string, double>>(affect);
        pool.SetDirty();
    }

    public IEnumerable<KeyValuePair<string, double>> GetAffect()
    {
        return affect;
    }

    public void Invalidate()
    {
        pool.RemoveAffect(this);
        pool.SetDirty();
        pool = null;
    }

    public bool IsValid()
    {
        return pool != null;
    }
}


public class BuffAffectPool
{
    private List<BuffAffect> pool = new List<BuffAffect>();
    private Dictionary<string, double> totalAffect = new Dictionary<string, double>();
    private bool isDirty = true;

    public static void Combine(Dictionary<string, double> total, IEnumerable<KeyValuePair<string, double>> attach)
    {
        foreach (var pair in attach)
        {
            double val = 0;

            if (!total.TryGetValue(pair.Key, out val))
            {
                total.Add(pair.Key, 0);
            }

            total[pair.Key] = val + pair.Value;
        }
    }

    public void SetDirty()
    {
        isDirty = true;
    }

    public void Update()
    {
        if (isDirty)
        {
            totalAffect = new Dictionary<string, double>();

            foreach (var affect in pool)
            {
                Combine(totalAffect, affect.GetAffect());
            }

            isDirty = false;
        }
    }

    public double GetValue(string key)
    {
        double val = 0;
        totalAffect.TryGetValue(key, out val);
        return val;
    }

    public void ResetValue(string key)
    {
        totalAffect.Remove(key);
    }

    public void AddAffect(BuffAffect affect)
    {
        pool.Add(affect);
    }

    public void RemoveAffect(BuffAffect affect)
    {
        pool.Remove(affect);
    }
}