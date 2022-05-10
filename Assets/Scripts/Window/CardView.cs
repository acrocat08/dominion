using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardView : MonoBehaviour {

    public void Open(Card card) {
        var cardInstance = CardDrawer.instance.Draw(card, CardState.ui);
        cardInstance.transform.localScale = Vector3.one;
        cardInstance.transform.SetParent(transform);
        cardInstance.transform.localPosition = Vector3.zero;
    }


}
