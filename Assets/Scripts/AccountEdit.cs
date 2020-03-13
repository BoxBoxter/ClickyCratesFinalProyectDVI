using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AccountEdit : MonoBehaviour
{
    public Player player; // Asociamos el loginManager para obtener parametros como el tokken o la url del servidor
    public InputField firstName;
    public InputField lastName;
    public InputField nickName;
    public InputField dateOfBirth;
    public InputField city;
    public Image avatar;

    public Button editAccount;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        StartCoroutine(LoadAvatar());

        firstName.text = player.FirstName;
        lastName.text = player.LastName;
        nickName.text = player.NickName;
        dateOfBirth.text = player.DateOfBirth.ToString(@"yy/MM/dd");
        city.text = player.City;

    }

    public void OnEditAccountClick()
    {
        StartCoroutine(TryEdit());
    }

    private IEnumerator LoadAvatar()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(player.BlobUri))
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
                avatar.sprite = Sprite.Create(texture,
                    new Rect(0.0f, 0.0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100.0f);
            }
        }
    }

    private IEnumerator TryEdit()
    {
        UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/api/Player/UpdateAccount", "POST");

        PlayerAPI playerAPI = new PlayerAPI();

        playerAPI.FirstName = firstName.text;
        playerAPI.LastName = lastName.text;
        playerAPI.NickName = nickName.text;
        playerAPI.DateBirth = dateOfBirth.text;
        playerAPI.City = city.text;
        

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

        httpClient.Dispose();
    }

    
}
