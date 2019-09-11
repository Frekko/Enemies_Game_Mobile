using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSoMetr : MonoBehaviour
{
    private Text _text;
    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponent<Text>();
    }
   
    
    void Update()
    {
        _text.text = "" + (int)(1f / Time.deltaTime);
    }
}
