namespace TicTacToe.Models
{
    public class MakeMove
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public char Player { get; set; }

        public MakeMove(int x, int y, char player)
        {
            Row = x;
            Col = y;
            Player = player;
        }
    }
}