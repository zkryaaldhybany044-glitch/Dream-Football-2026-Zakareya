using UnityEngine;
using Zakareya.DreamFootball.Ball;

namespace Zakareya.DreamFootball.Players
{
    /// <summary>
    /// Prototype goalkeeper contact volume. Deflects an incoming MatchBall
    /// away from the defended goal with a configurable lateral and vertical
    /// rebound. This is a physics response only; it has no keeper animation.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class GoalkeeperSaveZone : MonoBehaviour
    {
        [SerializeField] private bool defendPositiveXGoal;
        [SerializeField, Min(0.1f)] private float minimumIncomingSpeed = 0.75f;
        [SerializeField, Min(0f)] private float speedRetention = 0.8f;
        [SerializeField, Min(0.1f)] private float minimumReboundSpeed = 3f;
        [SerializeField, Min(0f)] private float lateralDeflection = 2.5f;
        [SerializeField, Min(0f)] private float verticalLift = 0.6f;
        [SerializeField, Min(0f)] private float cooldown = 0.25f;
        [SerializeField] private int fallbackDeflectionSide = 1;

        private float nextSaveTime;

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void OnValidate()
        {
            minimumIncomingSpeed = Mathf.Max(0.1f, minimumIncomingSpeed);
            speedRetention = Mathf.Max(0f, speedRetention);
            minimumReboundSpeed = Mathf.Max(0.1f, minimumReboundSpeed);
            lateralDeflection = Mathf.Max(0f, lateralDeflection);
            verticalLift = Mathf.Max(0f, verticalLift);
            cooldown = Mathf.Max(0f, cooldown);
            fallbackDeflectionSide = fallbackDeflectionSide < 0 ? -1 : 1;

            Collider trigger = GetComponent<Collider>();
            if (trigger != null && !trigger.isTrigger)
                trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            MatchBall ball = other.GetComponentInParent<MatchBall>();
            if (ball == null || ball.Body == null || Time.time < nextSaveTime)
                return;

            Vector3 incoming = ball.Velocity;
            float towardGoalSpeed = defendPositiveXGoal ? incoming.x : -incoming.x;
            if (towardGoalSpeed < minimumIncomingSpeed)
                return;

            float reboundSpeed = Mathf.Max(
                minimumReboundSpeed, towardGoalSpeed * speedRetention);

            float relativeZ = ball.transform.position.z - transform.position.z;
            int side = Mathf.Abs(relativeZ) > 0.15f
                ? (relativeZ > 0f ? 1 : -1)
                : fallbackDeflectionSide;

            float reboundX = defendPositiveXGoal ? -reboundSpeed : reboundSpeed;
            Vector3 reboundVelocity = new Vector3(
                reboundX,
                Mathf.Max(0f, incoming.y) + verticalLift,
                side * lateralDeflection);

            ball.Body.velocity = reboundVelocity;
            nextSaveTime = Time.time + cooldown;
        }
    }
}
