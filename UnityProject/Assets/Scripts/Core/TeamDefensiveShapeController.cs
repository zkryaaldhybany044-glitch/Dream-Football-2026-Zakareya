using System;
using UnityEngine;
using Zakareya.DreamFootball.Ball;
using Zakareya.DreamFootball.Players;

namespace Zakareya.DreamFootball.Core
{
    /// <summary>
    /// Applies a shared, bounded shift to a team's formation anchors while the
    /// opponent has estimated possession. It never moves player transforms.
    /// </summary>
    public sealed class TeamDefensiveShapeController : MonoBehaviour
    {
        [Serializable]
        private sealed class PlayerEntry
        {
            public PlayerTeamMember identity;
            public TeamPositioningAI positioning;
        }

        [Header("Team references")]
        [SerializeField, Min(0)] private int teamId;
        [SerializeField] private BallPossessionTracker possessionTracker;
        [SerializeField] private MatchBall ball;
        [SerializeField] private PlayerEntry[] players;

        [Header("Defensive compactness")]
        [SerializeField, Range(0f, 0.35f)] private float lengthShift = 0.12f;
        [SerializeField, Range(0f, 0.35f)] private float widthShift = 0.18f;
        [SerializeField, Min(0.05f)] private float refreshInterval = 0.15f;
        [SerializeField, Min(1f)] private float pitchHalfLength = 52.5f;
        [SerializeField, Min(1f)] private float pitchHalfWidth = 34f;

        private float nextRefreshTime;
        private Vector3 appliedOffset;

        private void OnValidate()
        {
            teamId = Mathf.Max(0, teamId);
            refreshInterval = Mathf.Max(0.05f, refreshInterval);
            pitchHalfLength = Mathf.Max(1f, pitchHalfLength);
            pitchHalfWidth = Mathf.Max(1f, pitchHalfWidth);
        }

        private void Update()
        {
            if (Time.time < nextRefreshTime) return;
            nextRefreshTime = Time.time + refreshInterval;

            bool defending = possessionTracker != null &&
                possessionTracker.CurrentOwner != null &&
                possessionTracker.CurrentOwner.TeamId != teamId;

            Vector3 desired = Vector3.zero;
            if (defending && ball != null)
            {
                Vector3 ballPosition = ball.transform.position;
                desired.x = Mathf.Clamp(ballPosition.x, -pitchHalfLength, pitchHalfLength) * lengthShift;
                desired.z = Mathf.Clamp(ballPosition.z, -pitchHalfWidth, pitchHalfWidth) * widthShift;
            }

            if ((desired - appliedOffset).sqrMagnitude < 0.0001f) return;
            appliedOffset = desired;

            if (players == null) return;
            for (int i = 0; i < players.Length; i++)
            {
                PlayerEntry entry = players[i];
                if (entry == null || entry.identity == null ||
                    entry.positioning == null || entry.identity.TeamId != teamId)
                    continue;

                entry.positioning.SetTacticalHomeOffset(appliedOffset);
            }
        }

        private void OnDisable()
        {
            appliedOffset = Vector3.zero;
            if (players == null) return;
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null && players[i].positioning != null)
                    players[i].positioning.SetTacticalHomeOffset(Vector3.zero);
            }
        }
    }
}
