using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Text;
using System;

public class GameManager : MonoBehaviour
{
    public List<GameObject> targets;
    private float spawnRate = 1.0f;
    public TextMeshProUGUI scoreText;
    private int score;
    public TextMeshProUGUI gameOverText;
    public bool isGameActive;
    public Button restartButton;
    public Button back;
    public Button quit;
    public GameObject titleScreen;

    public Player player; // Asociamos el loginManager para obtener parametros como el tokken o la url del servidor
    public GameObject jugador;
    public GameObject concuchara;


    private void Start()
    {
        player = FindObjectOfType<Player>();
        StartCoroutine(ShowPlayersOnline());
    }
    IEnumerator SpawnTarget()
    {
        while(isGameActive)
        {
            yield return new WaitForSeconds(spawnRate);
            int index = UnityEngine.Random.Range(0, targets.Count);
            Instantiate(targets[index]);
        }
    }
    
    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "Score: " + score;
    }

    public void GameOver()
    {
        gameOverText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
        back.gameObject.SetActive(true);
        quit.gameObject.SetActive(true);

        isGameActive = false;
        StartCoroutine(ModifyPlayeNoPlaying());
    }

    private IEnumerator ShowPlayersOnline()
    {
        while(true)
        {
            //Peticion jugadores

            using (UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/api/Player/OnlinePlaying", "GET"))
            {
                httpClient.downloadHandler = new DownloadHandlerBuffer();
                httpClient.SetRequestHeader("Content-type", "application/json");
                httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

                yield return httpClient.SendWebRequest();

                if (httpClient.isNetworkError || httpClient.isHttpError)
                {
                    throw new Exception("OnlinePlayers > GetPlayers: " + httpClient.error);
                }
                else
                {

                    string jsonResponse = httpClient.downloadHandler.text;

                    string response = "{\"players\":" + jsonResponse + "}";
                    ListPlayers players = JsonUtility.FromJson<ListPlayers>(response);

                    foreach (PlayerAPI str in players.players)
                    {
                        GameObject jugadorJugando = Instantiate(jugador, Vector3.zero, Quaternion.identity);
                        jugadorJugando.transform.SetParent(concuchara.transform);
                        jugadorJugando.transform.GetChild(1).GetComponent<Text>().text = str.NickName;
                        yield return LoadAvatar(str.BlobUri, jugadorJugando);
                        yield return LoadBadge(str.Badge, jugadorJugando);

                    }
                }
            }

            yield return new WaitForSeconds(5);
            EliminarHijos();
        }
       
    }

    private void EliminarHijos()
    {
        int hijos = concuchara.transform.childCount;
        GameObject cualBorrar;

        for(int i = 0; i < hijos; i++)
        {
            cualBorrar = concuchara.transform.GetChild(i).gameObject;
            Destroy(cualBorrar);
        }
    }
    private IEnumerator LoadAvatar(string urlImagenAvatar, GameObject donde)
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(urlImagenAvatar))
        {
            httpClient.downloadHandler = new DownloadHandlerTexture();
            yield return httpClient.SendWebRequest();
            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                Debug.Log("ERROR: Imagen Avatar");
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(httpClient);
                donde.transform.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(texture,
                    new Rect(0.0f, 0.0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100.0f);
            }
        }
    }

    private IEnumerator LoadBadge(string urlImagenBadge, GameObject donde)
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(player.BlobBadge))
        {
            httpClient.downloadHandler = new DownloadHandlerTexture();
            yield return httpClient.SendWebRequest();
            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                Debug.Log("ERROR: Imagen Badge");
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(httpClient);
                donde.transform.GetChild(2).GetComponent<Image>().sprite = Sprite.Create(texture,
                    new Rect(0.0f, 0.0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100.0f);
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void startGame(int difficulty)
    {
        isGameActive = true;
        score = 0;
        UpdateScore(0);
        titleScreen.gameObject.SetActive(false);
        spawnRate /= difficulty;
        StartCoroutine(SpawnTarget());
        StartCoroutine(UpdateEstadoJugador(difficulty));
    }

    private IEnumerator ModifyPlayeNoPlaying()
    {
        
        PlayerAPI playerAPI = new PlayerAPI();
        playerAPI.Id = player.Id.Replace("\"", "");

        using (UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/api/Player/ModifyPlayeNoPlaying", "POST"))
        {
            string playerData = JsonUtility.ToJson(playerAPI);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);
            httpClient.downloadHandler = new DownloadHandlerBuffer();
            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new Exception("RegisterNewPlayer > InsertPlayer: " + httpClient.error);
            }

            yield return PlayerNoPlaying();
        }
    }

    private IEnumerator PlayerNoPlaying()
    {

        UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/api/Player/TerminarPartida", "POST");

        HistorialModel playerHistorial = new HistorialModel();

        playerHistorial.Id = player.Id;
        playerHistorial.Score = score;

        string jsonData = JsonUtility.ToJson(playerHistorial);
        byte[] dataToSend = Encoding.UTF8.GetBytes(jsonData);
        httpClient.uploadHandler = new UploadHandlerRaw(dataToSend);

        httpClient.downloadHandler = new DownloadHandlerBuffer();

        httpClient.SetRequestHeader("Content-Type", "application/json");
        httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

        yield return httpClient.SendWebRequest();  // Blocking call

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            throw new Exception("OnHistorialPost: Network Error" + httpClient.error);
        }

        httpClient.Dispose();
    }

    private IEnumerator UpdateEstadoJugador(int difficulty)
    {
        PlayerAPI playerAPI = new PlayerAPI();
        playerAPI.Id = player.Id.Replace("\"", "");

        using (UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/api/Player/ModifyPlayePlaying", "POST"))
        {
            string playerData = JsonUtility.ToJson(playerAPI);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);
            httpClient.downloadHandler = new DownloadHandlerBuffer();
            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new Exception("RegisterNewPlayer > InsertPlayer: " + httpClient.error);
            }

            yield return PlayerPlaying(difficulty);
        }
    }

    private IEnumerator PlayerPlaying(int difficulty)
    {
      
        UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/api/Player/InsertPartida", "POST");

        HistorialModel playerHistorial = new HistorialModel();

        playerHistorial.Id = player.Id;
        playerHistorial.Dificultad = difficulty;
        playerHistorial.NickName = player.NickName;
     
        string jsonData = JsonUtility.ToJson(playerHistorial);
        byte[] dataToSend = Encoding.UTF8.GetBytes(jsonData);
        httpClient.uploadHandler = new UploadHandlerRaw(dataToSend);

        httpClient.downloadHandler = new DownloadHandlerBuffer();

        httpClient.SetRequestHeader("Content-Type", "application/json");
        httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

        yield return httpClient.SendWebRequest();  // Blocking call

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            throw new Exception("OnHistorialPost: Network Error" + httpClient.error);
        }

        httpClient.Dispose();
    }

}
