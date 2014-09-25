using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UvsChess;
using ShallowRed;

namespace StudentAI
{
    public class NodeMove
    {
        public char[] board;
        public int h;
        public NodeMove parent;
        public List<NodeMove> children;
        public NodeMove(){
           
        }

        public NodeMove (char[] _board, NodeMove _parent){
            board = _board;
            parent = _parent;
            children = new List<NodeMove>();
        }

        public NodeMove(char[] _board, int _h, NodeMove _parent)
        {
            board = _board;
            h = _h;
            parent = _parent;
            children = new List<NodeMove>();
        }

        public NodeMove(char[] _board)
        {
            board = _board;
            parent = null;
            children = new List<NodeMove>();
        }

    }

    public class GameTree
    {
        public NodeMove head;
        public GameTree(NodeMove init)
        {
            head = init;
        }
    }
    public class StudentAI : IChessAI
    {
        #region IChessAI Members that are implemented by the Student

        /// <summary>
        /// Shallow Red
        /// </summary>
        public string Name
        {
#if DEBUG
            get { return "StudentAI (Debug)"; }
#else
            get { return "StudentAI"; }
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
            String originalFenBoard=board.ToPartialFenBoard();
            char[] SRfen= FENExtensions.ToShallowRedFEN(originalFenBoard);
            char[] boardAfterMove= miniMax(SRfen, myColor);

            ChessMove move = FENExtensions.GenerateMove(SRfen, boardAfterMove);
            return move;
            throw (new NotImplementedException());

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
            bool white=false;
            if (colorOfPlayerMoving==ChessColor.White) white=true;
            string stdFen = boardBeforeMove.ToPartialFenBoard();
            char[] SRfen = FENExtensions.ToShallowRedFEN(stdFen);
            int to=moveToCheck.To.Y*9+moveToCheck.To.X;
            return FENExtensions.IsValidMove(SRfen,white,to);
            
            throw (new NotImplementedException());
            
        }

        public string convertToShallowRedForm (string originalFen)
        {
            string newForm="";
            foreach (char c in originalFen ){
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
            GameTree Tree=new GameTree(init);
            int depth =0;
            miniMaxHelper(ref Tree.head, depth+1, white, SRFen);
           // NodeMove choice = maxValue(Tree.head.children);
            //look at children of the head
            

            chosenBoard = Tree.head.board;
            return chosenBoard;
        }

        public void miniMaxHelper(ref NodeMove ParentNode, int depth, bool white, char[] SRFen)
        {
            const int MAXDEPTH = 4;
            ChessColor color = ChessColor.White;
            if (white)
            {
                color = ChessColor.White;
            }
            else
            {
                color = ChessColor.Black;
            }
            List<Char[]> LegalBoard = FEN.GetAvailableMoves(SRFen,color);

            foreach (char[] ChildrenBoard in LegalBoard)
            {
                int h;
                NodeMove childNode;
                if (depth == MAXDEPTH)
                {
                    //get heuristic value for the board
                    h = GetHeuristicValue(ChildrenBoard, color);
                    childNode = new NodeMove(ChildrenBoard, h, ParentNode);
                }
                else
                    childNode = new NodeMove(ChildrenBoard,ParentNode);
                ParentNode.children.Add(childNode);
                if (depth < MAXDEPTH)
                {
                    miniMaxHelper(ref childNode, depth+1, !white, ChildrenBoard);
                }

            }
            if (depth % 2 == 0) //get the min value for the opponent
            {
                NodeMove choice = maxValue(ParentNode.children);
                ParentNode.h = choice.h;

            }
            else
            {
                //get the max
                NodeMove choice;
                if (depth == 1) choice = minValue(ParentNode.children, true); 
                else choice = minValue(ParentNode.children,false);
                ParentNode.h = choice.h;
                if (depth == 1) { ParentNode.board = choice.board; }
            }          
        }

        public NodeMove minValue(List<NodeMove> children, bool isAtTop)
        {
            NodeMove choice= children[0];
            int minH=choice.h;
            //find min heuristics for moves in the children list
            if (isAtTop)
            {
                NodeMove[] equalBoard = new NodeMove[100];
                int i = 0;
                foreach (NodeMove child in children)
                {
                    if (child.h == minH)
                    {
                        equalBoard[i] = child;
                        i++;
                    }
                    if (child.h < minH)
                    {
                        minH = child.h;
                        choice = child;
                        equalBoard[0] = child;
                        i = 1;
                    }
                }
                if (i > 1)
                {
                    Random rnd = new Random();
                    int value = rnd.Next(0, i);
                    choice = equalBoard[value];
                }
            }
            else
            {
                foreach (NodeMove child in children)
                {
                    if (child.h < minH)
                    {
                        minH = child.h;
                        choice = child;
                    }
                }
            }
            return choice;
        }

        public NodeMove maxValue(List<NodeMove> children)
        {
            NodeMove choice = children[0];
            int maxH = choice.h;
            //find min heuristics for moves in the children list
            foreach (NodeMove child in children)
            {
                if (child.h > maxH)
                {
                    maxH = child.h;
                    choice = child;
                }
            }
            return choice;
        }


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
           /* Random rnd=new Random();
            int value = rnd.Next(1, 15);
            return value;*/
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
        #endregion
