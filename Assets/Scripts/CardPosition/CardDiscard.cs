using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class CardDiscard : CardPosition {

    List<Card> cards;
    CardInstance frontCard;

    void Start() {
        cards = new List<Card>();
    }

    public override IEnumerator Add(CardVariable target) {
        CardPosition prevHome = target.GetHome();
        var cardInstances = target.Move(this);
        cards.AddRange(cardInstances.Select(x => x.status));
        for(int i = 0; i < cardInstances.Count; i++) {
            cardInstances[i].Reverse(false);
            cardInstances[i].transform.SetParent(transform);
            cardInstances[i].transform.DOScale(Vector3.one * 0.5f, 0.5f).SetEase(Ease.Linear);
            cardInstances[i].transform.DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.Linear);
            cardInstances[i].transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutSine).WaitForCompletion();
        }
        yield return new WaitForSeconds(0.6f);
        for(int i = cardInstances.Count - 1; i >= 0; i--) Destroy(cardInstances[i].gameObject);
        UpdateFrontCard();
        if(prevHome is CardSupply && target.Get().Count > 0) {
            yield return StartCoroutine(PlayReaction(target.Get(), ActionTrigger.getThisCard, owner));
            yield return ActionAssembly.instance.RunReaction(ActionTrigger.get, new StagedCardVariable(target.Get(), this), owner);
        }
        if((prevHome is CardHand || prevHome is CardDeck) && target.Get().Count > 0) {
            yield return StartCoroutine(PlayReaction(target.Get(), ActionTrigger.discard, owner));
            yield return ActionAssembly.instance.RunReaction(ActionTrigger.discard, new StagedCardVariable(target.Get(), this), owner);
        }
    }

    public override CardVariable Get() {
        return new StagedCardVariable(new List<Card>(cards), this);
    }

    public override List<CardInstance> Remove(CardVariable target) {
        List<CardInstance> ret = new List<CardInstance>();
        var targetCards = target.Get();
        foreach(var card in targetCards) {
            cards.Remove(card);
            var instance = CardDrawer.instance.Draw(card, CardState.move);
            SetPos(instance.transform, false);
            ret.Add(instance);
        }
        UpdateFrontCard();
        return ret;
    }

    void SetPos(Transform card, bool isFrontCard) {
        card.rotation = Quaternion.Euler(0, 0, 0);
        card.SetParent(transform);
        if(!isFrontCard) card.localRotation = Quaternion.identity;
        card.localScale = Vector3.one * 0.5f;
        card.localPosition = Vector3.zero;
    }

    void UpdateFrontCard() {
        if(frontCard != null) Destroy(frontCard.gameObject);
        if(cards.Count == 0) return;
        frontCard = CardDrawer.instance.Draw(cards[cards.Count - 1], CardState.move);
        SetPos(frontCard.transform, true);
        frontCard.SetState(CardState.discard);
        frontCard.SetCount(cards.Count);   
    }
}
