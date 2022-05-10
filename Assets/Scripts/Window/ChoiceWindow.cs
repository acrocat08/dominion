using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceWindow : MonoBehaviour {

    [SerializeField] GameObject bar;
    [SerializeField] Transform container;
    [SerializeField] Text title;


    bool isClosed;
    List<int> choosed;
    int rest;
    List<GameObject> bars;


    public IEnumerator Open(List<SubAction> actions, int num) {
        transform.SetParent(GameObject.Find("Canvas").transform);
        isClosed = false;
        choosed = new List<int>();
        rest = num;
        bars = new List<GameObject>();
        title.text = string.Format("アクションを{0}つ選択", num);
        for(int i = 0; i < actions.Count; i++) {
            var obj = Instantiate(bar);
            obj.transform.SetParent(container);
            obj.transform.Find("Title").GetComponent<Text>().text = actions[i].summary;
            var index = i;
            obj.transform.Find("OK").GetComponent<Button>().onClick.AddListener(() => Choose(index));
            bars.Add(obj);
        }

        while(!isClosed) {
            yield return null;
        }

        yield return choosed;
    }

    void Choose(int index) {
        SoundPlayer.instance.PlaySoundEffect(SoundEffectType.se_ok, 0);
        Destroy(bars[index]);
        rest--;
        choosed.Add(index);
        if(rest == 0) Close();
    }

    public void Close() {
        WindowManager.instance.CloseWindow(gameObject);
        isClosed = true;
    }

}
