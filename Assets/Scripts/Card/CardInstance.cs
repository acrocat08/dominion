using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardInstance : MonoBehaviour {

    public Card status;
    [SerializeField] GameObject backObj;
    bool isReversed;
    CardClickListener clickListener;

    public CardState state;

    [SerializeField] Shadow handShadow;
    [SerializeField] GameObject cardCount;
    [SerializeField] GameObject cardCountMini;
    GameObject targetCardCount;
    [SerializeField] GameObject blackShadow;
    [SerializeField] GameObject blackShadowMini;
    GameObject targetShadow;
    [SerializeField] GameObject miniCard;

    int discount;
    [SerializeField] Text cardCostText;
    [SerializeField] List<Color> costTextColors;


    public void SetState(CardState state) { 
        this.state = state;
        if(state == CardState.hand) {
            MakeMiniCard(false);
            SetComponents(true, false, false);
        }
        else if(state == CardState.deck) {
            Reverse(true);
            MakeMiniCard(false);
            SetComponents(false, true, false);
        }
        else if(state == CardState.discard) {
            MakeMiniCard(false);
            SetComponents(false, true, true);
        }
        else if(state == CardState.field) {
            MakeMiniCard(false);
            SetComponents(false, false, true);
        }
        else if(state == CardState.select) {
            MakeMiniCard(false);
            SetComponents(false, true, false);
            SetNum(0);
        }
        else if(state == CardState.supply) {
            MakeMiniCard(true);
            SetComponents(false, true, false);
        }
        else if(state == CardState.select_supply) {
            MakeMiniCard(true);
            SetComponents(false, true, false);
        }
        else {
            MakeMiniCard(false);
            SetComponents(false, false, false);
        }
        GetComponent<CardSelectedMove>().OnReleased();
    }

    public void Reverse(bool toBack) {
        isReversed = toBack;
        backObj.SetActive(toBack);
    }

    public void MakeMiniCard(bool toMini) {
        targetCardCount = toMini ? cardCountMini : cardCount;
        targetShadow = toMini ? blackShadowMini : blackShadow;
        miniCard.SetActive(toMini);
        var child = transform.Find("Card");
        child.GetComponent<Image>().enabled = !toMini;
        child.Find("Description").gameObject.SetActive(!toMini);
        child.Find("Type").gameObject.SetActive(!toMini);
    }

    public void SetClickListener(CardClickListener listener) {
        clickListener = listener;
    }

    public void OnClickDown() {
        if(Input.GetMouseButtonDown(0)) {
            if(clickListener != null) clickListener.OnClickedCard(this);

        }
        else if(Input.GetMouseButtonDown(1)) {
            if(isReversed) return;
            if(WindowManager.instance.CheckOpenedCardView()) {
                WindowManager.instance.CloseCardView();
            }
            else {
                WindowManager.instance.OpenCardView(status);
            }
        }
    }

    public void SetComponents(bool shadow, bool count, bool black) {
        handShadow.enabled = shadow;
        targetShadow.SetActive(black);
        targetCardCount.SetActive(count);
    }

    public void SetCount(int num) {
        targetCardCount.transform.GetComponentInChildren<Text>().text = num.ToString();
        if(state == CardState.supply) {
            if(num == 0) targetShadow.SetActive(true);
            else targetShadow.SetActive(false);
        }
    }

    public void OnMouseFocused() {
        if(state != CardState.hand) return;
        GetComponent<CardSelectedMove>().OnSelected();
    }

    public void OnMouseUnFocused() {
        if(state != CardState.hand) return;
        GetComponent<CardSelectedMove>().OnReleased();
    }

    public void SetNum(int num) {
        if(state != CardState.select && state != CardState.select_supply) return;
        targetCardCount.GetComponent<CardNumDrawer>().ChangeNum(num);
    }

    public void AddDiscount(int val) {
        if(GetCost() - val < 0) return;
        discount += val;
        cardCostText.text = (status.cost - discount).ToString();
        if(discount == 0) cardCostText.color = costTextColors[0];
        else if(discount > 0) cardCostText.color = costTextColors[1];
        else if(discount < 0) cardCostText.color = costTextColors[2];
    }

    public void ResetDiscount() {
        discount = 0;
        cardCostText.text = (status.cost).ToString();
        cardCostText.color = costTextColors[0];
    }

    public int GetCost() {
        return status.cost - discount;
    }
}

public enum CardState {
    hand,
    deck,
    field,
    supply,
    discard,
    select,
    ui,
    move,
    select_supply,
}