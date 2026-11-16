using UnityEngine;
using Zakareya.DreamFootball.Ball;

namespace Zakareya.DreamFootball.Players
{
    /// <summary>
    /// Lightweight dribble assistance: while the player moves near the ball,
    /// applies a capped planar force to keep the ball in front without parenting
    /// it to the player or bypassing Rigidbody collision.
    /// </summary>
    [RequireComponent(typeof(FootballerMotor))]
    public sealed class BallControlAssist : MonoBehaviour
    {
        [SerializeField] private MatchBall ball;
        [SerializeField, Min(0.2f)] private float controlRadius = 1.35f;
        [SerializeField, Min(0.1f)] private float desiredBallDistance = 0.85f;
        [SerializeField, Min(0f)] private float positionGain = 11f;
        [SerializeField, Min(0f)] private float velocityDamping = 3.5f;
        [SerializeField, Min(0f)] private float maximumAcceleration = 18f;
        [SerializeField, Min(0.1f)] private float maximumBallHeight = 0.55f;

        private FootballerMotor motor;
        private Rigidbody ballBody;

        private void Awake()
        {
            motor = GetComponent<FootballerMotor>();
            CacheBallBody();
        }

        private void OnValidate()
        {
            controlRadius = Mathf.Max(0.2f, controlRadius);
            desiredBallDistance = Mathf.Max(0.1f, desiredBallDistance);
            positionGain = Mathf.Max(0f, positionGain);
            velocityDamping = Mathf.Max(0f, velocityDamping);
            maximumAcceleration = Mathf.Max(0f, maximumAcceleration);
            maximumBallHeight = Mathf.Max(0.1f, maximumBallHeight);
        }

        private void FixedUpdate()
        {
            if (ball == null || motor == null || !motor.HasMoveInput) return;
            if (ballBody == null) CacheBallBody();
            if (ballBody == null || ballBody.isKinematic) return;

            Vector3 playerPosition = transform.position;
            Vector3 ballPosition = ball.transform.position;
            if (ballPosition.y - playerPosition.y > maximumBallHeight) return;

            Vector3 playerToBall = ballPosition - playerPosition;
            playerToBall.y = 0f;
            if (playerToBall.sqrMagnitude > controlRadius * controlRadius) return;

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.001f) return;

            Vector3 desiredPosition = playerPosition + forward * desiredBallDistance;
            Vector3 positionError = desiredPosition - ballPosition;
            positionError.y = 0f;

            Vector3 planarVelocity = ballBody.velocity;
            planarVelocity.y = 0f;

            Vector3 acceleration = positionError * positionGain - planarVelocity * velocityDamping;
            acceleration = Vector3.ClampMagnitude(acceleration, maximumAcceleration);
            ballBody.AddForce(acceleration, ForceMode.Acceleration);
        }

        public void SetBall(MatchBall target)
        {
            ball = target;
            CacheBallBody();
        }

        private void CacheBallBody()
        {
            ballBody = ball != null ? ball.Body : null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, controlRadius);
        }
    }
}
