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
            char[] boardToCheck = FENExtensions.Move(SRfen, moveToCheck.From.X, moveToCheck.From.Y, moveToCheck.To.X, moveToCheck.To.Y);
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
            bool white = true;
            if (colorOfPlayerMoving == ChessColor.White)
                white = false;
            if (FENExtensions.InCheck(boardToCheck, white))
                testFlag = ChessFlag.Check;
            if (testFlag != moveToCheck.Flag)
                return false;
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
            Char[] chosenBoard;
            bool white;
            int alpha = -10000;
            int beta = 10000;
            int cutoff = 4;
            if (color == ChessColor.White) white = true;
            else white = false;
            // List<Char[]> legalBoards= FEN.GetAvailableMoves (SRFen, white); //AvailableMoves defined by Greg
            //need to build tree
            NodeMove init = new NodeMove(SRFen);
            GameTree Tree = new GameTree(init);
            int depth = 0;
            minValue(ref Tree.head, depth + 1, white, alpha, beta, cutoff);
            //call min
            chosenBoard = Tree.head.board;
            this.Log("CHOSEN MOVE" + Tree.head.h);
            return chosenBoard;
        }
        public void maxValue(ref NodeMove parentNode, int depth, bool white, int alpha, int beta, int cutoff)
        {
            ChessColor color;
            NodeMove choice;
            if (white)
                color = ChessColor.White;
            else
                color = ChessColor.Black;
            generateChildren(ref parentNode, color);
            if (parentNode.children[0] == null)
            { //no moves available
                parentNode.h = -1000;
                return;
            }
            int maximumValue = -10000;
            int i = 0;
            while (parentNode.children[i] != null)
            { //process all the children move
                if (depth != cutoff)
                    minValue(ref parentNode.children[i], depth + 1, !white, alpha, beta, cutoff);
                else
                    parentNode.children[i].h = Heuristic.GetHeuristicValue(parentNode.children[i].board, color);
                if (depth == 1)
                {
                    if (maximumValue == parentNode.children[i].h)
                    {
                        //choose randomly between current choice and this move
                        Random rnd = new Random();
                        int value = rnd.Next(0, 1);
                        if (value == 0)
                            parentNode.board = parentNode.children[i].board;
                    }
                    else if (parentNode.children[i].h > maximumValue)
                    {
                        maximumValue = parentNode.children[i].h;
                        parentNode.board = parentNode.children[i].board;
                        //parentNode.h = minimumValue;
                    }
                    if (maximumValue >= beta)
                    {
                        parentNode.h = maximumValue;
                        // this.Log("pruned: depth" + depth + " h= " + parentNode.h + "beta" + beta);
                        return;
                    }
                    alpha = Math.Max(alpha, maximumValue);
                }
                else
                {
                    maximumValue = Math.Max(maximumValue, parentNode.children[i].h);
                    if (maximumValue >= beta)
                    {
                        parentNode.h = maximumValue;
                        // this.Log("pruned: depth" + depth + " h= " + parentNode.h+ "beta" + beta);
                        return;
                    }
                    alpha = Math.Max(alpha, maximumValue);
                }
                ++i;
            }
            parentNode.h = maximumValue;
            this.Log("depth" + depth + " h= " + parentNode.h);
        }
        public void minValue(ref NodeMove parentNode, int depth, bool white, int alpha, int beta, int cutoff)
        {
            ChessColor color;
            //NodeMove choice;
            if (white)
                color = ChessColor.White;
            else
                color = ChessColor.Black;
            generateChildren(ref parentNode, color);
            if (parentNode.children[0] == null)
            { //no moves available
                parentNode.h = -1000;
            }
            int minimumValue = 10000;
            int i = 0;
            while (parentNode.children[i] != null)
            { //process all the children move
                if (depth != cutoff)
                    maxValue(ref parentNode.children[i], depth + 1, !white, alpha, beta, cutoff);
                else
                    //get heuristics value
                    parentNode.children[i].h = Heuristic.GetHeuristicValue(parentNode.children[i].board, color);
                if (depth == 1)
                {
                    if (minimumValue == parentNode.children[i].h)
                    {
                        //choose randomly between current choice and this move
                        Random rnd = new Random();
                        int value = rnd.Next(0, 1);
                        if (value == 0)
                            parentNode.board = parentNode.children[i].board;
                    }
                    else if (parentNode.children[i].h < minimumValue)
                    {
                        minimumValue = parentNode.children[i].h;
                        parentNode.board = parentNode.children[i].board;
                        //parentNode.h = minimumValue;
                    }
                    if (minimumValue <= alpha)
                    {
                        parentNode.h = minimumValue;
                        // this.Log("pruned: depth" + depth + " h= " + parentNode.h + "alpha" + alpha);
                        return;
                    }
                    beta = Math.Min(beta, minimumValue);
                }
                else
                {
                    minimumValue = Math.Min(minimumValue, parentNode.children[i].h);
                    if (minimumValue <= alpha)
                    {
                        parentNode.h = minimumValue;
                        // this.Log("pruned: depth" + depth + " h= " + parentNode.h + "alpha:" + alpha);
                        return;
                    }
                    beta = Math.Min(beta, minimumValue);
                }
                ++i;
            }
            parentNode.h = minimumValue;
            this.Log("depth" + depth + " h= " + parentNode.h);
        }
        public void generateChildren(ref NodeMove parentNode, ChessColor color)
        {
            //generate children for parentNode
            LightList LegalBoard = FEN.GetAvailableMoves(parentNode.board, color, false);
            NodeMove childNode;
            for (int i = 0; i < LegalBoard.Count; ++i)
            {
                childNode = new NodeMove(LegalBoard[i]);
                parentNode.children[i] = childNode;
            }
        }
        /* public void miniMaxHelper(ref NodeMove ParentNode, int depth, bool white, char[] SRFen, ref int alpha, ref int beta, int cutoff)
        {
        Stopwatch timer = new Stopwatch();
        timer.Start();
        // TimeSpan time = new TimeSpan(0);
        // const int MAXDEPTH = 2;
        ChessColor color = ChessColor.White;
        if (white)
        color = ChessColor.White;
        else
        color = ChessColor.Black;
        List<Char[]> LegalBoard = FEN.GetAvailableMoves(SRFen, color);
        int i = 0;
        foreach (char[] ChildrenBoard in LegalBoard){
        int h;
        NodeMove childNode;
        if (depth == cutoff){
        //get heuristic value for the board
        h = Heuristic.GetHeuristicValue(ChildrenBoard, color);
        childNode = new NodeMove(ChildrenBoard, h, ParentNode);
        }
        else
        childNode = new NodeMove(ChildrenBoard, ParentNode);
        ParentNode.children[i] = childNode;
        i++;
        if (depth < cutoff){
        miniMaxHelper(ref childNode, depth + 1, !white, ChildrenBoard, ref alpha, ref beta, cutoff);
        // time = time+ timer.Elapsed;
        }
        }
        if (depth % 2 == 0) {//get the max value for the opponent
        NodeMove choice = maxValue(ParentNode.children, ref alpha, ref beta);
        ParentNode.h = choice.h;
        }
        else {
        //get the max
        NodeMove choice;
        if (depth == 1) choice = minValue(ParentNode.children, true, ref alpha, ref beta);
        else choice = minValue(ParentNode.children, false, ref alpha, ref beta);
        ParentNode.h = choice.h;
        if (depth == 1) { ParentNode.board = choice.board; }
        }
        timer.Stop();
        if (depth != cutoff && depth != 3)
        {
        Console.Write("depth:");
        Console.WriteLine(depth);
        Console.Write("timer:");
        Console.WriteLine(timer.Elapsed);
        }
        }
        public NodeMove minValue(NodeMove[] children, bool isAtTop, ref int alpha, ref int beta)
        {
        NodeMove choice = children[0];
        int minH = choice.h;
        //find min heuristics for moves in the children list
        if (isAtTop)
        {
        NodeMove[] equalBoard = new NodeMove[100];
        int countSameMin = 0;
        int i = 0;
        while (children[i] != null)
        {
        if (children[i].h == minH)
        {
        equalBoard[countSameMin] = children[i];
        countSameMin++;
        }
        if (children[i].h < minH)
        {
        minH = children[i].h;
        choice = children[i];
        equalBoard[0] = children[i];
        countSameMin = 1;
        }
        i++;
        }
        if (countSameMin > 1)
        {
        Random rnd = new Random();
        int value = rnd.Next(0, countSameMin);
        choice = equalBoard[value];
        }
        }
        else
        {
        int i = 0;
        while (children[i] != null)
        {
        if (children[i].h > minH)
        {
        minH = children[i].h;
        choice = children[i];
        }
        i++;
        }
        }
        return choice;
        }
        public NodeMove maxValue(NodeMove[] children, ref int alpha, ref int beta)
        {
        NodeMove choice = children[0];
        if (choice == null)
        {
        choice = new NodeMove(-99);
        return choice;
        }
        int maxH = choice.h;
        //find min heuristics for moves in the children list
        int i = 0;
        while (children[i] != null)
        {
        if (children[i].h > maxH)
        {
        maxH = children[i].h;
        choice = children[i];
        }
        i++;
        }
        return choice;
        }*/
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