using UnityEngine;
using Zakareya.DreamFootball.Ball;

namespace Zakareya.DreamFootball.Players
{
    /// <summary>
    /// Lightweight offline tactical positioning for one AI outfield player.
    /// Place a home anchor at the player's formation position. This component
    /// moves only its own transform and should not be combined with a motor
    /// that also writes to the same transform.
    /// </summary>
    public sealed class TeamPositioningAI : MonoBehaviour
    {
        public enum PlayerRole { Defender, Midfielder, Forward }

        [Header("Match references")]
        [SerializeField] private MatchBall ball;
        [SerializeField] private Transform homeAnchor;
        [SerializeField] private bool attackPositiveX = true;
        [SerializeField] private PlayerRole role = PlayerRole.Midfielder;

        [Header("Tactical shape")]
        [SerializeField, Min(0f)] private float ballInfluence = 0.3f;
        [SerializeField, Min(0f)] private float lateralShift = 0.18f;
        [SerializeField, Min(0.1f)] private float pitchHalfLength = 52.5f;
        [SerializeField, Min(0.1f)] private float pitchHalfWidth = 34f;

        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float moveSpeed = 4.2f;
        [SerializeField, Min(0.1f)] private float turnSpeed = 8f;
        [SerializeField, Min(0.01f)] private float positionTolerance = 0.15f;

        private Vector3 fallbackHomePosition;

        private void Awake()
        {
            fallbackHomePosition = transform.position;
        }

        private void OnValidate()
        {
            ballInfluence = Mathf.Clamp01(ballInfluence);
            lateralShift = Mathf.Clamp01(lateralShift);
            pitchHalfLength = Mathf.Max(0.1f, pitchHalfLength);
            pitchHalfWidth = Mathf.Max(0.1f, pitchHalfWidth);
            moveSpeed = Mathf.Max(0.1f, moveSpeed);
            turnSpeed = Mathf.Max(0.1f, turnSpeed);
            positionTolerance = Mathf.Max(0.01f, positionTolerance);
        }

        public void SetBall(MatchBall matchBall) => ball = matchBall;
        public void SetHomeAnchor(Transform anchor) => homeAnchor = anchor;
        public void SetRole(PlayerRole newRole) => role = newRole;
        public void SetAttackPositiveX(bool positiveX) => attackPositiveX = positiveX;

        private void Update()
        {
            if (ball == null) return;

            Vector3 home = homeAnchor != null ? homeAnchor.position : fallbackHomePosition;
            Vector3 ballPosition = ball.transform.position;
            float direction = attackPositiveX ? 1f : -1f;

            float roleInfluence = role == PlayerRole.Defender ? 0.16f :
                role == PlayerRole.Forward ? 0.42f : 0.30f;

            // Move the formation block toward the ball while retaining its shape.
            Vector3 target = home;
            target.x += (ballPosition.x - home.x) * ballInfluence * roleInfluence;
            target.z += (ballPosition.z - home.z) * ballInfluence * roleInfluence * lateralShift;

            // Forwards stay ahead of the ball more often; defenders avoid being
            // pulled too far beyond their formation line.
            float attackOffset = role == PlayerRole.Forward ? 1.4f :
                role == PlayerRole.Defender ? -1.0f : 0f;
            target.x += direction * attackOffset;

            target.x = Mathf.Clamp(target.x, -pitchHalfLength, pitchHalfLength);
            target.z = Mathf.Clamp(target.z, -pitchHalfWidth, pitchHalfWidth);
            target.y = home.y;

            Vector3 flatTarget = new Vector3(target.x, transform.position.y, target.z);
            if ((flatTarget - transform.position).sqrMagnitude > positionTolerance * positionTolerance)
                transform.position = Vector3.MoveTowards(
                    transform.position, flatTarget, moveSpeed * Time.deltaTime);

            Vector3 look = ballPosition - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.01f)
            {
                Quaternion desired = Quaternion.LookRotation(look.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, desired, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
            }
        }
    }
}
