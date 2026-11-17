using System;
using UnityEngine;
using Zakareya.DreamFootball.Players;

namespace Zakareya.DreamFootball.Core
{
    /// <summary>
    /// Builds formation anchors for one team's ten outfield players.
    /// The controller transform should be placed at pitch center.
    /// </summary>
    public sealed class TeamFormationController : MonoBehaviour
    {
        public enum FormationPreset
        {
            FourThreeThree,
            FourFourTwo,
            ThreeFiveTwo
        }

        [Serializable]
        private sealed class PlayerAssignment
        {
            public PlayerTeamMember identity;
            public TeamPositioningAI positioning;
            [Range(0, 9)] public int slotIndex;
            [NonSerialized] public Transform runtimeAnchor;
        }

        private struct FormationSlot
        {
            public float progress;
            public float lateral;
            public TeamPositioningAI.PlayerRole role;

            public FormationSlot(float progress, float lateral, TeamPositioningAI.PlayerRole role)
            {
                this.progress = progress;
                this.lateral = lateral;
                this.role = role;
            }
        }

        [Header("Team")]
        [SerializeField, Min(0)] private int teamId;
        [SerializeField] private bool attackPositiveX = true;
        [SerializeField] private FormationPreset formation = FormationPreset.FourThreeThree;
        [SerializeField] private PlayerAssignment[] players;

        [Header("Pitch dimensions")]
        [SerializeField, Min(1f)] private float pitchHalfLength = 52.5f;
        [SerializeField, Min(1f)] private float pitchHalfWidth = 34f;

        private void OnValidate()
        {
            teamId = Mathf.Max(0, teamId);
            pitchHalfLength = Mathf.Max(1f, pitchHalfLength);
            pitchHalfWidth = Mathf.Max(1f, pitchHalfWidth);
        }

        private void Start()
        {
            ApplyFormation();
        }

        /// <summary>Change preset at runtime and immediately reposition formation anchors.</summary>
        public void SetFormation(FormationPreset preset)
        {
            formation = preset;
            ApplyFormation();
        }

        public void SetAttackDirection(bool positiveX)
        {
            attackPositiveX = positiveX;
            ApplyFormation();
        }

        public void ApplyFormation()
        {
            FormationSlot[] slots = GetSlots(formation);
            if (players == null || players.Length == 0)
            {
                Debug.LogWarning("TeamFormationController has no player assignments.", this);
                return;
            }

            bool[] occupiedSlots = new bool[slots.Length];

            for (int i = 0; i < players.Length; i++)
            {
                PlayerAssignment assignment = players[i];
                if (assignment == null || assignment.identity == null ||
                    assignment.positioning == null)
                    continue;

                if (assignment.identity.TeamId != teamId)
                {
                    Debug.LogWarning(
                        $"Formation assignment skipped: player team ID {assignment.identity.TeamId} " +
                        $"does not match controller team ID {teamId}.", assignment.identity);
                    continue;
                }

                int slotIndex = assignment.slotIndex;
                if (slotIndex < 0 || slotIndex >= slots.Length)
                {
                    Debug.LogWarning($"Invalid formation slot index {slotIndex}.", this);
                    continue;
                }

                if (occupiedSlots[slotIndex])
                {
                    Debug.LogWarning($"Formation slot {slotIndex} is assigned more than once.", this);
                    continue;
                }

                occupiedSlots[slotIndex] = true;
                FormationSlot slot = slots[slotIndex];
                EnsureAnchor(assignment, slotIndex);

                float x = -pitchHalfLength + slot.progress * pitchHalfLength * 2f;
                if (!attackPositiveX) x = -x;

                float lateral = attackPositiveX ? slot.lateral : -slot.lateral;
                assignment.runtimeAnchor.localPosition = new Vector3(
                    x, 0f, lateral * pitchHalfWidth);

                assignment.positioning.SetHomeAnchor(assignment.runtimeAnchor);
                assignment.positioning.SetRole(slot.role);
                assignment.positioning.SetAttackPositiveX(attackPositiveX);
            }
        }

        private void EnsureAnchor(PlayerAssignment assignment, int slotIndex)
        {
            if (assignment.runtimeAnchor != null) return;

            GameObject anchor = new GameObject($"FormationAnchor_T{teamId}_Slot{slotIndex}");
            anchor.transform.SetParent(transform, false);
            assignment.runtimeAnchor = anchor.transform;
        }

        private static FormationSlot[] GetSlots(FormationPreset preset)
        {
            TeamPositioningAI.PlayerRole defender = TeamPositioningAI.PlayerRole.Defender;
            TeamPositioningAI.PlayerRole midfielder = TeamPositioningAI.PlayerRole.Midfielder;
            TeamPositioningAI.PlayerRole forward = TeamPositioningAI.PlayerRole.Forward;

            switch (preset)
            {
                case FormationPreset.FourFourTwo:
                    return new[]
                    {
                        new FormationSlot(0.20f, -0.78f, defender),
                        new FormationSlot(0.15f, -0.28f, defender),
                        new FormationSlot(0.15f,  0.28f, defender),
                        new FormationSlot(0.20f,  0.78f, defender),
                        new FormationSlot(0.48f, -0.82f, midfielder),
                        new FormationSlot(0.43f, -0.28f, midfielder),
                        new FormationSlot(0.43f,  0.28f, midfielder),
                        new FormationSlot(0.48f,  0.82f, midfielder),
                        new FormationSlot(0.82f, -0.28f, forward),
                        new FormationSlot(0.82f,  0.28f, forward)
                    };

                case FormationPreset.ThreeFiveTwo:
                    return new[]
                    {
                        new FormationSlot(0.17f, -0.58f, defender),
                        new FormationSlot(0.13f,  0.00f, defender),
                        new FormationSlot(0.17f,  0.58f, defender),
                        new FormationSlot(0.48f, -0.90f, midfielder),
                        new FormationSlot(0.42f, -0.45f, midfielder),
                        new FormationSlot(0.40f,  0.00f, midfielder),
                        new FormationSlot(0.42f,  0.45f, midfielder),
                        new FormationSlot(0.48f,  0.90f, midfielder),
                        new FormationSlot(0.82f, -0.28f, forward),
                        new FormationSlot(0.82f,  0.28f, forward)
                    };

                default: // 4-3-3
                    return new[]
                    {
                        new FormationSlot(0.20f, -0.78f, defender),
                        new FormationSlot(0.15f, -0.28f, defender),
                        new FormationSlot(0.15f,  0.28f, defender),
                        new FormationSlot(0.20f,  0.78f, defender),
                        new FormationSlot(0.47f, -0.52f, midfielder),
                        new FormationSlot(0.40f,  0.00f, midfielder),
                        new FormationSlot(0.47f,  0.52f, midfielder),
                        new FormationSlot(0.76f, -0.82f, forward),
                        new FormationSlot(0.87f,  0.00f, forward),
                        new FormationSlot(0.76f,  0.82f, forward)
                    };
            }
        }

        private void OnDestroy()
        {
            if (players == null) return;

            for (int i = 0; i < players.Length; i++)
            {
                PlayerAssignment assignment = players[i];
                if (assignment == null || assignment.runtimeAnchor == null) continue;

                if (Application.isPlaying)
                    Destroy(assignment.runtimeAnchor.gameObject);
                else
                    DestroyImmediate(assignment.runtimeAnchor.gameObject);
            }
        }
    }
}
