using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowManager : MonoBehaviour {

    [SerializeField] GameObject listedCardSelectWindow;
    [SerializeField] GameObject supplyCardSelectWindow;
    [SerializeField] GameObject cardView;
    [SerializeField] GameObject gameEndWindow;
    [SerializeField] GameObject textWindow;
    [SerializeField] GameObject choiceWindow;
    [SerializeField] GameObject decideActionWindow;
    [SerializeField] GameObject dictionaryWindow;

    public static WindowManager instance;    
    Transform canvas;

    bool isOpened;
    GameObject cardViewObj;
    GameObject textWindowObj;

    void Awake() {
        instance = this;
        isOpened = false;
        canvas = GameObject.Find("Canvas").transform;
    }

    public IEnumerator OpenListedCardSelectWindow(List<Card> cards, int minX, int maxX) {
        if(isOpened) yield break;
        isOpened = true;　
        var window = Instantiate(listedCardSelectWindow, canvas).GetComponent<ListedCardSelectWindow>();

        var c = window.Open(cards, minX, maxX);
        yield return StartCoroutine(c);

        yield return c.Current;
    }

    public IEnumerator OpenSupplyCardSelectWindow(List<Card> cards, List<int> rest, int minX, int maxX, List<int> discounts) {
        if(isOpened) yield break;
        isOpened = true;
        var window = Instantiate(supplyCardSelectWindow, canvas).GetComponent<SupplyCardSelectWindow>();

        var c = window.Open(cards, rest, minX, maxX, discounts);
        yield return StartCoroutine(c);

        yield return c.Current;

    }

    public void OpenTextWindow(string text) {
        var window = Instantiate(textWindow, canvas);
        textWindowObj = window;
        window.GetComponent<TextWindow>().Open(text);        
    }
    public void OpenDictionaryWindow(CardPackage package) {
        if(isOpened) return;
        isOpened = true;　
        var window = Instantiate(dictionaryWindow, canvas);
        window.GetComponent<DictionaryWindow>().Open(package);        
    }

    public void OpenCardView(Card card) {
        var window = Instantiate(cardView, canvas);
        cardViewObj = window;
        window.GetComponent<CardView>().Open(card);
    }

    public IEnumerator OpenChoiceWindow(List<SubAction> actions, int num) {
        if(isOpened) yield break;
        isOpened = true;　
        var window = Instantiate(choiceWindow, canvas).GetComponent<ChoiceWindow>();

        var c = window.Open(actions, num);
        yield return StartCoroutine(c);

        yield return c.Current; 
    }

    public IEnumerator OpenDecideActionWindow(SubAction action, List<Card> cards, bool showCards) {
        if(isOpened) yield break;
        isOpened = true;　
        var window = Instantiate(decideActionWindow, canvas).GetComponent<DecideActionWindow>();

        var c = window.Open(action, cards, showCards);
        yield return StartCoroutine(c);

        yield return c.Current; 
    }

    public IEnumerator OpenGameEndWindow(Dictionary<int, int> scores) {
        if(isOpened) yield break;
        isOpened = true;
        var window = Instantiate(gameEndWindow, canvas).GetComponent<GameEndWindow>();
        var c = window.Open(scores);
        yield return StartCoroutine(c);
    }

    public void CloseWindow(GameObject window) {
        Destroy(window);
        isOpened = false;
    }

    public void CloseCardView() {
        Destroy(cardViewObj);
        cardViewObj = null;
    }

    public void CloseTextWindow() {
        textWindowObj.GetComponent<TextWindow>().Close();
        Destroy(textWindowObj);
    }
    
    public bool CheckOpenedCardView() {
        return cardViewObj != null;
    }
}
