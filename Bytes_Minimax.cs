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
        /// <returns>byte[] board to represent the move</returns>
        /// 

        public static byte[] miniMax(byte[] SRFen, ChessColor color)
        {
            TimeSpan maxTime = TimeSpan.FromMilliseconds(5500);
            byte[] initialBoard = (byte[])SRFen.Clone();
            bool white= color == ChessColor.White? true:false;
            int alpha = -10000;
            int beta = 10000;
            int cutoff = 4;
            int depth = 0;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            int h;
            h = minValue(ref SRFen, depth + 1, white, alpha, beta, cutoff, ref timer);
            if (h == -5000) 
                return SRFen;
            byte[] bestSoFar = (byte[])SRFen.Clone();
            while (timer.Elapsed < maxTime && h != -9999)
            { 
                cutoff += 2;
                byte[] temp = (byte[])initialBoard.Clone();
                h = minValue(ref temp, depth + 1, white, alpha, beta, cutoff, bestSoFar, ref timer);
                if (temp!=null) bestSoFar = (byte[])temp.Clone();
                if (h == -5000) 
                    return bestSoFar;
            }
            Log("cutoff" + cutoff);
            return bestSoFar;
        }

        public static int maxValue(ref byte[] board, int depth, bool white, int alpha, int beta, int cutoff, ref Stopwatch timer)
        {
            TimeSpan maxTime = TimeSpan.FromMilliseconds(5500);
            ChessColor color = (white ? ChessColor.White : ChessColor.Black);
            int hValue, count;
            if (depth == cutoff)
                return Heuristic.GetHeuristicValue(board, color);
            LightList childrenBoard = FEN.GetAvailableMoves(board, color, false);
            count = childrenBoard.Count;
            if (count == 0){ //no moves available
               if (FEN.InCheck(board, white))
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
            byte[] tempBoard = null;
            while (i < count && timer.Elapsed < maxTime){ //process all the children move
                tempBoard = childrenBoard[i];
                if (timer.Elapsed < maxTime)
                    hValue = minValue(ref tempBoard, depth + 1, !white, alpha, beta, cutoff, ref timer);
                else
                    return -9999;
                if (depth == 1){
                    if (maximumValue == hValue){
                        //choose randomly between current choice and this move
                        Random rnd = new Random();
                        int value = rnd.Next(0, 1);
                        if (value == 0)
                            board = tempBoard;
                    }
                    else if (hValue > maximumValue) {
                        maximumValue = hValue;
                        board = childrenBoard[i];
                    }
                    if (maximumValue >= beta){
                        hValue = maximumValue;
                        childrenBoard = null;
                        return hValue;
                    }
                    alpha = Math.Max(alpha, maximumValue);
                }
                else
                {
                    maximumValue = Math.Max(maximumValue, hValue);
                    if (maximumValue >= beta){
                        hValue = maximumValue;
                        childrenBoard = null;
                        return hValue;
                    }
                    alpha = Math.Max(alpha, maximumValue);
                }
                ++i;
            }
            childrenBoard = null;
            if (timer.Elapsed > maxTime) 
                return -9999;
            hValue = maximumValue;

            return hValue;
        }
        public static int minValue(ref byte[] board, int depth, bool white, int alpha, int beta, int cutoff, ref Stopwatch timer)
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
                if (FEN.InCheck(board, white))
                    return -5000;//checkmate
                else
                    return -3000;//stalemate
            }
            //sort array of moves
            int[] Hchildren = new int[count];
            for (int idx = 0; idx < count; ++idx)
                Hchildren[idx] = Heuristic.GetHeuristicValue(childrenBoard[idx], color);
            sort(ref childrenBoard, ref Hchildren);
            int minimumValue = 10000;
            int i = 0;
            byte[] tempBoard = null;
            int move = -99;
            int moveFirst = -99;
            while (i < count)
            { //process all the children move
                    tempBoard = childrenBoard[i];
                    if (timer.Elapsed < maxTime)
                        hValue = maxValue(ref tempBoard, depth + 1, !white, alpha, beta, cutoff, ref timer);
                    else
                        return -9999;
                if (depth == 1) {
                    if (hValue == -5000) {
                        board = childrenBoard[i];
                        childrenBoard = null;
                       
                       Log("move position: " + i + " count=" + count);
                        return hValue;
                    }
                    if (minimumValue == hValue){//store move into list of moves of same value
                        equalMoves.Add(tempBoard);
                        move = i;
                    }
                    else if (hValue < minimumValue){//new minimum value, reset the move list
                        minimumValue = hValue;
                        board = childrenBoard[i];
                        move = i;
                        moveFirst = i;
                        equalMoves.Empty();
                        equalMoves.Add(board);
                    }
                    if (minimumValue <= alpha){
                        board=getRandomMove(equalMoves);
                        childrenBoard = null;
                       Log("move position: " + move + " count=" + count);
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
                Log("movefirst position: " + moveFirst + "move last " +move+" count=" + count);
            }

            childrenBoard = null;
            if (timer.Elapsed > maxTime) return -9999;
            hValue = minimumValue;
            return hValue;
        }

        //This min value version is only called at depth 1 when a previously found best move is used to inform the ordering of moves

        public static int minValue(ref byte[] board, int depth, bool white, int alpha, int beta, int cutoff, byte[] best, ref Stopwatch timer)
        {
            ChessColor color = (white ? ChessColor.White : ChessColor.Black);
            int hValue;
            LightList equalMoves = new LightList();
            TimeSpan maxTime = TimeSpan.FromMilliseconds(5500);
           
            LightList childrenBoard = FEN.GetAvailableMoves(board, color, false);
            int count = childrenBoard.Count;
            if (count == 0)
            { 
                if (FEN.InCheck(board, white))
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
                for (int index = 0; index < FEN.OUTOFBOUNDSHIGH; ++index)
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
                shift(ref childrenBoard, ref Hchildren, indexFound);
            }
            int minimumValue = 10000;
            int i = 0;
            byte[] tempBoard = null;
            int move = -99;
            int moveFirst = -99;
            while (i < count)
            { //process all the children moves
                tempBoard = childrenBoard[i];
                if (timer.Elapsed < maxTime)
                {
                    hValue = maxValue(ref tempBoard, depth + 1, !white, alpha, beta, cutoff, ref timer);
                    if (hValue == -9999)
                    {
                        Log("i: " + i);
                        break;
                    }
                }
                else
                    break;
               if (hValue == -5000)
               {
                    board = childrenBoard[i];
                    childrenBoard = null;
                    Log("move position: " + i + " count=" + count);
                    return hValue;
               }
               if (minimumValue == hValue)
               {
                   equalMoves.Add(tempBoard);
                   move = i;
               }
               else if (hValue < minimumValue)
               {
                   minimumValue = hValue;
                   board = childrenBoard[i];
                   equalMoves.Empty();
                   equalMoves.Add(board);
                   move = i;
                   moveFirst = i;
               }
               if (minimumValue <= alpha)
               {
                    board = getRandomMove(equalMoves);
                    childrenBoard = null;
                    Log("move position: " + move + " count=" + count);
                    return hValue;
               }
               beta = Math.Min(beta, minimumValue);
               ++i;
            }
            if (equalMoves.Count!=0) 
                board = getRandomMove(equalMoves);
            else 
                board=null;
            childrenBoard = null;
            Log("movefirst position: " + moveFirst+" move last: "+move + " count=" + count);
            return minimumValue;
        }

        //returns a move/board randomly from a set of board with equal quality
        private static byte[] getRandomMove(LightList equalMoves)
        {
            if (equalMoves.Count == 1)
            {
                return equalMoves[0];
            }
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
                        byte[] tempBoard;
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

        private static void shift(ref LightList children, ref int[] h, int indexFound)
        {
            byte [] tempBest=children[indexFound];
            int Hbest=h[indexFound];

            for (int i = indexFound; i > 0; --i)
            {
                children.Replace(children[i - 1], i);
                h[i] = h[i - 1];
            }
            children.Replace(tempBest, 0);
            h[0] = Hbest;
        }
    }
}
