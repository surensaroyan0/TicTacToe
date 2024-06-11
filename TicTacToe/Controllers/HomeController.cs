using Microsoft.AspNetCore.Mvc;
using TicTacToe.Models;

namespace TicTacToe.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private const string BoardKey = "TicTacToeBoard";
        private const string MoveCountKey = "MoveCount";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var board = HttpContext.Session.Get<char[,]>(BoardKey) ?? new TicTacToeGame().InitializeBoard();
            HttpContext.Session.Set(BoardKey, board);
            
            var moveCount = HttpContext.Session.GetInt32(MoveCountKey) ?? 0;
            HttpContext.Session.Set(MoveCountKey, moveCount);

            return View(board);
        }

        [HttpPost]
        public IActionResult Index([FromBody] MakeMove move)
        {
            var board = HttpContext.Session.Get<char[,]>(BoardKey) ?? new TicTacToeGame().InitializeBoard();
            var moveCount = HttpContext.Session.GetInt32(MoveCountKey) ?? 0;

            var ticTacToeGame = new TicTacToeGame 
            { 
                Board = board,
                MovesCount = moveCount,
            };

            var playerWon = ticTacToeGame.IsWinningMove(move.Row, move.Col, move.Player, 5);

            board[move.Row, move.Col] = move.Player;
            moveCount++;

            if (!playerWon)
            {
                var opponent = move.Player == 'X' ? 'O' : 'X';
                var opponentMove = ticTacToeGame.OpponentMove(opponent, move.Row, move.Col);
                var isOpponentWon = ticTacToeGame.IsWinningMove(opponentMove.Item1, opponentMove.Item2, opponent, 5);

                board[opponentMove.Item1, opponentMove.Item2] = opponent;
                moveCount++;
                HttpContext.Session.Set(BoardKey, board);
                HttpContext.Session.SetInt32(MoveCountKey, moveCount);

                var response = new
                {
                    board,
                    isDraw = ticTacToeGame.IsDraw(),
                    winner = isOpponentWon ? (char?)opponent : null,
                    opponentMove
                };

                return Json(response);
            }
            else 
            {
                HttpContext.Session.Set(BoardKey, board);
                HttpContext.Session.SetInt32(MoveCountKey, moveCount);

                var response = new
                {
                    board,
                    IsDraw = ticTacToeGame.IsDraw(),
                    winner = playerWon ? (char?) move.Player : null,
                    botMove = (int?)null
                };

                return Json(response);
            }
        }

        [HttpPost]
        [Route("clear-session")]
        public IActionResult ClearSession() 
        {
            HttpContext.Session.Clear();
            return Ok();
        }
    }
}
