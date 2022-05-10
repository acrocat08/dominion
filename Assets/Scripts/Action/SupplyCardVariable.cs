using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SupplyCardVariable : CardVariable {

    List<Card> cards;
    List<int> filter;

    public SupplyCardVariable(List<Card> cards, List<int> filter, CardSupply supply) {
        this.cards = cards;
        this.filter = filter;
        this.home = supply;
    }

    public override List<Card> Get() {
        List<Card> ret = new List<Card>();
        var rest = ((CardSupply)home).GetRest();
        for(int i = 0; i < cards.Count; i++) {
            for(int j = 0; j < rest[i] * filter[i]; j++) {
                ret.Add(cards[i]);
            }
        }
        return ret;
    }

    public override List<CardVariable> Split(List<Card> target) {
        List<int> xFilter = new List<int>(filter);
        List<int> yFilter = new List<int>(filter);
        for(int i = 0; i < filter.Count; i++) {
            if(target.Contains(cards[i])) yFilter[i] = 0;
            else xFilter[i] = 0; 
        }
        return new List<CardVariable>(){new SupplyCardVariable(cards, xFilter, ((CardSupply)home)),
            new SupplyCardVariable(cards, yFilter, ((CardSupply)home))};
    }

    public override List<CardVariable> SplitSingle() {
        return cards.Select(x => new StagedCardVariable(new List<Card>(){x}, home) as CardVariable).ToList();
    }


    public override List<CardInstance> Move(CardPosition next) {
        return new List<CardInstance>();
    }

    public override IEnumerator GetWindow(int minX, int maxX) {
        List<int> filtered = new List<int>(((CardSupply)home).GetRest());
        int sum = 0;
        for(int i = 0; i < cards.Count; i++) {
            filtered[i] *= filter[i];
            sum += filtered[i];
        }
        if(sum < minX) return null;
        return WindowManager.instance.OpenSupplyCardSelectWindow(cards, filtered, minX, maxX, ((CardSupply)home).GetDiscounts());
    }

    public override List<CardVariable> Choice(List<Card> target) {
        return new List<CardVariable>() {
            new StagedCardVariable(target, home) as CardVariable,
            new SupplyCardVariable(cards, filter, ((CardSupply)home)) as CardVariable
        };
    }

    public override List<Card> Select(int minCost, int maxCost, string type) {
        List<Card> target = new List<Card>();
        foreach(var card in cards) {
            var costs = ((CardSupply)home).GetDiscounts();
            if(card.cost - costs[cards.IndexOf(card)] < minCost) continue;
            if(card.cost - costs[cards.IndexOf(card)] > maxCost) continue;
            if(type != "" && !card.types.Contains(type)) continue;
            target.Add(card);
        }
        return target;
    }

    public override List<Card> Select(List<Card> target) {
        return target;
    }  

    public override bool CheckSkippableChoice(int a, int b) {
        var c = Get();
        if(a == 0 && b == 0) return true;
        if(c.Count <= a) return true;
        if(a == b && c.Distinct().Count() == 1) return true;
        return false;
    }

    public override List<Card> Choice(int a, int b) {
        var c = Get();
        var ret = new List<Card>();
        if(a == 0 && b == 0) {}
        else if(c.Count <= a) ret = c;
        else if(a == b && c.Distinct().Count() == 1) ret = c.Take(a).ToList();
        return ret;
    }

}
