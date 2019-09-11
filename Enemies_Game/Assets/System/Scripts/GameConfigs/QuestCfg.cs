using System;

[Serializable]
public class QuestCfg : GameConfig
{
    public string targetType = "game_finish";
    public int minCount = 0;
    public int maxCount = 0;
    public int step = 1;
    
    public int rewardScores = 0;
    public string rewardItem = null;
    public int rewardItemCount = 1;
    
    public bool tutorial = false;

    public int StepsCount() => ((maxCount - minCount) / step) + 1;
}