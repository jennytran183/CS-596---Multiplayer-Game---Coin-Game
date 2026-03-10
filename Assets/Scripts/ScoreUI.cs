using Unity.Netcode;
using UnityEngine;
using TMPro; 

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;  

    void Update()
    {
        if (scoreText == null) return;

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            scoreText.text = "Not connected";
            return;
        }

        var players = FindObjectsOfType<PlayerController>();
        if (players.Length == 0)
        {
            scoreText.text = "Waiting for players...";
            return;
        }

        
        PlayerController me = null;
        foreach (var p in players)
        {
            if (p.IsOwner)
            {
                me = p;
                break;
            }
        }

        if (me == null)
        {
            scoreText.text = "Waiting for my player...";
            return;
        }

        scoreText.text = $"My Score: {me.Score.Value}";
    }
}