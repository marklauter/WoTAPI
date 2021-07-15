using System;
using System.Diagnostics;
using System.Linq;

namespace WoT.API.Sandbox.Models
{
    [DebuggerDisplay("{Date}, battles: {BattleCount}, wins: {WinCount}, wr: {WinRate}")]
    public class BattleSession
    {
        public BattleSession(DateTime date, BattleEvent[] battles)
        {
            this.Id = Guid.NewGuid();
            this.Date = date;
            this.Battles = battles;
            this.BattleCount = battles.Length;
            this.WinCount = battles
                .Count(b => b.BattleResult == BattleResults.Victory);
            this.WinRate = this.WinCount / (this.BattleCount * 1.0) * 100;
        }

        public Guid Id {get;}

        public BattleEvent[] Battles { get; }

        public int BattleCount { get; }

        public int WinCount { get; }

        public DateTime Date { get; }

        public double WinRate { get; }
    }
}
