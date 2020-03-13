using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LogOutLogin : MonoBehaviour
{
    // Start is called before the first frame update
    public Player player;

    void Start()
    {
        player = FindObjectOfType<Player>();
    }
    public void OnLogoutButtonClicked()
    {
        StartCoroutine(MakePlayerOffline());
    }

    private IEnumerator TryLogout()
    {
        UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/api/Account/Logout", "POST");
        httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

        yield return httpClient.SendWebRequest();


        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            throw new Exception("Login > TryLogout: " + httpClient.error);
        }
        else
        {
            player.Token = string.Empty;
            player.Id = string.Empty;
            player.FirstName = string.Empty;
            player.LastName = string.Empty;
            player.DateOfBirth = DateTime.MinValue;
            player.NickName = string.Empty;
            player.City = string.Empty;
            player.DateJoined = DateTime.MinValue;
            player.BlobUri = string.Empty;
            player.BlobBadge = string.Empty;
            player.Email = string.Empty;
            SceneManager.LoadScene("Main Menu");
        }

    }

    private IEnumerator MakePlayerOffline()
    {
        PlayerAPI playerAPI = new PlayerAPI();
        playerAPI.Id = player.Id.Replace("\"", "");

        using (UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/api/Player/ModifyPlayerOffline", "POST"))
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
        yield return TryLogout();
    }
}
