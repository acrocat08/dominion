using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTurnManager : MonoBehaviour {

    public static GameTurnManager instance;

    [SerializeField] List<GamePlayer> players;
    [SerializeField] CardSupply supply;

    GameTurn turn;

    int turnCount;
    [SerializeField] bool isOnline;

    GamePlayer nowPlayer;

    NetworkManager network;
    [SerializeField] GameObject button;
    [SerializeField] GameObject dictButton;

    int playerNum;




    void Start() {
        instance = this;
        turn = GameTurn.prepare;
        turnCount = 0;
        StartCoroutine(WaitForPlayers());
    }

    IEnumerator WaitForPlayers() {
        yield return null;
        network = NetworkManager.instance;
        if(isOnline) {
            playerNum = players.Count;
            WindowManager.instance.OpenTextWindow("他のプレイヤーを待機中");
            yield return network.ConnectNetwork();
            WindowManager.instance.CloseTextWindow();
        }
        else playerNum = 1;
        if(GetNowPlayerId() == network.GetMyId() || !network.CheckConnected()) {
            var supplyCards = supply.ChoiceCards();
            supply.Setup(supplyCards);
            network.SendCards(NetworkMessageType.prepareSupply, supplyCards);
            turn = GameTurn.start;
            button.SetActive(true);
        }
        else {
            var c = network.Wait(NetworkMessageType.prepareSupply, GetNowPlayerId());
            yield return c;
            supply.Setup(network.UnzipCard(c.Current));
            turn = GameTurn.wait;
            yield return network.Wait(NetworkMessageType.nextPhase, GetNowPlayerId());
        }
        while(turn == GameTurn.start) yield return null;
        dictButton.SetActive(false);
        yield return StartGame();
    }

    public void OnButtonClicked() {
        if(InputLocker.instance.isLocked()) return;
        if(turn == GameTurn.wait) return;
        if(turn == GameTurn.start) {
            turn = GameTurn.action;
        }
        else if(turn == GameTurn.action) {
            turn = GameTurn.purchase;
        }
        else if(turn == GameTurn.purchase && nowPlayer.CanPlayNight()) {
            turn = GameTurn.night;

        }
        else if(turn == GameTurn.night || turn == GameTurn.purchase && !nowPlayer.CanPlayNight()) {
            turn = GameTurn.wait;
            button.SetActive(false);
        }

        network.SendId(NetworkMessageType.nextPhase, new List<int>(){0});
        SoundPlayer.instance.PlaySoundEffect(SoundEffectType.se_ok, 0);

    }

    IEnumerator StartGame() {
        for(int i = 0; i < playerNum; i++) {
            players[GetPlayerIdIndex(GetNowPlayerId() + i)].SetId(GetNowPlayerId() + i);
            yield return StartCoroutine(players[GetPlayerIdIndex(GetNowPlayerId() + i)].OnStartGame());
        }
        nowPlayer = players[GetPlayerIdIndex(GetNowPlayerId())];
        SoundPlayer.instance.PlayBackGroundMusic(BackGroundMusicType.bgm_main);
        yield return StartTurn();
    }

    IEnumerator StartTurn() {
        yield return nowPlayer.OnGetTurn(false);
        while(turn == GameTurn.action) yield return null;
        yield return GoToPurchasePhase();
    }

    IEnumerator GoToPurchasePhase() {
        yield return nowPlayer.OnPurchasePhase();
        while(turn == GameTurn.purchase) yield return null;
        if(turn == GameTurn.night) {
            yield return NightTurn();
        }
        else {
            yield return ChangePlayer();
        }
    }

    IEnumerator NightTurn() {
        yield return nowPlayer.OnGetTurn(true);
        while(turn == GameTurn.night) yield return null;
        yield return ChangePlayer();
    }

    IEnumerator ChangePlayer() {
        yield return nowPlayer.OnEndTurn();
        supply.ResetDiscount();
        turnCount++;
        if(supply.CheckGameEnd()) {
            yield return GameEnd();
            yield break;
        }
        nowPlayer = players[GetPlayerIdIndex(GetNowPlayerId())];
        if(GetNowPlayerId() == network.GetMyId()) {
            turn = GameTurn.action;
            button.SetActive(true);
        }
        else {
            turn = GameTurn.wait;
        }
        yield return StartTurn();
    }

    IEnumerator GameEnd() {
        yield return new WaitForSeconds(0.5f);
        for(int i = 0; i < playerNum; i++) {
            yield return StartCoroutine(players[GetPlayerIdIndex(GetNowPlayerId() + i)].OnGameEnd());
        }
        var scores = new Dictionary<int, int>();
        scores[network.GetMyId()] = players[0].GetScore();
        network.SendId(NetworkMessageType.sendScore, new List<int>(){players[0].GetScore()});
        for(int i = 0; i < playerNum; i++) {
            if(i == network.GetMyId()) continue;
            var c = network.Wait(NetworkMessageType.sendScore, i);
            yield return c;
            scores[i] = ((NetworkMessage)c.Current).GetId()[0];
        }
        InputLocker.instance.Lock();
        yield return StartCoroutine(WindowManager.instance.OpenGameEndWindow(scores));
        InputLocker.instance.Lock();
    }

    public int GetNowPlayerId() {
        return turnCount % playerNum;
    }

    int GetPlayerIdIndex(int id) {
        return (id - network.GetMyId() + playerNum) % playerNum; 
    }

    public int GetPlayerNum() {
        return playerNum;
    }

    public void OpenCardDict() {
        WindowManager.instance.OpenDictionaryWindow(supply.GetPackage());
    }


}


public enum GameTurn {
    prepare,
    start,
    action,
    purchase,
    night,
    wait,
    end,
}
