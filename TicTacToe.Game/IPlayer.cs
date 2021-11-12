using System.Threading.Tasks;

namespace TicTacToe.Game
{
    public interface IPlayer
    {
        string GetName() => Type.ToString();
        PlayerTypes Type { get; }
        
        Task<int> MakeMove(GameState state);
        void GameEnded(GameState state, ResultState result, int myNumber);
    }

    public enum PlayerTypes
    {
        Human,
        RandomBot,
        MiniMax
    }
}