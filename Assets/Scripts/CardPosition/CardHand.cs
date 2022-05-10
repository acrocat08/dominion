using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
public class CardHand : CardPosition {

    List<CardInstance> cards;
    [SerializeField] float anglePerCard;
    [SerializeField] float minHandAngle;
    [SerializeField] float maxHandAngle;
    [SerializeField] float handRange;

    [SerializeField] List<Card> baseCoinCards;  //FIX

    [SerializeField] bool isReversed;


    void Start() {
        cards = new List<CardInstance>();
    }

    public override IEnumerator Add(CardVariable target) {
        CardPosition prevHome = target.GetHome();
        var cardInstances = target.Move(this);
        foreach(var card in cardInstances) {
            card.SetState(CardState.hand);
            card.transform.SetParent(transform);
            card.Reverse(isReversed);
            ActionAssembly.instance.AddCardHolder(new TargetCardVariable(new List<CardInstance>(){card}, this), ActionTarget.reactionWithHand);
        }
        cards.AddRange(cardInstances);
        LineUp();
        yield return new WaitForSeconds(0.2f);
        if(prevHome is CardSupply && target.Get().Count > 0) {
            yield return StartCoroutine(PlayReaction(target.Get(), ActionTrigger.getThisCard, owner));
            yield return ActionAssembly.instance.RunReaction(ActionTrigger.get, target, owner);
        }
        if(prevHome.GetOwner() == owner && prevHome is CardDeck && target.Get().Count > 0) {
            yield return StartCoroutine(PlayReaction(target.Get(), ActionTrigger.takeThisCard, owner));
            yield return ActionAssembly.instance.RunReaction(ActionTrigger.takeCard, target, owner);

        }
    }

    public override CardVariable Get() {
        return new StagedCardVariable(cards.Select(x => x.status).ToList(), this);
    }

    public override CardVariable GetSameOne(CardInstance card) {
        var target = cards.Where(x => x == card).First();
        return new TargetCardVariable(new List<CardInstance>(){target}, this);
    }

    public override List<CardInstance> Remove(CardVariable target) {
        List<CardInstance> ret = new List<CardInstance>();
        foreach(var card in target.Get()) {
            var now = cards.Where(x => x.status == card).First();
            ret.Add(now);
            cards.Remove(now);
            ActionAssembly.instance.DeleteCardHolder(now, ActionTarget.reactionWithHand);
        }
        LineUp();
        return ret;
    }

    public override void RemoveInstance(List<CardInstance> target) {
        foreach(var card in target) {
            cards.Remove(card);
            ActionAssembly.instance.DeleteCardHolder(card, ActionTarget.reactionWithHand);
        }
        LineUp();
    }

    public void LineUp() {
        var handAngle = Mathf.Clamp(cards.Count * anglePerCard, minHandAngle, maxHandAngle);
        for(int i = 0; i < cards.Count; i++) {
            float angle = (handAngle / cards.Count) * (cards.Count - i - 0.5f) - handAngle / 2;
            var dir = Quaternion.Euler(0, 0, angle) * Vector3.up;
            cards[i].transform.DOLocalMove(dir * handRange, 0.5f).SetEase(Ease.OutSine);
            cards[i].transform.DOLocalRotate(new Vector3(0, 0, angle), 0.5f).SetEase(Ease.Linear);
            cards[i].transform.DOScale(Vector3.one * 1f, 0.5f).SetEase(Ease.Linear);
            cards[i].transform.SetSiblingIndex(i);
        }
    }

    public void SetClickListener(CardClickListener player) {
        foreach(var card in cards) {
            card.SetClickListener(player);
        }
    }

    public IEnumerator StartPurchaseWithHand(CardField field) {
        //yield return null;
        var cardCopy = new List<CardInstance>(cards);
        foreach(var card in cardCopy) {
            if(!card.status.HasActionTrigger(ActionTrigger.playCoin)) continue;
            yield return ActionExcuter.instance.AddXToField(field, new TargetCardVariable(new List<CardInstance>(){card}, this));
            yield return ActionAssembly.instance.Run(card.status.action, owner, 
                new TargetCardVariable(new List<CardInstance>(){card}, field), ActionTrigger.playCoin, ActionTarget.myself, null, null);
        }
        yield return StartCoroutine(PlayReactionWithInstance(cards, ActionTrigger.playCoin, owner));
    }

    public CardInstance GetAt(int pos) {
        return cards[pos];
    }

    public int GetIndex(CardInstance card) {
        return cards.IndexOf(card);
    }
}
