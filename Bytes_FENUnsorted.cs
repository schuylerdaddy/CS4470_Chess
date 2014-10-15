using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UvsChess;
namespace ShallowRed
{
    public static class FENUnranked
    {
        #region constants
        //byte constants
        public static readonly byte UPPER_MASK = 0x01;
        public static readonly byte LOWER_MASK = 0xfe;
        public static readonly byte _ = 0x00;
        public static readonly byte p = 0x02;
        public static readonly byte r = 0x04;
        public static readonly byte b = 0x06;
        public static readonly byte n = 0x08;
        public static readonly byte q = 0x0A;
        public static readonly byte k = 0x0C;
        public static readonly byte P = 0x01;
        public static readonly byte R = 0x03;
        public static readonly byte B = 0x05;
        public static readonly byte N = 0x07;
        public static readonly byte Q = 0x09;
        public static readonly byte K = 0x0B;
        private static readonly int UP = 8;
        private static readonly int UPRIGHT = 9;
        private static readonly int UPLEFT = 7;
        private static readonly int UPRIGHTRIGHT = 10;
        private static readonly int UPLEFTLEFT = 6;
        private static readonly int UPUPRIGHT = 17;
        private static readonly int UPUPLEFT = 15;
        private static readonly int DOWN = -8;
        private static readonly int DOWNLEFT = -9;
        private static readonly int DOWNRIGHT = -7;
        private static readonly int DOWNLEFTLEFT = -10;
        private static readonly int DOWNRIGHTRIGHT = -6;
        private static readonly int DOWNDOWNLEFT = -17;
        private static readonly int DOWNDOWNRIGHT = -15;
        private static readonly int LEFT = -1;
        private static readonly int RIGHT = 1;
        private static readonly int UPUP = 16;
        private static readonly int DOWNDOWN = -16;
        public static readonly int OUTOFBOUNDSHIGH = 64;
        private static readonly int OUTOFBOUNDSLOW = -1;
        public static readonly int COLUMN = 8;
        private static readonly int COL8 = 7;
        private static readonly int COL7 = 6;
        private static readonly int COL1 = 0;
        private static readonly int COL2 = 1;
        #endregion
        public static AILoggerCallback Log { get; set; }


        public static bool IsValidMove(byte[] board, bool white, int to)
        {
            if (to < 0 || to >= OUTOFBOUNDSHIGH) return false;
            if (board[to] == _)
                return true;
            return !white ? board[to].IsUpper() : board[to].IsLower();
        }
        public static bool TakesOpponentPiece(byte[] board, bool white, int to)
        {
            return !white ? board[to].IsUpper() : board[to].IsLower() && board[to] > 0;
        }

        #region MapAdditions
        public static void AddAdjacentMaps(this byte[] board, bool white, int i, ref LeafList moves, bool allowCheck)
        {
            int idx = i + UP;
            byte[] temp;
            while (idx < OUTOFBOUNDSHIGH)
            {
                if (IsValidMove(board, white, idx))
                {
                    temp = board.Move(i, idx);
                    if (!InCheck(temp, white))
                        moves.Add(temp,idx);
                    if (TakesOpponentPiece(board, white, idx))
                        break;
                }
                else break;
                idx += UP;
            }
            idx = i + DOWN;
            while (idx > OUTOFBOUNDSLOW)
            {
                if (IsValidMove(board, white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp,idx);
                    if (TakesOpponentPiece(board, white, idx))
                        break;
                }
                else break;
                idx += DOWN;
            }
            idx = i % COLUMN;
            int bse = i - idx;
            while (--idx > -1)
            {
                if (IsValidMove(board, white, bse + idx))
                {
                    temp = board.Move(i, bse + idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, idx);
                    if (TakesOpponentPiece(board, white, bse + idx))
                        break;
                }
                else break;
            }
            idx = i % COLUMN;
            bse = i - idx;
            while (++idx < 8)
            {
                if (IsValidMove(board, white, bse + idx))
                {
                    temp = board.Move(i, bse + idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, idx);
                    if (TakesOpponentPiece(board, white, bse + idx))
                        break;
                }
                else break;
            }
        }

        public static void AddDiagonalMaps(this byte[] board, bool white, int i, ref LeafList moves, bool allowCheck)
        {
            byte[] temp;
            int idx = i + UPLEFT;
            while (idx < OUTOFBOUNDSHIGH && idx % COLUMN != COL8)
            {
                if (IsValidMove(board, white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, idx);
                    if (board.TakesOpponentPiece(white, idx))
                        break;
                }
                else break;
                idx += UPLEFT;
            }
            idx = i + UPRIGHT;
            while (idx < OUTOFBOUNDSHIGH && idx % COLUMN != COL1)
            {
                if (IsValidMove(board, white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, idx);
                    if (board.TakesOpponentPiece(white, idx))
                        break;
                }
                else break;
                idx += UPRIGHT;
            }
            idx = i + DOWNLEFT;
            while (idx > OUTOFBOUNDSLOW && idx % COLUMN != COL8)
            {
                if (board.IsValidMove(white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, idx);
                    if (board.TakesOpponentPiece(white, idx))
                        break;
                }
                else break;
                idx += DOWNLEFT;
            }
            idx = i + DOWNRIGHT;
            while (idx > OUTOFBOUNDSLOW && idx % COLUMN != 0)
            {
                if (board.IsValidMove(white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, idx);
                    if (board.TakesOpponentPiece(white, idx))
                        break;
                }
                else break;
                idx += DOWNRIGHT;
            }
        }
        public static void AddKnightMoves(this byte[] board, bool white, int i, ref LeafList moves, bool allowCheck)
        {
            int originRow = i % COLUMN;
            int idx = i + UPUPRIGHT;
            byte[] temp;
            if (idx < OUTOFBOUNDSHIGH && originRow < COL8 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp, idx);
            }
            idx = i + UPUPLEFT;
            if (idx < OUTOFBOUNDSHIGH && originRow > COL1 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp, idx);
            }
            idx = i + UPRIGHTRIGHT;
            if (idx < OUTOFBOUNDSHIGH && originRow < COL7 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp, idx);
            }
            idx = i + UPLEFTLEFT;
            if (idx < OUTOFBOUNDSHIGH && originRow > COL2 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp, idx);
            }
            idx = i + DOWNDOWNLEFT;
            if (idx > OUTOFBOUNDSLOW && originRow > COL1 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp, idx);
            }
            idx = i + DOWNDOWNRIGHT;
            if (idx > OUTOFBOUNDSLOW && originRow < COL8 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp, idx);
            }
            idx = i + DOWNLEFTLEFT;
            if (idx > OUTOFBOUNDSLOW && originRow > COL2 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp, idx);
            }
            idx = i + DOWNRIGHTRIGHT;
            if (idx > OUTOFBOUNDSLOW && originRow < COL7 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp, idx);
            }
        }
        public static void AddKingMoves(this byte[] board, bool white, int i, ref LeafList moves, bool allowCheck)
        {
            byte[] temp;
            int idx = i + UPLEFT;
            if (idx < OUTOFBOUNDSHIGH)
            {
                if (idx % COLUMN != COL8 && board.IsValidMove(white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, idx);
                }
                if (board.IsValidMove(white, ++idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, idx);
                }
                if (++idx % COLUMN != COL1 && board.IsValidMove(white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, idx);
                }
            }
            idx = i + RIGHT;
            if (idx % COLUMN != COL1 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp, idx);
            }
            idx = i + LEFT;
            if (idx % COLUMN != COL8 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(temp, white))
                    moves.Add(temp, idx);
            }
            idx = i + DOWNRIGHT;
            if (idx > -1)
            {
                if (i % COLUMN != COL8 && board.IsValidMove(white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, idx);
                }
                if (board.IsValidMove(white, --idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, idx);
                }
                if (i % COLUMN != COL1 && board.IsValidMove(white, --idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, idx);
                }
            }
        }
        public static void AddPawnMoves(this byte[] board, bool white, int i, ref LeafList moves, bool allowCheck)
        {
            byte[] temp;
            if (!white)
            {
                if (i / COLUMN == 1)
                {
                    if (board[i + UPUP] == _ && board[i + UP] == _)
                    {
                        temp = board.MovePawn(i, i + UPUP, white);
                        if (allowCheck || allowCheck || !InCheck(temp, white))
                            moves.Add(temp, i + UPUP);
                    }
                }
                if (board[i + UP] == _)
                {
                    temp = board.MovePawn(i, i + UP, white);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, i + UP);
                }
                if (board[i + UPLEFT].IsUpper() && (i + UPLEFT) % COLUMN != COL8)
                {
                    temp = board.MovePawn(i, i + UPLEFT, white);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, i + UPLEFT);
                }
                if (i < 54 && board[i + UPRIGHT].IsUpper() && (i + UPRIGHT) % COLUMN != COL1)
                {
                    temp = board.MovePawn(i, i + UPRIGHT, white);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, i + UPRIGHT);
                }
            }
            else
            {
                if (i / COLUMN == COL7)
                {
                    if (board[i + DOWNDOWN] == _ && board[i + DOWN] == _)
                    {
                        temp = board.MovePawn(i, i + DOWNDOWN, white);
                        if (allowCheck || !InCheck(temp, white))
                            moves.Add(temp, i + DOWNDOWN);
                    }
                }
                if (board[i + DOWN] == _)
                {
                    temp = board.MovePawn(i, i + DOWN, white);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, i + DOWN);
                }
                if (board[i + DOWNRIGHT].IsLower() && (i + DOWNRIGHT) % COLUMN != COL1)
                {
                    temp = board.MovePawn(i, i + DOWNRIGHT, white);
                    if (allowCheck || !InCheck(temp, white))
                        moves.Add(temp, i + DOWNRIGHT);
                }
                if (i > 8 && board[i + DOWNLEFT].IsLower() && (i + DOWNLEFT) % COLUMN != COL8)
                {
                    temp = board.MovePawn(i, i + DOWNLEFT, white);
                    if (!InCheck(temp, white))
                        moves.Add(temp, i + DOWNLEFT);
                }
            }
        }
        #endregion
        #region 'check' algorithms
        public static bool PieceNotSafe(byte[] board, int piecePosition, bool white)
        {
            return DiagonalCheck(board, white, piecePosition) || KnightCheck(board, white, piecePosition) || ColRowCheck(board, white, piecePosition);
        }
        public static bool InCheck(byte[] board, bool white)
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
        private static bool KnightCheck(byte[] board, bool white, int pos)
        {
            bool check = false;
            byte enemyKnight;
            if (white)
            {
                enemyKnight = n;
            }
            else
            {
                enemyKnight = N;
            }
            // Array of all possible attacking knight positions
            int[] knights = new int[8] { pos + DOWNLEFTLEFT, pos + DOWNDOWNLEFT, pos + UPLEFTLEFT, pos + UPUPLEFT, pos + UPRIGHTRIGHT, pos + UPUPRIGHT, pos + DOWNRIGHTRIGHT, pos + DOWNDOWNRIGHT };
            for (int i = 0; i < knights.Length; i++)
            {
                int idx = knights[i];
                int toX = idx % COLUMN;
                // left moves
                if (i < 4)
                {
                    if (toX < pos % COLUMN && idx > OUTOFBOUNDSLOW && idx < OUTOFBOUNDSHIGH)
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
                    if (toX > pos % COLUMN && idx > OUTOFBOUNDSLOW && idx < OUTOFBOUNDSHIGH)
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
        private static bool ColRowCheck(byte[] board, bool white, int kingPos)
        {
            byte enemyRook;
            byte enemyQueen;
            byte enemyKing;
            if (white)
            {
                enemyRook = r;
                enemyQueen = q;
                enemyKing = k;
            }
            else
            {
                enemyRook = R;
                enemyQueen = Q;
                enemyKing = K;
            }
            // For each direction
            #region move_UP
            int pos = kingPos + UP;
            //Check first space for king as well as rook and queen
            if (pos < OUTOFBOUNDSHIGH)
            {
                if (board[pos] == enemyKing)
                {
                    return true;
                }
            }
            // Move in that direction until you hit an edge or another piece
            while (pos < OUTOFBOUNDSHIGH)
            {
                // If you hit a rook or a queen we're in check
                if (board[pos] == enemyRook || board[pos] == enemyQueen)
                {
                    //this.Log(" - " + color + " is in check on ColRow from " + pos.ToString());
                    return true;
                }
                // If you hit something that's not enemy q or r, but isn't blank, then we're safe.
                else if (board[pos] != _)
                {
                    break;
                }
                pos += UP;
            }
            #endregion
            #region move_DOWN
            pos = kingPos + DOWN;
            //Check first space for king as well as rook and queen
            if (pos > OUTOFBOUNDSLOW)
            {
                if (board[pos] == enemyKing)
                {
                    return true;
                }
            }
            // Move in that direction until you hit an edge or another piece
            while (pos > OUTOFBOUNDSLOW)
            {
                // If you hit a rook or a queen we're in check
                if (board[pos] == enemyRook || board[pos] == enemyQueen)
                {
                    //this.Log(" - " + color + " is in check on ColRow from " + pos.ToString());
                    return true;
                }
                // If you hit something that's not enemy q or r, but isn't blank, then we're safe.
                else if (board[pos] != _)
                {
                    break;
                }
                pos += DOWN;
            }
            #endregion
            #region moveRIGHT
            pos = kingPos + RIGHT;
            //Check first space for king as well as rook and queen
            if (pos % COLUMN != COL1)
            {
                if (board[pos] == enemyKing)
                {
                    return true;
                }
            }
            // Move in that direction until you hit an edge or another piece
            while (pos % COLUMN != COL1)
            {
                // If you hit a rook or a queen we're in check
                if (board[pos] == enemyRook || board[pos] == enemyQueen)
                {
                    //this.Log(" - " + color + " is in check on ColRow from " + pos.ToString());
                    return true;
                }
                // If you hit something that's not enemy q or r, but isn't blank, then we're safe.
                else if (board[pos] != _)
                {
                    break;
                }
                pos += RIGHT;
            }
            #endregion
            #region moveLEfT
            pos = kingPos + LEFT;
            //Check first space for king as well as rook and queen
            if (pos > OUTOFBOUNDSLOW && pos % COLUMN != COL8)
            {
                if (board[pos] == enemyKing)
                {
                    return true;
                }
            }
            // Move in that direction until you hit an edge or another piece
            while (pos > OUTOFBOUNDSLOW && pos % COLUMN != COL8)
            {
                // If you hit a rook or a queen we're in check
                if (board[pos] == enemyRook || board[pos] == enemyQueen)
                {
                    //this.Log(" - " + color + " is in check on ColRow from " + pos.ToString());
                    return true;
                }
                // If you hit something that's not enemy q or r, but isn't blank, then we're safe.
                else if (board[pos] != _)
                {
                    break;
                }
                pos += LEFT;
            }
            #endregion
            return false;
        }
        private static bool DiagonalCheck(byte[] board, bool white, int kingPos)
        {
            byte enemyBishop;
            byte enemyPawn;
            byte enemyQueen;
            byte enemyKing;
            if (white)
            {
                //this.Log("Checking Diagonals for White");
                enemyBishop = b;
                enemyQueen = q;
                enemyPawn = p;
                enemyKing = k;
            }
            else
            {
                //this.Log("Checking Diagonals for Black");
                enemyBishop = B;
                enemyQueen = Q;
                enemyPawn = P;
                enemyKing = K;
            }
            // For each advance direction
            #region UPLEFT
            int pos = kingPos + UPLEFT;
            // Check Pawns:
            if (pos < OUTOFBOUNDSHIGH && pos % COLUMN != COL8)
            {
                if ((board[pos] == enemyPawn && !white) || board[pos] == enemyKing)
                {
                    return true;
                }
            }
            // Move in that direction until you hit an edge or another piece
            while (pos < OUTOFBOUNDSHIGH && pos % COLUMN != COL8)
            {
                // If you hit a bishop or a queen we're in check
                if (board[pos] == enemyBishop || board[pos] == enemyQueen)
                {
                    //this.Log(" - " + color + " king (at pos " + kingPos + ") is in check on Diagonals from pos " + pos);
                    return true;
                }
                // If you hit something that's not enemy q or r, but isn't blank, then we're safe.
                else if (board[pos] != _)
                {
                    break;
                }
                pos += UPLEFT;
            }
            #endregion UPLEFT
            #region UPRIGHT
            pos = kingPos + UPRIGHT;
            // Check Pawns:
            if (pos < OUTOFBOUNDSHIGH && pos % COLUMN != COL1)
            {
                if ((board[pos] == enemyPawn && !white) || board[pos] == enemyKing)
                {
                    return true;
                }
            }
            // Move in that direction until you hit an edge or another piece
            while (pos < OUTOFBOUNDSHIGH && pos % COLUMN != COL1)
            {
                // If you hit a bishop or a queen we're in check
                if (board[pos] == enemyBishop || board[pos] == enemyQueen)
                {
                    //this.Log(" - " + color + " king (at pos " + kingPos + ") is in check on Diagonals from pos " + pos);
                    return true;
                }
                // If you hit something that's not enemy q or r, but isn't blank, then we're safe.
                else if (board[pos] != _)
                {
                    break;
                }
                pos += UPRIGHT;
            }
            #endregion UPRIGHT
            #region DOWNLEFT
            pos = kingPos + DOWNLEFT;
            // Check Pawns:
            if (pos > OUTOFBOUNDSLOW && pos % COLUMN != COL8)
            {
                if ((board[pos] == enemyPawn && white) || board[pos] == enemyKing)
                {
                    return true;
                }
            }
            // Move in that direction until you hit an edge or another piece
            while (pos > OUTOFBOUNDSLOW && pos % COLUMN != COL8)
            {
                // If you hit a bishop or a queen we're in check
                if (board[pos] == enemyBishop || board[pos] == enemyQueen)
                {
                    //this.Log(" - " + color + " king (at pos " + kingPos + ") is in check on Diagonals from pos " + pos);
                    return true;
                }
                // If you hit something that's not enemy q or r, but isn't blank, then we're safe.
                else if (board[pos] != _)
                {
                    break;
                }
                pos += DOWNLEFT;
            }
            #endregion
            #region DOWNRIGHT
            pos = kingPos + DOWNRIGHT;
            // Check Pawns:
            if (pos > OUTOFBOUNDSLOW && pos % COLUMN != COL1)
            {
                if ((board[pos] == enemyPawn && white) || board[pos] == enemyKing)
                {
                    return true;
                }
            }
            // Move in that direction until you hit an edge or another piece
            while (pos > OUTOFBOUNDSLOW && pos % COLUMN != COL1)
            {
                // If you hit a bishop or a queen we're in check
                if (board[pos] == enemyBishop || board[pos] == enemyQueen)
                {
                    //this.Log(" - " + color + " king (at pos " + kingPos + ") is in check on Diagonals from pos " + pos);
                    return true;
                }
                // If you hit something that's not enemy q or r, but isn't blank, then we're safe.
                else if (board[pos] != _)
                {
                    break;
                }
                pos += DOWNRIGHT;
            }
            #endregion
            return false;
        }
        private static int GetKingPos(byte[] board, bool white)
        {
            byte kingChar = white ? K : k;
            for (int i = 0; i < OUTOFBOUNDSHIGH; ++i)
            {
                if (board[i] == kingChar)
                {
                    return i;
                }
            }
            return -1;
        }
        public static LeafList GetAvailableMoves(byte[] board, ChessColor color, bool allowCheck)
        {
            bool white = color == ChessColor.White;
            LeafList moves = new LeafList();
            //iterate thru entire board {64} including row delimiters {7}
            for (int i = 0; i < OUTOFBOUNDSHIGH; ++i)
            {
                if (!white)
                {
                    switch (board[i])
                    {
                        case 0x02:
                            board.AddPawnMoves(white, i, ref moves, allowCheck);
                            break;
                        case 0x04:
                            board.AddAdjacentMaps(white, i, ref moves, allowCheck);
                            break;
                        case 0x06:
                            board.AddDiagonalMaps(white, i, ref moves, allowCheck);
                            break;
                        case 0x08:
                            board.AddKnightMoves(white, i, ref moves, allowCheck);
                            break;
                        case 0x0A:
                            board.AddAdjacentMaps(white, i, ref moves, allowCheck);
                            board.AddDiagonalMaps(white, i, ref moves, allowCheck);
                            break;
                        case 0x0C:
                            board.AddKingMoves(white, i, ref moves, allowCheck);
                            break;
                        default: break;
                    }
                }
                else
                {
                    switch (board[i])
                    {
                        case 0x01:
                            board.AddPawnMoves(white, i, ref moves, allowCheck);
                            break;
                        case 0x03:
                            board.AddAdjacentMaps(white, i, ref moves, allowCheck);
                            break;
                        case 0x05:
                            board.AddDiagonalMaps(white, i, ref moves, allowCheck);
                            break;
                        case 0x07:
                            board.AddKnightMoves(white, i, ref moves, allowCheck);
                            break;
                        case 0x09:
                            board.AddAdjacentMaps(white, i, ref moves, allowCheck);
                            board.AddDiagonalMaps(white, i, ref moves, allowCheck);
                            break;
                        case 0x0B:
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
