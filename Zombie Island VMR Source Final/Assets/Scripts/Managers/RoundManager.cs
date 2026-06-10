using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RoundManager : MonoBehaviour
{
    /*urls para os updates*/
    private const string UpdateRoundUrl = "http://localhost:6969/updateRound";
    private const string GetRoundUrl = "http://localhost:6969/getRound";
    private const string UpdateKillsUrl = "http://localhost:6969/updateKills";

    private int _playerId;

    public void SetPlayerId(int playerId)
    {
        _playerId = playerId;
    }

    public void SaveRound(int round)
    {
        StartCoroutine(SaveRoundToDatabase(round));
    }
    /*ao clicar no save game ele vai atualizar a ronda atual na base de dados*/
    private IEnumerator SaveRoundToDatabase(int round)
    {
        PlayerData playerData = new PlayerData { id = _playerId, round = round };
        string json = JsonUtility.ToJson(playerData);

        UnityWebRequest request = new UnityWebRequest(UpdateRoundUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
    }

    public void SaveKills(int kills)
    {
        StartCoroutine(SaveKillsToDatabase(kills));
    }
    /*ao clicar save game vai atualizar a quantidade de kills feitas*/
    private IEnumerator SaveKillsToDatabase(int kills)
    {
        PlayerData playerData = new PlayerData { id = _playerId, kills = kills };
        string json = JsonUtility.ToJson(playerData);

        UnityWebRequest request = new UnityWebRequest(UpdateKillsUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
    }

    [System.Serializable]
    public class PlayerData
    {
        public int id;
        public int round;
        public int kills; 
    }
}
