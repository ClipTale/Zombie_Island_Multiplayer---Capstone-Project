using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode.Transports.UTP;

public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private GameObject playUI;
    [SerializeField] private GameObject ServerHostClientUI;
    [SerializeField] private GameObject SaveLoadUI;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private TMP_InputField addressInputField;
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private TextMeshProUGUI playerCountText;

    private NetworkVariable<int> playersNum = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);

    /*Serve para começar uma sessao como host*/
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        SaveLoadUI.SetActive(true);
        ServerHostClientUI.SetActive(false);
    }
    /*Serve para entrar como cliente*/
    public void StartClient()
    {
        /*este if serve para caso o client queira conectar remotamente ao host através do ip*/
        if (addressInputField.text != "")
        {
            /*vai buscar o componente do UnityTransport*/
            UnityTransport unityTransport = networkManager.GetComponent<UnityTransport>();
            /*altera a connection data para o ip colocado na caixa de texto*/
            unityTransport.SetConnectionData(addressInputField.text, 7777);
        }
        NetworkManager.Singleton.StartClient();
        ServerHostClientUI.SetActive(false);
        playUI.SetActive(false);
        Time.timeScale = 1f;
        /*para que nao tenha erros, barra de vida está sempre visivel mas escondida fora da camara, isto faz com que ela vá para o lugar dela quando o client se conectar*/
        healthBar.anchoredPosition = new Vector2(400,970);
    }

    private void Update()
    {
        /*if para atualizar o playerCountText apenas quando alguem se conectar*/
        if (playersNum.Value != 0)
        {
            playerCountText.text = "Players: " + playersNum.Value.ToString();
        }
        /*serve para que apenas o server possa alterar o valor da quantidade de clientes conectados*/
        if (!IsServer) return;
        playersNum.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }

   
}