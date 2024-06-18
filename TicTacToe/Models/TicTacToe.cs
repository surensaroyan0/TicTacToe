namespace TicTacToe.Models
{
    public class TicTacToeGame
    {
        public char[,] Board { get; set; }
        public int MovesCount { get; set; }
        private int BoardSize = 100;
        private char EmptyCell = '-';

        public TicTacToeGame()
        {
            Board = InitializeBoard();
            MovesCount = 0;
        }

        public char[,] InitializeBoard()
        {   
            var board = new char[BoardSize, BoardSize];
            
            for (int i = 0; i < BoardSize; i++)
                {
                for (int j = 0; j < BoardSize; j++)
                {
                    board[i, j] = EmptyCell;
                }
            }
            return board;
        }

        public bool IsDraw()
        {
            return MovesCount == BoardSize * BoardSize;
        }

        public bool IsWinningMove(int x, int y, char player, int loopCount)
        {
            return CheckDirection(x, y, player, 1, 0, loopCount)  ||  // Check horizontally
                   CheckDirection(x, y, player, 0, 1, loopCount)  ||  // Check vertically
                   CheckDirection(x, y, player, 1, 1, loopCount)  ||  // Check diagonal right-down
                   CheckDirection(x, y, player, 1, -1, loopCount);    // Check diagonal right-up
        }

        private bool CheckDirection(int x, int y, char player, int dx, int dy, int loopCount)
        {
            int count = 1;
            count += CountInDirection(x, y, player, dx, dy, loopCount - 1);
            count += CountInDirection(x, y, player, -dx, -dy, loopCount - 1);
            return count >= loopCount;
        }

        private int CountInDirection(int x, int y, char player, int dx, int dy, int loopCount)
        {
            int count = 0;

            for (int i = 1; i <= loopCount; i++)
            {
                int nx = x + i * dx;
                int ny = y + i * dy;
                if (nx >= 0 && nx < BoardSize && ny >= 0 && ny < BoardSize && Board[nx, ny] == player)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            return count;
        }

        private (int, int) FindPotentialWin(char player, int requiredCount)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    if (Board[x, y] == EmptyCell)
                    {
                        Board[x, y] = player;
                        if (IsWinningMove(x, y, player, requiredCount))
                        {
                            if (requiredCount == 5)
                            {
                                Board[x, y] = EmptyCell;
                                return (x, y);
                            }
                            if (requiredCount == 4)
                            {
                                if (CanCompleteLine(x, y, player, 4))
                                {
                                    Board[x, y] = EmptyCell;
                                    return (x, y);
                                }
                            }
                            else if (requiredCount == 3)
                            {
                                if (CanCompleteLine(x, y, player, 3))
                                {
                                    Board[x, y] = EmptyCell;
                                    return (x, y);
                                }
                            }
                        }
                        Board[x, y] = EmptyCell;
                    }  
                }
            }
            return (-1, -1);
        }

        private (int, int, int) PreventLoosing(char player)
        {
            char opponent = player == 'X' ? 'O' : 'X';
            (int, int)[] preventMoves = new (int, int)[3];
            preventMoves[0] = (-1, -1);
            preventMoves[1] = (-1, -1);
            preventMoves[2] = (-1, -1);
            
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    if (Board[x, y] == EmptyCell)
                    {
                        Board[x, y] = opponent;

                        if (IsWinningMove(x, y, opponent, 5))
                        {
                            preventMoves[0] = (x, y);
                        }

                        if (IsWinningMove(x, y, opponent, 4))
                        {
                            if (CanCompleteLine(x, y, opponent, 4))
                            {
                                preventMoves[1] = (x, y);
                            }
                        }

                        if (IsWinningMove(x, y, opponent, 3))
                        {
                            if (CanCompleteLine(x, y, opponent, 3))
                            {
                                preventMoves[2] = (x, y);
                            }
                        }

                        Board[x, y] = EmptyCell;
                    }
                }
            }

            if (preventMoves[0] != (-1, -1))
            {
                return (preventMoves[0].Item1, preventMoves[0].Item2, 1);
            }
            if (preventMoves[1] != (-1, -1))
            {
                return (preventMoves[1].Item1, preventMoves[1].Item2, 0);
            }
            if (preventMoves[2] != (-1, -1))
            {
                return (preventMoves[2].Item1, preventMoves[2].Item2, -1);
            }

            return (-1, -1, -2);
        }

        private (int, int) PreventFeatureLines(int row, int col)
        {
            List<(int, int)> possibleMoves = new List<(int, int)>();
            int[] directions = {-1, 0, 1};

            for (int i = 1; i < 3; i++)
            {
                foreach (int dx in directions)
                {
                    foreach (int dy in directions)
                    {
                        if (dx == 0 && dy == 0)
                        {
                            continue;
                        }

                        int newRow = row + dx * i;
                        int newCol = col + dy * i;

                        if (newRow >= 0 && newRow <= BoardSize && newCol >= 0 && newCol <= BoardSize)
                        {
                            possibleMoves.Add((newRow, newCol)); 
                        }
                    }
                }
            }
        
            Random random = new Random();
            (int, int) randomMove;

            do
            {
                randomMove = possibleMoves[random.Next(possibleMoves.Count)];
            } while (Board[randomMove.Item1, randomMove.Item2] != EmptyCell);

            return randomMove;
        }

        private bool CanCompleteLine(int x, int y, char player, int requiredCount)
        {
            // Define directions for horizontal, vertical, and two diagonals
            int[][] directions = new int[][]
            {
                new int[] { 1, 0 },
                new int[] { 0, 1 },
                new int[] { 1, 1 },
                new int[] { 1, -1 }
            };

            int requiredSpaces = 5 - requiredCount;

            foreach (var direction in directions)
            {
                int dx = direction[0];
                int dy = direction[1];
                int consecutiveCount = 1;
                int spaces = 0;

                for (int i = 1; i < 5; i++)
                {
                    int newX = x + i * dx;
                    int newY = y + i * dy;

                    if (newX >= 0 && newX < BoardSize && newY >= 0 && newY < BoardSize)
                    {
                        if (Board[newX, newY] == player)
                        {
                            consecutiveCount++;
                        }
                        else if (Board[newX, newY] == EmptyCell)
                        {
                            spaces++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                for (int i = 1; i < 5; i++)
                {
                    int newX = x - i * dx;
                    int newY = y - i * dy;

                    if (newX >= 0 && newX < BoardSize && newY >= 0 && newY < BoardSize)
                    {
                        if (Board[newX, newY] == player)
                        {
                            consecutiveCount++;
                        }
                        else if (Board[newX, newY] == EmptyCell)
                        {
                            spaces++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (consecutiveCount >= requiredCount && spaces >= requiredSpaces)
                {
                    return true;
                }
            }
            return false;
        }


        public (int, int) OpponentMove(char player, int row, int col)
        {
            // if I can make 5 in row
            var winningMove = FindPotentialWin(player, 5);
            if (winningMove.Item1 != -1)
            {
                return winningMove;
            }

            // if opponent can make 5 in row
            var preventLoosing = PreventLoosing(player);
            if (preventLoosing.Item3 == 1)
            {
                return (preventLoosing.Item1, preventLoosing.Item2);
            }
            
            // if I can make 4 in row
            var findFourInRow = FindPotentialWin(player, 4);
            if (findFourInRow.Item1 != -1)
            {
                return findFourInRow;
            }

            // if opponent can make 4 in row
            if (preventLoosing.Item3 == 0)
            {
                return (preventLoosing.Item1, preventLoosing.Item2);
            }

            // if I can make 3 in row
            var findThreeInRow = FindPotentialWin(player, 3);
            if (findThreeInRow.Item1 != -1)
            {
                return findThreeInRow;
            }

            // if opponent can make 3 in row
            if (preventLoosing.Item3 == -1)
            {
                return (preventLoosing.Item1, preventLoosing.Item2);
            }

            return PreventFeatureLines(row, col);
        }
    }
}