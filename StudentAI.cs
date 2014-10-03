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
            get { return "Shallow Red (Debug)"; }
#else
            get { return "Shallow Red"; }
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
                List<char[]> possibleOpponentMove = FEN.GetAvailableMoves(boardAfterMove, white ? ChessColor.White : ChessColor.Black);
                if (possibleOpponentMove.Count == 0) move.Flag = ChessFlag.Checkmate;
            }
            else
                move.Flag = ChessFlag.NoFlag;

            //get available from opponent to check for checkmate

            return move;
            //  throw (new NotImplementedException());

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
            List<char[]> legalBoards = FEN.GetLegalMoves(SRfen, colorOfPlayerMoving);
            char[] boardToCheck = FENExtensions.Move(SRfen, moveToCheck.From.X, moveToCheck.From.Y, moveToCheck.To.X, moveToCheck.To.Y);
            bool legal = false;
            //check that move results in a legal board
            foreach (char[] board in legalBoards)
            {
                bool equal = true;
                for (int i = 0; i < 71; i++)
                {
                    if (board[i] != boardToCheck[i])
                    {
                        equal = false;
                        break;
                    }
                }
                if (equal == true)
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
            /*  ChessColor colorOfOpponent;
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
            if (color == ChessColor.White) white = true;
            else white = false;
            // List<Char[]> legalBoards= FEN.GetAvailableMoves (SRFen, white); //AvailableMoves defined by Greg
            //need to build tree
            NodeMove init = new NodeMove(SRFen);
            GameTree Tree = new GameTree(init);
            int depth = 0;
            miniMaxHelper(ref Tree.head, depth + 1, white, SRFen);
            // NodeMove choice = maxValue(Tree.head.children);
            //look at children of the head


            chosenBoard = Tree.head.board;
            return chosenBoard;
        }

        public void miniMaxHelper(ref NodeMove ParentNode, int depth, bool white, char[] SRFen)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            //    TimeSpan time = new TimeSpan(0);
            const int MAXDEPTH = 2;
            ChessColor color = ChessColor.White;
            if (white)
            {
                color = ChessColor.White;
            }
            else
            {
                color = ChessColor.Black;
            }

            List<Char[]> LegalBoard = FEN.GetAvailableMoves(SRFen, color);
            int i = 0;
            foreach (char[] ChildrenBoard in LegalBoard)
            {
                int h;
                NodeMove childNode;
                if (depth == MAXDEPTH)
                {
                    //get heuristic value for the board
                    h = Heuristic.GetHeuristicValue(ChildrenBoard, color);
                    childNode = new NodeMove(ChildrenBoard, h, ParentNode);
                }
                else
                    childNode = new NodeMove(ChildrenBoard, ParentNode);
                ParentNode.children[i] = childNode;
                i++;
                if (depth < MAXDEPTH)
                {

                    miniMaxHelper(ref childNode, depth + 1, !white, ChildrenBoard);

                    //   time = time+ timer.Elapsed;

                }

            }

            if (depth % 2 == 0) //get the max value for the opponent
            {
                NodeMove choice = maxValue(ParentNode.children);
                ParentNode.h = choice.h;

            }
            else
            {
                //get the max
                NodeMove choice;
                if (depth == 1) choice = minValue(ParentNode.children, true);
                else choice = minValue(ParentNode.children, false);
                ParentNode.h = choice.h;
                if (depth == 1) { ParentNode.board = choice.board; }
            }
            timer.Stop();
            if (depth != MAXDEPTH && depth != 3)
            {
                Console.Write("depth:");
                Console.WriteLine(depth);
                Console.Write("timer:");
                Console.WriteLine(timer.Elapsed);
            }
        }

        public NodeMove minValue(NodeMove[] children, bool isAtTop)
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

        public NodeMove maxValue(NodeMove[] children)
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
            get{ return log; }
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
