using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFollowingProgress : MonoBehaviour
{
    public Image targetProgress;
    public float time = 0.2f;

    private Image _currentProgress;
    // Start is called before the first frame update
    void Start()
    {
        _currentProgress = GetComponent<Image>();
        if (_currentProgress == null) enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Mathf.Abs(targetProgress.fillAmount -_currentProgress.fillAmount) < 0.001f) return;
        
        var step = (targetProgress.fillAmount - _currentProgress.fillAmount) / time;
        _currentProgress.fillAmount += step * Time.deltaTime;
    }
}
