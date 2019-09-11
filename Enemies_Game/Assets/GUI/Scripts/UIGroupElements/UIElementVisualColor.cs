using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIElement))]
public class UIElementVisualColor : MonoBehaviour
{
   public UIElement ParentElement;
   
   
   public Color baseColor = Color.gray;
   public Color selectedColor = Color.white;
   public Color activeColor = Color.cyan;
   
  

   public float ActiveTime = 0.15f;

   private void Awake()
   {
      if (ParentElement == null)
         ParentElement = GetComponent<UIElement>();
      
      ParentElement.SelectionChanged += SelectionChanged;
      ParentElement.Activated += OnActivated;
   }

   private void Start()
   {
      SetSelectedColor();
   }

   private void OnActivated(object userData)
   {
      var i= GetComponent<Image>();
      if(i != null) i.color = activeColor;

      if(gameObject.activeInHierarchy)
         StartCoroutine(SetSelectedSpriteAfter());
   }

   IEnumerator SetSelectedSpriteAfter()
   {
      yield return new WaitForSeconds(ActiveTime);
      SetSelectedColor();
   }



   private void SelectionChanged(bool selected, object userData)
   {
      SetSelectedColor(selected);
   }

   void SetSelectedColor()
   {
      SetSelectedColor(ParentElement.IsSelected);
   }

   void SetSelectedColor(bool selected)
   {
      var i= GetComponent<Image>();
      if(i != null) i.color = selected ? selectedColor : baseColor;
     
   }
}
