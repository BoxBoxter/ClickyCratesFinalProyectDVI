using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGameScene : MonoBehaviour
{
    public void OnGoToGameSceneButtonClick()
    {
        SceneManager.LoadScene("Prototype 5");
    }
    public void OnGoToRegisterSceneButtonClick()
    {
        SceneManager.LoadScene("Resgister Form");
    }

    public void OnGoBackFromRegisterSceneButtonClick()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void OnGoLoginMenuSceneButtonCLick()
    {
        SceneManager.LoadScene("Login Menu");
    }

    public void OnGoToAcoountButtonClick()
    {
        SceneManager.LoadScene("Account Form");
    }

}
