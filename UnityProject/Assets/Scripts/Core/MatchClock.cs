using System;
using UnityEngine;

namespace Zakareya.DreamFootball.Core
{
    /// <summary>Clock for one 45-minute half; gameplay time is independent of wall-clock time.</summary>
    public sealed class MatchClock : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float halfMinutes = 45f;
        [SerializeField, Min(0.1f)] private float realSecondsPerMatchMinute = 2f;

        public float ElapsedMatchSeconds { get; private set; }
        public bool IsRunning { get; private set; }
        public float HalfDurationSeconds => halfMinutes * 60f;
        public bool IsComplete => ElapsedMatchSeconds >= HalfDurationSeconds;
        public event Action<int, int> MinuteChanged;

        private int lastReportedMinute = -1;

        public int ElapsedMinutes =>
            Mathf.Clamp(Mathf.FloorToInt(ElapsedMatchSeconds / 60f), 0, Mathf.CeilToInt(halfMinutes));

        public void StartClock()
        {
            if (!IsComplete) IsRunning = true;
        }

        public void StopClock() => IsRunning = false;

        public void ResetClock()
        {
            ElapsedMatchSeconds = 0f;
            lastReportedMinute = -1;
            IsRunning = false;
            MinuteChanged?.Invoke(0, Mathf.CeilToInt(halfMinutes));
        }

        private void Update()
        {
            if (!IsRunning) return;

            ElapsedMatchSeconds = Mathf.Min(
                ElapsedMatchSeconds + Time.deltaTime * (60f / realSecondsPerMatchMinute),
                HalfDurationSeconds);

            int minute = ElapsedMinutes;
            if (minute != lastReportedMinute)
            {
                lastReportedMinute = minute;
                MinuteChanged?.Invoke(minute, Mathf.CeilToInt(halfMinutes));
            }

            if (IsComplete) StopClock();
        }
    }
}