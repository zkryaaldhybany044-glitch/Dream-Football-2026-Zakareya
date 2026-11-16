using UnityEngine;

namespace Zakareya.DreamFootball.Core
{
    [RequireComponent(typeof(MatchClock))]
    public sealed class MatchController : MonoBehaviour
    {
        [SerializeField] private MatchPhase phase = MatchPhase.PreMatch;
        private MatchClock clock;

        public MatchPhase Phase => phase;
        public MatchClock Clock => clock;

        private void Awake() => clock = GetComponent<MatchClock>();

        public void BeginFirstHalf()
        {
            if (phase != MatchPhase.PreMatch && phase != MatchPhase.HalfTime) return;
            phase = MatchPhase.FirstHalf;
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
            clock.StartClock();
        }

        public void EndMatch()
        {
            clock.StopClock();
            phase = MatchPhase.FullTime;
        }

        public void SetPaused(bool paused)
        {
            if (paused && (phase == MatchPhase.FirstHalf || phase == MatchPhase.SecondHalf))
            {
                clock.StopClock();
                phase = MatchPhase.Paused;
            }
            else if (!paused && phase == MatchPhase.Paused)
            {
                phase = MatchPhase.SecondHalf;
                clock.StartClock();
            }
        }
    }
}
