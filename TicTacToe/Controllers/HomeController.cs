using Microsoft.AspNetCore.Mvc;
using TicTacToe.Models;

namespace TicTacToe.Controllers
{
    public class HomeController : Controller
    {
        const string BOARD_KEY = "TicTacToeBoard";
        const string MOVE_COUNT_KEY = "MovesCount";
        const string UNDO_STACK_KEY = "UndoStack";
        const string REDO_STACK_KEY = "RedoStack";

        [HttpGet]
        public IActionResult Index()
        {
            var board = HttpContext.Session.Get<char[,]>(BOARD_KEY) ?? new TicTacToeGame().InitializeBoard();
            HttpContext.Session.Set(BOARD_KEY, board);

            return View(board);
        }

        [HttpPost]
        public IActionResult Index([FromBody] MakeMove move)
        {
            var board = HttpContext.Session.Get<char[,]>(BOARD_KEY) ?? new TicTacToeGame().InitializeBoard();
            var movesCount = HttpContext.Session.GetInt32(MOVE_COUNT_KEY) ?? 0;
            var undoStack = HttpContext.Session.Get<Stack<MakeMove>>(UNDO_STACK_KEY) ?? new Stack<MakeMove>();
            
            var ticTacToeGame = new TicTacToeGame { Board = board, MovesCount = movesCount };
            var isPlayerWon = ticTacToeGame.IsWinningMove(move.Row, move.Col, move.Player, 5);


            HttpContext.Session.Set(REDO_STACK_KEY, new Stack<MakeMove>());
            board[move.Row, move.Col] = move.Player;
            movesCount++;
            undoStack.Push(move);

            if (!isPlayerWon)
            {
                var opponent = move.Player == 'X' ? 'O' : 'X';
                var opponentMove = ticTacToeGame.OpponentMove(opponent, move.Row, move.Col);
                var isOpponentWon = ticTacToeGame.IsWinningMove(opponentMove.Item1, opponentMove.Item2, opponent, 5);

                board[opponentMove.Item1, opponentMove.Item2] = opponent;
                undoStack.Push(new MakeMove(opponentMove.Item1, opponentMove.Item2, opponent));
                movesCount++;

                undoStack = ReverseStack(undoStack);

                HttpContext.Session.Set(BOARD_KEY, board);
                HttpContext.Session.SetInt32(MOVE_COUNT_KEY, movesCount);
                HttpContext.Session.Set(UNDO_STACK_KEY, undoStack);

                var response = new
                {
                    board,
                    isDraw = ticTacToeGame.IsDraw(),
                    winner = isOpponentWon ? (char?)opponent : null,
                    opponentMove = new { item1 = opponentMove.Item1, item2 = opponentMove.Item2 }
                };

                return Json(response);
            }
            else
            {
                HttpContext.Session.Set(BOARD_KEY, board);
                HttpContext.Session.SetInt32(MOVE_COUNT_KEY, movesCount);
                HttpContext.Session.Set(UNDO_STACK_KEY, undoStack);

                var response = new
                {
                    board,
                    isDraw = ticTacToeGame.IsDraw(),
                    winner = isPlayerWon ? (char?)move.Player : null,
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
        public IActionResult UndoMoves()
        {
            var board = HttpContext.Session.Get<char[,]>(BOARD_KEY);
            var undoStack = HttpContext.Session.Get<Stack<MakeMove>>(UNDO_STACK_KEY) ?? new Stack<MakeMove>();
            var redoStack = HttpContext.Session.Get<Stack<MakeMove>>(REDO_STACK_KEY) ?? new Stack<MakeMove>();
            var movesCount = HttpContext.Session.GetInt32(MOVE_COUNT_KEY) ?? 0;
            var movesToUndo = new List<MakeMove>();

            if (undoStack.Count >= 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    var move = undoStack.Pop();
                    movesToUndo.Add(move);
                    board[move.Row, move.Col] = '\0';
                    redoStack.Push(move);
                    movesCount--;
                }
                undoStack = ReverseStack(undoStack);
                redoStack = ReverseStack(redoStack);

                HttpContext.Session.Set(BOARD_KEY, board);
                HttpContext.Session.Set(UNDO_STACK_KEY, undoStack);
                HttpContext.Session.Set(REDO_STACK_KEY, redoStack);
                HttpContext.Session.SetInt32(MOVE_COUNT_KEY, movesCount);
            }

            var response = new { undoStack = movesToUndo };

            return Json(response);
        }

        
        [HttpPost]
        [Route("redo-moves")]
        public IActionResult RedoMoves()
        {
            var board = HttpContext.Session.Get<char[,]>(BOARD_KEY);
            var undoStack = HttpContext.Session.Get<Stack<MakeMove>>(UNDO_STACK_KEY) ?? new Stack<MakeMove>();
            var redoStack = HttpContext.Session.Get<Stack<MakeMove>>(REDO_STACK_KEY) ?? new Stack<MakeMove>();
            var movesCount = HttpContext.Session.GetInt32(MOVE_COUNT_KEY) ?? 0;
            var movesToRedo = new List<MakeMove>();

            if (redoStack.Count >= 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    var move = redoStack.Pop();
                    movesToRedo.Add(move);
                    board[move.Row, move.Col] = '\0';
                    undoStack.Push(move);
                    movesCount--;
                }
                undoStack = ReverseStack(undoStack);
                redoStack = ReverseStack(redoStack);

                HttpContext.Session.Set(BOARD_KEY, board);
                HttpContext.Session.Set(UNDO_STACK_KEY, undoStack);
                HttpContext.Session.Set(REDO_STACK_KEY, redoStack);
                HttpContext.Session.SetInt32(MOVE_COUNT_KEY, movesCount);
            }

            var response = new { redoStack = movesToRedo };

            return Json(response);
        }

        static Stack<MakeMove> ReverseStack(Stack<MakeMove> stack)
        {
            Stack<MakeMove> reverseStack = new Stack<MakeMove>();
            while (stack.Count > 0)
            {
                reverseStack.Push(stack.Pop());
            }
            
            return reverseStack;
        }
    }
}
