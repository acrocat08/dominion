using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextWindow : MonoBehaviour {

    [SerializeField] Text text;
    string contents;

    IEnumerator coroutine;

    public void Open(string contents) {
        this.contents = contents;
        text.text = contents;
        coroutine = Wait();
        StartCoroutine(coroutine);
    }

    IEnumerator Wait() {
        int n = 0;
        while(true) {
            text.text = contents + new string('.', n + 1);
            n = (n + 1) % 3;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void Close() {
        StopCoroutine(coroutine);
    }

}
