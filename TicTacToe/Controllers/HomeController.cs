using Microsoft.AspNetCore.Mvc;
using TicTacToe.Models;
using System.Collections.Generic;

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
            HttpContext.Session.SetInt32(MoveCountKey, moveCount);

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
            var undoStack = HttpContext.Session.Get<Stack<MakeMove>>(UndoStackKey) ?? new Stack<MakeMove>();

            var ticTacToeGame = new TicTacToeGame { Board = board, MovesCount = moveCount };
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
                undoStack.Push(new MakeMove(opponentMove.Item1, opponentMove.Item2, opponent));
                moveCount++;

                undoStack = ReverseStack(undoStack);

                HttpContext.Session.Set(BoardKey, board);
                HttpContext.Session.SetInt32(MoveCountKey, moveCount);
                HttpContext.Session.Set(UndoStackKey, undoStack);

                var response = new
                {
                    board,
                    isDraw = ticTacToeGame.IsDraw(),
                    winner = isOpponentWon ? (char?)opponent : null,
                    opponentMove = new { item1 = opponentMove.Item1, item2 = opponentMove.Item2 },
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
                    isDraw = ticTacToeGame.IsDraw(),
                    winner = playerWon ? (char?)move.Player : null,
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
            var board = HttpContext.Session.Get<char[,]>(BoardKey);
            var undoStack = HttpContext.Session.Get<Stack<MakeMove>>(UndoStackKey);
            var redoStack = HttpContext.Session.Get<Stack<MakeMove>>(RedoStackKey) ?? new Stack<MakeMove>();
            var moveCount = HttpContext.Session.GetInt32(MoveCountKey) ?? 0;
            var movesToUndo = new List<MakeMove>();

            if (undoStack.Count >= 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    var move = undoStack.Pop();
                    movesToUndo.Add(move);
                    board[move.Row, move.Col] = '\0';
                    redoStack.Push(move);
                    moveCount--;
                }
                undoStack = ReverseStack(undoStack);
                redoStack = ReverseStack(redoStack);

                HttpContext.Session.Set(BoardKey, board);
                HttpContext.Session.Set(UndoStackKey, undoStack);
                HttpContext.Session.Set(RedoStackKey, redoStack);
                HttpContext.Session.SetInt32(MoveCountKey, moveCount);
            }

            var response = new { undoStack = movesToUndo };

            return Json(response);
        }

        
        [HttpPost]
        [Route("redo-moves")]
        public IActionResult RedoMoves()
        {
            var board = HttpContext.Session.Get<char[,]>(BoardKey);
            var undoStack = HttpContext.Session.Get<Stack<MakeMove>>(UndoStackKey);
            var redoStack = HttpContext.Session.Get<Stack<MakeMove>>(RedoStackKey);
            var moveCount = HttpContext.Session.GetInt32(MoveCountKey) ?? 0;
            var movesToRedo = new List<MakeMove>();


            if (redoStack.Count >= 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    var move = redoStack.Pop();
                    movesToRedo.Add(move);
                    board[move.Row, move.Col] = '\0';
                    undoStack.Push(move);
                    moveCount--;
                }
                undoStack = ReverseStack(undoStack);
                redoStack = ReverseStack(redoStack);

                HttpContext.Session.Set(BoardKey, board);
                HttpContext.Session.Set(UndoStackKey, undoStack);
                HttpContext.Session.Set(RedoStackKey, redoStack);
                HttpContext.Session.SetInt32(MoveCountKey, moveCount);
            }

            var response = new { redoStack = movesToRedo };

            return Json(response);
        }

        static Stack<MakeMove> ReverseStack(Stack<MakeMove> stack)
        {
            Stack<MakeMove> tempStack = new Stack<MakeMove>();
            while (stack.Count > 0)
            {
                tempStack.Push(stack.Pop());
            }
            return tempStack;
        }
    }
}
