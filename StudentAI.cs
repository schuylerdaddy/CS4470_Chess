using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UvsChess;
using ShallowRed;
using System.IO;
using System.Diagnostics;
namespace StudentAI
{
    public class StudentAI : IChessAI
    {
        #region IChessAI Members that are implemented by the Student
        /// <summary>
        /// Shallow Red
        /// </summary>
        ///
        public string Name
        {
#if Debug
get { return "Shallow Red 2 (Debug)"; }
#else
            get { return "Shallow Red 2"; }
#endif
        }
        
        /// <summary>
        /// Evaluates the chess board and decided which move to make. This is the main method of the AI.
        /// The framework will call this method when it's your turn.
        /// </summary>
        /// <param name="board">Current chess board< /param>
        /// <param name="yourColor">Your color</param>
        /// <returns> Returns the best chess move the player has for the given chess board</returns>
        public ChessMove GetNextMove(ChessBoard board, ChessColor myColor)
        {
            //convert Fen for to shallow red fen form
            String originalFenBoard = board.ToPartialFenBoard();
            char[] SRfen = FENExtensions.ToShallowRedFEN(originalFenBoard);
            char[] boardAfterMove = miniMax(SRfen, myColor);
            ChessMove move = FENExtensions.GenerateMove(SRfen, boardAfterMove);
            bool white = true;
            if (myColor == ChessColor.White)
                white = false;
            if (FENExtensions.InCheck(boardAfterMove, white))
            {
                move.Flag = ChessFlag.Check;
                LightList possibleOpponentMove = FEN.GetAvailableMoves(boardAfterMove, white ? ChessColor.White : ChessColor.Black,false);
                if (possibleOpponentMove.Count == 0) move.Flag = ChessFlag.Checkmate;
            }
            else
            {
                move.Flag = ChessFlag.NoFlag;
                //check for stalemate
                LightList possibleOpponentMove = FEN.GetAvailableMoves(boardAfterMove, white ? ChessColor.White : ChessColor.Black, false);
                if (possibleOpponentMove.Count == 0) move.Flag = ChessFlag.Stalemate;
            }
            return move;
            // throw (new NotImplementedException());
        }
        /// <summary>
        /// Validates a move. The framework uses this to validate the opponents move.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <param name="colorOfPlayerMoving">This is the color of the player who's making the move.</param>
        /// <returns>Returns true if the move was valid</returns>
        public bool IsValidMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            string stdFen = boardBeforeMove.ToPartialFenBoard();
            char[] SRfen = FENExtensions.ToShallowRedFEN(stdFen);
            //need board after the move
            LightList legalBoards = FEN.GetAvailableMoves(SRfen, colorOfPlayerMoving, true);
            int to = moveToCheck.To.X + (9 * moveToCheck.To.Y);
            int from = moveToCheck.From.X + (9 * moveToCheck.From.Y);
            bool white = (colorOfPlayerMoving == ChessColor.White) ? true : false;
            char[] boardToCheck;
            if (SRfen[from] == 'p'|| SRfen[from]=='P') 
                boardToCheck= FENExtensions.MovePawn(SRfen, from, to, white);
            else  
                boardToCheck = FENExtensions.Move(SRfen, moveToCheck.From.X, moveToCheck.From.Y, moveToCheck.To.X, moveToCheck.To.Y);
            
            bool legal = false;
            //check that move results in a legal board
            for(int idx =0; idx < legalBoards.Count;++idx)
            {
                char[] board = legalBoards[idx];
                bool equal = true;
                for (int i = 0; i < 71; i++)
                {
                    if (board[i] != boardToCheck[i])
                    {
                        equal = false;
                        break;
                    }
                }
                if (equal)
                {
                    legal = true;
                    break;
                }
            }
            if (!legal)
                return legal;
            //if move is legal check the flag
            ChessFlag testFlag = ChessFlag.NoFlag;
            //check if move result in check for us
          
            if (FENExtensions.InCheck(boardToCheck, !white))
                testFlag = ChessFlag.Check;
            if (testFlag != moveToCheck.Flag)
                return false;

            //check stalemate flag
            //check if checkmate
            //getAvailable moves, if none then checkMate
            /* ChessColor colorOfOpponent;
            if (colorOfPlayerMoving == ChessColor.White)
            colorOfOpponent = ChessColor.Black;
            else
            colorOfOpponent = ChessColor.White;*/
            return legal;
            //throw (new NotImplementedException());
        }
        public string convertToShallowRedForm(string originalFen)
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
            return newForm;
        }
        public char[] miniMax(char[] SRFen, ChessColor color)
        {
            //Char[] chosenBoard;
            bool white;
            int alpha = -10000;
            int beta = 10000;
            int cutoff = 4;
            if (color == ChessColor.White) white = true;
            else white = false;
            // List<Char[]> legalBoards= FEN.GetAvailableMoves (SRFen, white); //AvailableMoves defined by Greg
            //need to build tree
            NodeMove init = new NodeMove(SRFen);
          //  GameTree Tree = new GameTree(init);
            int depth = 0;
            minValue(ref SRFen, depth + 1, white, alpha, beta, cutoff);
            //call min
            //chosenBoard = init.board;
            //this.Log("CHOSEN MOVE" + init.h);
            return SRFen;
        }
        public int maxValue(ref char[] board, int depth, bool white, int alpha, int beta, int cutoff)
        {
            ChessColor color;
           // NodeMove choice;
            int hValue;
            if (white)
                color = ChessColor.White;
            else
                color = ChessColor.Black;
            LightList childrenBoard = FEN.GetAvailableMoves(board, color, false);
            //generateChildren(ref parentNode, color);
            int count = childrenBoard.Count;
            if (count == 0)
            { //no moves available
                //parentNode.h = -1000;
                //return;
                return -1000;
            }
            int maximumValue = -10000;
            int i = 0;
            char[] tempBoard=null;
            while (i<count)
            { //process all the children move
                if (depth != cutoff)  {
                   tempBoard = childrenBoard[i];
                    hValue= minValue(ref tempBoard, depth + 1, !white, alpha, beta, cutoff);
                 }
                else
                    hValue = Heuristic.GetHeuristicValue(childrenBoard[i], color);
                if (depth == 1)             {
                    if (maximumValue == hValue)    {
                        //choose randomly between current choice and this move

                        Random rnd = new Random();
                        int value = rnd.Next(0, 1);
                        if (value == 0)
                            board = tempBoard;                    }
                    else if (hValue > maximumValue)                    {
                        maximumValue = hValue;
                        board = childrenBoard[i];
                        //parentNode.h = minimumValue;                   
                    }
                    if (maximumValue >= beta)               {
                        hValue = maximumValue;
                        return hValue;                    }
                    alpha = Math.Max(alpha, maximumValue);                }
                else                {
                    maximumValue = Math.Max(maximumValue, hValue);
                    if (maximumValue >= beta)                    {
                        hValue = maximumValue;
                        return hValue;                    }
                    alpha = Math.Max(alpha, maximumValue);                }
                ++i;            }
            hValue = maximumValue;
            return hValue;
        }
        public int minValue(ref char[] board, int depth, bool white, int alpha, int beta, int cutoff)
        {
            
            ChessColor color;
            int hValue;
            LightList equalMoves=new LightList();
            //NodeMove choice;
            if (white)
                color = ChessColor.White;
            else
                color = ChessColor.Black;
            LightList childrenBoard = FEN.GetAvailableMoves(board, color, false);
            //generateChildren(ref parentNode, color);
            int count = childrenBoard.Count;
            if (count == 0)
            { //no moves available
                //parentNode.h = -1000;
                //return;
                return -1000;
            }
            int minimumValue = 10000;
            int i = 0;
            char[] tempBoard = null;
            while (i<count)
            { //process all the children move
                if (depth != cutoff)
                {
                    tempBoard = childrenBoard[i];
                    hValue= maxValue(ref tempBoard, depth + 1, !white, alpha, beta, cutoff);
                }
                else
                    //get heuristics value
                    hValue = Heuristic.GetHeuristicValue(childrenBoard[i], color);
                if (depth == 1)
                {
                    if (minimumValue == hValue)
                    {
                        //choose randomly between current choice and this move
                        equalMoves.Add(tempBoard);

                    }
                    else if (hValue < minimumValue)
                    {
                        minimumValue = hValue;
                        board = childrenBoard[i];
                        equalMoves.Empty();
                        equalMoves.Add(board);
                        //parentNode.h = minimumValue;
                    }
                    if (minimumValue <= alpha)
                    {
                        //hValue = minimumValue;
                        // this.Log("pruned: depth" + depth + " h= " + parentNode.h + "alpha" + alpha);
                        if (equalMoves.Count == 1)
                            board = equalMoves[0];
                        else
                        {
                            Random rnd = new Random();
                            int value = rnd.Next(0, equalMoves.Count-1);
                            board = equalMoves[value];
                        }
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
                      /*  int k = 0;
                        while (parentNode.children[k] != null)
                        {
                            parentNode.children[k] = null;
                            k++;
                        }*/
  //                      GC.Collect();
                        // this.Log("pruned: depth" + depth + " h= " + parentNode.h + "alpha:" + alpha);
                        return hValue;
                    }
                    beta = Math.Min(beta, minimumValue);
                }
                ++i;
            }
            hValue = minimumValue;
         /*   int j = 0;
            while (parentNode.children[j] != null)
            {
                parentNode.children[j] = null;
                j++;
            }*/
           // GC.Collect();
          //  this.Log("depth" + depth + " h= " + parentNode.h);
            if (depth == 1)
            {
                if (equalMoves.Count == 1)
                    board = equalMoves[0];
                else
                {
                    Random rnd = new Random();
                    int value = rnd.Next(0, equalMoves.Count - 1);
                    board = equalMoves[value];
                }
            }
            return hValue;
        }
 
        #endregion
        #region IChessAI Members that should be implemented as automatic properties and should NEVER be touched by students.
        /// <summary>
        /// This will return false when the framework starts running your AI. When the AI's time has run out,
        /// then this method will return true. Once this method returns true, your AI should return a
        /// move immediately.
        ///
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        public AIIsMyTurnOverCallback IsMyTurnOver { get; set; }
        /// <summary>
        /// Call this method to print out debug information. The framework subscribes to this event
        /// and will provide a log window for your debug messages.
        ///
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="message"></param>
        private AILoggerCallback log;
        public AILoggerCallback Log
        {
            get { return log; }
            set
            {
                log = value;
                Heuristic.Log = value;
                FEN.Log = value;
                FENExtensions.Log = value;
            }
        }
        /// <summary>
        /// Call this method to catch profiling information. The framework subscribes to this event
        /// and will print out the profiling stats in your log window.
        ///
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="key"></param>
        public AIProfiler Profiler { get; set; }
        /// <summary>
        /// Call this method to tell the framework what decision print out debug information. The framework subscribes to this event
        /// and will provide a debug window for your decision tree.
        ///
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="message"></param>
        public AISetDecisionTreeCallback SetDecisionTree { get; set; }
        #endregion
    }
}
