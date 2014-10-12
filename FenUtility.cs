using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UvsChess;
namespace ShallowRed
{
    public static class FENExtensions
    {
        public static AILoggerCallback Log { get; set; }
        public static ChessMove GenerateMove(this char[] from, char[] to)
        {
            int[] pos = new int[2];
            int changes = 0;
            for (int i = 0; i < from.Length && changes < 2; ++i)
            {
                if (from[i] != to[i])
                {
                    pos[changes++] = i;
                }
            }
            if (to[pos[1]] == '_') //spot vacated
                return new ChessMove(new ChessLocation(pos[1] % 9, pos[1] / 9), new ChessLocation(pos[0] % 9, pos[0] / 9));
            return new ChessMove(new ChessLocation(pos[0] % 9, pos[0] / 9), new ChessLocation(pos[1] % 9, pos[1] / 9));
        }
        public static char[] ToShallowRedFEN(this string originalFen)
        {
            string newForm = "";
            foreach (char c in originalFen)
            {
                if (!char.IsDigit(c)) newForm += c;
                else
                {
                    int count = Int32.Parse(c.ToString());
                    for (int i = 0; i < count; ++i)
                    {
                        newForm += "_";
                    }
                }
            }
            return newForm.ToArray();
        }
        public static char[] Move(this char[] board, int fromX, int fromY, int toX, int toY)
        {
            return board.Move(fromX + (9 * fromY), toX + (9 * toY));
        }
        public static char[] Move(this char[] board, int from, int to)
        {
            char[] b = (char[])board.Clone();
            b[to] = b[from];
            b[from] = '_';
            
            return b;
        }
        public static char[] MovePawn(this char[] board, int from, int to, bool white)
        {
            char[] b = (char[])board.Clone();
            if (white)
            {
                b[to] = (to < 8) ? 'Q' : 'P';
                b[from] = '_';
            }
            else
            {
                b[to] = (to > 62) ? 'q' : 'p';
                b[from] = '_';
            }
            return b;
        }
        public static bool IsValidMove(this char[] board, bool white, int to)
        {
            if (to < 0 || to >= 71) return false;
            if (board[to] == '_')
                return true;
            return !white ? Char.IsUpper(board[to]) : Char.IsLower(board[to]);
        }
        public static bool TakesOpponentPiece(this char[] board, bool white, int to)
        {
            return !white ? (Char.IsUpper(board[to])) : (Char.IsLower(board[to]));
        }
        private static bool EmptySpace(this char[] board, bool white, int to)
        {
            return board[to] == '_';
        }

        #region MapAdditions

        public static void AddAdjacentMaps(this char[] board, bool white, int i, ref LightList moves, bool allowCheck)
        {
            int idx = i + 9;
            char[] temp;
            while (idx < 71)
            {
                if (IsValidMove(board, white, idx))
                {
                    temp = board.Move(i, idx);
                    if (!InCheck(temp, white))
                        moves.Add(temp);
                    if (TakesOpponentPiece(board, white, idx))
                        break;
                }
                else break;
                idx += 9;
            }
            idx = i - 9;
            while (idx > -1)
            {
                if (IsValidMove(board, white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                    if (TakesOpponentPiece(board, white, idx))
                        break;
                }
                else break;
                idx -= 9;
            }
            idx = i % 9;
            int bse = i - idx;
            while (--idx > -1)
            {
                if (IsValidMove(board, white, bse + idx))
                {
                    temp = board.Move(i, bse + idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                    if (TakesOpponentPiece(board, white, bse + idx))
                        break;
                }
                else break;
            }
            idx = i % 9;
            bse = i - idx;
            while (++idx < 8)
            {
                if (IsValidMove(board, white, bse + idx))
                {
                    temp = board.Move(i, bse + idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                    if (TakesOpponentPiece(board, white, bse + idx))
                        break;
                }
                else break;
            }
        }
        public static void AddDiagonalMaps(this char[] board, bool white, int i, ref LightList moves, bool allowCheck)
        {
            char[] temp;
            int idx = i + 8;
            while (idx < 71 && idx % 9 >= 0)
            {
                if (IsValidMove(board, white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                    if (board.TakesOpponentPiece(white, idx))
                        break;
                }
                else break;
                idx += 8;
            }
            idx = i + 10;
            while (idx < 71 && idx % 9 <= 8)
            {
                if (IsValidMove(board, white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                    if (board.TakesOpponentPiece(white, idx))
                        break;
                }
                else break;
                idx += 10;
            }
            idx = i - 10;
            while (idx > -1 && idx % 9 >= 0)
            {
                if (board.IsValidMove(white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                    if (board.TakesOpponentPiece(white, idx))
                        break;
                }
                else break;
                idx -= 10;
            }
            idx = i - 8;
            while (idx > -1 && idx % 9 <= 8)
            {
                if (board.IsValidMove(white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                    if (board.TakesOpponentPiece(white, idx))
                        break;
                }
                else break;
                idx -= 8;
            }
        }
        public static void AddKnightMoves(this char[] board, bool white, int i, ref LightList moves, bool allowCheck)
        {
            int originRow = i % 9;
            int idx = i + 19;
            char[] temp;
            if (idx < 71 && originRow < 7 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp);
            }
            idx = i + 17;
            if (idx < 71 && originRow > 0 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp);
            }
            idx = i + 11;
            if (idx < 71 && originRow < 6 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp);
            }
            idx = i + 7;
            if (idx < 71 && originRow % 9 > 1 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp);
            }
            idx = i - 19;
            if (idx > -1 && originRow > 0 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp);
            }
            idx = i - 17;
            if (idx > -1 && originRow < 7 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp);
            }
            idx = i - 11;
            if (idx > -1 && originRow > 1 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp);
            }
            idx = i - 7;
            if (idx > -1 && originRow < 6 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp);
            }
        }
        public static void AddKingMoves(this char[] board, bool white, int i, ref LightList moves, bool allowCheck)
        {
            char[] temp;
            int idx = i + 8;
            if (idx < 71)
            {
                if (idx % 9 != 8 && board.IsValidMove(white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                }
                if (board.IsValidMove(white, ++idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                }
                if (++idx % 9 != 8 && board.IsValidMove(white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                }
            }
            idx = i + 1;
            if (idx % 9 != 0 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp);
            }
            idx = i - 1;
            if (idx % 9 != 8 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp);
            }
            idx = i - 8;
            if (idx > -1)
            {
                if (idx % 9 != 0 && board.IsValidMove(white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                }
                if (board.IsValidMove(white, --idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                }
                if (idx % 9 != 0 && board.IsValidMove(white, --idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                }
            }
        }
        public static void AddPawnMoves(this char[] board, bool white, int i, ref LightList moves,bool allowCheck)
        {
            char[] temp;
            if (!white)
            {
                if (i / 9 == 1)
                {
                    if (board[i + 18] == '_' && board[i + 9] == '_')
                    {
                        temp = board.MovePawn(i, i + 18, white);
                        if (allowCheck || allowCheck || !InCheck(temp, white))
                            moves.Add(temp);
                    }
                }
                if (board[i + 9] == '_')
                {
                    temp = board.MovePawn(i, i + 9, white);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                }
                if (Char.IsUpper(board[i + 8]))
                {
                    temp = board.MovePawn(i, i + 8, white);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                }
                if (i < 61 && Char.IsUpper(board[i + 10]))
                {
                    temp = board.MovePawn(i, i + 10, white);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                }
            }
            else
            {
                if (i / 9 == 6)
                {
                    if (board[i - 18] == '_' && board[i - 9] == '_')
                    {
                        temp = board.MovePawn(i, i - 18, white);
                        if (allowCheck || !InCheck(temp, white))
                            moves.Add(temp);
                    }
                }
                if (board[i - 9] == '_')
                {
                    temp = board.MovePawn(i, i - 9, white);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                }
                if (Char.IsLower(board[i - 8]))
                {
                    temp = board.MovePawn(i, i - 8, white);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp);
                }
                if (i > 10 && Char.IsLower(board[i - 10]))
                {
                    temp = board.MovePawn(i, i - 10, white);
                    if (!InCheck(board, white))
                        moves.Add(temp);
                }
            }
        }

        #endregion

        #region 'check' algorithms

        public static bool PieceNotSafe(char[] board, int piecePosition, bool white)
        {
            return DiagonalCheck(board, white, piecePosition) || KnightCheck(board, white, piecePosition) || ColRowCheck(board, white, piecePosition);
        }

        public static bool InCheck(char[] board, bool white)
        {
            int kingPos = GetKingPos(board, white);
            bool check = DiagonalCheck(board, white, kingPos) || KnightCheck(board, white, kingPos) || ColRowCheck(board, white, kingPos);
            /*
            if (check){
            this.Log("- Yep, we're definitely in check!");
            }
            else {
            this.Log("- Nope, no check here.");
            }
            */
            return check;
        }
        private static bool KnightCheck(char[] board, bool white, int pos)
        {
            bool check = false;
            char enemyKnight;
            if (white)
            {
                enemyKnight = 'n';
            }
            else
            {
                enemyKnight = 'N';
            }
            // Array of all possible attacking knight positions
            int[] knights = new int[8] { pos - 11, pos - 19, pos + 7, pos + 17, pos + 11, pos + 19, pos - 7, pos - 17 };
            for (int i = 0; i < knights.Length; i++)
            {
                int idx = knights[i];
                int toX = idx % 9;
                // left moves
                if (i < 4)
                {
                    if (toX < pos % 9 && idx > 0 && idx < 71)
                    {
                        if (board[idx] == enemyKnight)
                        {
                            //this.Log("In Check by Knight at pos: " + idx);
                            check = true;
                            break;
                        }
                    }
                }
                else //right moves
                {
                    if (toX > pos % 9 && toX != 8 && idx > 0 && idx < 71)
                    {
                        if (board[idx] == enemyKnight)
                        {
                            //this.Log("In Check by Knight at pos: " + idx);
                            check = true;
                            break;
                        }
                    }
                }
            }
            return check;
        }
        private static bool ColRowCheck(char[] board, bool white, int kingPos)
        {
            bool check = false;
            char enemyRook;
            char enemyQueen;
            char enemyKing;
            if (white)
            {
                enemyRook = 'r';
                enemyQueen = 'q';
                enemyKing = 'k';
            }
            else
            {
                enemyRook = 'R';
                enemyQueen = 'Q';
                enemyKing = 'K';
            }
            int[] directions = new int[4] {
-9, // up
9, // down
-1, // left
1 // right
};
            // For each direction
            for (int i = 0; i < directions.Length; i++)
            {
                int shift = directions[i];
                int pos = kingPos + shift;
                //Check first space for king as well as rook and queen
                if ((pos > -1 && pos < 71 && pos % 9 != 8))
                {
                    if (board[pos] == enemyKing)
                    {
                        check = true;
                        break;
                    }
                }
                // Move in that direction until you hit an edge or another piece
                while (pos > -1 && pos < 71 && pos % 9 != 8)
                {
                    // If you hit a rook or a queen we're in check
                    if (board[pos] == enemyRook || board[pos] == enemyQueen)
                    {
                        //this.Log(" - " + color + " is in check on ColRow from " + pos.ToString());
                        check = true;
                        break;
                    }
                    // If you hit something that's not enemy q or r, but isn't blank, then we're safe.
                    else if (board[pos] != '_')
                    {
                        // check is already false
                        break;
                    }
                    pos += shift;
                }
            }
            /*
            if (check)
            {
            this.Log("- We are in check on Col/Row!!");
            }
            */
            return check;
        }
        private static bool DiagonalCheck(char[] board, bool white, int kingPos)
        {
            bool check = false;
            char enemyBishop;
            char enemyPawn;
            char enemyQueen;
            char enemyKing;
            int[] advanceDiagonals;
            int[] retreatDiagonals;
            string color;
            if (white)
            {
                color = "White";
                //this.Log("Checking Diagonals for White");
                enemyBishop = 'b';
                enemyQueen = 'q';
                enemyPawn = 'p';
                enemyKing = 'k';
                advanceDiagonals = new int[2] {
-10, // up left
-8 // up right
};
                retreatDiagonals = new int[2] {
10, // down left
8 // down right
};
            }
            else
            {
                color = "Black";
                //this.Log("Checking Diagonals for Black");
                enemyBishop = 'B';
                enemyQueen = 'Q';
                enemyPawn = 'P';
                enemyKing = 'K';
                advanceDiagonals = new int[2] {
10, // down left
8 // down right
};
                retreatDiagonals = new int[2] {
-10, // up left
-8 // up right
};
            }
            // For each advance direction
            for (int i = 0; i < advanceDiagonals.Length; i++)
            {
                int shift = advanceDiagonals[i];
                int pos = kingPos + shift;
                // Check Pawns:
                if (pos > 0 && pos < 71 && pos % 9 != 8)
                {
                    if (board[pos] == enemyPawn || board[pos] == enemyKing)
                    {
                        check = true;
                        break;
                    }
                }
                // Move in that direction until you hit an edge or another piece
                while (pos > -1 && pos < 71 && pos % 9 != 8)
                {
                    // If you hit a bishop or a queen we're in check
                    if (board[pos] == enemyBishop || board[pos] == enemyQueen)
                    {
                        //this.Log(" - " + color + " king (at pos " + kingPos + ") is in check on Diagonals from pos " + pos);
                        check = true;
                        break;
                    }
                    // If you hit something that's not enemy q or r, but isn't blank, then we're safe.
                    else if (board[pos] != '_')
                    {
                        // check is already false
                        break;
                    }
                    pos += shift;
                }
            }
            // For each retreat direction
            for (int i = 0; i < retreatDiagonals.Length; i++)
            {
                int shift = retreatDiagonals[i];
                int pos = kingPos + shift;
                // Move in that direction until you hit an edge or another piece
                while (pos > -1 && pos < 71 && pos % 9 != 8)
                {
                    // If you hit a bishop or a queen we're in check
                    if (board[pos] == enemyBishop || board[pos] == enemyQueen)
                    {
                        //this.Log(" - " + color + " king (at pos " + kingPos + ") is in check on Diagonals from pos " + pos);
                        check = true;
                        break;
                    }
                    // If you hit something that's not enemy q or r, but isn't blank, then we're safe.
                    else if (board[pos] != '_')
                    {
                        // check is already false
                        break;
                    }
                    pos += shift;
                }
            }
            return check;
        }

        private static int GetKingPos(char[] board, bool white)
        {
            char kingChar = white ? 'K' : 'k';
            for (int i = 0; i < 71; ++i)
            {
                if (board[i] == kingChar)
                {
                    return i;
                }
            }
            return 0;
        }

        public static int GetPiecePos(char[] board, bool white, char piece)
        {
            char pieceChar = white ? Char.ToUpper(piece) : Char.ToLower(piece);
            for (int i = 0; i < 71; ++i)
            {
                if (board[i] == pieceChar)
                {
                    return i;
                }
            }
            return 0;
        }
    }
        #endregion

    #region FEN class {get availmoves}
    public static class FEN
    {
        public static AILoggerCallback Log { get; set; }

        public static LightList GetAvailableMoves(char[] board, ChessColor color, bool allowCheck)
        {
            bool white = color == ChessColor.White;
            LightList moves = new LightList();
            //iterate thru entire board {64} including row delimiters {7}
            for (int i = 0; i < 71; ++i)
            {
                if (!white)
                {
                    switch (board[i])
                    {
                        case 'p':
                            board.AddPawnMoves(white, i, ref moves,allowCheck);
                            break;
                        case 'r':
                            board.AddAdjacentMaps(white, i, ref moves,allowCheck);
                            break;
                        case 'b':
                            board.AddDiagonalMaps(white, i, ref moves, allowCheck);
                            break;
                        case 'n':
                            board.AddKnightMoves(white, i, ref moves, allowCheck);
                            break;
                        case 'q':
                            board.AddAdjacentMaps(white, i, ref moves, allowCheck);
                            board.AddDiagonalMaps(white, i, ref moves, allowCheck);
                            break;
                        case 'k':
                            board.AddKingMoves(white, i, ref moves, allowCheck);
                            break;
                        default: break;
                    }
                }
                else
                {
                    switch (board[i])
                    {
                        case 'P':
                            board.AddPawnMoves(white, i, ref moves, allowCheck);
                            break;
                        case 'R':
                            board.AddAdjacentMaps(white, i, ref moves, allowCheck);
                            break;
                        case 'B':
                            board.AddDiagonalMaps(white, i, ref moves, allowCheck);
                            break;
                        case 'N':
                            board.AddKnightMoves(white, i, ref moves, allowCheck);
                            break;
                        case 'Q':
                            board.AddAdjacentMaps(white, i, ref moves, allowCheck);
                            board.AddDiagonalMaps(white, i, ref moves, allowCheck);
                            break;
                        case 'K':
                            board.AddKingMoves(white, i, ref moves, allowCheck);
                            break;
                        default: break;
                    }
                }
            }
            return moves;
        }
    }
#endregion
}


