using System;
using UnityEngine;

namespace Zakareya.DreamFootball.Core
{
    /// <summary>Stores the match score and broadcasts successful goal registrations.</summary>
    public sealed class MatchScoreboard : MonoBehaviour
    {
        [SerializeField, Min(0)] private int homeScore;
        [SerializeField, Min(0)] private int awayScore;

        public int HomeScore => homeScore;
        public int AwayScore => awayScore;
        public event Action<int, int> ScoreChanged;
        public event Action<bool> GoalScored;

        public void RegisterGoal(bool homeTeamScored)
        {
            if (homeTeamScored) homeScore++;
            else awayScore++;

            ScoreChanged?.Invoke(homeScore, awayScore);
            GoalScored?.Invoke(homeTeamScored);
        }

        public void ResetScore()
        {
            homeScore = 0;
            awayScore = 0;
            ScoreChanged?.Invoke(homeScore, awayScore);
        }
    }
}