using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StagedCardVariable : CardVariable {

    readonly List<Card> cards;

    public StagedCardVariable(List<Card> cards, CardPosition pos) {
        this.cards = cards;
        this.home = pos;
    }

    public override List<Card> Get() {
        return cards;
    }

    public override string ToString() {
        string ret = "";
        foreach(var card in cards) {
            ret += card.card_name + ";";
        }
        return ret;
    }

    public override List<CardVariable> Split(List<Card> target) {
        List<Card> x = new List<Card>();  
        List<Card> y = new List<Card>(cards);  

        foreach(var card in target) {
            var t = y.Where(x => x == card).First();
            x.Add(t);
            y.Remove(t);
        }
        return new List<CardVariable>(){new StagedCardVariable(x, home), new StagedCardVariable(y, home)};
    }

    public override List<CardVariable> SplitSingle() {
        return cards.Select(x => new StagedCardVariable(new List<Card>(){x}, home) as CardVariable).ToList();
    }

    public override List<CardInstance> Move(CardPosition next) {
        var ret = home.Remove(this);
        Debug.Log(home + " => " + next + " : " + ToString());
        if(cards.Count > 0) SoundPlayer.instance.PlaySoundEffect(SoundEffectType.se_take_card, 0);
        home = next;
        return ret;
    }

    public override IEnumerator GetWindow(int minX, int maxX) {
        return WindowManager.instance.OpenListedCardSelectWindow(cards, minX, maxX);
    }

    public override List<CardVariable> Choice(List<Card> target) {
        return Split(target);
    }

    public override List<Card> Select(int minCost, int maxCost, string type) {
        List<Card> ret = new List<Card>();
        foreach(var card in cards) {
            if(card.cost < minCost) continue;
            if(card.cost > maxCost) continue;
            if(type != "" && !card.types.Contains(type)) continue;
            ret.Add(card);
        }
        return ret;
    }

    public override List<Card> Select(List<Card> target) {
        List<Card> ret = new List<Card>();
        foreach(var card in cards) {
            if(target.Contains(card)) ret.Add(card);
        }
        return ret;
    }

    public override List<Card> Distinct() {
        return cards.Distinct().ToList();
    }

    public override bool CheckSame(CardInstance card) {
        return Get().Contains(card.status);
    }

    public override bool CheckSkippableChoice(int a, int b) {
        if(a == 0 && b == 0) return true;
        if(cards.Count <= a) return true;
        if(a == b && cards.Distinct().Count() == 1) return true;
        return false;
    }

    public override List<Card> Choice(int a, int b) {
        var ret = new List<Card>();
        if(a == 0 && b == 0) {}
        else if(cards.Count <= a) ret = cards;
        else if(a == b && cards.Distinct().Count() == 1) ret = cards.Take(a).ToList();
        return ret;
    }

}