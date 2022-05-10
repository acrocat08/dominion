using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputLocker : MonoBehaviour {

    public static InputLocker instance;
    int count;

    void Start() {
        instance = this;
        count = 0;
    }

    public void Lock() {
        count++;
    }

    public void UnLock() {
        count--;
    }

    public bool isLocked() {
        return count > 0;
    }
}
