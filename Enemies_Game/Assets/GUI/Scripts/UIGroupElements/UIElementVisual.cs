using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIElement))]
public class UIElementVisual : MonoBehaviour
{
   public UIElement ParentElement;
   
   public List<GameObject> Objects;
   
   
   public Color textColor = Color.gray;
   public Color selectedTextColor = Color.white;
   public Color activeTextColor = Color.red;
   
   
   public Sprite _default;
   public Sprite _selected;
   public Sprite _activated;

   public float ActiveTime = 0.15f;

   private void Awake()
   {
      if (ParentElement == null)
         ParentElement = GetComponent<UIElement>();
      
      ParentElement.SelectionChanged += SelectionChanged;
      ParentElement.Activated += OnActivated;

      string spritesFolder = "UI/Sprites/";

      if (Objects == null || Objects.Count <= 0)
      {
         var objs = GetComponentsInChildren<Transform>();
         Objects = new List<GameObject>();
         foreach (var o in objs)
         {
            Objects.Add(o.gameObject);
         }
      }
   }

   private void Start()
   {
      SetSelectedSprite();
   }

   private void OnActivated(object userData)
   {
      foreach (var o in Objects)
      {
         var i= o.GetComponent<Image>();
         if(i != null) i.sprite = _activated;
         
         var t= o.GetComponent<Text>();
         if(t != null) t.color = activeTextColor;
         
         var ta= o.GetComponent<TMPro.TextMeshProUGUI>();
         if(ta != null) ta.color = activeTextColor;
      }

      if(gameObject.activeInHierarchy)
         StartCoroutine(SetSelectedSpriteAfter());
   }

   IEnumerator SetSelectedSpriteAfter()
   {
      yield return new WaitForSeconds(ActiveTime);
      SetSelectedSprite();
   }



   private void SelectionChanged(bool selected, object userData)
   {
      SetSelectedSprite(selected);
   }

   void SetSelectedSprite()
   {
      SetSelectedSprite(ParentElement.IsSelected);
   }

   void SetSelectedSprite(bool selected)
   {
      foreach (var o in Objects)
      {
         var i= o.GetComponent<Image>();
         if(i != null) i.sprite = selected ? _selected : _default;
         
         var t= o.GetComponent<Text>();
         if(t != null) t.color = selected ? selectedTextColor : textColor;
         
         var ta= o.GetComponent<TMPro.TextMeshProUGUI>();
         if(ta != null) ta.color = selected ? selectedTextColor : textColor;
      }
   }
}
