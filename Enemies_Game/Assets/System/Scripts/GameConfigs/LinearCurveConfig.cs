
using System;
using System.Collections.Generic;
using System.Globalization;

public static class LinearCurveExtention
{
    public static LinearCurveConfig LinearCurve(this GameConfigUpgradable thiz, string key, int level,
        LinearCurveConfig defaultValue = null)
    {
        var values = thiz.RawValue(key, level);

        if (!string.IsNullOrEmpty(values))
        {
            return LinearCurveConfig.CreateFromString(values);
        }

        return defaultValue;
    }
}

[Serializable]
public class LinearCurveConfig
{
    [Serializable]
    public class CurvePoint
    {
        public float x;
        public float y;
    }
    
    public List<CurvePoint> Points = new List<CurvePoint>();
    
    public static LinearCurveConfig CreateFromString(string values)
    {
        LinearCurveConfig retVal = new LinearCurveConfig();
        var chunks = values.Split(new[] {' ', ',', '[', ']'}, StringSplitOptions.RemoveEmptyEntries);

        if (chunks.Length < 2)
        {
            throw new FormatException("LinearCurveConfig expects at least two 2D points");
        }

        for (int i = 0; i < chunks.Length; i+=2)
        {
            var xStr = chunks[i];
            var yStr = chunks[i + 1];

            var point = new CurvePoint()
            {
                x = float.Parse(xStr, CultureInfo.InvariantCulture),
                y = float.Parse(yStr, CultureInfo.InvariantCulture)
            };
            
            retVal.Points.Add(point);
        }
        
        return retVal;
    }

    public float Evaluate(float x)
    {
        if (Points == null || Points.Count < 1) return 1f;
       
        float lastValue = Points[0].y;
        
        if (x <= Points[0].x) return lastValue;
        
        for (int i = 0; i < Points.Count - 1; i++)
        {
            var p1 = Points[i];
            var p2 = Points[i+1];
            
            if (IsPointWithinSegment(p1, p2, x))
            {
                return ValueWithinSegment(p1, p2, x);
            }

            lastValue = p2.y;
        }

        return lastValue;
    }

    bool IsPointWithinSegment(CurvePoint p1, CurvePoint p2, float x)
    {
        return p1.x < x && p2.x >= x;
    }

    float ValueWithinSegment(CurvePoint p1, CurvePoint p2, float x)
    {
        var k = (p1.y - p2.y) / (p1.x - p2.x);
        var y = k*(x - p1.x) + p1.y;

        return y;
    }
}
