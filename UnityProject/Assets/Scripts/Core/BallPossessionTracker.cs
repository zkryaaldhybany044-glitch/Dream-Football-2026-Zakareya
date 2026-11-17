using System;
using UnityEngine;
using Zakareya.DreamFootball.Ball;

namespace Zakareya.DreamFootball.Core
{
    /// <summary>
    /// Lightweight proximity-based possession estimate. Assign every outfield
    /// player and goalkeeper with a PlayerTeamMember component. This is not
    /// authoritative ball ownership and should be tuned against real gameplay.
    /// </summary>
    public sealed class BallPossessionTracker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MatchBall ball;
        [SerializeField] private PlayerTeamMember[] players;

        [Header("Detection")]
        [SerializeField, Min(0.1f)] private float claimDistance = 1.35f;
        [SerializeField, Min(0.1f)] private float releaseDistance = 2.0f;
        [SerializeField, Min(0f)] private float maximumClaimSpeed = 2.2f;
        [SerializeField, Min(0f)] private float releaseSpeed = 3.5f;
        [SerializeField, Min(0.02f)] private float scanInterval = 0.12f;

        private float nextScanTime;
        private PlayerTeamMember currentOwner;

        public PlayerTeamMember CurrentOwner => currentOwner;
        public int CurrentTeamId => currentOwner != null ? currentOwner.TeamId : -1;
        public bool HasPossession => currentOwner != null;

        public event Action<PlayerTeamMember> PossessionChanged;

        private void OnValidate()
        {
            claimDistance = Mathf.Max(0.1f, claimDistance);
            releaseDistance = Mathf.Max(claimDistance, releaseDistance);
            maximumClaimSpeed = Mathf.Max(0f, maximumClaimSpeed);
            releaseSpeed = Mathf.Max(maximumClaimSpeed, releaseSpeed);
            scanInterval = Mathf.Max(0.02f, scanInterval);
        }

        private void Update()
        {
            if (ball == null || Time.time < nextScanTime) return;
            nextScanTime = Time.time + scanInterval;

            float ballSpeed = ball.Velocity.magnitude;
            if (currentOwner != null)
            {
                if (!currentOwner.gameObject.activeInHierarchy ||
                    Vector3.Distance(currentOwner.transform.position, ball.transform.position) > releaseDistance ||
                    ballSpeed > releaseSpeed)
                {
                    SetOwner(null);
                }
                else
                {
                    return;
                }
            }

            if (ballSpeed > maximumClaimSpeed || players == null) return;

            PlayerTeamMember nearest = null;
            float nearestDistance = claimDistance;
            for (int i = 0; i < players.Length; i++)
            {
                PlayerTeamMember candidate = players[i];
                if (candidate == null || !candidate.gameObject.activeInHierarchy) continue;

                float distance = Vector3.Distance(candidate.transform.position, ball.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = candidate;
                }
            }

            if (nearest != null) SetOwner(nearest);
        }

        public void ForceRelease()
        {
            SetOwner(null);
        }

        private void SetOwner(PlayerTeamMember owner)
        {
            if (currentOwner == owner) return;
            currentOwner = owner;
            PossessionChanged?.Invoke(currentOwner);
        }
    }
}
