using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace Diorama
{
public class ConfirmDialog : MonoBehaviour
{

    public TextMeshProUGUI label;
    public GameObject actionButton1;
    public GameObject actionButton2;
    
    Button actionButton1but;
    TextMeshProUGUI actionButton1label;
    Button actionButton2but;
    TextMeshProUGUI actionButton2label;
    void OnEnable() {
        actionButton1but = actionButton1.GetComponentInChildren<Button>();
        actionButton1label = actionButton1.GetComponentInChildren<TextMeshProUGUI>();
        actionButton2but = actionButton2.GetComponentInChildren<Button>();
        actionButton2label = actionButton2.GetComponentInChildren<TextMeshProUGUI>();

    }
    void OnDisable() {
        actionButton1but.onClick.RemoveAllListeners();
        actionButton2but.onClick.RemoveAllListeners();
    }
    
    public void Show(string description, string confirmLabel) {
        Show(description, confirmLabel, () => Hide());
    }
    public void Show(string description, 
        string action1label = null, UnityAction action1cb = null, 
        string action2label = null, UnityAction action2cb = null) {
            
        // TODO: Tween
        gameObject.SetActive(true);

        label.text = description;

        actionButton1.SetActive(!string.IsNullOrEmpty(action1label));
        actionButton1label.text = action1label;
        if(action1cb != null) 
            actionButton1.GetComponentInChildren<Button>().onClick.AddListener(action1cb);

        actionButton2.SetActive(!string.IsNullOrEmpty(action2label));
        actionButton2label.text = action2label;
        if(action2cb != null) 
            actionButton2.GetComponentInChildren<Button>().onClick.AddListener(action2cb);
        
    }

    public void Hide() {
        // TODO: Tween
        gameObject.SetActive(false);
    }
}
}
