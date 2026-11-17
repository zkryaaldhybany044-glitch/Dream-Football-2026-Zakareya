using UnityEngine;
using Zakareya.DreamFootball.Ball;

namespace Zakareya.DreamFootball.Players
{
    /// <summary>
    /// Optional pressure behavior for a designated AI player. Enable it only
    /// while the team is defending. This component owns its Transform movement;
    /// do not run it alongside TeamPositioningAI or another movement motor.
    /// </summary>
    public sealed class TeamPressingAI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MatchBall ball;
        [SerializeField] private Transform homeAnchor;

        [Header("Press behavior")]
        [SerializeField] private bool pressingEnabled;
        [SerializeField, Min(0.1f)] private float engageDistance = 18f;
        [SerializeField, Min(0.1f)] private float releaseDistance = 23f;
        [SerializeField, Min(0f)] private float standOffDistance = 1.8f;
        [SerializeField, Min(0.1f)] private float pressSpeed = 4.8f;
        [SerializeField, Min(0.1f)] private float returnSpeed = 3.8f;
        [SerializeField, Min(0.1f)] private float turnSpeed = 9f;
        [SerializeField, Min(0.1f)] private float pitchHalfLength = 52.5f;
        [SerializeField, Min(0.1f)] private float pitchHalfWidth = 34f;

        private bool hasEngaged;
        private bool externallyManaged;
        private Vector3 fallbackHome;

        private void Awake()
        {
            fallbackHome = transform.position;
        }

        private void OnValidate()
        {
            engageDistance = Mathf.Max(0.1f, engageDistance);
            releaseDistance = Mathf.Max(engageDistance, releaseDistance);
            standOffDistance = Mathf.Max(0f, standOffDistance);
            pressSpeed = Mathf.Max(0.1f, pressSpeed);
            returnSpeed = Mathf.Max(0.1f, returnSpeed);
            turnSpeed = Mathf.Max(0.1f, turnSpeed);
            pitchHalfLength = Mathf.Max(0.1f, pitchHalfLength);
            pitchHalfWidth = Mathf.Max(0.1f, pitchHalfWidth);
        }

        public void SetBall(MatchBall matchBall) => ball = matchBall;

        /// <summary>When externally managed, this component stops moving while disabled so another positioning motor can take over.</summary>
        public void SetExternallyManaged(bool managed) => externallyManaged = managed;

        /// <summary>Call from the match/team state when this team is defending.</summary>
        public void SetPressingEnabled(bool enabled)
        {
            pressingEnabled = enabled;
            if (!enabled) hasEngaged = false;
        }

        private void Update()
        {
            if (ball == null || (!pressingEnabled && externallyManaged)) return;

            Vector3 home = homeAnchor != null ? homeAnchor.position : fallbackHome;
            Vector3 position = transform.position;
            Vector3 ballPosition = ball.transform.position;
            Vector3 flatToBall = ballPosition - position;
            flatToBall.y = 0f;
            float distance = flatToBall.magnitude;

            if (pressingEnabled)
            {
                if (!hasEngaged && distance <= engageDistance) hasEngaged = true;
                else if (hasEngaged && distance >= releaseDistance) hasEngaged = false;
            }
            else
            {
                hasEngaged = false;
            }

            Vector3 target = home;
            float speed = returnSpeed;
            if (hasEngaged)
            {
                Vector3 approach = flatToBall.sqrMagnitude > 0.001f
                    ? flatToBall.normalized : transform.forward;
                target = ballPosition - approach * standOffDistance;
                speed = pressSpeed;
            }

            target.x = Mathf.Clamp(target.x, -pitchHalfLength, pitchHalfLength);
            target.z = Mathf.Clamp(target.z, -pitchHalfWidth, pitchHalfWidth);
            target.y = position.y;

            transform.position = Vector3.MoveTowards(
                position, target, speed * Time.deltaTime);

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
