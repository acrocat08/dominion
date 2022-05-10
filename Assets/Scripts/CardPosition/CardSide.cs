using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class CardSide : CardPosition {

    List<CardInstance> cards;
    Dictionary<CardInstance, List<CardInstance>> dict;

    void Start() {
        cards = new List<CardInstance>();
        dict = new Dictionary<CardInstance, List<CardInstance>>();
    }

    public override IEnumerator Add(CardVariable target) {
        List<CardInstance> cardInstances = target.Move(this);
        cards.AddRange(cardInstances);
        dict[cardInstances[0]] = new List<CardInstance>();
        for(int i = cardInstances.Count - 1; i >= 0; i--) {
            cardInstances[i].SetState(CardState.field);
            cardInstances[i].transform.SetParent(transform);
            cardInstances[i].Reverse(false);
            cardInstances[i].transform.DOScale(Vector3.one * 0.5f, 0.5f).SetEase(Ease.Linear);
            cardInstances[i].transform.DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.Linear);
            cardInstances[i].transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutSine).WaitForCompletion();
            ActionAssembly.instance.AddCardHolder(new TargetCardVariable(new List<CardInstance>(){cardInstances[i]}, this), ActionTarget.reactionWithSide);
        }
        Debug.Log(dict.Keys.Count);
        yield return new WaitForSeconds(0.6f);
    }    

    public IEnumerator Add(CardVariable target, CardVariable key) {
        if(target.Get().Count == 0) yield return null;
        List<CardInstance> cardInstances = target.Move(this);
        var k = dict.Keys.Where(x => dict[x].Count == 0).First();
        dict[k].AddRange(cardInstances);
        for(int i = cardInstances.Count - 1; i >= 0; i--) {
            cardInstances[i].SetState(CardState.field);
            cardInstances[i].transform.SetParent(transform);
            cardInstances[i].Reverse(false);
            cardInstances[i].transform.DOScale(Vector3.one * 0.5f, 0.5f).SetEase(Ease.Linear);
            cardInstances[i].transform.DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.Linear);
            cardInstances[i].transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutSine).WaitForCompletion();
        }
        yield return new WaitForSeconds(0.6f);
    } 


    public override CardVariable Get() {
        return new StagedCardVariable(new List<Card>(cards.Select(x => x.status)), this);
    }

    public CardVariable Get(CardVariable key) {
        if(!(key is TargetCardVariable)) return new CardVariable();
        var k = ((TargetCardVariable)key).GetInstances()[0];
        if(!dict.Keys.Contains(k)) return new TargetCardVariable(new List<CardInstance>(), this);
        return new TargetCardVariable(dict[k], this);
    }

    public CardVariable GetAll() {
        var ret = new List<CardInstance>();
        ret.AddRange(cards);
        foreach(var key in dict.Keys) {
            ret.AddRange(dict[key]);
        }
        return new TargetCardVariable(ret, this);
    }


    public override void RemoveInstance(List<CardInstance> target) {
        var tmp = new List<CardInstance>(target);
        foreach(var card in tmp) {
            if(cards.Where(x => x == card).Count() > 0) {
                var now = cards.Where(x => x == card).First();
                cards.Remove(now);
                if(dict[now].Count == 0) dict.Remove(now);
                ActionAssembly.instance.DeleteCardHolder(now, ActionTarget.reactionWithSide);
            }

            foreach(var key in dict.Keys) {
                if(dict[key].Where(x => x == card).Count() > 0) {
                    var nowd = dict[key].Where(x => x == card).First();
                    dict[key].Remove(nowd);
                }
            }
        }
        Debug.Log(dict.Keys.Count);
    }


    void SetPos(Transform card) {
        card.rotation = Quaternion.Euler(0, 0, 0);
        card.SetParent(transform);
        card.localRotation = Quaternion.identity;
        card.localScale = Vector3.one * 0.5f;
        card.localPosition = Vector3.zero;
    }

    public IEnumerator PlaySustainAction() {
        yield return StartCoroutine(PlayReactionWithInstance(cards, ActionTrigger.getTurnWithSide, owner));
    }

}
