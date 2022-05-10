using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DecideActionWindow : MonoBehaviour {


    [SerializeField] GameObject cardWindow;
    [SerializeField] Transform cardView;
    [SerializeField] Text summary;
    bool isClosed;
    int command;

    public IEnumerator Open(SubAction action, List<Card> cards, bool showCards) {
        transform.SetParent(GameObject.Find("Canvas").transform);
        isClosed = false;

        if(!showCards) {
            cardWindow.SetActive(false);
        }
        else {
            foreach(Card card in cards) {
                CardInstance instance = CardDrawer.instance.Draw(card, CardState.select);
                instance.transform.localScale = Vector3.one * 0.75f;
                instance.transform.SetParent(cardView);
            }
        }
        summary.text = action.summary;

        while(!isClosed){
            yield return null;
        }

        yield return new List<int>(){command};
    }

    public void OnClickedButton(int command) {
        SoundPlayer.instance.PlaySoundEffect(SoundEffectType.se_ok, 0);
        this.command = command;
        isClosed = true;
        WindowManager.instance.CloseWindow(gameObject);
    }




}
