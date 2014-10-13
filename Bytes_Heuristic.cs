using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UvsChess;

namespace ShallowRed
{
    public static class Heuristic
    {
        public static AILoggerCallback Log { get; set; }
        private const int edgeIndex = 8;

        /// <summary>
        /// Purpose: To calculate a heuristic value for the given board state
        /// </summary>
        /// <param name="boardState"></param>
        /// <param name="color"></param>
        /// <returns>integer representing the heuristic</returns>
        public static int GetHeuristicValue(byte[] boardState, ChessColor color)
        {
            int pH = GetPieceValueHeuristic(boardState, color);
            int pS = PieceSafety(boardState, color);
            return pH + pS;
        }

        /// <summary>
        /// Purpose: To calculate a heuristic based purely off piece value
        /// </summary>
        /// <param name="boardState"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private static int GetPieceValueHeuristic(byte[] boardState, ChessColor color)
        {
            int whiteValue = 0;
            int blackValue = 0;
            for (int i = 0; i < boardState.Length; i++)
            {
                if (boardState[i] != FEN._)
                {
                    if (boardState[i].IsUpper())
                        whiteValue += (pieceValues[boardState[i]] + GetPiecePositionValue(boardState[i], i));
                    else
                        blackValue += (pieceValues[boardState[i]] + GetPiecePositionValue(boardState[i], i));
                }
            }

            //Log("WV -" + whiteValue.ToString());
            //Log("BV -" + blackValue.ToString());

            if (color == ChessColor.White)
            {
                if (whiteValue - 20000 < 1500)
                {
                    lateGame = true;
                }
                return whiteValue - blackValue;
            }
            else
            {
                if (blackValue - 20000 < 1500)
                {
                    lateGame = true;
                }
                return blackValue - whiteValue;
            }
        }

        /// <summary>
        /// Purpose: Lower the Heuristic value if our queen is put in danger
        /// </summary>
        /// <param name="boardState"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private static int PieceSafety(byte[] boardState, ChessColor color)
        {
            int hazardPenalty = 0;
            bool white = color == ChessColor.White;
            //hazardPenalty += FEN.PieceNotSafe(boardState, FEN.GetPiecePos(boardState, white, FEN.q), white) ? -400 : 0;
            //hazardPenalty += FEN.PieceNotSafe(boardState, FEN.GetPiecePos(boardState, white, FEN.r), white) ? -200 : 0;
            //hazardPenalty += FEN.PieceNotSafe(boardState, FEN.GetPiecePos(boardState, white, FEN.n), white) ? -150 : 0;
            //hazardPenalty += FEN.PieceNotSafe(boardState, FEN.GetPiecePos(boardState, white, FEN.b), white) ? -155 : 0;
            //hazardPenalty += FEN.PieceNotSafe(boardState, FEN.GetPiecePos(boardState, white, FEN.p), white) ? -40 : 0;

            return hazardPenalty;
        }

        private static int GetPiecePositionValue(Byte piece, int index)
        {
            //index = index - index / edgeIndex; // ignores the "/" character index
            switch (piece)
            {
                case 0x01:
                    return whitPawnLocationValues[index];
                case 0x02:
                    return blackPawnLocationValues[index];
                case 0x07:
                    return whiteKnightLocationValues[index];
                case 0x08:
                    return blackKnightLocationValues[index];
                case 0x05:
                    return whiteBishopLocationValues[index];
                case 0x06:
                    return blackBishopLocationValues[index];
                case 0x03:
                    return whiteRookLocationValues[index];
                case 0x04:
                    return blackRookLocationValues[index];
                case 0x09:
                    return whiteQueenLocationValues[index];
                case 0x0A:
                    return blackQueenLocationValues[index];
                case 0x0B:
                    if (lateGame)
                    {
                        return whiteKingLateGameLocationValues[index];
                    }
                    else
                    {
                        return whiteKingMidGameLocationValues[index];
                    }
                case 0x0C:
                    if (lateGame)
                    {
                        return blackKingLateGameLocationValues[index];
                    }
                    else
                    {
                        return blackKingMidGameLocationValues[index];
                    }
                default:
                    throw new Exception("Unrecognized Character");
            }
        }
        private static Dictionary<byte, int> pieceValues = new Dictionary<byte, int>
{
{0x01, 100},  //pawns
{0x02, 100},
{0x03, 320},  //rooks
{0x04, 320},
{0x05, 330},  //bishops
{0x06, 330},
{0x07, 500},  //Knights
{0x08, 500},
{0x09, 900},  //Queens
{0x0A, 900},
{0x0B, 20000}, //Kings
{0x0C, 20000}
};

        private static bool lateGame = false;

        #region " Location Values "
        private static int[] whitPawnLocationValues =
{
0, 0, 0, 0, 0, 0, 0, 0,
50, 50, 50, 50, 50, 50, 50, 50,
10, 10, 20, 30, 30, 20, 10, 10,
5, 5, 10, 25, 25, 10, 5, 5,
0, 0, 0, 20, 20, 0, 0, 0,
5, -5,-10, 0, 0,-10, -5, 5,
5, 10, 10,-20,-20, 10, 10, 5,
0, 0, 0, 0, 0, 0, 0, 0
};
        private static int[] blackPawnLocationValues =
{
0, 0, 0, 0, 0, 0, 0, 0,
5, 10, 10,-20,-20, 10, 10, 5,
5, -5,-10, 0, 0,-10, -5, 5,
0, 0, 0, 20, 20, 0, 0, 0,
5, 5, 10, 25, 25, 10, 5, 5,
10, 10, 20, 30, 30, 20, 10, 10,
50, 50, 50, 50, 50, 50, 50, 50,
0, 0, 0, 0, 0, 0, 0, 0
};
        private static int[] whiteKnightLocationValues =
{
-50,-40,-30,-30,-30,-30,-40,-50,
-40,-20, 0, 0, 0, 0,-20,-40,
-30, 0, 10, 15, 15, 10, 0,-30,
-30, 5, 15, 20, 20, 15, 5,-30,
-30, 0, 15, 20, 20, 15, 0,-30,
-30, 5, 10, 15, 15, 10, 5,-30,
-40,-20, 0, 5, 5, 0,-20,-40,
-50,-40,-30,-30,-30,-30,-40,-50
};
        private static int[] blackKnightLocationValues =
{
-50,-40,-30,-30,-30,-30,-40,-50,
-40,-20, 0, 5, 5, 0,-20,-40,
-30, 5, 10, 15, 15, 10, 5,-30,
-30, 0, 15, 20, 20, 15, 0,-30,
-30, 5, 15, 20, 20, 15, 5,-30,
-30, 0, 10, 15, 15, 10, 0,-30,
-40,-20, 0, 0, 0, 0,-20,-40,
-50,-40,-30,-30,-30,-30,-40,-50
};
        private static int[] whiteBishopLocationValues =
{
-20,-10,-10,-10,-10,-10,-10,-20,
-10, 0, 0, 0, 0, 0, 0,-10,
-10, 0, 5, 10, 10, 5, 0,-10,
-10, 5, 5, 10, 10, 5, 5,-10,
-10, 0, 10, 10, 10, 10, 0,-10,
-10, 10, 10, 10, 10, 10, 10,-10,
-10, 5, 0, 0, 0, 0, 5,-10,
-20,-10,-10,-10,-10,-10,-10,-20
};
        private static int[] blackBishopLocationValues =
{
-20,-10,-10,-10,-10,-10,-10,-20,
-10, 5, 0, 0, 0, 0, 5,-10,
-10, 10, 10, 10, 10, 10, 10,-10,
-10, 0, 10, 10, 10, 10, 0,-10,
-10, 5, 5, 10, 10, 5, 5,-10,
-10, 0, 5, 10, 10, 5, 0,-10,
-10, 0, 0, 0, 0, 0, 0,-10,
-20,-10,-10,-10,-10,-10,-10,-20
};
        private static int[] whiteRookLocationValues =
{
0, 0, 0, 0, 0, 0, 0, 0,
5, 10, 10, 10, 10, 10, 10, 5,
-5, 0, 0, 0, 0, 0, 0, -5,
-5, 0, 0, 0, 0, 0, 0, -5,
-5, 0, 0, 0, 0, 0, 0, -5,
-5, 0, 0, 0, 0, 0, 0, -5,
-5, 0, 0, 0, 0, 0, 0, -5,
0, 0, 0, 5, 5, 0, 0, 0
};
        private static int[] blackRookLocationValues =
{
0, 0, 0, 5, 5, 0, 0, 0,
-5, 0, 0, 0, 0, 0, 0, -5,
-5, 0, 0, 0, 0, 0, 0, -5,
-5, 0, 0, 0, 0, 0, 0, -5,
-5, 0, 0, 0, 0, 0, 0, -5,
-5, 0, 0, 0, 0, 0, 0, -5,
5, 10, 10, 10, 10, 10, 10, 5,
0, 0, 0, 0, 0, 0, 0, 0
};
        private static int[] whiteQueenLocationValues =
{
-20,-10,-10, -5, -5,-10,-10,-20,
-10, 0, 0, 0, 0, 0, 0,-10,
-10, 0, 5, 5, 5, 5, 0,-10,
-5, 0, 5, 5, 5, 5, 0, -5,
0, 0, 5, 5, 5, 5, 0, -5,
-10, 5, 5, 5, 5, 5, 0,-10,
-10, 0, 5, 0, 0, 0, 0,-10,
-20,-10,-10, -5, -5,-10,-10,-20
};
        private static int[] blackQueenLocationValues =
{
-20,-10,-10, -5, -5,-10,-10,-20,
-10, 0, 0, 0, 0, 5, 0,-10,
-10, 0, 5, 5, 5, 5, 5,-10,
-5, 0, 5, 5, 5, 5, 0, 0,
-5, 0, 5, 5, 5, 5, 0, -5,
-10, 0, 5, 5, 5, 5, 0,-10,
-10, 0, 0, 0, 0, 0, 0,-10,
-20,-10,-10, -5, -5,-10,-10,-20
};
        private static int[] whiteKingMidGameLocationValues =
{
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-20,-30,-30,-40,-40,-30,-30,-20,
-10,-20,-20,-20,-20,-20,-20,-10,
20, 20, 0, 0, 0, 0, 20, 20,
20, 30, 10, 0, 0, 10, 30, 20
};
        private static int[] blackKingMidGameLocationValues =
{
20, 30, 10, 0, 0, 10, 30, 20,
20, 20, 0, 0, 0, 0, 20, 20,
-10,-20,-20,-20,-20,-20,-20,-10,
-20,-30,-30,-40,-40,-30,-30,-20,
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30
};
        private static int[] whiteKingLateGameLocationValues =
{
-50,-40,-30,-20,-20,-30,-40,-50,
-30,-20,-10, 0, 0,-10,-20,-30,
-30,-10, 20, 30, 30, 20,-10,-30,
-30,-10, 30, 40, 40, 30,-10,-30,
-30,-10, 30, 40, 40, 30,-10,-30,
-30,-10, 20, 30, 30, 20,-10,-30,
-30,-30, 0, 0, 0, 0,-30,-30,
-50,-30,-30,-30,-30,-30,-30,-50
};
        private static int[] blackKingLateGameLocationValues =
{
-50,-30,-30,-30,-30,-30,-30,-50,
-30,-30, 0, 0, 0, 0,-30,-30,
-30,-10, 20, 30, 30, 20,-10,-30,
-30,-10, 30, 40, 40, 30,-10,-30,
-30,-10, 30, 40, 40, 30,-10,-30,
-30,-10, 20, 30, 30, 20,-10,-30,
-30,-20,-10, 0, 0,-10,-20,-30,
-50,-40,-30,-20,-20,-30,-40,-50
};
        #endregion
    }
}
