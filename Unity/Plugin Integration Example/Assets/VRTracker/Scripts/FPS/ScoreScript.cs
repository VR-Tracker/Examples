using UnityEngine;

public class ScoreScript : MonoBehaviour {

    private int Score = 0;

    public int getScore()
    {
        return Score;
    }

    public void setScore(int newScore)
    {
        Score = newScore;
    }

    public int addScore(int myScore)
    {
        Score += myScore;
        return Score;
    }

    public void RemoveScore(int myScore)
    {
        Score -= myScore;
    }
}
