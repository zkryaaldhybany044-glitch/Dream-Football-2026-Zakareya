using UnityEngine;

namespace Zakareya.DreamFootball.Core
{
    /// <summary>Team identity metadata for one player, used by match-level systems.</summary>
    public sealed class PlayerTeamMember : MonoBehaviour
    {
        [SerializeField, Min(0)] private int teamId;
        public int TeamId => teamId;

        public void SetTeamId(int id) => teamId = Mathf.Max(0, id);
    }
}
