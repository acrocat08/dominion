using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class DictionaryWindow : MonoBehaviour {

    [SerializeField] Dropdown cardType;
    [SerializeField] Dropdown cost;
    [SerializeField] Transform cardView;

    [SerializeField] List<string> type_name;

    CardPackage package;

    int nowType, nowCost;

    List<GameObject> nowCards;

    public void Open(CardPackage package) {
        nowCards = new List<GameObject>();
        this.package = package;
        nowCost = -1;
        nowType = -1;
        DrawCard();
    }

    void DrawCard() {
        var tmp = new List<GameObject>(nowCards);
        foreach(var obj in tmp) {
            Destroy(obj);
        }
        nowCards = new List<GameObject>();
        var target = package.baseCards
            .Concat(package.cards)
            .Where(x => nowCost == -1 || x.cost == nowCost || nowCost == 8 && x.cost >= nowCost)
            .Where(x => nowType == -1 || x.types.Contains(type_name[nowType]))
            .OrderBy(x => x.cost)
            .ThenBy(x => CardDrawer.instance.GetIndexCardFrame(x.types))
            .ThenBy(x => x.card_name)
            .ToList();
        foreach(var card in target) {
            CardInstance instance = CardDrawer.instance.Draw(card, CardState.select);
            instance.transform.localScale = Vector3.one * 0.75f;
            instance.transform.SetParent(cardView);
            nowCards.Add(instance.gameObject);
        } 
    }

    public void SetNowType(int val) {
        nowType = val - 1;
        DrawCard();
    }
    public void SetNowCost(int val) {
        nowCost = val - 1;
        DrawCard();
    }


    public void Close() {
        WindowManager.instance.CloseWindow(gameObject);
    }

}
