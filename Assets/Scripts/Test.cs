using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour {

    [SerializeField] NetworkManager network;

    [SerializeField] List<Card> testCards;

    [SerializeField] CardPackage cardPackage;

    [SerializeField] Text log;
    [SerializeField] Button button;

    int nextPlayer;

    void Update() {
        if(Input.GetKeyDown(KeyCode.Space)) {
            WindowManager.instance.OpenDictionaryWindow(cardPackage);
        }
    }


}
