using UnityEngine;
using UnityEngine.EventSystems;
using Zakareya.DreamFootball.Players;

namespace Zakareya.DreamFootball.Input
{
    /// <summary>Attach to a UI joystick background with a child handle RectTransform.</summary>
    public sealed class TouchJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private FootballerMotor target;
        [SerializeField, Range(0.2f, 1f)] private float handleRange = 0.7f;

        private Canvas canvas;
        private Camera uiCamera;

        private void Awake()
        {
            if (background == null) background = transform as RectTransform;
            canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                uiCamera = canvas.worldCamera;
        }

        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null || handle == null || target == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, uiCamera, out Vector2 localPoint)) return;

            Vector2 halfSize = background.rect.size * 0.5f;
            Vector2 normalized = new Vector2(
                halfSize.x > 0 ? localPoint.x / halfSize.x : 0,
                halfSize.y > 0 ? localPoint.y / halfSize.y : 0);
            normalized = Vector2.ClampMagnitude(normalized, 1f);
            handle.anchoredPosition = normalized * halfSize * handleRange;
            target.SetMoveInput(normalized);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (handle != null) handle.anchoredPosition = Vector2.zero;
            if (target != null) target.SetMoveInput(Vector2.zero);
        }
    }
}
