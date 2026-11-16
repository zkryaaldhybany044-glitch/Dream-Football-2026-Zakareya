using UnityEngine;

namespace Zakareya.DreamFootball.Core
{
    [RequireComponent(typeof(MatchClock))]
    public sealed class MatchController : MonoBehaviour
    {
        [SerializeField] private MatchPhase phase = MatchPhase.PreMatch;
        private MatchPhase phaseBeforePause = MatchPhase.FirstHalf;
        private MatchClock clock;

        public MatchPhase Phase => phase;
        public MatchClock Clock => clock;

        private void Awake() => clock = GetComponent<MatchClock>();

        public void BeginFirstHalf()
        {
            if (phase != MatchPhase.PreMatch) return;
            phase = MatchPhase.FirstHalf;
            clock.ResetClock();
            clock.StartClock();
        }

        public void ReachHalfTime()
        {
            if (phase != MatchPhase.FirstHalf) return;
            clock.StopClock();
            phase = MatchPhase.HalfTime;
        }

        public void BeginSecondHalf()
        {
            if (phase != MatchPhase.HalfTime) return;
            phase = MatchPhase.SecondHalf;
            clock.ResetClock();
            clock.StartClock();
        }

        public void EndMatch()
        {
            if (phase == MatchPhase.FullTime) return;
            clock.StopClock();
            phase = MatchPhase.FullTime;
        }

        public void SetPaused(bool paused)
        {
            if (paused && (phase == MatchPhase.FirstHalf || phase == MatchPhase.SecondHalf))
            {
                phaseBeforePause = phase;
                clock.StopClock();
                phase = MatchPhase.Paused;
            }
            else if (!paused && phase == MatchPhase.Paused)
            {
                phase = phaseBeforePause;
                clock.StartClock();
            }
        }
    }
}
