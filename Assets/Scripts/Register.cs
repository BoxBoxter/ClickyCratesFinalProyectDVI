using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

public class Register : MonoBehaviour
{
    // Cached references
    public InputField emailInputField;
    public InputField confirmEmailInputFied;
    public InputField passwordInputField;
    public InputField confirmPasswordInputField;
    public InputField userInputField;
    public InputField firstNameInputField;
    public InputField lastNameInputField;
    public InputField dateBirthInputField;
    public InputField cityInputField;
    public Image avatar;

    public Button registerButton;
    public Text messageBoardText;
   

    public Player player;

    private void Start()
    {
        player = FindObjectOfType<Player>();
        Screen.orientation = ScreenOrientation.Portrait;
    }

    IEnumerator postRegisterNetUser()
    {
        if (string.IsNullOrEmpty(emailInputField.text))
        {
            throw new NullReferenceException("Email can't be void");
        }
        if (string.IsNullOrEmpty(userInputField.text))
        {
            throw new NullReferenceException("User can't be void");
        }
        if (string.IsNullOrEmpty(passwordInputField.text))
        {
            throw new NullReferenceException("User can't be void");
        }
        if(passwordInputField.text == confirmPasswordInputField.text)
        {
            UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/api/Account/Register", "POST");

            ASPNetUser newUser = new ASPNetUser();
            newUser.Email = emailInputField.text;
            newUser.Password = passwordInputField.text;
            newUser.ConfirmPassword = confirmPasswordInputField.text;

            string jsonData = JsonUtility.ToJson(newUser);
            byte[] dataToSend = Encoding.UTF8.GetBytes(jsonData);
            httpClient.uploadHandler = new UploadHandlerRaw(dataToSend);

            httpClient.SetRequestHeader("Content-Type", "application/json");

            yield return httpClient.SendWebRequest();  // Blocking call

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new Exception("OnRegisterButtonClick: Network Error" + httpClient.error);
            }

            messageBoardText.text += "\n" + httpClient.responseCode;

            httpClient.Dispose();
        }
        else
        {
            throw new Exception("ERROR: Not the same password");
        }
        
    }

    IEnumerator postPlayer()
    {

        UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/api/Player/InsertNewPlayer", "POST");
        
        PlayerAPI newPlayer = new PlayerAPI();

        newPlayer.Id = player.Id;
        newPlayer.FirstName = firstNameInputField.text;
        newPlayer.LastName = lastNameInputField.text;
        newPlayer.Email = emailInputField.text;
        newPlayer.DateBirth = dateBirthInputField.text;
        newPlayer.NickName = userInputField.text;
        newPlayer.City = cityInputField.text;
        newPlayer.BlobUri = "https://imagenesblob.blob.core.windows.net/imagenesblob/avatar/1.jpg"; // This need to be a choice
        newPlayer.Badge = "https://imagenesblob.blob.core.windows.net/imagenesblob/badge/1.png"; // This is the beginer avatar
        player.BlobUri = newPlayer.BlobUri;

        string jsonData = JsonUtility.ToJson(newPlayer);
        byte[] dataToSend = Encoding.UTF8.GetBytes(jsonData);
        httpClient.uploadHandler = new UploadHandlerRaw(dataToSend);

        httpClient.downloadHandler = new DownloadHandlerBuffer();

        httpClient.SetRequestHeader("Content-Type", "application/json");
        httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

        yield return httpClient.SendWebRequest();  // Blocking call

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            throw new Exception("OnRegisterButtonClick: Network Error" + httpClient.error);
        }

        messageBoardText.text = "\n" + httpClient.responseCode;

        httpClient.Dispose();
        
    }

    private IEnumerator GetToken()
    {
        if (string.IsNullOrEmpty(player.Token))
        {
            UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/Token", "POST");

            // application/x-www-form-urlencoded
            WWWForm dataToSend = new WWWForm();
            dataToSend.AddField("grant_type", "password");
            dataToSend.AddField("username", emailInputField.text);
            dataToSend.AddField("password", passwordInputField.text);

            httpClient.uploadHandler = new UploadHandlerRaw(dataToSend.data);
            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Accept", "application/json");

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new Exception("Helper > InitToken: " + httpClient.error);
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

    private IEnumerator GetAspNetUserId()
    {
        UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/api/Account/UserId", "GET");

        httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
        httpClient.SetRequestHeader("Accept", "application/json");

        httpClient.downloadHandler = new DownloadHandlerBuffer();

        yield return httpClient.SendWebRequest();

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            throw new Exception("Helper > GetPlayerId: " + httpClient.error);
        }
        else
        {
            player.Id = httpClient.downloadHandler.text.Replace("\"", "");
        }

        httpClient.Dispose();
    }

    private IEnumerator InsertPlayerOnline()
    {
        PlayerAPI playerAPI = new PlayerAPI();
        playerAPI.Id = player.Id.Replace("\"", "");

        using (UnityWebRequest httpClient = new UnityWebRequest(player.httpServer + "/api/Player/RegisterOnlinePlayer", "POST"))
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
                throw new Exception("RegisterNewOnlinePlayer > InsertPlayer: " + httpClient.error);
            }

            messageBoardText.text += "\nRegisterNewPlayer > InsertPlayer: " + httpClient.responseCode;
        }

    }

    public void OnRegisterButtonClick()
    {
        StartCoroutine(RegisterNewUser());
        //StartCoroutine(postRegisterNetUser());
        //StartCoroutine(GetToken());
        //StartCoroutine(GetAspNetUserId());
        //StartCoroutine(postPlayer());
    }

    private IEnumerator RegisterNewUser()
    {
        yield return postRegisterNetUser();
        yield return GetToken();
        yield return GetAspNetUserId();
        yield return postPlayer();
        yield return InsertPlayerOnline();
        yield return LoadImage();
    }

    private IEnumerator LoadImage()
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

}
