using UnityEngine;
using Zakareya.DreamFootball.Ball;

namespace Zakareya.DreamFootball.Players
{
    /// <summary>
    /// Simple offline pass selection for one AI player. Assign teammates and
    /// opponents in the Inspector. This component makes a pass decision only
    /// when the ball is within its configured control range.
    /// </summary>
    public sealed class TeamPassingAI : MonoBehaviour
    {
        [Header("Match references")]
        [SerializeField] private MatchBall ball;
        [SerializeField] private Transform[] teammates;
        [SerializeField] private Transform[] opponents;
        [SerializeField] private bool attackPositiveX = true;
        [SerializeField] private bool passingEnabled = true;

        [Header("Pass selection")]
        [SerializeField, Min(0.2f)] private float controlRange = 1.5f;
        [SerializeField, Min(1f)] private float minimumPassDistance = 3f;
        [SerializeField, Min(1f)] private float maximumPassDistance = 24f;
        [SerializeField, Min(0f)] private float forwardProgressWeight = 0.35f;
        [SerializeField, Min(0f)] private float spaceWeight = 1.2f;
        [SerializeField, Min(0f)] private float laneRiskWeight = 2f;
        [SerializeField, Min(0.1f)] private float passImpulse = 6.5f;
        [SerializeField, Min(0f)] private float lift = 0.12f;
        [SerializeField, Min(0f)] private float decisionInterval = 1.1f;

        private float nextDecisionTime;

        private void OnValidate()
        {
            controlRange = Mathf.Max(0.2f, controlRange);
            minimumPassDistance = Mathf.Max(1f, minimumPassDistance);
            maximumPassDistance = Mathf.Max(minimumPassDistance, maximumPassDistance);
            forwardProgressWeight = Mathf.Max(0f, forwardProgressWeight);
            spaceWeight = Mathf.Max(0f, spaceWeight);
            laneRiskWeight = Mathf.Max(0f, laneRiskWeight);
            passImpulse = Mathf.Max(0.1f, passImpulse);
            lift = Mathf.Max(0f, lift);
            decisionInterval = Mathf.Max(0f, decisionInterval);
        }

        public void SetBall(MatchBall matchBall) => ball = matchBall;
        public void SetTeammates(Transform[] players) => teammates = players;
        public void SetOpponents(Transform[] players) => opponents = players;
        public void SetPassingEnabled(bool enabled) => passingEnabled = enabled;

        private void Update()
        {
            if (!passingEnabled || ball == null || Time.time < nextDecisionTime) return;
            if (Vector3.Distance(transform.position, ball.transform.position) > controlRange)
                return;

            Transform bestReceiver = FindBestReceiver();
            if (bestReceiver == null) return;

            Vector3 origin = ball.transform.position;
            Vector3 target = bestReceiver.position;
            Vector3 direction = target - origin;
            direction.y = Mathf.Max(direction.y, lift);
            if (direction.sqrMagnitude < 0.001f) return;

            ball.Kick(direction.normalized * passImpulse);
            nextDecisionTime = Time.time + decisionInterval;
        }

        private Transform FindBestReceiver()
        {
            if (teammates == null || teammates.Length == 0) return null;

            Transform best = null;
            float bestScore = float.NegativeInfinity;
            Vector3 origin = ball.transform.position;
            float attackDirection = attackPositiveX ? 1f : -1f;

            for (int i = 0; i < teammates.Length; i++)
            {
                Transform candidate = teammates[i];
                if (candidate == null || candidate == transform) continue;

                Vector3 offset = candidate.position - origin;
                offset.y = 0f;
                float distance = offset.magnitude;
                if (distance < minimumPassDistance || distance > maximumPassDistance)
                    continue;

                float forwardProgress = Mathf.Clamp(
                    offset.x * attackDirection / maximumPassDistance, -1f, 1f);
                float nearestOpponent = GetNearestOpponentDistance(candidate.position);
                float space = Mathf.Clamp01(nearestOpponent / 8f);
                float laneRisk = GetPassLaneRisk(origin, candidate.position);

                float score = space * spaceWeight
                    + forwardProgress * forwardProgressWeight
                    - laneRisk * laneRiskWeight
                    - distance / maximumPassDistance * 0.15f;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        private float GetNearestOpponentDistance(Vector3 position)
        {
            if (opponents == null || opponents.Length == 0) return 8f;

            float nearest = float.PositiveInfinity;
            for (int i = 0; i < opponents.Length; i++)
            {
                if (opponents[i] == null) continue;
                Vector3 delta = opponents[i].position - position;
                delta.y = 0f;
                nearest = Mathf.Min(nearest, delta.magnitude);
            }

            return float.IsPositiveInfinity(nearest) ? 8f : nearest;
        }

        private float GetPassLaneRisk(Vector3 start, Vector3 end)
        {
            if (opponents == null || opponents.Length == 0) return 0f;

            Vector3 lane = end - start;
            lane.y = 0f;
            float laneLength = lane.magnitude;
            if (laneLength < 0.01f) return 1f;
            Vector3 direction = lane / laneLength;
            float risk = 0f;

            for (int i = 0; i < opponents.Length; i++)
            {
                Transform opponent = opponents[i];
                if (opponent == null) continue;

                Vector3 toOpponent = opponent.position - start;
                toOpponent.y = 0f;
                float along = Vector3.Dot(toOpponent, direction);
                if (along <= 0.5f || along >= laneLength - 0.5f) continue;

                float lateral = Vector3.Distance(
                    start + direction * along, opponent.position);
                float exposure = 1f - Mathf.Clamp01(lateral / 3f);
                risk = Mathf.Max(risk, exposure);
            }

            return risk;
        }
    }
}
