using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ButtonManager : MonoBehaviour
{
    private GameManager gameManager;
    [SerializeField] private GameObject LoginUI;
    [SerializeField] private GameObject HostClientUI;
    [SerializeField] private GameObject StartGameUI;
    [SerializeField] private GameObject PlayUI;
    [SerializeField] private LoginManager loginManager;
    [SerializeField] private GameObject pausedUI;
    [SerializeField] private TextMeshProUGUI roundUI;
    [SerializeField] private GameObject DeathUI;

    /*url para a chamada do servidor ao entrar no jogo*/
    private const string UpdateTimesPlayedUrl = "http://localhost:6969/updateTimesPlayed";
    private int _playerId;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        Time.timeScale = 0f;
    }

    public void PlayButton()
    {
        LoginUI.SetActive(true);
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void StartNewGameButton()
    {
        if (gameManager != null)
        {
            StartGameUI.SetActive(false);
            PlayUI.SetActive(false);
            gameManager.NewGame();
            Time.timeScale = 1f;
            PlayUI.SetActive(false);
            StartCoroutine(UpdateTimesPlayedInDatabase());
        }
    }

    public void LoadGameButton()
    {
        if (gameManager != null)
        {
            PlayUI.SetActive(false);
            StartGameUI.SetActive(false);
            gameManager.LoadRound();
            Time.timeScale = 1f;
            PlayUI.SetActive(false);
            StartCoroutine(UpdateTimesPlayedInDatabase());
        }
    }

    public void BackButton()
    {
        StartGameUI.SetActive(false);
        LoginUI.SetActive(false);
        PlayUI.SetActive(true);
    }

    public void LoginButton()
    {
        loginManager.OnLoginButtonClicked();
    }

    public void SpectateButton()
    {
        DeathUI.SetActive(false);
    }

    public void SetPlayerId(int playerId)
    {
        _playerId = playerId;
    }
    /*Serve para quando o host entrar no jogo, dar update a quantidade de vezes que jogou*/
    private IEnumerator UpdateTimesPlayedInDatabase()
    {

        PlayerData playerData = new PlayerData { id = _playerId };
        string json = JsonUtility.ToJson(playerData);

        /*faz o request atraves do url*/
        UnityWebRequest request = new UnityWebRequest(UpdateTimesPlayedUrl, "POST");
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
    }
}
