using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
[System.Serializable]
public class Card : ScriptableObject {

    public string card_name;
    public Sprite image;
    public List<string> types;
    [TextArea]
    public string description;
    public int cost;
    public List<Card> subCards;

    public CardAction action;

    public bool HasActionTrigger(ActionTrigger trigger) {
        foreach(var subaction in action.subaction) {
            if(subaction.trigger == trigger) return true;
        }
        return false;
    }

}
