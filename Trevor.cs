//Here are the initial methods for the heuristic, call the GetHeuristicValue method for whatever the current heuristic that
//  myself and Richard are currently working on.
        /// <summary>
        /// Purpose: To calculate a heuristic value for the given board state
        /// </summary>
        /// <param name="boardState"></param>
        /// <param name="color"></param>
        /// <returns>integer representing the heuristic</returns>
        private int GetHeuristicValue(char[] boardState, ChessColor color)
        {
            return GetPieceValueHeuristic(boardState, color);
        }

        /// <summary>
        /// Purpose: To calculate a heuristic based purely off piece value
        /// </summary>
        /// <param name="boardState"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private int GetPieceValueHeuristic(char[] boardState, ChessColor color)
        {
            int whiteValue = 0;
            int blackValue = 0;
            for (int i = 0; i < boardState.Length; i++)
            {
                if (boardState[i] != '_' && boardState[i] != '/' && char.ToLower(boardState[i]) != 'k')
                {
                    if (char.IsUpper(boardState[i]))
                        whiteValue += pieceValues[char.ToLower(boardState[i])];
                    else
                        blackValue += pieceValues[boardState[i]];
                }
            }

            if (color == ChessColor.White)
                return whiteValue - blackValue;
            else
                return blackValue - whiteValue;
        }

        private static Dictionary<char, int> pieceValues = new Dictionary<char, int>
        {
               {'p', 1},
               {'n', 3},
               {'b', 3},
               {'r', 5},
               {'q', 10}
        };
