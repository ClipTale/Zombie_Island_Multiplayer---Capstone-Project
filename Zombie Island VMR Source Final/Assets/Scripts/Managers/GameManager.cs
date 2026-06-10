using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    private bool gameStarted = false;
    private bool isPaused = false;
    private int _playerId;
    private int _totalEnemiesKilled = 0; 

    [SerializeField] RectTransform HealthUI;
    [SerializeField] private GameObject DeathUI;
    [SerializeField] private TextMeshProUGUI RoundInfoText;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private GameObject _startGameUI;
    [SerializeField] private GameObject _hostUI;
    [SerializeField] private GameObject _readyButton;
    [SerializeField] private GameObject _saveButton;
    [SerializeField] private GameObject _pauseUI;
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private Transform _playerTransform;

    [SerializeField] private float _enemyHealthPerRound;
    [SerializeField] private int _enemiesPerRound = 2;
    [SerializeField] private float _spawnInterval;
    private int _currentRound = 1;
    private float _enemyMaxHealthForRound = 20;
    [SerializeField] private int _startingEnemies = 5;
    private int _enemiesToSpawn;
    private int _spawnedEnemies = 0; 
    private List<GameObject> _activeEnemies = new List<GameObject>();
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField][Range(0, 1)] private float _closestSpawnPointPercentage = 0.5f;

    private void Start()
    {
        /*ao entrar vai buscar a quantidade de kills salva na database*/
        if (IsServer)
        {
            LoadTotalEnemiesKilled();
        }
    }

    public void SetPlayerId(int playerId)
    {
        _playerId = playerId;
    }

    public void NewGame()
    {
        _currentRound = 1;
        Time.timeScale = 0f;
        _startGameUI.SetActive(true);
        _pauseUI.SetActive(false); 
        HealthUI.anchoredPosition = new Vector2(400, 970);
        healthUIClientRpc();
    }
    /*ao clicar no load game vai iniciar a coroutine que vai buscar a ronda atual do host a database*/
    public void LoadRound()
    {
        StartCoroutine(FetchRoundFromServer(_playerId)); 
        Time.timeScale = 0f;
        _startGameUI.SetActive(true);
        _pauseUI.SetActive(false);
        HealthUI.anchoredPosition = new Vector2(400, 970);
        healthUIClientRpc();
    }
    /*vai buscar a ronda do host atraves do id que recebe do login manager*/
    private IEnumerator FetchRoundFromServer(int playerId)
    {
        /*url para fazer o request ao server*/
        string url = $"http://localhost:6969/getRound?id={playerId}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                /*atualiza o currentRound para o round que foi buscar a database*/
                string jsonResponse = webRequest.downloadHandler.text;
                RoundData roundData = JsonUtility.FromJson<RoundData>(jsonResponse);
                _currentRound = roundData.round;
            }
        }
    }
    /*serve para ir buscar atraves do id a quantidade de kills ja feitas pelo host*/
    private void LoadTotalEnemiesKilled()
    {
        StartCoroutine(FetchTotalEnemiesKilledFromServer(_playerId));
    }
    /*vai buscar as kills que o host fez a database atraves do id*/
    private IEnumerator FetchTotalEnemiesKilledFromServer(int playerId)
    {
        /*url para o request*/
        string url = $"http://localhost:6969/getKills?id={playerId}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                PlayerData playerData = JsonUtility.FromJson<PlayerData>(jsonResponse);
                _totalEnemiesKilled = playerData.kills;
            }
           
        }
    }

    [System.Serializable]
    private class RoundData
    {
        public int round;
    }

    [System.Serializable]
    private class PlayerData
    {
        public int id;
        public int kills;
    }

    private void Update()
    {
        /*apenas o server/host tem permissao para mexer com o update*/
        if (!IsServer) return; 

        HandlePauseInput();
        /*vai buscar o playerTransform através da tag*/
        if (_playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                _playerTransform = playerObject.transform;
            }
            else
            {
                return; 
            }
        }

        CheckRoundProgress();
    }

    private void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        /*quando o host colocar o jogo em pausa avisar os clientes*/
        if (IsServer)
        {
            ShowPauseMenuClientRpc(); 
        }
        else
        {
            _pauseUI.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
        }
    }
    /*mostra o menu de pausa aos clientes*/
    [ClientRpc]
    private void ShowPauseMenuClientRpc()
    {
        _pauseUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true; 
        /*apenas o host pode dar resume do jogo*/
        if (!IsServer)
        {
            
            if (_resumeButton != null)
            {
                _resumeButton.interactable = false;
            }
        }
    }

    public void ResumeGame()
    {
        /*avisar o cliente quando o host der resume*/
        if (IsServer)
        {
            ResumeGameClientRpc();
        }
        else
        {
            _pauseUI.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
        }
    }

    private void StartNextRound()
    {
        /*apenas o host pode começar a ronda*/
        if (!IsServer) return; 
        /*atualiza o texto com a ronda atual*/
        roundText.text = "Round: " + _currentRound;
        /*atualiza o texto para o cliente*/
        UpdateRoundTextClientRpc(_currentRound);

        
        _enemyMaxHealthForRound += _enemyHealthPerRound;

        
        _enemiesToSpawn = _startingEnemies + (_currentRound - 1) * _enemiesPerRound;
        _spawnedEnemies = 0;

     
        Transform closestSpawnPoint = GetClosestSpawnPoint();

       
        int enemiesAtClosestPoint = Mathf.CeilToInt(_enemiesToSpawn * _closestSpawnPointPercentage);
        int enemiesAtRandomPoints = _enemiesToSpawn - enemiesAtClosestPoint;

       
        StartCoroutine(SpawnEnemies(closestSpawnPoint, enemiesAtClosestPoint));

      
        int remainingSpawnPoints = _spawnPoints.Length - 1;
        foreach (Transform spawnPoint in _spawnPoints)
        {
            if (spawnPoint != closestSpawnPoint)
            {
                int enemiesPerRandomPoint = Mathf.CeilToInt((float)enemiesAtRandomPoints / remainingSpawnPoints);
                StartCoroutine(SpawnEnemies(spawnPoint, enemiesPerRandomPoint));
                enemiesAtRandomPoints -= enemiesPerRandomPoint;
                remainingSpawnPoints--;
            }
        }

        _currentRound++;
    }

    private IEnumerator SpawnEnemies(Transform spawnPoint, int enemiesToSpawn)
    {
        if (!IsServer) yield break; 

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (_spawnedEnemies >= _enemiesToSpawn)
            {
                yield break; 
            }

            GameObject newEnemy = Instantiate(_enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            newEnemy.GetComponent<NetworkObject>().Spawn(true); 
            newEnemy.GetComponent<Enemy>().OnEnemyKilled += HandleEnemyKilled;

            newEnemy.GetComponent<Enemy>().SetMaxHealth(_enemyMaxHealthForRound);

            _activeEnemies.Add(newEnemy);
            _spawnedEnemies++;

            yield return new WaitForSeconds(_spawnInterval);
        }
    }

   private void HandleEnemyKilled(GameObject enemy)
    {
        
        if (!IsServer) return;

        _activeEnemies.Remove(enemy);
        _totalEnemiesKilled++; 
       
        if (_activeEnemies.Count == 0 && _spawnedEnemies >= _enemiesToSpawn && gameStarted)
        {
            /*quando a ronda acabar avisa o cliente que tem de esperar pelo host*/
            ShowStartGameButtonClientRpc(); 
        }
    }

    private void CheckRoundProgress()
    {
        if (!IsServer) return; 

        if (_activeEnemies.Count == 0 && _spawnedEnemies >= _enemiesToSpawn && gameStarted)
        {
            /*quando a ronda acabar avisa o cliente que tem de esperar pelo host*/
            ShowStartGameButtonClientRpc(); 
        }
    }

    private Transform GetClosestSpawnPoint()
    {
        Transform closestSpawnPoint = null;
        float closestDistance = float.MaxValue;

        foreach (Transform spawnPoint in _spawnPoints)
        {
            float distance = Vector3.Distance(_playerTransform.position, spawnPoint.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSpawnPoint = spawnPoint;
            }
        }

        return closestSpawnPoint;
    }

    public void ReadyToStart()
    {
        /*para reutilizar este botao, ele vai buscar o componente de texto e muda o para Next Round*/
        _startGameUI.GetComponent<Button>().GetComponentInChildren<TextMeshProUGUI>().text = "Next Round";
        RoundInfoText.text = "";
        StartNextRound();
        _saveButton.SetActive(false);
        _startGameUI.SetActive(false);
        _pauseUI.SetActive(false);
        Time.timeScale = 1f;
        gameStarted = true;

        /*avisa o cliente que a proxima ronda vai começar ao retirar os UIs da frente*/
        HideStartGameUIClientRpc();
        ResumeGameClientRpc();
    }
    
    [ClientRpc]
    private void HideStartGameUIClientRpc()
    {
        RoundInfoText.text = "";
        _startGameUI.SetActive(false); 
        _saveButton.SetActive(false);
        _pauseUI.SetActive(false);
    }
    /*atualiza a ronda para o cliente*/
    [ClientRpc]
    private void UpdateRoundTextClientRpc(int round)
    {
        roundText.text = "Round: " + round;
    }
    /*avisa o cliente quando o jogo dá resume*/
    [ClientRpc]
    private void ResumeGameClientRpc()
    {
        _pauseUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false; 
    }
    /*avisa o cliente quando a ronda acaba para esperar o host, caso seja o host tem acesso a dois botoes*/
    [ClientRpc]
    private void ShowStartGameButtonClientRpc()
    {
        if (IsHost)
        {
            _saveButton.SetActive(true);
            _startGameUI.SetActive(true);
        }
        if (!IsHost)
        {
            RoundInfoText.text = "Waiting for Host!";
        }
        Time.timeScale = 0f;
        isPaused = true;
    }
    /*ao clicar no botao de save game, vai salvar a ronda e os enemies que foram mortos até clicar no botao de save*/
    public void SaveCurrentRound()
    {
        roundManager.SaveRound(_currentRound);
        roundManager.SaveKills(_totalEnemiesKilled);
        /*caso ja tenha clicado no botao de save muda este numero para 0 para nao salvar enemies que ja foram mortos anteriormente*/
        _totalEnemiesKilled = 0;
    }
    /*ao morrer chama as funçoes de morte*/
    public void onDeath()
    {
        ShowDeathUIClientRpc();
        HandlePlayerDeath();
    }
    /*avisa o cliente que morreram*/
    [ClientRpc]
    private void ShowDeathUIClientRpc()
    {
        DeathUI.SetActive(true);
    }
    /*para atualizar a posiçao do healthUI caso nao tenha atualizado antes*/
    [ClientRpc]
    private void healthUIClientRpc()
    {
        HealthUI.anchoredPosition = new Vector2(400, 970);
    }
    [System.Serializable]
    private class DeathData
    {
        public int id;
    }
    /*serve para atualizar as mortes que o host ja teve na database*/
    private void HandlePlayerDeath()
    {
        StartCoroutine(UpdateDeathsOnServer(_playerId));
    }
    /*vai atualizar na database a quantidade de mortes que o host teve*/
    private IEnumerator UpdateDeathsOnServer(int playerId)
    {
        string url = "http://localhost:6969/updateDeaths";
        DeathData deathData = new DeathData { id = playerId };
        string json = JsonUtility.ToJson(deathData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();        
    }
}
