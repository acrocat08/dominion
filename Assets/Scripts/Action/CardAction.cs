using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CardAction {

    public List<SubAction> subaction;

}

[System.Serializable]
public class SubAction {
    public string summary;
    public ActionTrigger trigger;
    public ActionTarget target;
    public ActionCond cond;
    public ActionForce force;
    public List<ActionParts> actionParts;

}


[System.Serializable]
public class ActionParts {
    public ActionType type;
    public string n1;
    public string n2;
    public string s;
}

public enum ActionTrigger {
    play,
    startPurchaseWithHand,
    startPurchaseAfterPlay,
    getTurnWithSide,
    purchaseThisCard,
    getThisCard,
    goToDiscard,
    goToWaste,
    takeThisCard,
    endGame,
    endTurnWithField,

/////
    purchase,
    get,
    discard,
    waste,
    takeCard,
    getTurn,
    startPurchasePhase,
    endTurn,
    playCoin,
    playScore,
    playNight,

}

public enum ActionTarget {
    myself,
    reactionAfterPlay,
    reactionWithHand,
    reactionWithSide,
    reactionWithSupply,

}


public enum ActionCond {
    always,
    AEqualsB,
    AIsMoreThanB,
    AIsLessThanB,
    ANotEqualsB,
    PEqualsQ,
    PNotEqualsQ,
}

public enum ActionForce {
    forced,
    optional,
    optionalWithCards,
    choice,
    follow,
    unfollow,
}

public enum ActionType {
    gainCoins,
    gainActions,
    gainPurchases,
    takeCards,
    selectAllFromHand,
    selectSomeFromDeck,
    selectAllFromField,
    selectAllFromDiscard,
    selectAllFromWaste,
    selectAllFromSupply,
    selectThisCard,
    selectCostAndGroupFromX,
    sortX,
    choiceFromX,
    addXToHand,
    addXToDeck,
    addXToField,
    addXToDiscard,
    addXToWaste,
    addXToSupply,
    getCountOfX,
    getCostOfX,
    calcAdd,
    calcSub,
    calcMulti,
    calcDiv,
    calcSurplus,
    exchangeXAndY,
    exchangeYAndZ,
    playActionInX,
    exchangeAAndB,
    exchangeBAndC,
    getActionCount,
    selectAllFromSide,
    addXToSide,
    selectCardSameOfZ,
    gainScore,
    selectAllFromDeck,
    getMaxCostOfX,
    getMinCostOfX,
    rotateP,
    resetP,
    gotoSubAction,
    getEmptySupply,
    discountCostOfX,
    gainBlock,
    selectUniqueCardsOfY,
    selectAllFromSideWithKey,
    addXToSideWithKey,
    getNowCoin,
    getNowAction,
    getNowPurchase,
    selectSubCards,
    selectRandomCard,

}

