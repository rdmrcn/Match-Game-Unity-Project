using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    public int Score { get; private set; }
    public int Combo { get; private set; }

    [Header("Scoring")]
    [SerializeField] private int basePoint = 10;          // base
    [SerializeField] private float comboBonus = 0.5f;     // combo multiplier bonus per chain

    public void ResetAll()
    {
        Score = 0;
        Combo = 0;
    }

    public void ResetCombo()
    {
        Combo = 0;
    }

    public void IncreaseCombo()
    {
        Combo++;
    }

    // groupScore = (n*n*basePoint) * (1 + combo*comboBonus)
    public int AddBlastScore(int groupSize)
    {
        int raw = groupSize * groupSize * basePoint;
        float mult = 1f + (Combo * comboBonus);
        int gained = Mathf.RoundToInt(raw * mult);

        Score += gained;
        return gained;
    }
}