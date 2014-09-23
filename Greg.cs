namespace ShallowRed
{
    public static class FENExtensions
    {
        static public ChessMove GenerateMove(this char[] from, char[] to)
        {
            int[] pos = new int[2]; 

            int changes = 0;
            for(int i = 0; i< from.Length && changes<2;++i)
            {
                if(from[i] != to[i])
                {
                        pos[changes++] = i;
                }
            }

            if (to[pos[1]] == '_') //spot vacated
                return new ChessMove(new ChessLocation(pos[1] % 9, pos[1] / 9), new ChessLocation(pos[0] % 9, pos[0] / 9));

            return new ChessMove(new ChessLocation(pos[0] % 9, pos[0] / 9), new ChessLocation(pos[1] % 9, pos[1] / 9));
        }
        
        static public char[] ToShallowRedFEN(this string originalFen)
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

        static public char[] Move(this char[] board, int fromX, int fromY, int toX, int toY)
        {
            return board.Move(fromX + (9 * fromY), toX + (9 * toY));
        }
        static public char[] Move(this char[] board, int from, int to)
        {
            char[] b = (char[])board.Clone();
            b[to] = b[from];
            b[from] = '_';
            return b;
        }

        static private bool IsValidMove(this char[] board, bool white, int to)
        {
            if (board[to] == '_')
                return true;
            return white ? !Char.IsLower(board[to]) : Char.IsUpper(board[to]);
        }

        static private bool TakesOpponentPiece(this char[] board, bool white, int to)
        {
            return (Char.IsUpper(board[to]));
        }

        static private bool EmptySpace(this char[] board, bool white, int to)
        {
            return board[to] == '_';
        }

        static public void AddAdjacentMaps(this char[] board, bool white, int i, ref List<char[]> moves)
        {
            int idx = i + 9;
            while (idx < 71)
            {
                if (IsValidMove(board, white, idx))
                {
                    moves.Add(Move(board, i, idx));
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
                    moves.Add(Move(board, i, idx));
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
                    moves.Add(Move(board, i, bse+idx));
                    if (TakesOpponentPiece(board, white, bse+idx))
                        break;
                }
                else break;
            }

            
            idx = i % 9;
            bse = i - idx;

            while (++idx < 8)
            {
                if (IsValidMove(board, white, bse+idx))
                {
                    moves.Add(Move(board, i, bse+idx));
                    if (TakesOpponentPiece(board, white, bse+idx))
                        break;
                }
                else break;
            }

        }

        static public void AddDiagonalMaps(this char[] board, bool white, int i, ref List<char[]> moves)
        {
            int idx = i + 8;
            while (idx < 71 && idx % 9 >= 0)
            {
                if (IsValidMove(board, white, idx))
                {
                    moves.Add(Move(board, i, idx));
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
                    moves.Add(board.Move(i, idx));
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
                    moves.Add(board.Move(i, idx));
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
                    moves.Add(board.Move(i, idx));
                    if (board.TakesOpponentPiece(white, idx))
                        break;
                }
                else break;
                idx -= 8;
            }
        }

        static public void AddKnightMoves(this char[] board, bool white, int i, ref List<char[]> moves)
        {
            int idx = i + 19;
            if (idx < 71 && idx % 9 != 0 && board.IsValidMove(white, idx))
                moves.Add(board.Move(i, idx));
            idx = i + 17;
            if (idx < 71 && idx % 9 != 8 && board.IsValidMove(white, idx))
                moves.Add(board.Move(i, idx));
            idx = i + 11;
            if (idx < 71 && idx % 9 != 0 && board.IsValidMove(white, idx))
                moves.Add(board.Move(i, idx));
            idx = i + 7;
            if (idx < 71 && idx % 9 != 8 && board.IsValidMove(white, idx))
                moves.Add(board.Move(i, idx));
            idx = i - 19;
            if (idx > -1 && idx % 9 != 8 && board.IsValidMove(white, idx))
                moves.Add(board.Move(i, idx));
            idx = i - 17;
            if (idx > -1 && idx % 9 != 0 && board.IsValidMove(white, idx))
                moves.Add(board.Move(i, idx));
            idx = i - 11;
            if (idx > -1 && idx % 9 != 8 && board.IsValidMove(white, idx))
                moves.Add(board.Move(i, idx));
            idx = i - 7;
            if (idx > -1 && idx % 9 != 0 && board.IsValidMove(white, idx))
                moves.Add(board.Move(i, idx));
        }

        static public void AddKingMoves(this char[] board, bool white, int i, ref List<char[]> moves)
        {
            int idx = i + 8;
            if (idx < 71)
            {
                if (idx % 9 != 8 && board.IsValidMove(white, i))
                    moves.Add(board.Move(i, idx));
                if (board.IsValidMove(white, ++idx))
                    moves.Add(board.Move(i, idx));
                if (++idx % 9 != 0 && board.IsValidMove(white, i))
                    moves.Add(board.Move(i, idx));

                idx = i + 1;
                if (idx % 9 != 0 && board.IsValidMove(white, i))
                    moves.Add(board.Move(i, idx));
                idx = i - 1;
                if (idx % 9 != 8 && board.IsValidMove(white, i))
                    moves.Add(board.Move(i, idx));

                idx = i - 8;
                if (idx > -1)
                {
                    if (idx % 9 != 0 && board.IsValidMove(white, idx))
                        moves.Add(board.Move(i, idx));
                    if (board.IsValidMove(white, --idx))
                        moves.Add(board.Move(i, idx));
                    if (--idx % 9 != 8 && board.IsValidMove(white, idx))
                        moves.Add(board.Move(i, idx));
                }
            }
        }

        static public void AddPawnMoves(this char[] board, bool white, int i, ref List<char[]> moves)
        {
                if (i / 9 == 1)
                {
                    if (board.IsValidMove(white, i + 18))
                        moves.Add(board.Move(i, i + 18));
                }
                if (board.IsValidMove(white, i + 9))
                        moves.Add(board.Move(i, i + 9));           
        }
    }
    public static class FEN
    {
        static public List<char[]> GetAvailableMoves(char[] board, bool white)
        {
            List<char[]> moves = new List<char[]>();
            //iterate thru entire board {64} including row delimiters {7}
            for (int i = 0; i < 71; ++i)
            {
                if (!white)
                {
                    switch (board[i])
                    {
                        case 'p':


                        case 'r':
                            board.AddAdjacentMaps(white, i, ref moves);
                            break;
                        case 'b':
                            board.AddDiagonalMaps(white, i, ref moves);
                            break;
                        case 'n':
                            board.AddKnightMoves(white, i, ref moves);
                            break;
                        case 'q':
                            board.AddAdjacentMaps(white, i, ref moves);
                            board.AddDiagonalMaps(white, i, ref moves);
                            break;
                        case 'k':
                            board.AddKingMoves(white, i, ref moves);
                            break;
                        default: break;
                    }
                }
                else
                {
                    switch (board[i])
                    {
                        case 'P':
                            board.AddPawnMoves(white, i, ref moves);
                            break;
                        case 'R':
                            board.AddAdjacentMaps(white, i, ref moves);
                            break;
                        case 'B':
                            board.AddDiagonalMaps(white, i, ref moves);
                            break;
                        case 'N':
                            board.AddKnightMoves(white, i, ref moves);
                            break;
                        case 'Q':
                            board.AddAdjacentMaps(white, i, ref moves);
                            board.AddDiagonalMaps(white, i, ref moves);
                            break;
                        case 'K':
                            board.AddKingMoves(white, i, ref moves);
                            break;
                        default: break;
                    }
                }
            }
            return moves;
        }

    }
}
