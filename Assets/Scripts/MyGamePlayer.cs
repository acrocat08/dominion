using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MyGamePlayer : GamePlayer {

    int turn; //FIX
    [SerializeField] int turnLimit; //FIX
    [SerializeField] Text buttonUI;

    NetworkManager network;



    public override IEnumerator OnStartGame() {
        Debug.Log("--start" + " " + this.ToString());
        network = NetworkManager.instance;
        canAction = false;
        canPurchase = false;
        yield return ShuffleDeck();
        var c = ActionExcuter.instance.TakeCards(hand, deck, discard, this, 5);
        yield return StartCoroutine(c);
        actionNum = coinNum = purchaseNum = 0;
        SetBlock(false);
        UpdateUI();
    }

    public override IEnumerator OnGetTurn(bool isNight) {
        Debug.Log("--getTurn" + " " + this.ToString());
        this.isNight = isNight;
        canAction = true;

        if(!isNight) {
            turn++;
            transform.Find("Container").DOLocalMoveY(moveRange, 0.5f).SetEase(Ease.OutSine);
            supply.MoveSupply(true);
            yield return new WaitForSeconds(0.5f);
            actionNum = purchaseNum = 1;
            coinNum = 0;
            actionCount = 0;
            UpdateUI();
            SetBlock(false);
            buttonUI.text = "アクション終了 >";
            yield return side.PlaySustainAction();
        }
        else {
            buttonUI.text = "夜行終了 >";
        }
        supply.SetClickListener(this);
    }

    public override IEnumerator OnEndTurn() {
        Debug.Log("--endTurn" + " " + this.ToString());
        IEnumerator c = null;
        yield return field.PlayRactionInEndTurn();
        yield return ActionAssembly.instance.RunReaction(ActionTrigger.endTurn, null, this);

        c = ActionExcuter.instance.SelectAllFromHand(hand);
        yield return StartCoroutine(c);
        c = ActionExcuter.instance.AddXToField(field, (CardVariable)c.Current);
        yield return StartCoroutine(c);
        c = ActionExcuter.instance.SelectAllFromField(field);
        yield return StartCoroutine(c);
        c = ActionExcuter.instance.AddXToDiscard(discard, (CardVariable)c.Current);
        yield return StartCoroutine(c);
        c = ActionExcuter.instance.TakeCards(hand, deck, discard, this, 5);
        yield return StartCoroutine(c);
        ActionAssembly.instance.DeleteAllCardHolder(ActionTarget.reactionAfterPlay);
        canPurchase = false;
        actionNum = coinNum = purchaseNum = 0;
        actionCount = 0;
        transform.Find("Container").DOLocalMoveY(0, 0.5f).SetEase(Ease.OutSine);
        yield return new WaitForSeconds(0.5f);
        field.ResetField();
    }

    public override IEnumerator OnPurchasePhase() {
        Debug.Log("--purchasePhase" + " " + this.ToString());
        canAction = false;
        canPurchase = true;
        yield return hand.StartPurchaseWithHand(field);
        buttonUI.text = "購入終了 >";
    }


    public override void OnClickedCard(CardInstance card) {
        if(InputLocker.instance.isLocked()) return;
        if(card.state == CardState.hand) {
            StartCoroutine(PlayCardCoroutine(card));
        }
        if(card.state == CardState.supply) {
            StartCoroutine(PurchaseCoroutine(card));
        }
    }

    IEnumerator PlayCardCoroutine(CardInstance card) {
        if(!CheckPlayable(card.status)) yield break;
        if(canAction && !isNight && actionNum <= 0) yield break;
        Debug.Log("--onPlay" + " " + this.ToString());
        network.SendId(NetworkMessageType.playCard, new List<int>(){hand.GetIndex(card)});
        if(canAction && !isNight) {
            GainActions(-1);
            actionCount++;
        }
        var x = hand.GetSameOne(card);
        var c = ActionExcuter.instance.AddXToField(field, x);
        yield return StartCoroutine(c);
        // Card Action...
        if(canAction && !isNight) {
            yield return ActionAssembly.instance.RunReaction(ActionTrigger.play, new TargetCardVariable(new List<CardInstance>(){card}, field), this);
            yield return ActionAssembly.instance.Run(card.status.action, this, 
                new TargetCardVariable(new List<CardInstance>(){card}, field), ActionTrigger.play, ActionTarget.myself, null, null);
            ActionAssembly.instance.AddCardHolder(new TargetCardVariable(new List<CardInstance>(){card}, field), ActionTarget.reactionAfterPlay);
        }
        else if(canAction && isNight) {
            yield return ActionAssembly.instance.RunReaction(ActionTrigger.playNight, new TargetCardVariable(new List<CardInstance>(){card}, field), this);
            yield return ActionAssembly.instance.Run(card.status.action, this, 
            new TargetCardVariable(new List<CardInstance>(){card}, field), ActionTrigger.playNight, ActionTarget.myself, null, null);
            //ActionAssembly.instance.AddCardHolder(new TargetCardVariable(new List<CardInstance>(){card}, field), ActionTarget.reactionAfterPlay);
        }
        else {
            yield return ActionAssembly.instance.RunReaction(ActionTrigger.playCoin, new TargetCardVariable(new List<CardInstance>(){card}, field), this);
            yield return ActionAssembly.instance.Run(card.status.action, this, 
            new TargetCardVariable(new List<CardInstance>(){card}, field), ActionTrigger.playCoin, ActionTarget.myself, null, null);
            //ActionAssembly.instance.AddCardHolder(new TargetCardVariable(new List<CardInstance>(){card}, field), ActionTarget.reactionAfterPlay);
        }

    }

    IEnumerator PurchaseCoroutine(CardInstance card) {
        if(!canPurchase || purchaseNum <= 0) yield break;
        if(coinNum < card.GetCost()) yield break;
        if(!supply.CheckBuyable(card.status)) yield break;
        Debug.Log("--onPurchase" + " " + this.ToString());
        network.SendId(NetworkMessageType.purchaseCard, new List<int>(){supply.GetIndex(card)});
        GainPurchases(-1);
        GainCoins(-card.GetCost());
        SoundPlayer.instance.PlaySoundEffect(SoundEffectType.se_purchase, 0);
        var x = supply.GetSameOne(card.status);
        var c = ActionExcuter.instance.AddXToDiscard(discard, x);
        yield return StartCoroutine(c);
        yield return ActionAssembly.instance.Run(card.status.action, this, 
            new StagedCardVariable(new List<Card>{card.status}, discard), 
            ActionTrigger.purchaseThisCard, ActionTarget.myself, null, null);
        yield return ActionAssembly.instance.RunReaction(ActionTrigger.purchase, 
            new StagedCardVariable(new List<Card>{card.status}, discard), this);
    }

    public override IEnumerator OnGameEnd() {
        Debug.Log("--gameEnd" + " " + this.ToString());
        transform.Find("Container").DOLocalMoveY(moveRange, 0.5f).SetEase(Ease.OutSine);
        supply.MoveSupply(true);
        yield return new WaitForSeconds(0.5f);
        yield return deck.Add(discard.Get());
        yield return deck.Add(field.Get());
        yield return deck.Add(hand.Get());
        yield return deck.Add(side.GetAll());
        yield return deck.OnGameEnd(field);
        transform.Find("Container").DOLocalMoveY(0, 0.5f).SetEase(Ease.OutSine);
        yield return new WaitForSeconds(0.5f);
    }
    
    bool CheckPlayable(Card card) {
        if(canAction && !isNight && card.types.Contains("action")) return true;
        if(canAction && isNight && card.types.Contains("night")) return true;
        if(canPurchase && card.types.Contains("coin")) return true;
        return false;
    }


    protected override void UpdateUI() {
        statusUI.text = string.Format("プレイヤー{0} / アクション×{1}  コイン×{2}  購入×{3}  得点×{4}", id + 1, actionNum, coinNum, purchaseNum, score);
    }

    public override IEnumerator ShuffleDeck() {
        deck.Shuffle();
        network.SendCards(NetworkMessageType.suffleCard, deck.Get().Get());
        yield return null;
    }

    public override IEnumerator ChoiceCards(CardVariable x, int a, int b) {
        if(x.CheckSkippableChoice(a, b)) {
            var choiced = x.Choice(a, b);
            network.SendCards(NetworkMessageType.choiceCard, choiced);
            Debug.Log("choice : " + choiced);
            yield return x.Choice(choiced);
        }
        else {
            IEnumerator c = x.GetWindow(a, b);
            InputLocker.instance.Lock();
            yield return StartCoroutine(c);
            InputLocker.instance.UnLock();
            network.SendCards(NetworkMessageType.choiceCard, (List<Card>)c.Current);
            var choiced = x.Choice((List<Card>)c.Current);
            Debug.Log("choice : " + choiced[0]);
            yield return choiced;
        }
    }

    public override IEnumerator ChoiceActions(List<SubAction> actions, int num) {
        var c = WindowManager.instance.OpenChoiceWindow(actions, num);
        InputLocker.instance.Lock();
        yield return c;
        InputLocker.instance.UnLock();
        network.SendId(NetworkMessageType.choiceActions, (List<int>)c.Current);
        var choiced = (List<int>)c.Current;
        Debug.Log("choice : " + string.Join(";", choiced));
        yield return choiced;
    }

    public override IEnumerator DecideAction(SubAction action, List<Card> cards, bool showCards) {
        var c = WindowManager.instance.OpenDecideActionWindow(action, cards, showCards);
        InputLocker.instance.Lock();
        yield return c;
        InputLocker.instance.UnLock();
        network.SendId(NetworkMessageType.decideAction, (List<int>)c.Current);
        var choiced = (List<int>)c.Current;
        Debug.Log("choice : " + string.Join(";", choiced));
        yield return choiced;
    }

}