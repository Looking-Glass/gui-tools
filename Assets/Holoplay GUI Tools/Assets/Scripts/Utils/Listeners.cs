using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Diorama {

public class Listeners {
    Dictionary<Button, UnityAction> callbacks = new Dictionary<Button, UnityAction>();

    public Listeners Add(Button button, UnityAction cb) {
        try {
            button.onClick.AddListener(cb);
            callbacks.Add(button, cb);
        } catch (System.Exception e) {
            Debug.LogException(e, button);
        }
        return this;
    }


    public void RemoveAll() {
        foreach (var pair in callbacks) 
            pair.Key.onClick.RemoveListener(pair.Value);
        callbacks.Clear();
    }
}

}
