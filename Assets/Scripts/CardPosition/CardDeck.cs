using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using DG.Tweening;

public class CardDeck : CardPosition {

    [SerializeField] List<Card> initialCards;
    Stack<Card> cards;

    CardInstance frontCard;

    void Start() {
        frontCard = CardDrawer.instance.Draw(null, CardState.deck);
        SetPos(frontCard.transform, true);
        cards = new Stack<Card>();
        foreach(var card in initialCards) {
            cards.Push(card);
        }
        frontCard.SetCount(cards.Count);
    }


    public void Shuffle() {
        cards = new Stack<Card>(cards.OrderBy(x => Guid.NewGuid()));
    }

    public override IEnumerator Add(CardVariable target) {
        CardPosition prevHome = target.GetHome();
        var cardInstances = target.Move(this);
        foreach(var card in cardInstances) {
            cards.Push(card.status);
        }
        for(int i = cardInstances.Count - 1; i >= 0; i--) {
            cardInstances[i].transform.SetParent(transform);
            cardInstances[i].transform.DOScale(Vector3.one * 0.5f, 0.5f).SetEase(Ease.Linear);
            cardInstances[i].transform.DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.Linear);
            cardInstances[i].transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutSine).WaitForCompletion();
        }
        yield return new WaitForSeconds(0.6f);
        for(int i = cardInstances.Count - 1; i >= 0; i--) Destroy(cardInstances[i].gameObject);
        frontCard.SetCount(cards.Count);
        if(prevHome is CardSupply && target.Get().Count > 0){
            yield return StartCoroutine(PlayReaction(target.Get(), ActionTrigger.getThisCard, owner));
            yield return ActionAssembly.instance.RunReaction(ActionTrigger.get, new StagedCardVariable(target.Get(), this), owner);

        }
    }


    public CardVariable Get(int num) {
        List<Card> ret = new List<Card>();
        Stack<Card> tmp = new Stack<Card>();
        for(int i = 0; i < num; i++) {
            if(cards.Count <= 0) break;
            var now = cards.Pop();
            ret.Add(now);
            tmp.Push(now);
        }
        while(tmp.Count > 0) cards.Push(tmp.Pop());
        return new StagedCardVariable(ret, this);
    }

    public override CardVariable Get() {
        return new StagedCardVariable(new List<Card>(cards), this);
    }

    public override List<CardInstance> Remove(CardVariable target) {
        List<CardInstance> ret = new List<CardInstance>();
        var targetCards = new List<Card>(target.Get());
        Stack<Card> tmp = new Stack<Card>();
        while(targetCards.Count > 0) {
            var now = cards.Pop();
            if(targetCards.Contains(now)) {
                targetCards.Remove(now);
                var instance = CardDrawer.instance.Draw(now, CardState.move);
                SetPos(instance.transform, false);
                ret.Add(instance);
            }
            else tmp.Push(now);
        }
        while(tmp.Count > 0) cards.Push(tmp.Pop());
        frontCard.SetCount(cards.Count);
        return ret;
    }

    void SetPos(Transform card, bool isFrontCard) {
        card.SetParent(transform);
        card.localPosition = Vector3.zero;
        if(!isFrontCard) card.localRotation = Quaternion.identity;
        card.localScale = Vector3.one * 0.5f;
    }

    public int GetRest() {
        return cards.Count;
    }

    public IEnumerator OnGameEnd(CardField field) {
        var cardCopy = new List<Card>(cards.ToList());
        foreach(var card in cardCopy) {
            if(!card.HasActionTrigger(ActionTrigger.endGame)) continue;
            yield return ActionExcuter.instance.AddXToField(field, new StagedCardVariable(new List<Card>(){card}, this));
            yield return ActionAssembly.instance.Run(card.action, owner, 
                new StagedCardVariable(new List<Card>(){card}, field), ActionTrigger.endGame, ActionTarget.myself, null, null);
        }
    }

    public void SetAll(List<Card> cards) {
        cards.Reverse();
        this.cards = new Stack<Card>(cards);
    }
}
