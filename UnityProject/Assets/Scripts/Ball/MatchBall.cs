using UnityEngine;

namespace Zakareya.DreamFootball.Ball
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class MatchBall : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float radius = 0.11f;
        [SerializeField, Min(0f)] private float linearDamping = 0.08f;
        [SerializeField, Min(0f)] private float angularDamping = 0.05f;
        [SerializeField, Min(0f)] private float maxSpeed = 28f;

        private Rigidbody body;

        private void OnValidate()
        {
            radius = Mathf.Max(0.01f, radius);
            linearDamping = Mathf.Max(0f, linearDamping);
            angularDamping = Mathf.Max(0f, angularDamping);
            maxSpeed = Mathf.Max(0f, maxSpeed);
        }

        public Rigidbody Body => body;
        public Vector3 Velocity => body != null ? body.velocity : Vector3.zero;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            SphereCollider sphere = GetComponent<SphereCollider>();
            sphere.radius = radius;
            body.mass = 0.43f;
            body.drag = linearDamping;
            body.angularDrag = angularDamping;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void FixedUpdate()
        {
            if (body.velocity.sqrMagnitude > maxSpeed * maxSpeed)
                body.velocity = body.velocity.normalized * maxSpeed;
        }

        public void Kick(Vector3 impulse)
        {
            body.AddForce(impulse, ForceMode.Impulse);
        }

        public void ResetBall(Vector3 worldPosition)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = worldPosition;
        }
    }
}
