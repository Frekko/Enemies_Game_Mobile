using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringExtentions 
{
   public static string Lcz(this string str)
   {
      return LocalizationManager.Localize(str);
   }
}
