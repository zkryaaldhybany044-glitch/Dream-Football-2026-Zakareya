using System;
using UnityEngine;

namespace Zakareya.DreamFootball.Core
{
    /// <summary>Configurable match-time source, independent of wall-clock time.</summary>
    public sealed class MatchClock : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float matchMinutes = 90f;
        [SerializeField, Min(1f)] private float realSecondsPerMatchMinute = 2f;

        public float ElapsedMatchSeconds { get; private set; }
        public bool IsRunning { get; private set; }
        public event Action<int, int> MinuteChanged;

        private int lastReportedMinute = -1;

        public int ElapsedMinutes =>
            Mathf.Clamp(Mathf.FloorToInt(ElapsedMatchSeconds / 60f), 0,
                Mathf.CeilToInt(matchMinutes));

        public void StartClock() => IsRunning = true;
        public void StopClock() => IsRunning = false;

        public void ResetClock()
        {
            ElapsedMatchSeconds = 0f;
            lastReportedMinute = -1;
            IsRunning = false;
            MinuteChanged?.Invoke(0, 0);
        }

        private void Update()
        {
            if (!IsRunning) return;
            ElapsedMatchSeconds += Time.deltaTime * (60f / realSecondsPerMatchMinute);
            int minute = ElapsedMinutes;
            if (minute != lastReportedMinute)
            {
                lastReportedMinute = minute;
                MinuteChanged?.Invoke(minute, Mathf.CeilToInt(matchMinutes));
            }
            if (ElapsedMatchSeconds >= matchMinutes * 60f) StopClock();
        }
    }
}
