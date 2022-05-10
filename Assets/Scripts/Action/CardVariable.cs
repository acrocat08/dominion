using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardVariable : MonoBehaviour {

    protected CardPosition home;

    public virtual List<Card> Get() {
        return new List<Card>();
    }

    public virtual List<CardVariable> Split(List<Card> cards) {
        return new List<CardVariable>();
    }

    public virtual List<CardVariable> SplitSingle() {
        return new List<CardVariable>();
    }


    public virtual List<CardInstance> Move(CardPosition next) {
        return new List<CardInstance>();
    }

    public virtual IEnumerator GetWindow(int minX, int mixX) {
        yield return null;
    }

    public virtual List<CardVariable> Choice(List<Card> target) {
        return new List<CardVariable>();
    }

    public virtual List<Card> Select(int minCost, int maxCost, string type) {
        return new List<Card>();
    }

    public virtual List<Card> Select(List<Card> target) {
        return new List<Card>();
    }

    public virtual List<Card> Distinct() {
        return new List<Card>();
    }


    public override string ToString() {
        return "";
    }

    public CardPosition GetHome() {
        return home;
    }

    public virtual bool CheckSame(CardInstance card) {
        return false;
    }

    public virtual bool CheckSkippableChoice(int a, int b) {
        return false;
    }

    public virtual List<Card> Choice(int a, int b) {
        return new List<Card>();
    }
    
}
