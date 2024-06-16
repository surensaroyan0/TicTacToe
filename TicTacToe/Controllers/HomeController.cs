using Microsoft.AspNetCore.Mvc;
using TicTacToe.Models;

namespace TicTacToe.Controllers
{
    public class HomeController : Controller
    {
        private const string BoardKey = "TicTacToeBoard";
        private const string MoveCountKey = "MoveCount";
        private const string UndoStackKey = "UndoStack";
        private const string RedoStackKey = "RedoStack";

        [HttpGet]
        public IActionResult Index()
        {
            var board = HttpContext.Session.Get<char[,]>(BoardKey) ?? new TicTacToeGame().InitializeBoard();
            HttpContext.Session.Set(BoardKey, board);
            
            var moveCount = HttpContext.Session.GetInt32(MoveCountKey) ?? 0;
            HttpContext.Session.Set(MoveCountKey, moveCount);

            var undoStack = HttpContext.Session.Get<Stack<MakeMove>>(UndoStackKey) ?? new Stack<MakeMove>();
            HttpContext.Session.Set(UndoStackKey, undoStack);

            var redoStack = HttpContext.Session.Get<Stack<MakeMove>>(RedoStackKey) ?? new Stack<MakeMove>();
            HttpContext.Session.Set(RedoStackKey, redoStack);

            return View(board);
        }

        [HttpPost]
        public IActionResult Index([FromBody] MakeMove move)
        {
            var board = HttpContext.Session.Get<char[,]>(BoardKey) ?? new TicTacToeGame().InitializeBoard();
            var moveCount = HttpContext.Session.GetInt32(MoveCountKey) ?? 0;
            var undoStack = HttpContext.Session.Get<Stack<MakeMove>>(RedoStackKey) ?? new Stack<MakeMove>();
            var ticTacToeGame = new TicTacToeGame 
            { 
                Board = board,
                MovesCount = moveCount,
            };
            var playerWon = ticTacToeGame.IsWinningMove(move.Row, move.Col, move.Player, 5);

            board[move.Row, move.Col] = move.Player;
            moveCount++;
            undoStack.Push(move);

            if (!playerWon)
            {
                var opponent = move.Player == 'X' ? 'O' : 'X';
                var opponentMove = ticTacToeGame.OpponentMove(opponent, move.Row, move.Col);
                var isOpponentWon = ticTacToeGame.IsWinningMove(opponentMove.Item1, opponentMove.Item2, opponent, 5);

                board[opponentMove.Item1, opponentMove.Item2] = opponent;
                moveCount++;
                HttpContext.Session.Set(BoardKey, board);
                HttpContext.Session.SetInt32(MoveCountKey, moveCount);
                HttpContext.Session.Set(UndoStackKey, undoStack);

                var response = new
                {
                    board,
                    isDraw = ticTacToeGame.IsDraw(),
                    winner = isOpponentWon ? (char?)opponent : null,
                    opponentMove,
                    lastMove = undoStack.Peek()
                };

                return Json(response);
            }
            else 
            {
                HttpContext.Session.Set(BoardKey, board);
                HttpContext.Session.SetInt32(MoveCountKey, moveCount);
                HttpContext.Session.Set(UndoStackKey, undoStack);

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

        [HttpPost]
        [Route("undo-moves")]
        public IActionResult UndoMoves([FromBody] List<MakeMove> undoneMoves)
        {
            var board = HttpContext.Session.Get<char[,]>(BoardKey);
            var undoStack = HttpContext.Session.Get<Stack<MakeMove>>(UndoStackKey);
            var redoStack = HttpContext.Session.Get<Stack<MakeMove>>(RedoStackKey) ?? new Stack<MakeMove>();
            var moveCount = HttpContext.Session.GetInt32(MoveCountKey);

            foreach (var move in undoneMoves)
            {
                board[move.Row, move.Col] = '\0';
                redoStack.Push(move);
                moveCount--;
            }
            undoStack.Pop();

            HttpContext.Session.Set(BoardKey, board);
            HttpContext.Session.Set(UndoStackKey, undoStack);
            HttpContext.Session.Set(RedoStackKey, redoStack);
            HttpContext.Session.Set(MoveCountKey, moveCount);

            return Ok();
        }
    }
}