using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

[Serializable]
public class GameConfigProperty
{
    public string k; //key
    public List<string> v = new List<string>(); //values
   

    public string Value(int level = -1)
    {
        if (level < 0) level = 0;
        if (level >= v.Count) level = v.Count - 1;

        return  v[level];
    }
}

[Serializable]
public class GameConfigUpgradable : ItemCfg
{
    public List<GameConfigProperty> Properties = new List<GameConfigProperty>();
    
  
    
    public List<string> RawValues(string key) 
    {
        var property = Properties.Find(p => p.k == key);
        if (property != null)
        {
           return new List<string>(property.v);
        }

        return null;
    }
    
    public virtual string RawValue(string key, int level = -1)
    {
        var property = Properties.Find(p => p.k == key);
        return property?.Value(level);
    }

    public virtual int Int(string key, int level = -1, int defaultVal = 0)
    {
        var val = RawValue(key, level);

        if (val != null && float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out var fval))
        {
            return (int)fval;
        }

        return defaultVal;
    }

    public virtual int MaxIdx(string key)
    {
        var property = Properties.Find(p => p.k == key);
        if (property != null)
        {
            return property.v.Count - 1;
        }
        return 0;
    }

    public virtual List<int> Ints(string key)
    {
        var val = RawValues(key);

        if (val != null)
        {
            var retVal = new List<int>(val.Count);

            for (int i = 0; i < val.Count; i++)
            {
                if (val[i] != null && float.TryParse(val[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var fval))
                {
                    retVal.Add((int)fval);
                }
            }
            
            return retVal;
        }

        return new List<int>();
    }
    
    public virtual float Float(string key, int level = -1, float defaultVal = 0f)
    {
        var val = RawValue(key, level);
        
        if (val != null && float.TryParse(val.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var fval))
        {
            return fval;
        }


        return defaultVal;
    }
    
    public virtual List<float> Floats(string key)
    {
        var val = RawValues(key);

        if (val != null)
        {
            var retVal = new List<float>(val.Count);

            for (int i = 0; i < val.Count; i++)
            {
                if (val[i] != null && float.TryParse(val[i].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var fval))
                {
                    retVal.Add(fval);
                }
            }
            
            return retVal;
        }

        return new List<float>();
    }
    
    public virtual string String(string key, int level = -1, string defaultVal = null)
    {
        var val = RawValue(key, level);

        if (val != null)
        {
            return val;
        }

        return defaultVal;
    }
    
    public override  GameConfig Clone()
    {
        return (GameConfigUpgradable)MemberwiseClone();
    }
}
