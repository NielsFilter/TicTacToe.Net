using System.Collections.Generic;
using System.Threading;

namespace TicTacToe.UI.CommandLine
{
    public class QLearningResultSet
    {
        public QLearningResultSet()
        {
            Results = new Dictionary<int, QLearningResult>();
        }
        
        public Dictionary<int, QLearningResult> Results { get; }
    }
    
    public class QLearningResult
    {
        private int _gamesWon;
        public int GetWins => _gamesWon;
        public int Won() => Interlocked.Increment(ref _gamesWon);

        private int _gamesLost;
        public int GetLosses => _gamesLost;
        public int Lost() => Interlocked.Increment(ref _gamesLost);
        
        private int _gamesDrawn;
        public int GetDraws => _gamesDrawn;
        public int Draw() => Interlocked.Increment(ref _gamesDrawn);
    }
}