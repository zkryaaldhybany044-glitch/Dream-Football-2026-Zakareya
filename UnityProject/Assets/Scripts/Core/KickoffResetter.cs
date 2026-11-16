using System.Collections;
using UnityEngine;
using Zakareya.DreamFootball.Ball;

namespace Zakareya.DreamFootball.Core
{
    /// <summary>
    /// Optional goal-restart helper. Assign the ball, its kickoff spot, and player
    /// transforms. It freezes the ball briefly, then resets all assigned actors.
    /// </summary>
    public sealed class KickoffResetter : MonoBehaviour
    {
        [SerializeField] private MatchScoreboard scoreboard;
        [SerializeField] private MatchBall ball;
        [SerializeField] private Transform ballKickoffPoint;
        [SerializeField] private Transform[] players;
        [SerializeField, Min(0f)] private float restartDelay = 1.5f;

        private Coroutine pendingRestart;

        private void OnEnable()
        {
            if (scoreboard != null) scoreboard.GoalScored += OnGoalScored;
        }

        private void OnDisable()
        {
            if (scoreboard != null) scoreboard.GoalScored -= OnGoalScored;
            if (pendingRestart != null) StopCoroutine(pendingRestart);
        }

        private void OnGoalScored(bool homeTeamScored)
        {
            if (ball == null || ballKickoffPoint == null) return;
            if (pendingRestart != null) StopCoroutine(pendingRestart);
            pendingRestart = StartCoroutine(RestartAfterDelay());
        }

        private IEnumerator RestartAfterDelay()
        {
            if (ball.Body != null) ball.Body.isKinematic = true;
            yield return new WaitForSeconds(restartDelay);

            if (players != null)
            {
                foreach (Transform player in players)
                {
                    if (player == null) continue;
                    CharacterController controller = player.GetComponent<CharacterController>();
                    if (controller != null) controller.enabled = false;
                    player.SetPositionAndRotation(
                        player.position.x == float.NaN ? player.position : player.position,
                        player.rotation);
                    if (controller != null) controller.enabled = true;
                }
            }

            ball.ResetBall(ballKickoffPoint.position);
            if (ball.Body != null) ball.Body.isKinematic = false;
            pendingRestart = null;
        }
    }
}