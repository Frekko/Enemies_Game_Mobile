using System;
using System.Collections.Generic;



[Serializable]
public class GameConfig
{
    public string Id;
    public virtual GameConfig Clone()
    {
        return (GameConfig)MemberwiseClone();
    }
}
