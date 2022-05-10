using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GamePlayer : MonoBehaviour, CardClickListener {

    [SerializeField] protected CardDeck deck;
    [SerializeField] protected CardHand hand;
    [SerializeField] protected CardField field;
    [SerializeField] protected CardDiscard discard;
    [SerializeField] protected CardSupply supply;
    [SerializeField] protected CardSide side;

    protected int actionNum;
    protected int coinNum;
    protected int purchaseNum;
    protected int score;
    protected int actionCount;
    protected bool canAction, canPurchase;
    [SerializeField] protected float moveRange;

    protected int id;
    [SerializeField] protected Text statusUI;

    bool hasBlock;
    protected bool isNight;


    public virtual IEnumerator OnStartGame() {
        yield return null;
    }
    public virtual IEnumerator OnGetTurn(bool isNight) {
        yield return null;
    }

    public virtual IEnumerator OnPurchasePhase() {
        yield return null;
    }

    public virtual IEnumerator OnEndTurn() {
        yield return null;
    }

    public virtual IEnumerator OnGameEnd() {
        yield return null;
    }

    public virtual void OnClickedCard(CardInstance card) {
    }




    
    public void GainActions(int a) {
        actionNum += a;
        UpdateUI();
    }

    public int GetActions() {
        return actionNum;
    }

    public void GainCoins(int a) {
        coinNum += a;
        UpdateUI();
    }

    public int GetCoins() {
        return coinNum;
    }

    public void GainPurchases(int a) {
        purchaseNum += a;
        UpdateUI();
    }  

    public int getNowPurchases() {
        return purchaseNum;
    }

    public void GainScore(int a) {
        score += a;
        UpdateUI();
    }

    public int GetScore() {
        return score;
    }

    protected virtual void UpdateUI() {

    }

    public int GetActionCount() {
        return actionCount;
    }

    public virtual IEnumerator ShuffleDeck() {
        yield return null;
    }


    public void SetId(int id) {
        this.id = id;
    }

    public virtual IEnumerator ChoiceCards(CardVariable x, int a, int b) {
        yield return null;
    }

    public virtual IEnumerator ChoiceActions(List<SubAction> actions, int num) {
        yield return null;
    }
    public virtual IEnumerator DecideAction(SubAction action, List<Card> cards, bool showCards) {
        yield return null;
    }

    public void SetBlock(bool isActive) {
        hasBlock = isActive;
    }

    public bool HasBlock() {
        return hasBlock;
    }

    public bool CanPlayNight() {
        return hand.Get().Get().Where(x => x.types.Contains("night")).Count() > 0;
    }

}
