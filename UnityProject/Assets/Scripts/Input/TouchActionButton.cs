using UnityEngine;
using UnityEngine.EventSystems;
using Zakareya.DreamFootball.Players;

namespace Zakareya.DreamFootball.Input
{
    /// <summary>
    /// Connect a Unity UI Button-like object to this component to provide touch
    /// sprint, pass, and shot actions without polling the legacy Input Manager.
    /// The component must receive UI pointer events through an EventSystem.
    /// </summary>
    public sealed class TouchActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public enum ActionType
        {
            Sprint,
            Pass,
            Shoot,
            Tackle
        }

        [SerializeField] private ActionType action = ActionType.Pass;
        [SerializeField] private FootballerMotor motor;
        [SerializeField] private PlayerBallActions ballActions;
        [SerializeField] private TackleAction tackleAction;
        [SerializeField] private Transform passTarget;
        [SerializeField] private Transform shotTarget;
        [SerializeField, Min(1f)] private float defaultPassDistance = 8f;
        [SerializeField, Min(5f)] private float defaultShotDistance = 24f;

        public void OnPointerDown(PointerEventData eventData)
        {
            switch (action)
            {
                case ActionType.Sprint:
                    if (motor != null) motor.SetSprint(true);
                    break;

                case ActionType.Pass:
                    if (ballActions != null)
                    {
                        Vector3 target = passTarget != null
                            ? passTarget.position
                            : (motor != null ? motor.transform.position : transform.position) + (motor != null ? motor.transform.forward : transform.forward) * defaultPassDistance;
                        ballActions.Pass(target);
                    }
                    break;

                case ActionType.Tackle:
                    if (tackleAction != null) tackleAction.TryTackle();
                    break;

                case ActionType.Shoot:
                    if (ballActions != null)
                    {
                        Vector3 target = shotTarget != null
                            ? shotTarget.position
                            : (motor != null ? motor.transform.position : transform.position) + (motor != null ? motor.transform.forward : transform.forward) * defaultShotDistance;
                        ballActions.Shoot(target);
                    }
                    break;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ReleaseSprint();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (action == ActionType.Sprint)
                ReleaseSprint();
        }

        private void OnDisable()
        {
            ReleaseSprint();
        }

        private void ReleaseSprint()
        {
            if (action == ActionType.Sprint && motor != null)
                motor.SetSprint(false);
        }
    }
}
