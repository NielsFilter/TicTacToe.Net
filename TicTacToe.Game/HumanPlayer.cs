using System.Threading.Tasks;

namespace TicTacToe.Game
{
    public abstract class HumanPlayer : IPlayer
    {
        public PlayerTypes Type => PlayerTypes.Human;
        public abstract Task<int> MakeMove(GameState state);
        public virtual void GameEnded(GameState state, ResultState result, int myNumber) {}
    }
}