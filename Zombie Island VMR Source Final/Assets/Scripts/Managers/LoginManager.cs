using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI feedbackText;
    [SerializeField] private GameObject hostclientUI;
    [SerializeField] private GameObject loginUI;
    [SerializeField] private RoundManager roundManager; 
    [SerializeField] private ButtonManager buttonManager;
    [SerializeField] private GameManager gameManager;

    /*ao clicar no botao de dar login, vai buscar o texto escrito nas caixas de texto*/
    public void OnLoginButtonClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        /*começa a coroutine de verificaçao*/
        StartCoroutine(Login(username, password));
    }
    /*verifica na database se existe uma conta com o username dado e se a palavra passe está correta*/
    private IEnumerator Login(string username, string password)
    {
        string url = "http://localhost:6969/login";
        LoginData loginData = new LoginData { username = username, password = password };
        string json = JsonUtility.ToJson(loginData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        /*se receber resposta do server vai verificar se o username e a password sao validos*/
        if (request.result == UnityWebRequest.Result.Success)
        {            
            LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            /*se nao for valido da erro*/
            if (loginResponse == null || loginResponse.id == 0)
            {
                feedbackText.text = "Invalid username or password";
            }
            /*se for valido ele vai dar os dados necessarios aos outros scripts*/
            else
            {
                feedbackText.text = "Login successful!";

                
                roundManager.SetPlayerId(loginResponse.id);

                
                gameManager.SetPlayerId(loginResponse.id);

                buttonManager.SetPlayerId(loginResponse.id);

               
                hostclientUI.SetActive(true);
                loginUI.SetActive(false);
            }
        }
        /*se nao conseguir conectar ao server mostra mensagem de erro*/
        else
        {
            feedbackText.text = "Login failed: " + request.error;
        }
    }

    [System.Serializable]
    public class LoginData
    {
        public string username;
        public string password;
    }

    [System.Serializable]
    public class LoginResponse
    {
        public int id;
        public string username;
        public int round;
    }
}
