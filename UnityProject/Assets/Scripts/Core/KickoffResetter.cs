using System.Collections;
using UnityEngine;
using Zakareya.DreamFootball.Ball;

namespace Zakareya.DreamFootball.Core
{
    /// <summary>
    /// Optional goal-restart helper. Assign matching player and spawn-point arrays,
    /// plus the ball and its kickoff spot, in the Inspector.
    /// </summary>
    public sealed class KickoffResetter : MonoBehaviour
    {
        [SerializeField] private MatchScoreboard scoreboard;
        [SerializeField] private MatchBall ball;
        [SerializeField] private Transform ballKickoffPoint;
        [SerializeField] private Transform[] players;
        [SerializeField] private Transform[] playerSpawnPoints;
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
            if (ball != null && ball.Body != null) ball.Body.isKinematic = false;
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

            if (players != null && playerSpawnPoints != null)
            {
                int count = Mathf.Min(players.Length, playerSpawnPoints.Length);
                for (int i = 0; i < count; i++)
                {
                    Transform player = players[i];
                    Transform spawn = playerSpawnPoints[i];
                    if (player == null || spawn == null) continue;

                    CharacterController controller = player.GetComponent<CharacterController>();
                    if (controller != null) controller.enabled = false;
                    player.SetPositionAndRotation(spawn.position, spawn.rotation);
                    if (controller != null) controller.enabled = true;
                }
            }

            ball.ResetBall(ballKickoffPoint.position);
            if (ball.Body != null) ball.Body.isKinematic = false;
            pendingRestart = null;
        }
    }
}