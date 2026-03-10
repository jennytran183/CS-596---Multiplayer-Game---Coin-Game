using Unity.Netcode;
using UnityEngine;
using TMPro;

public class HUDUI : MonoBehaviour
{
    public TextMeshProUGUI scoreboardText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI winnerText;

    NetworkGameManager gm;

    void Start()
    {
        gm = FindObjectOfType<NetworkGameManager>();
    }

    void Update()
    {
        // Scoreboard
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            if (scoreboardText != null) scoreboardText.text = "Not connected";
            if (timerText != null) timerText.text = "";
            return;
        }

        var players = FindObjectsOfType<PlayerController>();
        if (players.Length == 0)
        {
            if (scoreboardText != null) scoreboardText.text = "Waiting for players...";
            if (timerText != null) timerText.text = "Time: --";
            return;
        }

        if (scoreboardText != null)
        {
            string s = "";
            foreach (var p in players)
                s += $"Player {p.OwnerClientId + 1}: {p.Score.Value}\n";
            scoreboardText.text = s;
        }

        // Timer (requires gm.TimeLeft NetworkVariable)
        if (timerText != null)
        {
            if (gm == null) gm = FindObjectOfType<NetworkGameManager>();

            if (gm != null)
            {
                int secs = Mathf.CeilToInt(gm.TimeLeft.Value);
                timerText.text = $"Time: {secs}";
            }
            else
            {
                timerText.text = "Time: --";
            }
        }
        if (winnerText != null && gm != null)
        {
            if (gm.GameOver.Value)
            {
                winnerText.gameObject.SetActive(true);

                if (gm.IsTie.Value)
                    winnerText.text = $"TIE! ({gm.WinnerScore.Value})";
                else
                    winnerText.text = $"WINNER: Player {gm.WinnerClientId.Value + 1} ({gm.WinnerScore.Value})";
            }
            else
            {
                winnerText.gameObject.SetActive(false);
            }
        }

    }   
}