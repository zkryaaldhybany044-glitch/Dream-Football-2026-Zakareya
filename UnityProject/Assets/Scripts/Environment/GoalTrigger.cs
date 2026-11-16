using UnityEngine;
using Zakareya.DreamFootball.Ball;
using Zakareya.DreamFootball.Core;

namespace Zakareya.DreamFootball.Environment
{
    /// <summary>
    /// Place this on a trigger volume spanning the inside of a goal mouth.
    /// Configure which team is credited when the ball crosses this goal.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class GoalTrigger : MonoBehaviour
    {
        [SerializeField] private MatchScoreboard scoreboard;
        [SerializeField] private MatchBall matchBall;
        [Tooltip("True when the home team scores in this goal; false when the away team scores.")]
        [SerializeField] private bool homeTeamScoresHere = true;

        private bool goalAlreadyRegistered;

        private void Reset()
        {
            Collider goalCollider = GetComponent<Collider>();
            goalCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (goalAlreadyRegistered || scoreboard == null) return;

            MatchBall enteredBall = other.GetComponentInParent<MatchBall>();
            if (enteredBall == null || (matchBall != null && enteredBall != matchBall)) return;

            goalAlreadyRegistered = true;
            scoreboard.RegisterGoal(homeTeamScoresHere);
        }

        private void OnTriggerExit(Collider other)
        {
            MatchBall exitedBall = other.GetComponentInParent<MatchBall>();
            if (exitedBall != null && (matchBall == null || exitedBall == matchBall))
                goalAlreadyRegistered = false;
        }
    }
}