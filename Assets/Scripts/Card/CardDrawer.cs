using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDrawer : MonoBehaviour {

    [SerializeField] GameObject framework;
    [SerializeField] List<Sprite> cardFrames;
    [SerializeField] List<Sprite> miniCardFrames;
    [SerializeField] List<Color> textColors;
    [SerializeField] Card dummy;

    Vector2 maxImageSize;

    public static CardDrawer instance;
    void Awake() {
        instance = this;
        maxImageSize = framework.transform.Find("Card").transform.Find("Image")
            .GetComponent<RectTransform>().sizeDelta;

    }

    public CardInstance Draw(Card card, CardState state) {
        if(card == null) card = dummy;
        GameObject obj = GameObject.Instantiate(framework);
        Transform child = obj.transform.Find("Card").transform;
        Text name = child.transform.Find("Name").GetComponent<Text>();
        Text description = child.transform.Find("Description").GetComponent<Text>();
        Text type = child.transform.Find("Type").GetComponent<Text>();
        Text cost = child.transform.Find("Cost").GetComponent<Text>();
        Image image = child.transform.Find("Image").GetComponent<Image>();
        name.text = card.card_name;
        name.color = textColors[GetIndexCardFrame(card.types)];
        description.text = card.description;
        description.color = textColors[GetIndexCardFrame(card.types)];
        type.text = GetTypeText(card.types);
        type.color = textColors[GetIndexCardFrame(card.types)];
        cost.text = card.cost.ToString();
        image.sprite = card.image;
        ResizeImage(image);
        child.GetComponent<Image>().sprite = cardFrames[GetIndexCardFrame(card.types)];
        child.Find("Mini").GetComponent<Image>().sprite = miniCardFrames[GetIndexCardFrame(card.types)];
        obj.transform.SetParent(GameObject.Find("Canvas").transform);
        CardInstance instance = obj.GetComponent<CardInstance>();
        instance.status = card;
        instance.SetState(state);
        return instance;
    }

    string GetTypeText(List<string> types) {
        string ret = "";
        for(int i = 0; i < types.Count; i++) {
            if(types[i].Length > 0 && types[i][0] == '$') {
                continue;
            }
            if(i > 0) ret += "・";
            if(types[i].Length > 0 && types[i][0] == '#') {
                ret += types[i].Substring(1);
            }
            switch(types[i]) {
                case "action": ret += "アクション"; break;
                case "coin": ret += "コイン"; break;
                case "score": ret += "得点"; break;
                case "attack": ret += "アタック"; break;
                case "reaction": ret += "リアクション"; break;
                case "penalty": ret += "呪い"; break;
                case "sustain": ret += "持続"; break;
                case "reserve": ret += "リザーブ"; break;
                case "night": ret += "夜行"; break;
            }

        }

        ret = "＜ " + ret + " ＞";
        return ret;
    }

    public int GetIndexCardFrame(List<string> types) {
        int ret = 0;
        var ls = new List<string>() {"action", "reaction", "reserve", "sustain", "attack", "night", "penalty", "score", "coin"};
        for(int i = 0; i < ls.Count; i++) {
            if(types.Contains(ls[i])) ret = i;
        }
        return ret;
    }

    public void ResizeImage(Image image) {
        var spriteSize = image.sprite.bounds.size;

        float per = 0;
        per = maxImageSize.x / spriteSize.x;
        per = Mathf.Min(per, maxImageSize.y / spriteSize.y);
        
        var rect = image.GetComponent<RectTransform>();
        rect.sizeDelta = spriteSize * per;
    }

}
