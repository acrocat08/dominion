using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ListedCardSelectWindow : MonoBehaviour, CardClickListener {

    [SerializeField] Transform cardView;

    List<CardInstance> selected;

    bool isClosed;
    int minX, maxX;
    int cardNum;

    void Start() {
        selected = new List<CardInstance>();
        isClosed = false;
        cardNum = 0;
    }
    public IEnumerator Open(List<Card> cards, int minX, int maxX) {
        this.minX = minX;
        this.maxX = maxX;
        transform.SetParent(GameObject.Find("Canvas").transform);
        //transform.position = Vector3.zero;

        foreach(Card card in cards) {
            CardInstance instance = CardDrawer.instance.Draw(card, CardState.select);
            instance.transform.localScale = Vector3.one * 0.75f;
            instance.GetComponent<CardInstance>().SetClickListener(this);
            instance.transform.SetParent(cardView);
            cardNum++;
        }

        while(!isClosed){
            yield return null;
        }

        yield return selected.Select(x => x.status).ToList();
    }

    public void OnClickedCard(CardInstance card) {
        SoundPlayer.instance.PlaySoundEffect(SoundEffectType.se_take_card, 0);
        if(selected.Contains(card)) {
            card.SetNum(0);
            selected.Remove(card);
        }
        else{
            card.SetNum(1);
            selected.Add(card);
        }
    }

    public void Close() {
        if(selected.Count > maxX) return;
        if(selected.Count < minX) return;
        SoundPlayer.instance.PlaySoundEffect(SoundEffectType.se_ok, 0);
        WindowManager.instance.CloseWindow(gameObject);
        isClosed = true;
    }

    public void Reset() {
        foreach(var card in selected) {
            card.SetNum(0);
        }
        selected = new List<CardInstance>();
    }

}
