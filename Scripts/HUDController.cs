using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [Header("TMP Text References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text statusText;

    public void SetScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    public void SetMoves(int moves)
    {
        if (movesText != null)
            movesText.text = $"Moves: {moves}";
    }

    public void SetTime(float seconds)
    {
        if (timerText == null) return;

        if (seconds < 0f) seconds = 0f;

        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);

        timerText.text = $"{mins:00}:{secs:00}";
    }

    public void SetStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
    }

    public void ClearStatus()
    {
        if (statusText != null)
            statusText.text = "";
    }
}