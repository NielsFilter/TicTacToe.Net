using System.Threading.Tasks;
using TicTacToe.Bots;
using TicTacToe.Game;

namespace TicTacToe.UI.Web.Services
{
    public class WebHumanPlayer : HumanPlayer
    {
        public TaskCompletionSource<int> PlayerMoveTaskCompletionSource;
        public override async Task<int> MakeMove(GameState state)
        {
            PlayerMoveTaskCompletionSource = new TaskCompletionSource<int>();
            return await PlayerMoveTaskCompletionSource.Task;
        }
    }
}