using UnityEngine;

namespace Zakareya.DreamFootball.Players
{
    /// <summary>
    /// Basic goalkeeper positioning prototype. Tracks the ball laterally while
    /// staying on its goal line; this is not a complete save or dive system.
    /// </summary>
    public sealed class GoalkeeperMotor : MonoBehaviour
    {
        [SerializeField] private Transform ball;
        [SerializeField] private bool defendPositiveXGoal;
        [SerializeField, Min(0.1f)] private float lateralSpeed = 4.5f;
        [SerializeField, Min(0.1f)] private float reactionDistance = 28f;
        [SerializeField, Min(0.1f)] private float goalHalfWidth = 3.66f;
        [SerializeField, Min(0f)] private float goalLineOffset = 0.8f;

        private float goalLineX;

        private void Start()
        {
            goalLineX = transform.position.x +
                (defendPositiveXGoal ? -goalLineOffset : goalLineOffset);
        }

        public void SetBall(Transform ballTarget) => ball = ballTarget;

        private void Update()
        {
            if (ball == null) return;

            Vector3 current = transform.position;
            float targetZ = current.z;

            float distanceToGoal = Mathf.Abs(ball.position.x - goalLineX);
            if (distanceToGoal <= reactionDistance)
                targetZ = Mathf.Clamp(ball.position.z, -goalHalfWidth, goalHalfWidth);

            float nextZ = Mathf.MoveTowards(current.z, targetZ, lateralSpeed * Time.deltaTime);
            transform.position = new Vector3(goalLineX, current.y, nextZ);

            Vector3 lookDirection = ball.position - transform.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRotation, 1f - Mathf.Exp(-6f * Time.deltaTime));
            }
        }
    }
}