using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardNumDrawer : MonoBehaviour {
    
    [SerializeField] Image numIcon;
    [SerializeField] Text numText;

    public void ChangeNum(int x) {
        if(x <= 0) {
            numIcon.enabled = false;
            numText.enabled = false;
        }
        else {
            numIcon.enabled = true;
            numText.enabled = true;
            numText.text = x.ToString();
        }
    }

}
