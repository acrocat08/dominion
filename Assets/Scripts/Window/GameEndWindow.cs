using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEndWindow : MonoBehaviour {

    bool isClosed;

    [SerializeField] Text text; 

    void Start() {
        isClosed = false;
    }

    public IEnumerator Open(Dictionary<int, int> scores) {
        transform.SetParent(GameObject.Find("Canvas").transform);
        //transform.position = Vector3.zero;
        var txt = "";
        for(int i = 0; i < scores.Count; i++) {
            txt += string.Format("プレイヤー{0}：{1}", i + 1, scores[i]);
            if(i == 0 || i == 2) txt += "    ";
            if(i == 1) txt += "\n";
        }
        text.text = txt;
        SoundPlayer.instance.StopBackGroundMusic();
        SoundPlayer.instance.PlaySoundEffect(SoundEffectType.se_gameend, 0);


        while(!isClosed){
            yield return null;
        }

    }

}
