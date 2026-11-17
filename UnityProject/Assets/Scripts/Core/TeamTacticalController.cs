using System;
using UnityEngine;
using Zakareya.DreamFootball.Ball;
using Zakareya.DreamFootball.Players;

namespace Zakareya.DreamFootball.Core
{
    /// <summary>
    /// Connects estimated ball possession to basic team behavior:
    /// only the current AI owner may auto-pass, and the nearest configured
    /// opponent becomes the designated presser/tackler.
    /// </summary>
    public sealed class TeamTacticalController : MonoBehaviour
    {
        [Serializable]
        private sealed class PlayerUnit
        {
            public PlayerTeamMember identity;
            public TeamPositioningAI positioning;
            public TeamPassingAI passing;
            public TeamPressingAI pressing;
            public TackleAction tackling;
        }

        [Header("Match references")]
        [SerializeField] private BallPossessionTracker possessionTracker;
        [SerializeField] private MatchBall ball;
        [SerializeField] private PlayerUnit[] players;

        [Header("Tactical timing")]
        [SerializeField, Min(0f)] private float looseBallGracePeriod = 0.8f;
        [SerializeField, Min(0.05f)] private float refreshInterval = 0.2f;

        private int lastPossessingTeamId = -1;
        private float lastPossessionTime = -1000f;
        private int appliedTeamId = int.MinValue;
        private PlayerTeamMember appliedPresser;
        private PlayerTeamMember appliedOwner;
        private float nextRefreshTime;

        private void OnValidate()
        {
            looseBallGracePeriod = Mathf.Max(0f, looseBallGracePeriod);
            refreshInterval = Mathf.Max(0.05f, refreshInterval);
        }

        private void OnEnable()
        {
            if (possessionTracker == null)
                possessionTracker = GetComponent<BallPossessionTracker>();

            if (possessionTracker != null)
                possessionTracker.PossessionChanged += HandlePossessionChanged;
        }

        private void Start()
        {
            RefreshTactics(true);
        }

        private void OnDisable()
        {
            if (possessionTracker != null)
                possessionTracker.PossessionChanged -= HandlePossessionChanged;

            DisableAllBehaviors();
        }

        private void Update()
        {
            if (Time.time < nextRefreshTime) return;
            nextRefreshTime = Time.time + refreshInterval;
            RefreshTactics(false);
        }

        private void HandlePossessionChanged(PlayerTeamMember owner)
        {
            if (owner != null)
            {
                lastPossessingTeamId = owner.TeamId;
                lastPossessionTime = Time.time;
            }

            nextRefreshTime = 0f;
        }

        private int GetEffectivePossessionTeam()
        {
            if (possessionTracker == null) return -1;

            PlayerTeamMember owner = possessionTracker.CurrentOwner;
            if (owner != null)
            {
                lastPossessingTeamId = owner.TeamId;
                lastPossessionTime = Time.time;
                return owner.TeamId;
            }

            bool graceActive = lastPossessingTeamId >= 0 &&
                Time.time - lastPossessionTime <= looseBallGracePeriod;

            return graceActive ? lastPossessingTeamId : -1;
        }

        private void RefreshTactics(bool force)
        {
            if (players == null || possessionTracker == null || ball == null)
            {
                DisableAllBehaviors();
                return;
            }

            int possessingTeamId = GetEffectivePossessionTeam();
            PlayerTeamMember presser = possessingTeamId >= 0
                ? FindNearestDefender(possessingTeamId)
                : null;
            PlayerTeamMember currentOwner = possessionTracker.CurrentOwner;

            if (!force && possessingTeamId == appliedTeamId &&
                presser == appliedPresser && currentOwner == appliedOwner)
                return;

            for (int i = 0; i < players.Length; i++)
            {
                PlayerUnit unit = players[i];
                if (unit == null) continue;

                bool isDesignatedPresser = unit.identity != null && unit.identity == presser;
                bool isCurrentOwner = currentOwner != null &&
                    unit.identity != null && unit.identity == currentOwner;

                if (unit.passing != null)
                    unit.passing.SetPassingEnabled(isCurrentOwner);

                bool pressingOwnsMovement = isDesignatedPresser && unit.pressing != null;

                if (unit.positioning != null)
                    unit.positioning.SetPositioningEnabled(!pressingOwnsMovement);

                if (unit.pressing != null)
                {
                    unit.pressing.SetExternallyManaged(true);
                    unit.pressing.SetPressingEnabled(isDesignatedPresser);
                }

                if (unit.tackling != null)
                    unit.tackling.SetAutomaticTackling(isDesignatedPresser);
            }

            appliedTeamId = possessingTeamId;
            appliedPresser = presser;
            appliedOwner = currentOwner;
        }

        private PlayerTeamMember FindNearestDefender(int possessingTeamId)
        {
            PlayerTeamMember nearest = null;
            float nearestSqrDistance = float.PositiveInfinity;
            Vector3 ballPosition = ball.transform.position;

            for (int i = 0; i < players.Length; i++)
            {
                PlayerUnit unit = players[i];
                if (unit == null || unit.identity == null ||
                    !unit.identity.gameObject.activeInHierarchy ||
                    unit.identity.TeamId == possessingTeamId ||
                    (unit.pressing == null && unit.tackling == null))
                    continue;

                Vector3 offset = unit.identity.transform.position - ballPosition;
                offset.y = 0f;
                float sqrDistance = offset.sqrMagnitude;

                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearest = unit.identity;
                }
            }

            return nearest;
        }

        private void DisableAllBehaviors()
        {
            if (players == null) return;

            for (int i = 0; i < players.Length; i++)
            {
                PlayerUnit unit = players[i];
                if (unit == null) continue;

                if (unit.passing != null) unit.passing.SetPassingEnabled(false);
                if (unit.positioning != null) unit.positioning.SetPositioningEnabled(true);
                if (unit.pressing != null)
                {
                    unit.pressing.SetExternallyManaged(true);
                    unit.pressing.SetPressingEnabled(false);
                }
                if (unit.tackling != null) unit.tackling.SetAutomaticTackling(false);
            }

            appliedTeamId = -1;
            appliedPresser = null;
            appliedOwner = null;
        }
    }
}
