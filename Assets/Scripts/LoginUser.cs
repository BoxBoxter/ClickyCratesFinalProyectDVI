using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginUser : MonoBehaviour
{
    private Player player; // Asociamos el loginManager para obtener parametros como el tokken o la url del servidor

    public InputField user; // Vinculo unity inputField de user a script
    public InputField password; // Vinculo password inputField de user a script
    public Button loginButton;

    public Text showMessage;


    private void Start()
    {
        player = FindObjectOfType<Player>();
        Screen.orientation = ScreenOrientation.Portrait;
    }

    public void OnLoginButtonClicked()
    {
        StartCoroutine(TryLogin());
    }

    private IEnumerator GetToken()
    {
        if (string.IsNullOrEmpty(player.Token))
        {
            UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/Token", "POST");

            // application/x-www-form-urlencoded
            WWWForm dataToSend = new WWWForm();
            dataToSend.AddField("grant_type", "password");
            dataToSend.AddField("username", user.text);
            dataToSend.AddField("password", password.text);

            httpClient.uploadHandler = new UploadHandlerRaw(dataToSend.data);
            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Accept", "application/json");

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                showMessage.text = "*Can't Login";
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                AuthorizationToken authToken = JsonUtility.FromJson<AuthorizationToken>(jsonResponse);
                player.Token = authToken.access_token;
            }
            httpClient.Dispose();
        }
    }

    private IEnumerator LoginPlayer()
    {
        UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/api/Player/Info", "GET");

        httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
        httpClient.SetRequestHeader("Accept", "application/json");

        httpClient.downloadHandler = new DownloadHandlerBuffer();

        yield return httpClient.SendWebRequest();

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            showMessage.text = "*Can't Login";
        }
        else
        {
            PlayerAPI playerAPI = JsonUtility.FromJson<PlayerAPI>(httpClient.downloadHandler.text);
            player.Id = playerAPI.Id;
            player.FirstName = playerAPI.FirstName;
            player.LastName = playerAPI.LastName;
            player.Email = playerAPI.Email;
            player.DateBirth = DateTime.Parse(playerAPI.DateBirth);
            player.NickName = playerAPI.NickName;
            player.City = playerAPI.City;
            player.DateJoined = DateTime.Parse(playerAPI.DateJoined);
            player.BlobUri = playerAPI.BlobUri;
            player.BlobBadge = playerAPI.Badge;
            yield return MakePlayerOnline();
            SceneManager.LoadScene("Login Menu");

        }

        httpClient.Dispose();
    }
       

    private IEnumerator MakePlayerOnline()
    {
        PlayerAPI playerAPI = new PlayerAPI();
        playerAPI.Id = player.Id.Replace("\"", "");

        using (UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/api/Player/ModifyPlayerOnline", "POST"))
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
        }

    }

    private IEnumerator TryLogin()
    {
        yield return GetToken();
        yield return LoginPlayer();
    }

}
