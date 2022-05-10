using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System;

public class CardSupply : CardPosition {

    List<Card> cards;
    [SerializeField] List<int> rest;

    [SerializeField] float cardIntervalX;
    [SerializeField] float cardIntervalY;
    [SerializeField] float actionCardPosX;
    [SerializeField] float baseCardSize;
    [SerializeField] float actionCardSize;

    List<CardInstance> cardInstances;

    [SerializeField] GamePlayer player;
    [SerializeField] CardPackage cardPackage;
    [SerializeField] int subPackageId;
    [SerializeField] bool doRandomize;

    [SerializeField] float moveRange;
    bool isMoved;

    [SerializeField] CardSubSupply subSupply;

    void Start() {
        cards = new List<Card>();
        cardInstances = new List<CardInstance>();
    }    

    public List<Card> ChoiceCards() {
        var ret = new List<Card>();
        var target = new List<Card>();
        if(doRandomize) {
            target = cardPackage.cards.OrderBy(x => Guid.NewGuid()).ToList();
            target = target.Take(10).ToList();
        }
        else {
            target = new List<Card>(cardPackage.subPackages[subPackageId].cards);
        }
        target.Sort((a, b) => a.cost - b.cost);
        ret.AddRange(target);
        return ret;
    }

    public void Setup(List<Card> target) {
        cards.AddRange(cardPackage.baseCards);
        cards.AddRange(target);
        for(int i = 0; i < 7; i++) {
            var card = CardDrawer.instance.Draw(cards[i], CardState.supply);
            card.transform.SetParent(transform);
            card.Reverse(false);
            card.SetClickListener((CardClickListener)player);
            card.transform.localScale = Vector3.one * baseCardSize;
            card.transform.localPosition = Vector3.down * cardIntervalY * (i % 4) * baseCardSize 
                + Vector3.right * cardIntervalX * (i / 4) * baseCardSize;
            cardInstances.Add(card);
        }
        for(int i = 0; i < 10; i++) {
            var card = CardDrawer.instance.Draw(cards[i + 7], CardState.supply);
            card.transform.SetParent(transform.Find("Container"));
            card.SetClickListener((CardClickListener)player);
            card.transform.localScale = Vector3.one * actionCardSize;
            card.transform.localPosition = Vector3.down * cardIntervalY * (i / 5) * actionCardSize 
                + Vector3.right * (cardIntervalX * (i % 5) * actionCardSize + actionCardPosX);
            cardInstances.Add(card);
        }
        UpdateCount();

        List<Card> subCards = new List<Card>();
        foreach(var card in target) {
            subCards.AddRange(card.subCards);
        }
        subSupply.SetSubCards(subCards);
    }

    public CardVariable GetSameOne(Card card) {
        var index = cards.IndexOf(card);
        if(rest[index] == 0) return new StagedCardVariable(new List<Card>(), this);
        rest[index]--;
        var cardInstance = CardDrawer.instance.Draw(card, CardState.move);
        SetPos(cardInstance.transform, cardInstances[index].transform);
        UpdateCount();
        return new TargetCardVariable(new List<CardInstance>(){cardInstance}, this);
    }


    void SetPos(Transform card, Transform pos) {
        card.rotation = Quaternion.Euler(0, 0, 0);
        card.SetParent(pos);
        card.localScale = Vector3.one * 0.5f;
        card.localPosition = Vector3.zero;
    }

    public override IEnumerator Add(CardVariable target) {
        var targetInstances = target.Move(this);
        var subCards = new List<CardInstance>();
        foreach(var card in targetInstances) {
            var index = cards.IndexOf(card.status);
            if(index < 0) {
                subCards.Add(card);
                continue;
            }
            rest[index]++;
            card.transform.SetParent(cardInstances[index].transform);
            card.transform.DOScale(Vector3.one * 0.5f, 0.5f).SetEase(Ease.Linear);
            card.transform.DORotate(Vector3.zero, 0.5f).SetEase(Ease.Linear);
            card.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutSine);
        }
        yield return new WaitForSeconds(0.6f);
        UpdateCount();
        yield return subSupply.Add(new TargetCardVariable(subCards, target.GetHome()));
        for(int i = targetInstances.Count - 1; i >= 0; i--) Destroy(targetInstances[i].gameObject);
    }

    public override List<CardInstance> Remove(CardVariable target) {
        List<CardInstance> ret = new List<CardInstance>();
        var targetCards = new List<Card>(target.Get());
        foreach(var card in targetCards) {
            var index = cards.IndexOf(card);
            rest[index]--;
            var instance = CardDrawer.instance.Draw(card, CardState.move);
            SetPos(instance.transform, cardInstances[index].transform);
            ret.Add(instance);
        }
        UpdateCount();
        return ret;
    }

    public override CardVariable Get() {
        List<int> filter = new List<int>();
        for(int i = 0; i < cards.Count; i++) filter.Add(rest[i] > 0 ? 1 : 0);
        return new SupplyCardVariable(cards, filter, this);
    }

    public List<int> GetRest() {
        return rest;
    }

    void UpdateCount() {
        for(int i = 0; i < cards.Count; i++) {
            cardInstances[i].SetCount(rest[i]);
        }
    }

    public bool CheckBuyable(Card target) {
        return rest[cards.IndexOf(target)] > 0;
    }

    public void SetClickListener(GamePlayer player) {   //FIX
        foreach(var card in cardInstances) {
            card.SetClickListener(player);
        }
    }

    public void MoveSupply(bool toDown) {
        transform.Find("Container").DOLocalMoveY(toDown ? 0 : moveRange, 0.5f).SetEase(Ease.OutSine);
    }

    public CardInstance GetAt(int pos) {
        return cardInstances[pos];
    }

    public int GetIndex(CardInstance card) {
        return cardInstances.IndexOf(card);
    }

    public bool CheckGameEnd() {
        if(rest[0] == 0) return true;
        if(rest.Count(x => x == 0) >= 3) return true;
        return false;
    }
    public int GetEmptyCard() {
        return rest.Count(x => x == 0);
    }

    public void AddDiscount(CardVariable target, int val) {
        var uniqued = target.Get().Distinct();
        foreach(var card in uniqued) {
            var index = cards.IndexOf(card);
            cardInstances[index].AddDiscount(val);
        }
    }

    public void ResetDiscount() {
        foreach(var card in cardInstances) {
            card.ResetDiscount();
        }
    }

    public List<int> GetDiscounts() {
        return cardInstances.Select(x => x.status.cost - x.GetCost()).ToList();
    }

    public CardPackage GetPackage() {
        return cardPackage;
    }
} 
