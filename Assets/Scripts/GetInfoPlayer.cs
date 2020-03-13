using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GetInfoPlayer : MonoBehaviour
{
    public Player player;
    public Text namePlayer;
    public Text nickNamePlayer;
    public UnityEngine.UI.Image avatar;
    public UnityEngine.UI.Image Badge;
    public Text textoJugador;
    public GameObject scroll;
    //public GameObject contexto;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        namePlayer.text = player.FirstName;
        nickNamePlayer.text = player.NickName;
        StartCoroutine(LoadAvatar());
        StartCoroutine(LoadBadge());
        Screen.orientation = ScreenOrientation.Landscape;
        StartCoroutine(GetGames());

    }

    private IEnumerator LoadAvatar()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(player.BlobUri))
        {
            httpClient.downloadHandler = new DownloadHandlerTexture();
            yield return httpClient.SendWebRequest();
            if(httpClient.isNetworkError || httpClient.isHttpError)
            {
                Debug.Log("ERROR: Imagen Avatar");
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(httpClient);
                avatar.sprite = Sprite.Create(texture,
                    new Rect(0.0f, 0.0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100.0f);
            }
        }
    }

    private IEnumerator LoadBadge()
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
                Badge.sprite = Sprite.Create(texture,
                    new Rect(0.0f, 0.0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100.0f);
            }
        }
    }

    public IEnumerator GetGames()
    {
        HistorialModel playerAPI = new HistorialModel();
        playerAPI.Id = player.Id.Replace("\"", "");

        using (UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/api/Player/Online", "GET"))
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
            else
            {

                string jsonResponse = httpClient.downloadHandler.text;

                string response = "{\"players\":" + jsonResponse + "}";
                ListHistorialPlayer players = JsonUtility.FromJson<ListHistorialPlayer>(response);

                foreach (HistorialModel str in players.players)
                {
                    {
                        DateTime fechaterminar = DateTime.Parse(str.Terminar); //obtenemos este valor de una bbdd
                        DateTime fechaEmpezar = DateTime.Parse(str.Empezar);
                        var horas = (fechaterminar - fechaEmpezar).ToString(@"mm\:ss\.ff");
                        Text textoPartida = Instantiate(textoJugador, Vector3.zero, Quaternion.identity);
                        textoPartida.text = "Score: " + str.Score +" Level: "+ str.Dificultad + " Time: " +horas+"\n";
                        textoPartida.fontSize = 14;
                        textoPartida.resizeTextForBestFit = true;
                        textoPartida.color = Color.black;
                        textoPartida.alignment = TextAnchor.MiddleCenter;
                        textoPartida.transform.SetParent(scroll.transform.GetChild(0).GetChild(0).transform);
                    }
                }
            }
        }

    }
}
