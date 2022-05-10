using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class NetworkManager : MonoBehaviourPunCallbacks {

    public static NetworkManager instance;

    bool isConnected;

    int myPlayerId;
    List<Queue<NetworkMessage>> queues;

    [SerializeField] CardPackage cardPackage;
    [SerializeField] PhotonView view;

    [SerializeField] int maxPlayerNum;


    void Start() {
        instance = this;
    }


    
    public IEnumerator ConnectNetwork() {
        if(isConnected) yield break;
        Debug.Log("connecting...");
        PhotonNetwork.ConnectUsingSettings();
        while(!isConnected) {
            Debug.Log("wait for other player...");
            yield return null;
        }
        Debug.Log("connected");
    }

    public override void OnConnectedToMaster() {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        var roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)maxPlayerNum;

        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public override void OnJoinedRoom() {
        myPlayerId = PhotonNetwork.CurrentRoom.PlayerCount - 1;
        StartCoroutine(WaitForOtherPlayer());
    }

    IEnumerator WaitForOtherPlayer() {
        while(PhotonNetwork.CurrentRoom.PlayerCount < maxPlayerNum) {
            yield return null;
        }
        PhotonNetwork.CurrentRoom.IsOpen = false;
        Setup();
    }

    void Setup() {
        isConnected = true;
        queues = new List<Queue<NetworkMessage>>();
        foreach(var player in PhotonNetwork.PlayerList) {
            queues.Add(new Queue<NetworkMessage>());
        }
    }

    public bool CheckConnected() {
        return isConnected;
    }

    public int GetMyId() {
        return myPlayerId;
    }

    public int GetPlayerNum() {
        return queues.Count;
    }


    public void SendCards(NetworkMessageType type, List<Card> cards) {
        if(!isConnected) return;
        var package = new List<Card>(cardPackage.baseCards);
        package.AddRange(cardPackage.cards);
        package.AddRange(cardPackage.subCards);
        view.RPC(nameof(ReceiveCards), RpcTarget.Others, new NetworkMessage(type, cards, package).ToCode(), myPlayerId);
    }

    [PunRPC]
    void ReceiveCards(string code, int senderId) {
        var message = new NetworkMessage(code);
        queues[senderId].Enqueue(message);
    }
    public void SendId(NetworkMessageType type, List<int> id) {
        if(!isConnected) return;
        view.RPC(nameof(ReceiveId), RpcTarget.Others, new NetworkMessage(type, id).ToCode(), myPlayerId);
    }

    [PunRPC]
    void ReceiveId(string code, int senderId) {
        var message = new NetworkMessage(code);
        queues[senderId].Enqueue(message);
    }

    public IEnumerator Wait(List<NetworkMessageType> types, int id) {
        if(!isConnected) yield break;
        InputLocker.instance.Lock();
        NetworkMessage ret = null;
        while(true) {
            while(queues[id].Count == 0) {
                yield return null;
            }
            var message = queues[id].Dequeue();
            if(types.Contains(message.type)) {
                ret = message;
                break;
            };
            yield return null;
        }
        InputLocker.instance.UnLock();
        yield return ret;
    }

    public IEnumerator Wait(NetworkMessageType type, int id) {
        var c = Wait(new List<NetworkMessageType>(){type}, id);
        yield return c;
        yield return c.Current;
    }

    public List<Card> UnzipCard(object obj) {
        var package = new List<Card>(cardPackage.baseCards);
        package.AddRange(cardPackage.cards);
        return ((NetworkMessage)obj).GetCards(package);
    }


}

public enum NetworkMessageType {

    playCard, // int
    purchaseCard, // int
    choiceCard, // List<Card>
    suffleCard, //List<Card>
    prepareSupply, //List<Card>
    choiceActions,
    decideAction,
    nextPhase, 
    sendScore,

}

public class NetworkMessage {
    public NetworkMessageType type;
    List<int> message;

    public NetworkMessage(NetworkMessageType type, List<Card> cards, List<Card> package) {
        this.type = type;
        message = new List<int>();
        foreach(var card in cards) {
            message.Add(package.IndexOf(card));
        }
    }

    public NetworkMessage(NetworkMessageType type, List<int> id) {
        this.type = type;
        message = new List<int>();
        foreach(var i in id) {
            message.Add(i);
        }
    }

    public NetworkMessage(string code) {
        var tmp = code.Split(new char[]{':'});
        this.type = (NetworkMessageType)(int.Parse(tmp[0]));
        message = new List<int>();
        foreach(var x in tmp[1].Split(new char[]{','})) {
            if(x == "") continue;
            message.Add(int.Parse(x));
        }
    }

    public string ToCode() {
        return ((int)type).ToString() + ":" + string.Join(",", message);
    }



    public List<Card> GetCards(List<Card> package) {
        List<Card> ret = new List<Card>();
        foreach(var x in message) {
            ret.Add(package[x]);
        }
        return ret;
    }

    public List<int> GetId() {
        return message;
    }

}