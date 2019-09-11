
using System;
using System.Collections.Generic;

[Serializable]
public class ItemCfg :GameConfig
{
  
   public string[] Tags;
   public string[] EquipTags;
  
   
   public bool HasTagsAll(params string[] tags)
   {
      if (tags == null || tags.Length <= 0) return false;
      
     

      if (Tags == null) return false;
      
      var tagsList = new List<string>(Tags);

      foreach (var tag in tags)
      {
         if (!tagsList.Contains(tag)) return false;
      }

      return true;
   }
   
   public bool HasTagsAny(params string[] tags)
   {
      if (tags == null || tags.Length <= 0) return false;
      
      
      if (Tags == null) return false;
      var tagsList = new List<string>(Tags);

      foreach (var tag in tags)
      {
         if (tagsList.Contains(tag)) return true;
      }

      return false;
   }

   public bool HasEquipTagsAll(params string[] tags)
   {
      if (tags == null || tags.Length <= 0) return false;

      if (EquipTags == null) return false;
      
      var tagsList = new List<string>(EquipTags);

      foreach (var tag in tags)
      {
         if (!tagsList.Contains(tag)) return false;
      }

      return true;
   }
}
