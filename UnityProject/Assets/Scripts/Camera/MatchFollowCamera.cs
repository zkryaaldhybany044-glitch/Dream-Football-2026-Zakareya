using UnityEngine;

namespace Zakareya.DreamFootball.CameraSystem
{
    /// <summary>
    /// Smooth third-person match camera. Assign the controlled footballer and the ball
    /// in the Inspector; the ball gently shifts the camera's look target during play.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MatchFollowCamera : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform ball;

        [Header("Framing")]
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 8f, -10f);
        [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1.2f, 0f);
        [SerializeField, Range(0f, 1f)] private float ballLookWeight = 0.25f;

        [Header("Smoothing")]
        [SerializeField, Min(0.01f)] private float positionSmoothTime = 0.18f;
        [SerializeField, Min(0.01f)] private float rotationSharpness = 8f;

        private Vector3 positionVelocity;

        public void SetTargets(Transform playerTarget, Transform ballTarget)
        {
            player = playerTarget;
            ball = ballTarget;
        }

        private void LateUpdate()
        {
            if (player == null) return;

            Vector3 desiredPosition = player.position + player.rotation * followOffset;
            transform.position = Vector3.SmoothDamp(
                transform.position, desiredPosition, ref positionVelocity, positionSmoothTime);

            Vector3 lookTarget = player.position + lookOffset;
            if (ball != null)
                lookTarget = Vector3.Lerp(lookTarget, ball.position, ballLookWeight);

            Vector3 direction = lookTarget - transform.position;
            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion desiredRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            float blend = 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, blend);
        }
    }
}