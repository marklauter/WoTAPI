using System;
using System.Diagnostics;

namespace WoT.API.Sandbox.Models
{
    [DebuggerDisplay("{StartedUtc}, {BattleResult}, {MapName}")]
    public class BattleEvent
        : Battle
    {
        public DateTime StartedUtc => FinishedUtc - this.BattleDuration;
        public DateTime FinishedUtc => DateTime.UnixEpoch.AddSeconds(this.finished_at);
        public int FriendlyKills => this.frags[0];
        public int EnemyKills => this.frags[1];
        public BattleResults BattleResult => this.result == "victory" ? BattleResults.Victory : BattleResults.Defeat;
        public TimeSpan BattleDuration => this.duration.HasValue ? TimeSpan.FromSeconds(this.duration.Value) : TimeSpan.FromSeconds(0);
        public BattleTypes BattleType => this.type == "SORTIE" ? BattleTypes.Skirmish : BattleTypes.Advance;
        public string MapName => this.arena.name;
    }
}
