using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ActionExcuter : MonoBehaviour {

    public static ActionExcuter instance;

    void Start() {
        instance = this;
    }

    public IEnumerator TakeCards(CardHand hand, CardDeck deck, CardDiscard discard, GamePlayer player, int a) {
        InputLocker.instance.Lock();
        for(int i = 0; i < a; i++) {
            if(deck.GetRest() == 0) {
                yield return deck.Add(discard.Get());
                yield return new WaitForSeconds(0.4f);
                yield return player.ShuffleDeck();
            }
            var card = deck.Get(1);
            yield return hand.Add(card);
            hand.SetClickListener(player);
        }
        InputLocker.instance.UnLock();
    }

    public IEnumerator SelectAllFromHand(CardHand hand) {
        yield return hand.Get();
    }

    public IEnumerator SelectSomeFromDeck(CardDeck deck, CardDiscard discard, int a) {
        if(deck.GetRest() < a) {
            yield return deck.Add(discard.Get());
            yield return new WaitForSeconds(0.4f);
            deck.Shuffle();
        }
        yield return deck.Get(a);
    }

    public IEnumerator SelectAllFromField(CardField field) {
        yield return field.Get();
    }

    public IEnumerator SelectAllFromDiscard(CardDiscard discard) {
        yield return discard.Get();
    }

    public IEnumerator SelectAllFromWaste(CardWaste waste) {
        yield return waste.Get();
    }

    public IEnumerator SelectAllFromSupply(CardSupply supply) {
        yield return supply.Get();
    }

    public IEnumerator SelectAllFromSide(CardSide side) {
        yield return side.Get();
    }

    public IEnumerator SelectCostAndGroupFromX(CardVariable x, int a, int b, string s) {
        var target = x.Select(a, b, s);
        yield return x.Split(target);
    }

    public IEnumerator SelectCardSameOfZ(CardVariable x, CardVariable z) {
        var target = z.Get();
        yield return x.Split(x.Select(target));
    }

    public IEnumerator ChoiceFromX(CardVariable x, GamePlayer player, int a, int b) {
        var c = player.ChoiceCards(x, a, b);
        yield return c;
        yield return (List<CardVariable>)c.Current;
    }

    public IEnumerator AddXToHand(CardHand hand, GamePlayer player, CardVariable x) {
        InputLocker.instance.Lock();
        yield return hand.Add(x);
        hand.SetClickListener(player);
        InputLocker.instance.UnLock();
    }

    public IEnumerator AddXToDeck(CardDeck deck, CardVariable x) {
        InputLocker.instance.Lock();
        yield return deck.Add(x);
        InputLocker.instance.UnLock();
    }    

    public IEnumerator AddXToField(CardField field, CardVariable x) {
        InputLocker.instance.Lock();
        yield return field.Add(x);
        InputLocker.instance.UnLock();
    }

    public IEnumerator AddXToDiscard(CardDiscard discard, CardVariable x) {
        InputLocker.instance.Lock();
        yield return discard.Add(x);
        InputLocker.instance.UnLock();
    }

    public IEnumerator AddXToWaste(CardWaste waste, CardVariable x) {
        InputLocker.instance.Lock();
        yield return waste.Add(x);
        InputLocker.instance.UnLock();
    }

    public IEnumerator AddXToSupply(CardSupply supply, CardVariable x) {
        InputLocker.instance.Lock();
        yield return supply.Add(x);
        InputLocker.instance.UnLock();
    }

    public IEnumerator AddXToSide(CardSide side, CardVariable x) {
        InputLocker.instance.Lock();
        yield return side.Add(x);
        InputLocker.instance.UnLock();
    }

    public IEnumerator SelectAllFromDeck(CardDeck deck) {
        yield return deck.Get();
    }

    public IEnumerator SelectUniqueCardsOfY(CardVariable y) {
        var target = y.Distinct();
        yield return y.Split(target)[0];
    }

    public IEnumerator SelectAllFromSideWithKey(CardSide side, CardVariable key) {
        yield return side.Get(key);
    }

    public IEnumerator AddXToSideWithKey(CardSide side, CardVariable x, CardVariable key) {
        InputLocker.instance.Lock();
        yield return side.Add(x, key);
        InputLocker.instance.UnLock();
    }

    public IEnumerator SelectSubCards(CardSubSupply sub, string type) {
        yield return sub.Get(type);
    }

    public IEnumerator SelectRandomCard(CardVariable x, int num) {
        var target = new List<Card>(x.Get());
        var choiced = new List<Card>();
        for(int i = 0; i < num; i++) {
            var tmp = target[Random.Range(0, target.Count)];
            choiced.Add(tmp);
            target.Remove(tmp);   
        }
        yield return x.Split(choiced);
    }


}
