using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSelectedMove : MonoBehaviour {

    [SerializeField] float moveRange;

    public void OnSelected() {
        transform.Find("Card").position += transform.up * moveRange;
    }
    public void OnReleased() {
        transform.Find("Card").localPosition = Vector3.zero;
    }

}
