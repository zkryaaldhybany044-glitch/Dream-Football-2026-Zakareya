using UnityEngine;
using Zakareya.DreamFootball.Ball;

namespace Zakareya.DreamFootball.Players
{
    /// <summary>
    /// Close-range ball challenge shared by human and AI defenders. A valid
    /// tackle deflects the ball using Rigidbody physics; it does not transfer
    /// possession or play a tackle animation.
    /// </summary>
    public sealed class TackleAction : MonoBehaviour
    {
        [Header("Ball reference")]
        [SerializeField] private MatchBall ball;

        [Header("Tackle conditions")]
        [SerializeField, Min(0.1f)] private float tackleRange = 1.25f;
        [SerializeField, Min(0.1f)] private float maximumBallHeight = 0.65f;
        [SerializeField, Range(-1f, 1f)] private float minimumFacingDot = -0.25f;
        [SerializeField, Min(0f)] private float tackleCooldown = 1.0f;

        [Header("Ball response")]
        [SerializeField, Min(0f)] private float tackleImpulse = 3.0f;
        [SerializeField, Min(0f)] private float forwardBias = 0.35f;
        [SerializeField, Min(0f)] private float liftImpulse = 0.08f;

        [Header("Optional AI behavior")]
        [SerializeField] private bool automaticTackling;
        [SerializeField, Min(0.05f)] private float automaticAttemptInterval = 0.3f;

        private float nextTackleTime;
        private float nextAutomaticAttemptTime;

        private void OnValidate()
        {
            tackleRange = Mathf.Max(0.1f, tackleRange);
            maximumBallHeight = Mathf.Max(0.1f, maximumBallHeight);
            tackleCooldown = Mathf.Max(0f, tackleCooldown);
            tackleImpulse = Mathf.Max(0f, tackleImpulse);
            forwardBias = Mathf.Clamp01(forwardBias);
            liftImpulse = Mathf.Max(0f, liftImpulse);
            automaticAttemptInterval = Mathf.Max(0.05f, automaticAttemptInterval);
        }

        private void Update()
        {
            if (!automaticTackling || Time.time < nextAutomaticAttemptTime)
                return;

            nextAutomaticAttemptTime = Time.time + automaticAttemptInterval;
            TryTackle();
        }

        /// <summary>Attempts one close-range tackle. Returns true if the ball was deflected.</summary>
        public bool TryTackle()
        {
            if (ball == null || Time.time < nextTackleTime)
                return false;

            Rigidbody body = ball.Body;
            if (body == null || body.isKinematic)
                return false;

            Vector3 ballPosition = ball.transform.position;
            float ballHeight = ballPosition.y - transform.position.y;
            if (ballHeight > maximumBallHeight || ballHeight < -0.25f)
                return false;

            Vector3 toBall = ballPosition - transform.position;
            toBall.y = 0f;
            float distance = toBall.magnitude;
            if (distance > tackleRange || distance < 0.001f)
                return false;

            Vector3 toBallDirection = toBall / distance;
            Vector3 facing = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            if (facing.sqrMagnitude < 0.001f)
                return false;

            if (Vector3.Dot(facing, toBallDirection) < minimumFacingDot)
                return false;

            Vector3 clearanceDirection = Vector3.Slerp(
                toBallDirection, facing, forwardBias).normalized;

            body.AddForce(
                clearanceDirection * tackleImpulse + Vector3.up * liftImpulse,
                ForceMode.Impulse);

            nextTackleTime = Time.time + tackleCooldown;
            return true;
        }

        public void SetBall(MatchBall target) => ball = target;

        /// <summary>Enable this for a simple autonomous defender; disable it for manual touch control.</summary>
        public void SetAutomaticTackling(bool enabled)
        {
            automaticTackling = enabled;
            nextAutomaticAttemptTime = Time.time;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, tackleRange);
        }
    }
}
