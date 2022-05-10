using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ActionAssembly : MonoBehaviour {

    [SerializeField] List<GamePlayer> players;
    [SerializeField] List<CardHand> hands;
    [SerializeField] List<CardDeck> decks;
    [SerializeField] List<CardField> fields;
    [SerializeField] List<CardDiscard> discards;
    [SerializeField] List<CardSide> sides;

    [SerializeField] CardWaste waste;
    [SerializeField] CardSupply supply;
    [SerializeField] CardSubSupply sub;


    public static ActionAssembly instance;

    ActionVariable v;
    Stack<ActionVariable> sv;
    int fp; 

    int loop;

    Dictionary<ActionTarget, List<CardVariable>> actionHolder;


    void Start() {
        actionHolder = new Dictionary<ActionTarget, List<CardVariable>>();
        instance = this;
        v = null;
        sv = new Stack<ActionVariable>();
    }

    void Update() {
        //DebugCardVariable();
    }

    void DebugCardVariable() {
        Debug.Log(v.x.ToString() + "//" + v.y.ToString() + "//" + v.z.ToString());
        Debug.Log(v.a + "//" + v.b + "//" + v.c);
    }

    public void AddCardHolder(CardVariable cards, ActionTarget target) {
        if(!actionHolder.ContainsKey(target)) actionHolder[target] = new List<CardVariable>();
        actionHolder[target].Add(cards);
    }

    public void DeleteCardHolder(CardInstance card, ActionTarget target) {
        var c = actionHolder[target].Where(x => x.CheckSame(card)).First();
        var ls = new List<CardVariable>();
        foreach(var t in actionHolder[target]) if(!t.CheckSame(card)) ls.Add(t);
        actionHolder[target] = ls;

    }

    public void DeleteAllCardHolder(ActionTarget target) {
        actionHolder[target] = new List<CardVariable>();
    }

    public IEnumerator RunReaction(ActionTrigger trigger, CardVariable targetCards, GamePlayer targetPlayer) {
        var tmpHolder = new Dictionary<ActionTarget, List<CardVariable>>(actionHolder);

        foreach(var target in tmpHolder.Keys) {
            var holder = new List<CardVariable>(tmpHolder[target]);
            foreach(var cards in holder) {
                yield return Run(cards.Get()[0].action, cards.GetHome().GetOwner(), cards, trigger, target, targetCards, targetPlayer);
            }
        }
    }

    public IEnumerator Run(CardAction action, GamePlayer owner, CardVariable mc, ActionTrigger trigger, ActionTarget target, CardVariable targetCards, GamePlayer targetPlayer) {
        if(v != null) sv.Push(v);
        v = new ActionVariable();
        v.mc = mc;
        fp = players.IndexOf(owner);
        v.p = fp;
        loop = -1;
        v.x = targetCards;
        v.q = players.IndexOf(targetPlayer);
        
        yield return StartCoroutine(RunCoroutine(action, trigger, target));
        if(sv.Count > 0) v = sv.Pop();
    }


    IEnumerator RunCoroutine(CardAction action, ActionTrigger trigger, ActionTarget target) {
        var subaction = action.subaction;
        var choiced = new List<SubAction>();
        while(true) {
            for(int i = 0; i < action.subaction.Count; i++) {
                if(!CheckRunnable(i, subaction[i], trigger, target, loop, choiced)) {
                    yield return null;
                    continue;
                }
                if(subaction[i].force == ActionForce.optional || subaction[i].force == ActionForce.optionalWithCards) {
                    var cards = new CardVariable();
                    if(v.x != null) cards = v.x;
                    var c = players[v.p].DecideAction(subaction[i], cards.Get(), subaction[i].force == ActionForce.optionalWithCards);
                    yield return c;
                    var command = ((List<int>)c.Current)[0];
                    if(command == 0) {
                        yield return null;
                        continue;
                    }
                }
                loop = -1;
                yield return StartCoroutine(RunActionSequence(subaction[i].actionParts));

                if(subaction[i].force != ActionForce.choice) {
                    for(int j = i + 1; j < subaction.Count; j++) {
                        if(subaction[j].force == ActionForce.choice) {
                            choiced.Add(subaction[j]);
                        }
                        else break;
                    }
                    if(choiced.Count > 0) {
                        var c = players[v.p].ChoiceActions(choiced, v.a);
                        yield return c;
                        choiced = ((List<int>)c.Current).Select(x => choiced[x]).ToList();
                    }
                }

            }

            if(loop == -1) break;
            yield return null;
        }

        EndAction();
    }

    bool CheckRunnable(int index, SubAction action, ActionTrigger trigger, ActionTarget target, int loop, List<SubAction> choiced) {
        if(!CheckCond(action.cond)) return false;
        if(action.trigger != trigger) return false;
        if(action.target != target) return false;
        if(loop != -1 && loop != index) return false;
        if(action.force == ActionForce.choice && !choiced.Contains(action)) return false;
        return true;
    }

    IEnumerator RunActionSequence(List<ActionParts> actions) {
        foreach(var action in actions) {
            yield return ExcuteActionFromActionType(action.type, ReadNum(action.n1), ReadNum(action.n2), action.s);
        }
    }

    void EndAction() {
        v.x = new CardVariable();
        v.y = new CardVariable();
        v.z = new CardVariable();
        v.mc = new CardVariable();
    }

    int ReadNum(string x) {
        if(x == "a") return v.a;
        if(x == "b") return v.b;
        if(x == "c") return v.c;
        if(x == "") return 0;
        if(x == "inf") return 10000;
        return int.Parse(x);
    }

    bool CheckCond(ActionCond cond) {
        if(v.p != fp && players[v.p].HasBlock()) return false;
        if(cond == ActionCond.always) return true;
        if(cond == ActionCond.AEqualsB) return v.a == v.b;
        if(cond == ActionCond.AIsMoreThanB) return v.a >=v.b;
        if(cond == ActionCond.AIsLessThanB) return v.a <=v.b;
        if(cond == ActionCond.ANotEqualsB) return v.a !=v.b;
        if(cond == ActionCond.PEqualsQ) return v.p == v.q;
        if(cond == ActionCond.PNotEqualsQ) return v.p != v.q;
        else return true;
    }


    IEnumerator ExcuteActionFromActionType(ActionType type, int n1, int n2, string s) {
        Debug.Log("play : " + type);
        Debug.Log("param : " + v.a + " " + v.b + " " + v.c + " " + v.x + " " + v.y + " " + v.z + " " + v.p + " " + v.q + " " + v.mc);
        IEnumerator co = null;
        if(type == ActionType.gainCoins) {
            players[v.p].GainCoins(n1);
        }
        else if(type == ActionType.gainActions) {
            players[v.p].GainActions(n1);
        }
        else if(type == ActionType.gainPurchases) {
            players[v.p].GainPurchases(n1);
        }
        else if(type == ActionType.takeCards) {
            yield return StartCoroutine(ActionExcuter.instance.TakeCards(hands[v.p], decks[v.p], discards[v.p], players[v.p], n1));
        }
        else if(type == ActionType.selectAllFromHand) {
            co = ActionExcuter.instance.SelectAllFromHand(hands[v.p]);
            yield return StartCoroutine(co);
            v.x = (CardVariable)co.Current;
        }
        else if(type == ActionType.selectSomeFromDeck) {
            co = ActionExcuter.instance.SelectSomeFromDeck(decks[v.p], discards[v.p], n1);
            yield return StartCoroutine(co);
            v.x = (CardVariable)co.Current;
        }
        else if(type == ActionType.selectAllFromField) {
            co = ActionExcuter.instance.SelectAllFromField(fields[v.p]);
            yield return StartCoroutine(co);
            v.x = (CardVariable)co.Current;
        }
        else if(type == ActionType.selectAllFromDiscard) {
            co = ActionExcuter.instance.SelectAllFromDiscard(discards[v.p]);
            yield return StartCoroutine(co);
            v.x = (CardVariable)co.Current;
        }
        else if(type == ActionType.selectAllFromWaste) {
            co = ActionExcuter.instance.SelectAllFromWaste(waste);
            yield return StartCoroutine(co);
            v.x = (CardVariable)co.Current;
        }
        else if(type == ActionType.selectAllFromSupply) {
            co = ActionExcuter.instance.SelectAllFromSupply(supply);
            yield return StartCoroutine(co);
            v.x = (CardVariable)co.Current;
        }
        else if(type == ActionType.selectThisCard) {
            v.x = v.mc;
        }
        else if(type == ActionType.selectCostAndGroupFromX) {
            co = ActionExcuter.instance.SelectCostAndGroupFromX(v.x, n1, n2, s);
            yield return StartCoroutine(co);
            v.x = ((List<CardVariable>)co.Current)[0];
            v.y = ((List<CardVariable>)co.Current)[1];            
        }
        else if(type == ActionType.choiceFromX) {
            co = ActionExcuter.instance.ChoiceFromX(v.x, players[v.p], n1, n2);
            yield return StartCoroutine(co);
            v.x = ((List<CardVariable>)co.Current)[0];
            v.y = ((List<CardVariable>)co.Current)[1];
        }
        else if(type == ActionType.addXToHand) {
            var tmp = new StagedCardVariable(v.x.Get(), hands[v.p]);
            yield return StartCoroutine(ActionExcuter.instance.AddXToHand(hands[v.p], players[v.p], v.x));
            v.x = tmp;
        }
        else if(type == ActionType.addXToDeck) {
            var tmp = new StagedCardVariable(v.x.Get(), decks[v.p]);
            yield return StartCoroutine(ActionExcuter.instance.AddXToDeck(decks[v.p], v.x));
            v.x = tmp;
        }
        else if(type == ActionType.addXToField) {
            var tmp = new StagedCardVariable(v.x.Get(), fields[v.p]);
            yield return StartCoroutine(ActionExcuter.instance.AddXToField(fields[v.p], v.x));
            v.x = tmp;
        }
        else if(type == ActionType.addXToDiscard) {
            var tmp = new StagedCardVariable(v.x.Get(), discards[v.p]);
            yield return StartCoroutine(ActionExcuter.instance.AddXToDiscard(discards[v.p], v.x));
            v.x = tmp;
        }
        else if(type == ActionType.addXToWaste) {
            var tmp = new StagedCardVariable(v.x.Get(), waste);
            yield return StartCoroutine(ActionExcuter.instance.AddXToWaste(waste, v.x));
            v.x = tmp;
        }
        else if(type == ActionType.addXToSupply) {
            var tmp = new StagedCardVariable(v.x.Get(), supply);
            yield return StartCoroutine(ActionExcuter.instance.AddXToSupply(supply, v.x));
            v.x = tmp;
        }
        else if(type == ActionType.getCountOfX) {
           v.a = v.x.Get().Count;
        }
        else if(type == ActionType.getCostOfX) {
            var cost = 0;
            foreach(var card in v.x.Get()) {
                cost += card.cost;
            }
           v.a = cost;
        }
        else if(type == ActionType.calcAdd) {
           v.a = n1 + n2;
        }
        else if(type == ActionType.calcSub) {
           v.a = n1 - n2;
        }
        else if(type == ActionType.calcMulti) {
           v.a = n1 * n2;
        }
        else if(type == ActionType.calcDiv) {
           v.a = n1 / n2;
        }
        else if(type == ActionType.calcSurplus) {
           v.a = n1 % n2;
        }
        else if(type == ActionType.exchangeXAndY) {
            CardVariable tmp = v.x;
            v.x = v.y;
            v.y = tmp;
        }
        else if(type == ActionType.exchangeYAndZ) {
            CardVariable tmp = v.y;
            v.y = v.z;
            v.z = tmp;
        }
        else if(type == ActionType.playActionInX) {
            foreach(var card in v.x.SplitSingle()) {
                for(int i = 0; i < n1; i++) {
                    yield return Run(card.Get()[0].action, players[v.p], card, ActionTrigger.play, ActionTarget.myself, null, null);
                }
            }
        }
        else if(type == ActionType.exchangeAAndB) {
            int tmp = v.a;
            v.a =v.b;
            v.b = tmp;
        }
        else if(type == ActionType.exchangeBAndC) {
            int tmp =v.b;
            v.b = v.c;
            v.c = tmp;
        }
        else if(type == ActionType.getActionCount) {
           v.a = players[v.p].GetActionCount();
        }
        else if(type == ActionType.selectAllFromSide) {
            co = ActionExcuter.instance.SelectAllFromSide(sides[v.p]);
            yield return StartCoroutine(co);
            v.x = (CardVariable)co.Current;
        }
        else if(type == ActionType.addXToSide) {
            var tmp = new StagedCardVariable(v.x.Get(), sides[v.p]);
            yield return StartCoroutine(ActionExcuter.instance.AddXToSide(sides[v.p], v.x));
            v.x = tmp;
        }
        else if(type == ActionType.selectCardSameOfZ) {
            co = ActionExcuter.instance.SelectCardSameOfZ(v.x, v.z);
            yield return StartCoroutine(co);
            v.x = ((List<CardVariable>)co.Current)[0];
            v.y = ((List<CardVariable>)co.Current)[1];   
        }
        else if(type == ActionType.gainScore) {
            players[v.p].GainScore(n1);
        }
        else if(type == ActionType.selectAllFromDeck) {
            co = ActionExcuter.instance.SelectAllFromDeck(decks[v.p]);
            yield return StartCoroutine(co);
            v.x = (CardVariable)co.Current;
        }
        else if(type == ActionType.getMaxCostOfX) {
            if(v.x.Get().Count == 0) v.a = 0;
            else v.a = v.x.Get().Select(x => x.cost).Max();
        }
        else if(type == ActionType.getMinCostOfX) {
            if(v.x.Get().Count == 0) v.a = 0;
            else v.a = v.x.Get().Select(x => x.cost).Min();
        }
        else if(type == ActionType.rotateP) {
            v.p = (v.p + 1) % GameTurnManager.instance.GetPlayerNum();
        }
        else if(type == ActionType.resetP) {
            v.p = fp;
        }
        else if(type == ActionType.gotoSubAction) {
            loop = n1;
        }
        else if(type == ActionType.getEmptySupply) {
            v.a = supply.GetEmptyCard();
        }
        else if(type == ActionType.discountCostOfX) {
            supply.AddDiscount(v.x, n1);
        }
        else if(type == ActionType.gainBlock) {
            players[v.p].SetBlock(true);
        }
        else if(type == ActionType.selectUniqueCardsOfY) {
            co = ActionExcuter.instance.SelectUniqueCardsOfY(v.y);
            yield return StartCoroutine(co);
            v.x = (CardVariable)co.Current;
        }
        else if(type == ActionType.selectAllFromSideWithKey) {
            co = ActionExcuter.instance.SelectAllFromSideWithKey(sides[v.p], v.mc);
            yield return StartCoroutine(co);
            v.x = (CardVariable)co.Current;   
        }
        else if(type == ActionType.addXToSideWithKey) {
            var tmp = new StagedCardVariable(v.x.Get(), sides[v.p]);
            yield return StartCoroutine(ActionExcuter.instance.AddXToSideWithKey(sides[v.p], v.x, v.mc));
            v.x = tmp;
        }
        else if(type == ActionType.getNowCoin) {
            v.a = players[v.p].GetCoins();
        }
        else if(type == ActionType.getNowPurchase) {
            v.a = players[v.p].getNowPurchases();
        }
        else if(type == ActionType.getNowAction) {
            v.a = players[v.p].GetActions();
        }
        else if(type == ActionType.selectSubCards) {
            co = ActionExcuter.instance.SelectSubCards(sub, s);
            yield return StartCoroutine(co);
            v.x = (CardVariable)co.Current; 
        }
        else if(type == ActionType.selectRandomCard) {
            co = ActionExcuter.instance.SelectRandomCard(v.x, n1);
            yield return StartCoroutine(co);
            v.x = ((List<CardVariable>)co.Current)[0];
            v.y = ((List<CardVariable>)co.Current)[1];   
        }

        

        
    }


}

public class ActionVariable {
    public int a, b, c;
    public CardVariable x, y, z;
    public int p, q;
    public CardVariable mc;

    public ActionVariable() {
        x = new StagedCardVariable(new List<Card>(){}, null);
        y = new StagedCardVariable(new List<Card>(){}, null);
        z = new StagedCardVariable(new List<Card>(){}, null);
        mc = new StagedCardVariable(new List<Card>(){}, null);
    }
    public ActionVariable(ActionVariable target) {
        a = target.a;
        b = target.b;
        c = target.c;
        x = target.x;
        y = target.y;
        z = target.z;
        p = target.p;
        q = target.q;
        mc = target.mc;
    }
}
