using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class CardField : CardPosition {

    List<CardInstance> cards;
    int frontPos;

    [SerializeField] float cardIntervalX;
    [SerializeField] float cardIntervalY;
    [SerializeField] int maxColumn;
    [SerializeField] float angleRange;
    [SerializeField] float cardOffset;


    void Start() {
        cards = new List<CardInstance>();
        frontPos = 0;
    }

    public override IEnumerator Add(CardVariable target) {
        CardPosition prevHome = target.GetHome();
        List<CardInstance> cardInstances = target.Move(this);
        foreach(var card in cardInstances) {
            card.SetState(CardState.field);
            card.transform.SetParent(transform);
            card.Reverse(false);
        }
        foreach(var card in cardInstances) {
            Put(card.transform, frontPos);
            cards.Add(card);
            frontPos++;
        }
        yield return new WaitForSeconds(0.6f);        
        if(prevHome is CardSupply && target.Get().Count > 0) {
            yield return StartCoroutine(PlayReaction(target.Get(), ActionTrigger.getThisCard, owner));
            yield return ActionAssembly.instance.RunReaction(ActionTrigger.get, target, owner);
        }
    }

    public override CardVariable Get() {
        return new StagedCardVariable(cards.Select(x => x.status).ToList(), this);
    }

    public override List<CardInstance> Remove(CardVariable target) {
        List<CardInstance> ret = new List<CardInstance>();
        foreach(var card in target.Get()) {
            var now = cards.Where(x => x.status == card).First();
            ret.Add(now);
            cards.Remove(now);
        }
        return ret;
    }

    public override void RemoveInstance(List<CardInstance> target) {
        foreach(var card in target) {
            cards.Remove(card);
        }
    }

    public void ResetField() {
        frontPos = 0;
    }
    
        
    void Put(Transform card, int posNum) {
        var offset = Vector3.right * (posNum % maxColumn) * cardIntervalX 
            + Vector3.down * (posNum / maxColumn) * cardIntervalY;
        float angle = angleRange * (2 * Random.value - 0.5f);
        card.DOScale(Vector3.one * 0.5f, 0.5f).SetEase(Ease.Linear);
        card.DOLocalRotate(new Vector3(0, 0, angle), 0.5f).SetEase(Ease.Linear);
        card.DOLocalMove((Vector3)Random.insideUnitCircle.normalized * cardOffset + offset, 0.5f)
            .SetEase(Ease.OutSine);
    }

    public IEnumerator PlayRactionInEndTurn() {
        yield return StartCoroutine(PlayReactionWithInstance(cards, ActionTrigger.endTurnWithField, owner));
    }
}
