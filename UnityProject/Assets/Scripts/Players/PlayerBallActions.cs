using UnityEngine;
using Zakareya.DreamFootball.Ball;

namespace Zakareya.DreamFootball.Players
{
    public sealed class PlayerBallActions : MonoBehaviour
    {
        [Header("Ball interaction")]
        [SerializeField] private MatchBall ball;
        [SerializeField] private Transform kickOrigin;
        [SerializeField, Min(0.2f)] private float controlRange = 1.45f;
        [SerializeField, Min(0f)] private float passImpulse = 5.5f;
        [SerializeField, Min(0f)] private float shotImpulse = 10.5f;
        [SerializeField, Min(0f)] private float lift = 0.12f;
        [SerializeField, Min(0f)] private float actionCooldown = 0.28f;

        private float nextActionTime;

        public bool CanPlayBall =>
            ball != null && Vector3.Distance(transform.position, ball.transform.position) <= controlRange;

        public void Pass(Vector3 target)
        {
            PlayBall(target, passImpulse);
        }

        public void Shoot(Vector3 target)
        {
            PlayBall(target, shotImpulse);
        }

        private void PlayBall(Vector3 target, float impulse)
        {
            if (!CanPlayBall || Time.time < nextActionTime) return;

            Vector3 origin = kickOrigin != null ? kickOrigin.position : transform.position + Vector3.up * 0.25f;
            Vector3 direction = target - origin;
            direction.y = Mathf.Max(direction.y, lift);
            if (direction.sqrMagnitude < 0.001f) direction = transform.forward;

            ball.Kick(direction.normalized * impulse);
            nextActionTime = Time.time + actionCooldown;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, controlRange);
        }
    }
}
