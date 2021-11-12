using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using TicTacToe.Bots;
using TicTacToe.Game;
using TicTacToe.UI.Web.Services;

namespace TicTacToe.UI.Web.Pages
{
    public partial class Play : ComponentBase
    {
        [Parameter]
        public int BoardSize { get; set; } = 3;

        [Parameter]
        public string? Bot { get; set; }

        private readonly List<IPlayer> _players = new();
        private string _playerTurnText = "";
        private string _botName = "";
        private string[] _positions = Array.Empty<string>();
        
        private string _winnerText = "";
        private bool _isGameInProgress = true;
        private string _result = "";

        private int _totalSpots;
        private WebAppGame? _webAppGame;
        private bool _isInitialized;

        protected override async Task OnInitializedAsync()
        {
            if (_isInitialized)
            {
                return;
            }
            _isInitialized = true;

            if (BoardSize != 3)
            {
                // for now the ui just caters for a 3x3 TicTacToe. Even though setting this to 4 or 5 should work
                BoardSize = 3;
            }

            _totalSpots = BoardSize * BoardSize;

            SetupBot();
            ResetBoard();
            await StartGameAsync();
        }

        private void GameEnded(ResultState result)
        {
            _isGameInProgress = false;
            _playerTurnText = "";

            if (result == ResultState.Draw)
            {
                _result = "draw";
                _winnerText = "It's a draw!";
                return;
            }

            // if it's currently the bots turn, then it means human won (previous move)
            var didHumanWin = _webAppGame?.GameState?.PlayersTurn is not WebHumanPlayer;
            if (didHumanWin)
            {
                _result = "win";
                _winnerText = "You won!";
            }
            else
            {
                _result = "lose";
                _winnerText = "You lost!";
            }
        }

        private void SetupBot()
        {
            IPlayer botPlayer;
            _botName = Bot ?? "";
            if (_botName.Equals(PlayerTypes.MiniMax.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                _botName = "MiniMax Bot";
                botPlayer = new WebMiniMaxBot(_httpClient);
            }
            else
            {
                _botName = "Random Bot";
                botPlayer = new RandomMoveBot(500);
            }

            _players.Add(botPlayer);
            _players.Add(new WebHumanPlayer());
        }

        private async Task RestartGameAsync()
        {
            ResetBoard();
            _players.Reverse();
            await StartGameAsync();
        }

        private async Task StartGameAsync()
        {
            _webAppGame = new WebAppGame(
                board => RedrawBoard(board.Positions),
                GameEnded);

            await _webAppGame.StartGameAsync(
                BoardSize,
                _players[0],
                _players[1]);
        }

        private Task MakeMove(int move)
        {
            if (_webAppGame?.GameState?.PlayersTurn is not WebHumanPlayer humanPlayer)
            {
                // someone's being trigger happy and clicking while waiting for the bot...
                return Task.CompletedTask;
            }

            humanPlayer.PlayerMoveTaskCompletionSource.TrySetResult(move);
            return Task.CompletedTask;
        }

        private void ResetBoard()
        {
            _isGameInProgress = true;
            _winnerText = "";
            RedrawBoard(new int[_totalSpots]);
        }

        private void RedrawBoard(int[] positions)
        {
            _positions = positions.Select(GetSymbolForNumber).ToArray();

            var playerToGo = _webAppGame?.GameState?.PlayersTurn.Type;

            if (playerToGo is PlayerTypes.Human)
            {
                var currentPlayerSymbol = _webAppGame?.GameState?.GetCurrentPlayersSymbol();
                _playerTurnText = $"It's your turn ({currentPlayerSymbol})";
            }
            else
            {
                _playerTurnText = "";
            }

            StateHasChanged();
        }

        private static string GetSymbolForNumber(int number) =>
            number switch
            {
                1 => Constants.PLAYER_1_SYMBOL,
                2 => Constants.PLAYER_2_SYMBOL,
                _ => ""
            };

        private string GetBoardSizeClass()
        {
            return $"board-size-{BoardSize}";
        }

        private string GetCssClass(int index)
        {
            var cssClasses = new List<string>()
            {
                "block"
            };

            // add a class for the player who owns this spot
            switch (_positions[index])
            {
                case Constants.PLAYER_1_SYMBOL:
                    cssClasses.Add($"spot_{Constants.PLAYER_1_SYMBOL}");
                    break;
                case Constants.PLAYER_2_SYMBOL:
                    cssClasses.Add($"spot_{Constants.PLAYER_2_SYMBOL}");
                    break;
            }

            return string.Join(" ", cssClasses);
        }
    }
}