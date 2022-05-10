using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CardPackage : ScriptableObject {

    public string package_name;
    public List<Card> baseCards;
    public List<Card> cards;
    public List<Card> subCards;
    public List<SubCardPackage> subPackages;

}

[System.Serializable]
public class SubCardPackage {
    public string name;
    public List<Card> cards;
}

