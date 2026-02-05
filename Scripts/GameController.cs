using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private HUDController hud;

    [Header("Lives / Moves (Only decreases on WRONG click)")]
    [SerializeField] private int startLives = 5;

    [Header("Timer")]
    [SerializeField] private float roundSeconds = 120f;

    [Header("Score")]
    [SerializeField] private int scorePerTile = 10;
    [SerializeField] private int targetScore = 3000;

    private int score;
    private int lives;
    private float timeLeft;
    private bool gameOver;

    private Coroutine restartCo;

    private void OnEnable()
    {
        if (boardManager != null)
            boardManager.BlastExecuted += OnBlastExecuted;
    }

    private void OnDisable()
    {
        if (boardManager != null)
            boardManager.BlastExecuted -= OnBlastExecuted;
    }

    private void Start()
    {
        RestartGameImmediate();
    }

    private void Update()
    {
        if (gameOver) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft < 0f) timeLeft = 0f;

        if (hud != null)
            hud.SetTime(timeLeft);

        if (timeLeft <= 0f)
        {
            RestartGame();
        }
    }

    // ✅ ONLY WRONG CLICK çağıracak (patlama yoksa)
    public void ConsumeLife()
    {
        if (gameOver) return;

        lives--;
        if (lives < 0) lives = 0;

        if (hud != null)
            hud.SetMoves(lives); // HUD move yazıyor ama aslında can / hamle

        if (lives <= 0)
        {
            RestartGame();
        }
    }

    private void OnBlastExecuted(int removedCount)
    {
        if (gameOver) return;

        score += removedCount * scorePerTile;

        if (hud != null)
            hud.SetScore(score);

        if (score >= targetScore)
        {
            RestartGame();
        }
    }

    public void RestartGame()
    {
        if (restartCo != null) return;
        restartCo = StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        gameOver = true;
        yield return new WaitForSeconds(0.2f);

        RestartGameImmediate();

        restartCo = null;
    }

    private void RestartGameImmediate()
    {
        gameOver = false;

        score = 0;
        lives = startLives;
        timeLeft = roundSeconds;

        if (hud != null)
        {
            hud.SetScore(score);
            hud.SetMoves(lives);   // UI: Moves: 5
            hud.SetTime(timeLeft);
            hud.ClearStatus();
        }

        if (boardManager != null)
            boardManager.ResetBoard();
    }
}
