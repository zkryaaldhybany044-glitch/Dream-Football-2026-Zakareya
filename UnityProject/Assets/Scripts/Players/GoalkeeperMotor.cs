using UnityEngine;
using Zakareya.DreamFootball.Ball;

namespace Zakareya.DreamFootball.Players
{
    /// <summary>
    /// Basic goalkeeper positioning prototype. Tracks the ball laterally and
    /// makes a short linear estimate of its crossing point when it is moving
    /// toward this goal. This is positioning only, not a save or dive system.
    /// </summary>
    public sealed class GoalkeeperMotor : MonoBehaviour
    {
        [SerializeField] private MatchBall ball;
        [SerializeField] private bool defendPositiveXGoal;
        [SerializeField, Min(0.1f)] private float lateralSpeed = 4.5f;
        [SerializeField, Min(0.1f)] private float reactionDistance = 28f;
        [SerializeField, Min(0.1f)] private float goalHalfWidth = 3.66f;
        [SerializeField, Min(0f)] private float goalLineOffset = 0.8f;
        [SerializeField, Min(0f)] private float predictionHorizon = 1.5f;
        [SerializeField, Min(0.01f)] private float minimumApproachSpeed = 0.5f;

        private float goalLineX;

        private void OnValidate()
        {
            lateralSpeed = Mathf.Max(0.1f, lateralSpeed);
            reactionDistance = Mathf.Max(0.1f, reactionDistance);
            goalHalfWidth = Mathf.Max(0.1f, goalHalfWidth);
            goalLineOffset = Mathf.Max(0f, goalLineOffset);
            predictionHorizon = Mathf.Max(0f, predictionHorizon);
            minimumApproachSpeed = Mathf.Max(0.01f, minimumApproachSpeed);
        }

        private void Start()
        {
            goalLineX = transform.position.x +
                (defendPositiveXGoal ? -goalLineOffset : goalLineOffset);
        }

        /// <summary>
        /// Allows a scene controller to assign the ball at runtime.
        /// The referenced object must contain a MatchBall component.
        /// </summary>
        public void SetBall(Transform ballTarget)
        {
            ball = ballTarget != null ? ballTarget.GetComponent<MatchBall>() : null;
        }

        private void Update()
        {
            if (ball == null) return;

            Vector3 ballPosition = ball.transform.position;
            Vector3 current = transform.position;
            float targetZ = current.z;

            float distanceToGoal = Mathf.Abs(ballPosition.x - goalLineX);
            if (distanceToGoal <= reactionDistance)
                targetZ = GetTargetLateralPosition(ballPosition, ball.Velocity);

            float nextZ = Mathf.MoveTowards(
                current.z, targetZ, lateralSpeed * Time.deltaTime);

            transform.position = new Vector3(goalLineX, current.y, nextZ);

            Vector3 lookDirection = ballPosition - transform.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    1f - Mathf.Exp(-6f * Time.deltaTime));
            }
        }

        private float GetTargetLateralPosition(Vector3 ballPosition, Vector3 ballVelocity)
        {
            float targetZ = ballPosition.z;
            bool movingTowardGoal = defendPositiveXGoal
                ? ballVelocity.x > minimumApproachSpeed
                : ballVelocity.x < -minimumApproachSpeed;

            if (movingTowardGoal)
            {
                float timeToGoal = (goalLineX - ballPosition.x) / ballVelocity.x;
                if (timeToGoal >= 0f && timeToGoal <= predictionHorizon)
                    targetZ += ballVelocity.z * timeToGoal;
            }

            return Mathf.Clamp(targetZ, -goalHalfWidth, goalHalfWidth);
        }
    }
}
