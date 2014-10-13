using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UvsChess;

namespace ShallowRed
{
    public static class FEN
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

        public static bool IsUpper(this byte b)
        {
            return (b & UPPER_MASK) == 1;
        }
        public static bool IsLower(this byte b)
        {
            return (b & UPPER_MASK) == 0 && b != 0;
        }

        public static Predicate<byte> IsOpponent;

        public static byte ToUpper(this byte b)
        {
            return b |= UPPER_MASK;
        }
        public static byte ToLower(this byte b)
        {
            return b &= LOWER_MASK;
        }
        public static ChessMove GenerateMove(this byte[] from, byte[] to)
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
            if (to[pos[1]] == _) //spot vacated
                return new ChessMove(new ChessLocation(pos[1] % COLUMN, pos[1] / COLUMN), new ChessLocation(pos[0] % COLUMN, pos[0] / COLUMN));
            return new ChessMove(new ChessLocation(pos[0] % COLUMN, pos[0] / COLUMN), new ChessLocation(pos[1] % COLUMN, pos[1] / COLUMN));
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
        public static byte[] ToShallowRedFENBytes(this string originalFen)
        {
            byte[] bytes = new byte[OUTOFBOUNDSHIGH];
            int byteCtr = 0;
            for (int i = 0; i < originalFen.Length; ++i)
            {
                char c = originalFen[i];
                if (!char.IsDigit(c))
                {
                    switch (c)
                    {
                        case 'p': bytes[byteCtr++] = p; break;
                        case 'P': bytes[byteCtr++] = P; break;
                        case 'r': bytes[byteCtr++] = r; break;
                        case 'R': bytes[byteCtr++] = R; break;
                        case 'b': bytes[byteCtr++] = b; break;
                        case 'B': bytes[byteCtr++] = B; break;
                        case 'q': bytes[byteCtr++] = q; break;
                        case 'Q': bytes[byteCtr++] = Q; break;
                        case 'k': bytes[byteCtr++] = k; break;
                        case 'K': bytes[byteCtr++] = K; break;
                        case 'n': bytes[byteCtr++] = n; break;
                        case 'N': bytes[byteCtr++] = N; break;
                    }
                }
                else
                {
                    int count = Int32.Parse(c.ToString());
                    for (int j = 0; j < count; ++j)
                    {
                        bytes[byteCtr++] = _;
                    }
                }
            }
            return bytes;
        }
        public static byte[] Move(this byte[] board, int fromX, int fromY, int toX, int toY)
        {
            return board.Move(fromX + (COLUMN * fromY), toX + (COLUMN * toY));
        }
        public static byte[] Move(this byte[] board, int from, int to)
        {
            byte[] b = (byte[])board.Clone();
            b[to] = b[from];
            b[from] = _;
            return b;
        }
        public static byte[] MovePawn(this byte[] board, int from, int to, bool white)
        {
            byte[] b = (byte[])board.Clone();
            if (white)
            {
                b[to] = (to < 8) ? Q : P;
                b[from] = _;
            }
            else
            {
                b[to] = (to > 53) ? q : p;
                b[from] = _;
            }
            return b;
        }
        public static bool IsValidMove(this byte[] board, bool white, int to)
        {
            if (to < 0 || to >= OUTOFBOUNDSHIGH) return false;
            if (board[to] == _)
                return true;
            return !white ? board[to].IsUpper() : board[to].IsLower();
        }
        public static bool TakesOpponentPiece(this byte[] board, bool white, int to)
        {
            return !white ? board[to].IsUpper() : board[to].IsLower() && board[to] > 0;
        }
        private static bool EmptySpace(this char[] board, bool white, int to)
        {
            return board[to] == _;
        }

        #region threat assesment algortihms

        public static bool AdjacentThreat(byte[] board, int i)
        {
            if (board[i].IsUpper()) //case white
            {
                int idx = i + UP;
                while (idx < OUTOFBOUNDSHIGH)
                {
                    if (board[idx].IsLower())
                    {
                        return true;
                    }
                    else if (board[idx].IsUpper())
                    {
                        break;
                    }
                    idx += UP;
                }

                idx = i + DOWN;
                while (idx > OUTOFBOUNDSLOW)
                {
                    if (board[idx].IsLower())
                    {
                        return true;
                    }
                    else if (board[idx].IsUpper())
                    {
                        break;
                    }
                    idx += DOWN;
                }

                idx = i + RIGHT;
                while (idx % COLUMN != COL1)
                {
                    if (board[idx].IsLower())
                    {
                        return true;
                    }
                    else if (board[idx].IsUpper())
                    {
                        break;
                    }
                    idx += RIGHT;
                }

                idx = i + LEFT;
                while (idx > OUTOFBOUNDSLOW && idx % COLUMN != COL8)
                {
                    if (board[idx].IsLower())
                    {
                        return true;
                    }
                    else if (board[idx].IsUpper())
                    {
                        break;
                    }
                    idx += LEFT;
                }
            }
            else //case lower
            {
                int idx = i + UP;
                while (idx < OUTOFBOUNDSHIGH)
                {
                    if (board[idx].IsLower())
                    {
                        break;
                    }
                    else if (board[idx].IsUpper())
                    {
                        return true;
                    }
                    idx += UP;
                }

                idx = i + DOWN;
                while (idx > OUTOFBOUNDSLOW)
                {
                    if (board[idx].IsLower())
                    {
                        break;
                    }
                    else if (board[idx].IsUpper())
                    {
                        return true;
                    }
                    idx += DOWN;
                }

                idx = i + RIGHT;
                while (idx % COLUMN != COL1)
                {
                    if (board[idx].IsLower())
                    {
                        break;
                    }
                    else if (board[idx].IsUpper())
                    {
                        return true;
                    }
                    idx += RIGHT;
                }

                idx = i + LEFT;
                while (idx > OUTOFBOUNDSLOW && idx % COLUMN != COL8)
                {
                    if (board[idx].IsLower())
                    {
                        break;
                    }
                    else if (board[idx].IsUpper())
                    {
                        return true;
                    }
                    idx += LEFT;
                }
            }
            return false;
        }

        public static bool DiagThreat(byte[] board, int i)
        {
            if (board[i].IsUpper()) //case white
            {
                int idx = i + UPRIGHT;
                while (idx < OUTOFBOUNDSHIGH && idx % COLUMN != COL1)
                {
                    if (board[idx].IsLower())
                    {
                        return true;
                    }
                    else if (board[idx].IsUpper())
                    {
                        break;
                    }
                    idx += UPRIGHT;
                }

                idx = i + UPLEFT;
                while (idx < OUTOFBOUNDSHIGH && idx % COLUMN != COL8)
                {
                    if (board[idx].IsLower())
                    {
                        return true;
                    }
                    else if (board[idx].IsUpper())
                    {
                        break;
                    }
                    idx += UPLEFT;
                }

                idx = i + DOWNRIGHT;
                while (idx > OUTOFBOUNDSLOW && idx % COLUMN != COL1)
                {
                    if (board[idx].IsLower())
                    {
                        return true;
                    }
                    else if (board[idx].IsUpper())
                    {
                        break;
                    }
                    idx += DOWNRIGHT;
                }

                idx = i + DOWNLEFT;
                while (idx > OUTOFBOUNDSLOW && idx % COLUMN != COL8)
                {
                    if (board[idx].IsLower())
                    {
                        return true;
                    }
                    else if (board[idx].IsUpper())
                    {
                        break;
                    }
                    idx += DOWNLEFT;
                }
            }
            else //case lower
            {
                int idx = i + UPRIGHT;
                while (idx < OUTOFBOUNDSHIGH && idx % COLUMN != COL1)
                {
                    if (board[idx].IsLower())
                    {
                        break;
                    }
                    else if (board[idx].IsUpper())
                    {
                        return true;
                    }
                    idx += UPRIGHT;
                }

                idx = i + UPLEFT;
                while (idx < OUTOFBOUNDSHIGH && idx % COLUMN != COL8)
                {
                    if (board[idx].IsLower())
                    {
                        break;
                    }
                    else if (board[idx].IsUpper())
                    {
                        return true;
                    }
                    idx += UPLEFT;
                }

                idx = i + DOWNRIGHT;
                while (idx > OUTOFBOUNDSLOW && idx % COLUMN != COL1)
                {
                    if (board[idx].IsLower())
                    {
                        break;
                    }
                    else if (board[idx].IsUpper())
                    {
                        return true;
                    }
                    idx += DOWNRIGHT;
                }

                idx = i + DOWNLEFT;
                while (idx > OUTOFBOUNDSLOW && idx % COLUMN != COL8)
                {
                    if (board[idx].IsLower())
                    {
                        break;
                    }
                    else if (board[idx].IsUpper())
                    {
                        return true;
                    }
                    idx += DOWNLEFT;
                }
            }
            return false;
        }

        public static bool KnightThreat(byte[] board, int i)
        {
            if (board[i].IsUpper()) //case upper
            {
                int idx = i + UPUPRIGHT;
                if (idx < OUTOFBOUNDSHIGH && i % COLUMN < COL8 && board[idx].IsLower())
                    return true;
                idx = i + UPUPLEFT;
                if (idx < OUTOFBOUNDSHIGH && i % COLUMN > COL1 && board[idx].IsLower())
                    return true;
                idx = i + UPRIGHTRIGHT;
                if (idx < OUTOFBOUNDSHIGH && i % COLUMN < COL7 && board[idx].IsLower())
                    return true;
                idx = i + UPLEFTLEFT;
                if (idx < OUTOFBOUNDSHIGH && i % COLUMN > COL2 && board[idx].IsLower())
                    return true;
                idx = i + DOWNDOWNRIGHT;
                if (idx > OUTOFBOUNDSLOW && i % COLUMN < COL8 && board[idx].IsLower())
                    return true;
                idx = i + DOWNDOWNLEFT;
                if (idx > OUTOFBOUNDSLOW && i % COLUMN > COL1 && board[idx].IsLower())
                    return true;
                idx = i + DOWNRIGHTRIGHT;
                if (idx > OUTOFBOUNDSLOW && i % COLUMN < COL7 && board[idx].IsLower())
                    return true;
                idx = i + DOWNLEFTLEFT;
                if (idx > OUTOFBOUNDSLOW && i % COLUMN > COL2 && board[idx].IsLower())
                    return true;
            }
            else
            {
                int idx = i + UPUPRIGHT;
                if (idx < OUTOFBOUNDSHIGH && i % COLUMN < COL8 && board[idx].IsUpper())
                    return true;
                idx = i + UPUPLEFT;
                if (idx < OUTOFBOUNDSHIGH && i % COLUMN > COL1 && board[idx].IsUpper())
                    return true;
                idx = i + UPRIGHTRIGHT;
                if (idx < OUTOFBOUNDSHIGH && i % COLUMN < COL7 && board[idx].IsUpper())
                    return true;
                idx = i + UPLEFTLEFT;
                if (idx < OUTOFBOUNDSHIGH && i % COLUMN > COL2 && board[idx].IsUpper())
                    return true;
                idx = i + DOWNDOWNRIGHT;
                if (idx > OUTOFBOUNDSLOW && i % COLUMN < COL8 && board[idx].IsUpper())
                    return true;
                idx = i + DOWNDOWNLEFT;
                if (idx > OUTOFBOUNDSLOW && i % COLUMN > COL1 && board[idx].IsUpper())
                    return true;
                idx = i + DOWNRIGHTRIGHT;
                if (idx > OUTOFBOUNDSLOW && i % COLUMN < COL7 && board[idx].IsUpper())
                    return true;
                idx = i + DOWNLEFTLEFT;
                if (idx > OUTOFBOUNDSLOW && i % COLUMN > COL2 && board[idx].IsUpper())
                    return true;
            }
            return false;
        }

        static public bool PawnThreat(byte[] board, int i)
        {
            if (board[i].IsUpper()) //case upper
            {
                if (i + DOWNLEFT > OUTOFBOUNDSLOW && i % COLUMN != COL1 && board[i + DOWNLEFT].IsLower())
                {
                    return true;
                }
                else if (i + DOWNRIGHT > OUTOFBOUNDSLOW && i % COLUMN != COL8 && board[i + DOWNRIGHT].IsLower())
                    return true;
            }
            else
            {
                if (i + UPLEFT < OUTOFBOUNDSHIGH && i % COLUMN != COL1 && board[i + UPLEFT].IsUpper())
                {
                    return true;
                }
                else if (i + UPRIGHT < OUTOFBOUNDSHIGH && i % COLUMN != COL8 && board[i + UPRIGHT].IsUpper())
                    return true;
            }
            return false;
        }
        #endregion

        #region MapAdditions

        public static void AddAdjacentMaps(this byte[] board, bool white, int i, ref BoardBuffer moves, bool allowCheck)
        {
            int idx = i + UP;
            byte[] temp;
            while (idx < OUTOFBOUNDSHIGH)
            {
                if (board.IsValidMove(white, idx))
                {
                    temp = board.Move(i, idx);
                    bool cap = board.TakesOpponentPiece(white, idx);
                    if (!InCheck(board, white))
                    {
                        if (cap)
                            moves.AddCapture(temp);
                        else if (PieceNotSafe(temp, idx, white))
                            moves.AddUnSafe(temp);
                        else if (AdjacentThreat(temp, idx))
                            moves.AddThreat(temp);
                        else if (!white)
                            moves.AddForward(temp);
                        else
                            moves.Add(temp);
                    }
                    if (cap)
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
                    bool cap = board.TakesOpponentPiece(white, idx);
                    if (!InCheck(board, white))
                    {
                        if (cap)
                            moves.AddCapture(temp);
                        else if (PieceNotSafe(temp, idx, white))
                            moves.AddUnSafe(temp);
                        else if (AdjacentThreat(temp, idx))
                            moves.AddThreat(temp);
                        else if (white)
                            moves.AddForward(temp);
                        else
                            moves.Add(temp);
                    }
                    if (cap)
                        break;
                }
                else break;
                idx += DOWN;
            }
            idx = i % COLUMN;
            int bse = i - idx;
            while (--idx > -1)
            {
                int ix = bse + idx;
                if (IsValidMove(board, white, ix))
                {
                    temp = board.Move(i, ix);
                    bool cap = board.TakesOpponentPiece(white, ix);
                    if (!InCheck(board, white))
                    {
                        if (cap)
                            moves.AddCapture(temp);
                        else if (PieceNotSafe(temp, ix, white))
                            moves.AddUnSafe(temp);
                        else if (AdjacentThreat(temp, ix))
                            moves.AddThreat(temp);
                        else
                            moves.Add(temp);
                    }
                    if (cap)
                        break;
                }
                else break;
            }
            idx = i % COLUMN;
            bse = i - idx;
            while (++idx < 8)
            {
                int ix = bse + idx;
                if (IsValidMove(board, white, ix))
                {
                    temp = board.Move(i, ix);
                    bool cap = board.TakesOpponentPiece(white, ix);
                    if (!InCheck(board, white))
                    {
                        if (cap)
                            moves.AddCapture(temp);
                        else if (PieceNotSafe(temp, ix, white))
                            moves.AddUnSafe(temp);
                        else if (AdjacentThreat(temp, ix))
                            moves.AddThreat(temp);
                        else
                            moves.Add(temp);
                    }
                    if (cap)
                        break;
                }
                else break;
            }
        }

        public static void AddDiagonalMaps(this byte[] board, bool white, int i, ref BoardBuffer moves, bool allowCheck)
        {
            byte[] temp;
            int idx = i + UPLEFT;
            while (idx < OUTOFBOUNDSHIGH && idx % COLUMN != COL8)
            {
                if (IsValidMove(board, white, idx))
                {
                    temp = board.Move(i, idx);
                    bool cap = board.TakesOpponentPiece(white, idx);
                    if (allowCheck || !InCheck(board, white))
                    {
                        if (cap)
                            moves.AddCapture(temp);
                        else if (PieceNotSafe(temp, idx, white))
                            moves.AddUnSafe(temp);
                        else if (DiagThreat(temp, idx))
                            moves.AddThreat(temp);
                        else if (!white)
                            moves.AddForward(temp);
                        else
                            moves.Add(temp);
                    }
                    if (cap)
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
                    bool cap = board.TakesOpponentPiece(white, idx);
                    if (allowCheck || !InCheck(board, white))
                    {
                        if (cap)
                            moves.AddCapture(temp);
                        else if (PieceNotSafe(temp, idx, white))
                            moves.AddUnSafe(temp);
                        else if (DiagThreat(temp, idx))
                            moves.AddThreat(temp);
                        else if (!white)
                            moves.AddForward(temp);
                        else
                            moves.Add(temp);
                    }
                    if (cap)
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
                    bool cap = board.TakesOpponentPiece(white, idx);
                    if (allowCheck || !InCheck(board, white))
                    {
                        if (cap)
                            moves.AddCapture(temp);
                        else if (PieceNotSafe(temp, idx, white))
                            moves.AddUnSafe(temp);
                        else if (DiagThreat(temp, idx))
                            moves.AddThreat(temp);
                        else if (white)
                            moves.AddForward(temp);
                        else
                            moves.Add(temp);
                    }
                    if (cap)
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
                    bool cap = board.TakesOpponentPiece(white, idx);
                    if (allowCheck || !InCheck(board, white))
                    {
                        if (cap)
                            moves.AddCapture(temp);
                        else if (PieceNotSafe(temp, idx, white))
                            moves.AddUnSafe(temp);
                        else if (DiagThreat(temp, idx))
                            moves.AddThreat(temp);
                        else if (white)
                            moves.AddForward(temp);
                        else
                            moves.Add(temp);
                    }
                    if (cap)
                        break;
                }
                else break;
                idx += DOWNRIGHT;
            }
        }
        public static void AddKnightMoves(this byte[] board, bool white, int i, ref BoardBuffer moves, bool allowCheck)
        {
            int originRow = i % COLUMN;
            int idx = i + UPUPRIGHT;
            byte[] temp;
            if (idx < OUTOFBOUNDSHIGH && originRow < COL8 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(board, white))
                {
                    if (white ? board[idx].IsLower() : board[idx].IsUpper())
                        moves.AddCapture(temp);
                    else if (PieceNotSafe(temp, idx, white))
                        moves.AddUnSafe(temp);
                    else if (KnightThreat(temp, idx))
                        moves.AddThreat(temp);
                    else if (!white)
                        moves.AddForward(temp);
                    else
                        moves.Add(temp);
                }
            }
            idx = i + UPUPLEFT;
            if (idx < OUTOFBOUNDSHIGH && originRow > COL1 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(board, white))
                {
                    if (white ? board[idx].IsLower() : board[idx].IsUpper())
                        moves.AddCapture(temp);
                    else if (PieceNotSafe(temp, idx, white))
                        moves.AddUnSafe(temp);
                    else if (KnightThreat(temp, idx))
                        moves.AddThreat(temp);
                    else if (!white)
                        moves.AddForward(temp);
                    else
                        moves.Add(temp);
                }
            }
            idx = i + UPRIGHTRIGHT;
            if (idx < OUTOFBOUNDSHIGH && originRow < COL7 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(board, white))
                {
                    if (white ? board[idx].IsLower() : board[idx].IsUpper())
                        moves.AddCapture(temp);
                    else if (PieceNotSafe(temp, idx, white))
                        moves.AddUnSafe(temp);
                    else if (KnightThreat(temp, idx))
                        moves.AddThreat(temp);
                    else if (!white)
                        moves.AddForward(temp);
                    else
                        moves.Add(temp);
                }
            }
            idx = i + UPLEFTLEFT;
            if (idx < OUTOFBOUNDSHIGH && originRow > COL2 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(board, white))
                {
                    if (white ? board[idx].IsLower() : board[idx].IsUpper())
                        moves.AddCapture(temp);
                    else if (PieceNotSafe(temp, idx, white))
                        moves.AddUnSafe(temp);
                    else if (KnightThreat(temp, idx))
                        moves.AddThreat(temp);
                    else if (!white)
                        moves.AddForward(temp);
                    else
                        moves.Add(temp);
                }
            }
            idx = i + DOWNDOWNLEFT;
            if (idx > OUTOFBOUNDSLOW && originRow > COL1 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(board, white))
                {
                    if (white ? board[idx].IsLower() : board[idx].IsUpper())
                        moves.AddCapture(temp);
                    else if (PieceNotSafe(temp, idx, white))
                        moves.AddUnSafe(temp);
                    else if (KnightThreat(temp, idx))
                        moves.AddThreat(temp);
                    else if (white)
                        moves.AddForward(temp);
                    else
                        moves.Add(temp);
                }
            }
            idx = i + DOWNDOWNRIGHT;
            if (idx > OUTOFBOUNDSLOW && originRow < COL8 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(board, white))
                {
                    if (white ? board[idx].IsLower() : board[idx].IsUpper())
                        moves.AddCapture(temp);
                    else if (PieceNotSafe(temp, idx, white))
                        moves.AddUnSafe(temp);
                    else if (KnightThreat(temp, idx))
                        moves.AddThreat(temp);
                    else if (white)
                        moves.AddForward(temp);
                    else
                        moves.Add(temp);
                }
            }
            idx = i + DOWNLEFTLEFT;
            if (idx > OUTOFBOUNDSLOW && originRow > COL2 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(board, white))
                {
                    if (white ? board[idx].IsLower() : board[idx].IsUpper())
                        moves.AddCapture(temp);
                    else if (PieceNotSafe(temp, idx, white))
                        moves.AddUnSafe(temp);
                    else if (KnightThreat(temp, idx))
                        moves.AddThreat(temp);
                    else if (white)
                        moves.AddForward(temp);
                    else
                        moves.Add(temp);
                }
            }
            idx = i + DOWNRIGHTRIGHT;
            if (idx > OUTOFBOUNDSLOW && originRow < COL7 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !InCheck(board, white))
                {
                    if (white ? board[idx].IsLower() : board[idx].IsUpper())
                        moves.AddCapture(temp);
                    else if (PieceNotSafe(temp, idx, white))
                        moves.AddUnSafe(temp);
                    else if (KnightThreat(temp, idx))
                        moves.AddThreat(temp);
                    else if (white)
                        moves.AddForward(temp);
                    else
                        moves.Add(temp);
                }
            }
        }
        public static void AddKingMoves(this byte[] board, bool white, int i, ref BoardBuffer moves, bool allowCheck)
        {
            byte[] temp;

            if (white)
                IsOpponent = IsLower;
            else
                IsOpponent = IsUpper;

            int idx = i + UPLEFT;
            if (idx < OUTOFBOUNDSHIGH)
            {
                if (idx % COLUMN != COL8 && board.IsValidMove(white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !PieceNotSafe(temp, idx, white))
                    {
                        if (!white)
                        {
                            moves.AddForward(temp);
                        }
                        else
                            moves.Add(temp);
                    }
                }
                if (board.IsValidMove(white, ++idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !PieceNotSafe(temp, idx, white))
                    {
                        if (!white)
                        {
                            moves.AddForward(temp);
                        }
                        else
                            moves.Add(temp);
                    }
                }
                if (++idx % COLUMN != COL1 && board.IsValidMove(white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !PieceNotSafe(temp, idx, white))
                    {
                        if (!white)
                        {
                            moves.AddForward(temp);
                        }
                        else
                            moves.Add(temp);
                    }
                }
            }
            idx = i + RIGHT;
            if (idx % COLUMN != COL1 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !PieceNotSafe(temp, idx, white))
                    moves.Add(temp);
            }
            idx = i + LEFT;
            if (idx % COLUMN != COL8 && board.IsValidMove(white, idx))
            {
                temp = board.Move(i, idx);
                if (allowCheck || !PieceNotSafe(temp, idx, white))
                    moves.Add(temp);

            }
            idx = i + DOWNRIGHT;
            if (idx > -1)
            {
                if (i % COLUMN != COL8 && board.IsValidMove(white, idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !PieceNotSafe(temp, idx, white))
                    {
                        if (white)
                        {
                            moves.AddForward(temp);
                        }
                        else
                            moves.Add(temp);
                    }
                }
                if (board.IsValidMove(white, --idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !PieceNotSafe(temp, idx, white))
                    {
                        if (white)
                        {
                            moves.AddForward(temp);
                        }
                        else
                            moves.Add(temp);
                    }
                }
                if (i % COLUMN != COL1 && board.IsValidMove(white, --idx))
                {
                    temp = board.Move(i, idx);
                    if (allowCheck || !PieceNotSafe(temp, idx, white))
                    {
                        if (white)
                        {
                            moves.AddForward(temp);
                        }
                        else
                            moves.Add(temp);
                    }
                }
            }
        }
        public static void AddPawnMoves(this byte[] board, bool white, int i, ref BoardBuffer moves, bool allowCheck)
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
                        {
                            if (PawnThreat(temp, i + UPUP))
                                moves.AddCapture(temp);
                            else
                                moves.AddForward(temp);
                        }
                    }
                }
                if (board[i + UP] == _)
                {
                    temp = board.MovePawn(i, i + UP, white);
                    if (allowCheck || !InCheck(temp, white))
                    {
                        if (PawnThreat(temp, i + UP))
                            moves.AddCapture(temp);
                        else
                            moves.AddForward(temp);
                    }
                }
                if (board[i + UPLEFT].IsUpper() && i % COLUMN != COL1)
                {
                    temp = board.MovePawn(i, i + UPLEFT, white);
                    if (allowCheck || !InCheck(temp, white))
                        moves.AddCapture(temp);
                }
                if (i < 54 && board[i + UPRIGHT].IsUpper() && i % COLUMN != COL8)
                {
                    temp = board.MovePawn(i, i + UPRIGHT, white);
                    if (allowCheck || !InCheck(temp, white))
                        moves.AddCapture(temp);
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
                        {
                            if (PawnThreat(temp, i + DOWNDOWN))
                                moves.AddCapture(temp);
                            else
                                moves.AddForward(temp);
                        }
                    }
                }
                if (board[i + DOWN] == _)
                {
                    temp = board.MovePawn(i, i + DOWN, white);
                    if (allowCheck || !InCheck(temp, white))
                    {
                        if (PawnThreat(temp, i + DOWN))
                            moves.AddCapture(temp);
                        else
                            moves.AddForward(temp);
                    }
                }
                if (board[i + DOWNRIGHT].IsLower() && i % COLUMN != COL1)
                {
                    temp = board.MovePawn(i, i + DOWNRIGHT, white);
                    if (allowCheck || !InCheck(temp, white))
                        moves.AddCapture(temp);
                }
                if (i > 8 && board[i + DOWNLEFT].IsLower() && i % COLUMN != COL8)
                {
                    temp = board.MovePawn(i, i + DOWNLEFT, white);
                    if (!InCheck(board, white))
                        moves.AddCapture(temp);
                }
            }
        }

        #endregion

        #region 'check' algorithms

        public static bool PieceNotSafe(byte[] board, int piecePosition, bool white)
        {
            if (piecePosition == -1)
            {
                return false;
            }
            else
            {
                return DiagonalCheck(board, white, piecePosition) || KnightCheck(board, white, piecePosition) || ColRowCheck(board, white, piecePosition);
            }
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
            if (pos % COLUMN != 0)
            {
                if (board[pos] == enemyKing)
                {
                    return true;
                }
            }
            // Move in that direction until you hit an edge or another piece
            while (pos % COLUMN != 0)
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
                if ((!white && board[pos] == enemyPawn) || board[pos] == enemyKing)
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
                if ((!white && board[pos] == enemyPawn) || board[pos] == enemyKing)
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
                if ((white && board[pos] == enemyPawn) || board[pos] == enemyKing)
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
                if ((white && board[pos] == enemyPawn) || board[pos] == enemyKing)
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

        public static int GetPiecePos(byte[] board, bool white, byte piece)
        {
            byte pieceChar = white ? piece.ToUpper() : piece.ToLower();
            for (int i = 0; i < OUTOFBOUNDSHIGH; ++i)
            {
                if (board[i] == pieceChar)
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion
        public static LightList GetAvailableMoves(byte[] board, ChessColor color, bool allowCheck)
        {
            bool white = color == ChessColor.White;
            LightList moves = new LightList();
            BoardBuffer buff = new BoardBuffer();
            //iterate thru entire board {64} including row delimiters {7}

            for (int i = 0; i < OUTOFBOUNDSHIGH; ++i)
            {
                if (!white)
                {
                    switch (board[i])
                    {
                        case 0x02:
                            board.AddPawnMoves(white, i, ref buff, allowCheck);
                            break;
                        case 0x04:
                            board.AddAdjacentMaps(white, i, ref buff, allowCheck);
                            break;
                        case 0x06:
                            board.AddDiagonalMaps(white, i, ref buff, allowCheck);
                            break;
                        case 0x08:
                            board.AddKnightMoves(white, i, ref buff, allowCheck);
                            break;
                        case 0x0A:
                            board.AddAdjacentMaps(white, i, ref buff, allowCheck);
                            board.AddDiagonalMaps(white, i, ref buff, allowCheck);
                            break;
                        case 0x0C:
                            board.AddKingMoves(white, i, ref buff, allowCheck);
                            break;
                        default: break;
                    }
                }
                else
                {
                    switch (board[i])
                    {
                        case 0x01:
                            board.AddPawnMoves(white, i, ref buff, allowCheck);
                            break;
                        case 0x03:
                            board.AddAdjacentMaps(white, i, ref buff, allowCheck);
                            break;
                        case 0x05:
                            board.AddDiagonalMaps(white, i, ref buff, allowCheck);
                            break;
                        case 0x07:
                            board.AddKnightMoves(white, i, ref buff, allowCheck);
                            break;
                        case 0x09:
                            board.AddAdjacentMaps(white, i, ref buff, allowCheck);
                            board.AddDiagonalMaps(white, i, ref buff, allowCheck);
                            break;
                        case 0x0B:
                            board.AddKingMoves(white, i, ref buff, allowCheck);
                            break;
                        default: break;
                    }
                }
            }

            return LightList.ConvertBuffer(buff);
        }
    }
}
