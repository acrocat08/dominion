using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TargetCardVariable : CardVariable {

    List<CardInstance> cards;

    public TargetCardVariable(List<CardInstance> cards, CardPosition pos) {
        this.cards = cards;
        this.home = pos;
    }

    public override List<Card> Get() {
        return cards.Select(x => x.status).ToList();
    }

    public override string ToString() {
        string ret = "";
        foreach(var card in cards) {
            ret += card.status.card_name + "(" + card.GetInstanceID() + ")" +  ";";
        }
        return ret;
    }

    public override List<CardVariable> Split(List<Card> target) {
        List<CardInstance> x = new List<CardInstance>();  
        List<CardInstance> y = cards; 
        foreach(var card in target) {
            var t = y.Where(x => x.status == card).First();
            x.Add(t);
            y.Remove(t);
        }
        return new List<CardVariable>(){new TargetCardVariable(x, home), new TargetCardVariable(y, home)};
    }

    public override List<CardVariable> SplitSingle() {
        return cards.Select(x => new TargetCardVariable(new List<CardInstance>(){x}, home) as CardVariable).ToList();
    }

    public override List<CardInstance> Move(CardPosition next) {
        Debug.Log(home + " => " + next + " : " + ToString());
        var ret = new List<CardInstance>(cards);
        home.RemoveInstance(cards);
        SoundPlayer.instance.PlaySoundEffect(SoundEffectType.se_take_card, 0);
        home = next;
        return ret;
    }

    public override IEnumerator GetWindow(int minX, int maxX) {
        if(cards.Count < minX) return null;
        if(minX == maxX && minX == 0) return null;
        return WindowManager.instance.OpenListedCardSelectWindow(cards.Select(x => x.status).ToList(), minX, maxX);
    }


    public override bool CheckSame(CardInstance card) {
        foreach(var c in cards) {
            if(c == card) return true;
        }
        return false;
    }


    public override List<CardVariable> Choice(List<Card> target) {
        return Split(target);
    }

    public override List<Card> Select(int minCost, int maxCost, string type) {
        List<Card> ret = new List<Card>();
        foreach(var card in cards) {
            if(card.status.cost < minCost) continue;
            if(card.status.cost > maxCost) continue;
            if(type != "" && !card.status.types.Contains(type)) continue;
            ret.Add(card.status);
        }
        return ret;
    }

    public override List<Card> Select(List<Card> target) {
        List<Card> ret = new List<Card>();
        foreach(var card in cards) {
            if(target.Contains(card.status)) ret.Add(card.status);
        }
        return ret;
    }

    public override List<Card> Distinct() {
        return cards.Select(x => x.status).Distinct().ToList();
    }

    public List<CardInstance> GetInstances() {
        return cards;
    }

    public override bool CheckSkippableChoice(int a, int b) {
        if(a == 0 && b == 0) return true;
        if(cards.Count <= a) return true;
        if(a == b && cards.Select(x => x.status).Distinct().Count() == 1) return true;
        return false;
    }

    public override List<Card> Choice(int a, int b) {
        var ret = new List<Card>();
        if(a == 0 && b == 0) {}
        else if(cards.Count <= a) ret = cards.Select(x => x.status).ToList();
        else if(a == b && cards.Select(x => x.status).Distinct().Count() == 1) ret = cards.Select(x => x.status).Take(a).ToList();
        return ret;
    }


}