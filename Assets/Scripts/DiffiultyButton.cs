using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DiffiultyButton : MonoBehaviour
{
    private Button button;
    private GameManager gameManager;
    public int difficulty;


    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(setDificult);

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void setDificult()
    {
        Debug.Log(gameObject.name + " was clicked");
        gameManager.startGame(difficulty);
    }
}
