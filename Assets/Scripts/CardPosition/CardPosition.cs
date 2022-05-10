using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPosition: MonoBehaviour {

    [SerializeField] protected GamePlayer owner;

    public virtual IEnumerator Add(CardVariable target) {
        yield return null;
    }

    public virtual CardVariable Get() {
        return new CardVariable();
    }

    public virtual CardVariable GetSameOne(CardInstance target) {
        return new CardVariable();
    }

    public virtual void RemoveInstance(List<CardInstance> target) {

    }

    public virtual List<CardInstance> Remove(CardVariable target) {
        return new List<CardInstance>();
    }

    protected IEnumerator PlayReactionWithInstance(List<CardInstance> target, ActionTrigger trigger, GamePlayer player) {
        if(player == null) yield break;
        var cardCopy = new List<CardInstance>(target);
        foreach(var card in cardCopy) {
            yield return ActionAssembly.instance.Run(card.status.action, player, 
                new TargetCardVariable(new List<CardInstance>{card}, this), trigger, ActionTarget.myself, null, null);
        }
    }

    protected IEnumerator PlayReaction(List<Card> target, ActionTrigger trigger, GamePlayer player) {
        if(player == null) yield break;
        var cardCopy = new List<Card>(target);
        foreach(var card in cardCopy) {
            yield return ActionAssembly.instance.Run(card.action, player, 
                new StagedCardVariable(new List<Card>(){card}, this), trigger, ActionTarget.myself, null, null);
        }
    }

    public GamePlayer GetOwner() {
        return owner;
    }
    
}
