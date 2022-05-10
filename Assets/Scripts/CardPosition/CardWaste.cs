using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class CardWaste : CardPosition {

    List<Card> cards;

    void Start() {
        cards = new List<Card>();
    }

    public override IEnumerator Add(CardVariable target) {
        var prevHome = target.GetHome();
        List<CardInstance> cardInstances = target.Move(this);
        cards.AddRange(cardInstances.Select(x => x.status));
        for(int i = cardInstances.Count - 1; i >= 0; i--) {
            cardInstances[i].Reverse(false);
            cardInstances[i].transform.SetParent(transform);
            cardInstances[i].transform.DOScale(Vector3.one * 0.5f, 0.5f).SetEase(Ease.Linear);
            cardInstances[i].transform.DORotate(Vector3.zero, 0.5f).SetEase(Ease.Linear);
            cardInstances[i].transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutSine).WaitForCompletion();
        }
        yield return new WaitForSeconds(0.6f);
        for(int i = cardInstances.Count - 1; i >= 0; i--) Destroy(cardInstances[i].gameObject);
        if(target.Get().Count > 0) {
            yield return StartCoroutine(PlayReaction(target.Get(), ActionTrigger.goToWaste, prevHome.GetOwner()));
            yield return ActionAssembly.instance.RunReaction(ActionTrigger.waste, target, owner);
        }
        

    }    

    public override CardVariable Get() {
        return new StagedCardVariable(new List<Card>(cards), this);
    }

    public override List<CardInstance> Remove(CardVariable target) {
        List<CardInstance> ret = new List<CardInstance>();
        cards = new List<Card>();
        var targetCards = target.Get();
        foreach(var card in targetCards) {
            cards.Remove(card);
            var instance = CardDrawer.instance.Draw(card, CardState.move);
            SetPos(instance.transform);
            ret.Add(instance);
        }
        return ret;
    }

    void SetPos(Transform card) {
        card.SetParent(transform);
        card.rotation = Quaternion.identity;
        card.localScale = Vector3.one * 0.5f;
        card.localPosition = Vector3.zero;
    }

}
