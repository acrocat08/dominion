using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyCardSelectWindow : MonoBehaviour, CardClickListener {

    [SerializeField] Transform cardView;
    Dictionary<Card, int> rest;
    Dictionary<Card, int> selectedNum;

    List<CardInstance> selected;
    bool isClosed;
    int minX, maxX;

    
    [SerializeField] float cardIntervalX;
    [SerializeField] float cardIntervalY;
    [SerializeField] float actionCardPosX;
    [SerializeField] float baseCardSize;
    [SerializeField] float actionCardSize;


    public IEnumerator Open(List<Card> cards, List<int> restVal, int minX, int maxX, List<int> discounts) {
        rest = new Dictionary<Card, int>();
        selectedNum = new Dictionary<Card, int>();
        selected = new List<CardInstance>();
        isClosed = false;
        this.minX = minX;
        this.maxX = maxX;
        MakeDict(cards, restVal);
        transform.SetParent(GameObject.Find("Canvas").transform);

        for(int i = 0; i < 7; i++) {
            if(rest[cards[i]] == 0) continue;
            var card = CardDrawer.instance.Draw(cards[i], CardState.select_supply);
            card.transform.SetParent(cardView);
            card.SetClickListener((CardClickListener)this);
            card.transform.localScale = Vector3.one * baseCardSize;
            card.transform.localPosition = Vector3.down * cardIntervalY * (i % 4) * baseCardSize 
                + Vector3.right * cardIntervalX * (i / 4) * baseCardSize;
            card.SetNum(0);
            card.AddDiscount(discounts[i]);
        }
        for(int i = 0; i < 10; i++) {
            if(rest[cards[i + 7]] == 0) continue;
            var card = CardDrawer.instance.Draw(cards[i + 7], CardState.select_supply);
            card.transform.SetParent(cardView);
            card.SetClickListener((CardClickListener)this);
            card.transform.localScale = Vector3.one * actionCardSize;
            card.transform.localPosition = Vector3.down * cardIntervalY * (i / 5) * actionCardSize 
                + Vector3.right * (cardIntervalX * (i % 5) * actionCardSize + actionCardPosX);
            card.SetNum(0);
            card.AddDiscount(discounts[i + 7]);
        }

        while(!isClosed) {
            yield return null;
        }         

        yield return MakeCardList();
    }

    void MakeDict(List<Card> cards, List<int> restVal) {
        for(int i = 0; i < cards.Count; i++) {
            rest.Add(cards[i], restVal[i]);
            selectedNum.Add(cards[i], 0);
        }
    }

    List<Card> MakeCardList() {
        List<Card> ret = new List<Card>();
        foreach(var card in selectedNum.Keys) {
            for(int i = 0; i < selectedNum[card]; i++) {
                ret.Add(card);
            }
        }
        return ret;
    }

    public void OnClickedCard(CardInstance card) {
        SoundPlayer.instance.PlaySoundEffect(SoundEffectType.se_take_card, 0);
        if(selectedNum[card.status] < rest[card.status]) {
            selectedNum[card.status]++;
            card.SetNum(selectedNum[card.status]);
            selected.Add(card);
        }
    }

    public void Close() {
        int sum = 0;
        foreach(var val in selectedNum.Values) sum += val;
        if(sum < minX) return;
        if(sum > maxX) return;
        SoundPlayer.instance.PlaySoundEffect(SoundEffectType.se_ok, 0);
        WindowManager.instance.CloseWindow(gameObject);
        isClosed = true;
    }

    public void Reset() {
        foreach(var card in selected) {
            card.SetNum(0);
            selectedNum[card.status] = 0;
        }
        selected = new List<CardInstance>();
    }

}
