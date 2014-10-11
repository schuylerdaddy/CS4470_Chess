using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using UvsChess;

namespace ShallowRed
{
    public static class Minimax
    {
        
        public static AILoggerCallback Log { get; set; }

        /// <summary>
        /// Purpose: To perform the minimax algorithm to determine chess move
        /// </summary>
        /// <param name="boardState"></param>
        /// <param name="color"></param>
        /// <returns>char[] board to represent the move</returns>
        /// 

        public static char[] miniMax(char[] SRFen, ChessColor color)
        {
            TimeSpan maxTime = TimeSpan.FromMilliseconds(5500);
            Char[] initialBoard = (char[])SRFen.Clone();
            bool white;
            int alpha = -10000;
            int beta = 10000;
            int cutoff = 4;
            if (color == ChessColor.White) white = true;
            else white = false;
            int depth = 0;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            int h;
            h = minValue(ref SRFen, depth + 1, white, alpha, beta, cutoff, ref timer);
            if (h == -5000) 
                return SRFen;
            char[] bestSoFar = (char[])SRFen.Clone();
            while (timer.Elapsed < maxTime && h != -9999)
            { 
                cutoff += 2;
                char[] temp = (char[])initialBoard.Clone();
                h = minValue(ref temp, depth + 1, white, alpha, beta, cutoff, bestSoFar, ref timer);
                if (h != -9999) bestSoFar = (char[])temp.Clone();
                if (h == -5000) 
                    return bestSoFar;
            }
            //this.Log("cutoff" + cutoff);
            return bestSoFar;
        }

        public static int maxValue(ref char[] board, int depth, bool white, int alpha, int beta, int cutoff, ref Stopwatch timer)
        {
            TimeSpan maxTime = TimeSpan.FromMilliseconds(5500);
            ChessColor color = (white ? ChessColor.White : ChessColor.Black);
            int hValue;
            if (depth == cutoff)
                return Heuristic.GetHeuristicValue(board, color);
            LightList childrenBoard = FEN.GetAvailableMoves(board, color, false);

            int count = childrenBoard.Count;
            if (count == 0){ //no moves available
               if (FENExtensions.InCheck(board, white))
                    return -5000;//checkmate
                else
                    return -3000;//stalemate;
            }
            //sort array of moves
            int[] Hchildren = new int[count];
            for (int idx = 0; idx < count; ++idx)
                Hchildren[idx] = Heuristic.GetHeuristicValue(childrenBoard[idx], color);
            sort(ref childrenBoard, ref Hchildren);

            int maximumValue = -10000;
            int i = 0;
            char[] tempBoard = null;
            while (i < count && timer.Elapsed < maxTime){ //process all the children move
                tempBoard = childrenBoard[i];
                if (timer.Elapsed < maxTime)
                    hValue = minValue(ref tempBoard, depth + 1, !white, alpha, beta, cutoff, ref timer);
                else
                    return -9999;
                
                if (depth == 1)
                {
                    if (maximumValue == hValue){
                        //choose randomly between current choice and this move

                        Random rnd = new Random();
                        int value = rnd.Next(0, 1);
                        if (value == 0)
                            board = tempBoard;
                    }
                    else if (hValue > maximumValue)
                    {
                        maximumValue = hValue;
                        board = childrenBoard[i];

                    }
                    if (maximumValue >= beta)
                    {
                        hValue = maximumValue;
                        childrenBoard = null;
                        return hValue;
                    }
                    alpha = Math.Max(alpha, maximumValue);
                }
                else
                {
                    maximumValue = Math.Max(maximumValue, hValue);
                    if (maximumValue >= beta)
                    {
                        hValue = maximumValue;
                        childrenBoard = null;
                        return hValue;
                    }
                    alpha = Math.Max(alpha, maximumValue);
                }
                ++i;
            }
            childrenBoard = null;
            if (timer.Elapsed > maxTime) return -9999;
            hValue = maximumValue;

            return hValue;
        }
        public static int minValue(ref char[] board, int depth, bool white, int alpha, int beta, int cutoff, ref Stopwatch timer)
        {
            TimeSpan maxTime = TimeSpan.FromMilliseconds(5500);
            int hValue;
            LightList equalMoves = new LightList();

            ChessColor color = (white ? ChessColor.White : ChessColor.Black);
            if (depth == cutoff)
                return Heuristic.GetHeuristicValue(board, color);
            LightList childrenBoard = FEN.GetAvailableMoves(board, color, false);

            int count = childrenBoard.Count;
            if (count == 0)
            { //no moves available
                if (FENExtensions.InCheck(board, white))
                    return -5000;//checkmate
                else
                    return -3000;//stalemate
            }
            //sort array of moves
            int[] Hchildren = new int[count];
            for (int idx = 0; idx < count; ++idx)
            {
                Hchildren[idx] = Heuristic.GetHeuristicValue(childrenBoard[idx], color);
            }
            sort(ref childrenBoard, ref Hchildren);
            int minimumValue = 10000;
            int i = 0;
            char[] tempBoard = null;
            while (i < count)
            { //process all the children move
                    tempBoard = childrenBoard[i];
                    if (timer.Elapsed < maxTime)
                        hValue = maxValue(ref tempBoard, depth + 1, !white, alpha, beta, cutoff, ref timer);
                    else
                        return -9999;
                if (depth == 1) {
                    if (hValue == -5000)
                    {
                        board = childrenBoard[i];
                        childrenBoard = null;
                        return hValue;
                    }
                    if (minimumValue == hValue){//store move into list of moves of same value
                        equalMoves.Add(tempBoard);
                    }
                    else if (hValue < minimumValue){//new minimum value, reset the move list
                        minimumValue = hValue;
                        board = childrenBoard[i];
                        equalMoves.Empty();
                        equalMoves.Add(board);
                    }
                    if (minimumValue <= alpha){
                        board=getRandomMove(equalMoves);
                        childrenBoard = null;
                        return hValue;
                    }
                    beta = Math.Min(beta, minimumValue);
                }
                else{
                    minimumValue = Math.Min(minimumValue, hValue);
                    if (minimumValue <= alpha){
                        hValue = minimumValue;
                        childrenBoard = null;
                        return hValue;
                    }
                    beta = Math.Min(beta, minimumValue);
                    if (minimumValue == -5000)
                        return -5000;
                }
                ++i;
            }
            hValue = minimumValue;

            if (depth == 1)
            {
                board = getRandomMove(equalMoves);
            }
            childrenBoard = null;
            if (timer.Elapsed > maxTime) return -9999;
            hValue = minimumValue;
            return hValue;
        }

        //This min value version is only called at depth 1 when a previously found best move is used to inform the ordering of moves

        public static int minValue(ref char[] board, int depth, bool white, int alpha, int beta, int cutoff, char[] best, ref Stopwatch timer)
        {
            ChessColor color = (white ? ChessColor.White : ChessColor.Black);
            int hValue;
            LightList equalMoves = new LightList();
           
            LightList childrenBoard = FEN.GetAvailableMoves(board, color, false);
            int count = childrenBoard.Count;
            if (count == 0)
            { //no moves available
                if (FENExtensions.InCheck(board, white))
                    return -5000;//checkmate
                else
                    return -3000;//stalemate
            }
            //sort array of moves
            int[] Hchildren = new int[count];
            for (int idx = 0; idx < count; ++idx)
                Hchildren[idx] = Heuristic.GetHeuristicValue(childrenBoard[idx], color);

            sort(ref childrenBoard, ref Hchildren);
            bool found = false;
            int indexFound = 0;
            for (int idx = 0; idx < count; ++idx)
            {
                found = true;
                for (int index = 0; index < 71; ++index)
                {
                    if (best[index] != childrenBoard[idx][index])
                    {
                        found = false;
                        break;
                    }
                }
                if (found == true)
                {
                    indexFound = idx;
                    break;
                }
            }
            //prioritize previously found best move
            if (indexFound != 0){//swap
                char[] temp;
                temp = childrenBoard[0];
                childrenBoard.Replace(childrenBoard[indexFound], 0);
                childrenBoard.Replace(temp, indexFound);
            }
            int minimumValue = 10000;
            int i = 0;
            char[] tempBoard = null;
            while (i < count)
            { //process all the children moves
                if (depth != cutoff)
                {
                    tempBoard = childrenBoard[i];
                    hValue = maxValue(ref tempBoard, depth + 1, !white, alpha, beta, cutoff, ref timer);
                }
                else //get heuristics value
                    hValue = Hchildren[i];
                if (depth == 1)
                {
                    if (hValue == -5000)
                    {
                        board = childrenBoard[i];
                        childrenBoard = null;
                        return hValue;
                    }
                    if (minimumValue == hValue)
                    {
                       equalMoves.Add(tempBoard);
                    }
                    else if (hValue < minimumValue)
                    {
                        minimumValue = hValue;
                        board = childrenBoard[i];
                        equalMoves.Empty();
                        equalMoves.Add(board);
                    }
                    if (minimumValue <= alpha)
                    {
                        board = getRandomMove(equalMoves);
                        childrenBoard = null;
                        return hValue;
                    }
                    beta = Math.Min(beta, minimumValue);
                }
                else
                {
                    minimumValue = Math.Min(minimumValue, hValue);
                    if (minimumValue <= alpha)
                    {
                        hValue = minimumValue;
                        childrenBoard = null;
                        return hValue;
                    }
                    beta = Math.Min(beta, minimumValue);
                    if (minimumValue == -5000)
                        return -5000;
                }
                ++i;
            }
            hValue = minimumValue;
            if (depth == 1)
                board = getRandomMove(equalMoves);
            
            childrenBoard = null;
            return hValue;
        }

        private static char[] getRandomMove(LightList equalMoves)
        {
            if (equalMoves.Count == 1)
                return equalMoves[0];
            else
            {
                Random rnd = new Random();
                int value = rnd.Next(0, equalMoves.Count - 1);
                return equalMoves[value];
            }
        }

        private static void sort(ref LightList children, ref int[] h)
        {
            int count = children.Count;
            bool swapped;
            do
            {
                swapped = false;
                for (int j = 1; j < count; ++j)
                {
                    if (h[j - 1] < h[j])
                    {
                        int temp;
                        char[] tempBoard;
                        temp = h[j];
                        tempBoard = children[j];
                        h[j] = h[j - 1];
                        h[j - 1] = temp;
                        children.Replace(children[j - 1], j);
                        children.Replace(tempBoard, j - 1);
                        swapped = true;
                    }
                }
                count--;
            } while (swapped);
        }
    }
}
