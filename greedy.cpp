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
        public NodeMove()
        {

        }

        public NodeMove(char[] _board, NodeMove _parent)
        {
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
            //this.Log("Let the Game Begin!");
            //convert Fen for to shallow red fen form
            String originalFenBoard = board.ToPartialFenBoard();
            char[] SRfen = FENExtensions.ToShallowRedFEN(originalFenBoard);

            char[] boardAfterMove = greedy(SRfen, myColor);
            //char[] boardAfterMove = miniMax(SRfen, myColor);

            ChessMove move = FENExtensions.GenerateMove(SRfen, boardAfterMove);
            this.Log("Our Color:" + ChessColor.White.ToString());
            bool white = (myColor == ChessColor.White) ? false : true;
            this.Log("Enemy team is: " + white.ToString());
            if (InCheck(boardAfterMove, white))
            {
                this.Log("They're in check!!");
                move.Flag = ChessFlag.Check;
                List<char[]> possibleOpponentMove = FEN.GetAvailableMoves(boardAfterMove, white ? ChessColor.White : ChessColor.Black);
                if (possibleOpponentMove.Count == 0) move.Flag = ChessFlag.Checkmate;
            }
            else
                move.Flag = ChessFlag.NoFlag;
            }
            else
            {
                this.Log("They're not in check");
                move.Flag = ChessFlag.NoFlag;
            }
            return move;

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
            List<char[]> legalBoards = FEN.GetAvailableMoves(SRfen, colorOfPlayerMoving);
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
            if (InCheck(boardToCheck, white))
                testFlag = ChessFlag.Check;
            if (testFlag != moveToCheck.Flag)
                return false;
            //check if checkmate
            //getAvailable moves, if none then checkMate
            ChessColor colorOfOpponent;
            if (colorOfPlayerMoving == ChessColor.White)
                colorOfOpponent = ChessColor.Black;
            else
                colorOfOpponent = ChessColor.White;
            List<char[]> opponentFutureMoves = FEN.GetAvailableMoves(boardToCheck, colorOfOpponent);
            //        if (opponentFutureMoves)
            //           testFlag = ChessFlag.Checkmate;
            //       if (testFlag != moveToCheck.Flag)
            //           return false;
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

        public char[] greedy(char[] SRFen, ChessColor color)
        {
            Char[] chosenBoard;
            bool white;
            //if (color == ChessColor.White) white = true;
            //else white = false;
            // List<Char[]> legalBoards= FEN.GetAvailableMoves (SRFen, white); //AvailableMoves defined by Greg
            //need to build tree
            NodeMove init = new NodeMove(SRFen);
            GameTree Tree = new GameTree(init);

            //const int MAXDEPTH = 4;
            List<Char[]> LegalBoard = FEN.GetAvailableMoves(SRFen, color);
            List<Char[]> EqualBoard = new List<Char[]>();

            chosenBoard = LegalBoard[0];
            if (color == ChessColor.White)
            {
                white = true;
            }
            else
            {
                white = false;
            }
            InCheck(SRFen, white);
            foreach (char[] ChildrenBoard in LegalBoard)
            {
                int h;
                NodeMove childNode;
                h = GetHeuristicValue(ChildrenBoard, color);
                childNode = new NodeMove(ChildrenBoard, h, Tree.head);

                Tree.head.children.Add(childNode);
            }

            NodeMove choice;
            List<NodeMove> choices = maxValues(Tree.head.children);

            if (choices.Count() > 1)
            {
                Random rnd = new Random();
                int value = rnd.Next(0, choices.Count());
                choice = choices[value];
            }
            else
            {
                choice = choices[0];
            }

            return choice.board;
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
            List<Char[]> LegalBoard = FEN.GetAvailableMoves(SRFen, color);

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
                    childNode = new NodeMove(ChildrenBoard, ParentNode);
                ParentNode.children.Add(childNode);
                if (depth < MAXDEPTH)
                {
                    miniMaxHelper(ref childNode, depth + 1, !white, ChildrenBoard);
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
                else choice = minValue(ParentNode.children, false);
                ParentNode.h = choice.h;
                if (depth == 1) { ParentNode.board = choice.board; }
            }
        }

        public NodeMove minValue(List<NodeMove> children, bool isAtTop)
        {
            NodeMove choice = children[0];
            int minH = choice.h;
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

        public List<NodeMove> maxValues(List<NodeMove> children)
        {
            List<NodeMove> choices = new List<NodeMove>();
            int maxH = children[0].h;
            //find min heuristics for moves in the children list
            foreach (NodeMove child in children)
            {
                if (child.h > maxH)
                {

                    maxH = child.h;
                    choices.Clear();
                    choices.Add(child);
                }
                else if (child.h == maxH)
                {
                    choices.Add(child);
                }
            }
            return choices;
        }

        public bool InCheck(char[] board, bool white)
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

        private bool KnightCheck(char[] board, bool white, int pos)
        {
            this.Log("Checking knights");
            bool check = false;
            char enemyKnight;

            enemyKnight = white ? 'n' : 'N';

            // Array of all possible attacking knight positions
            int[] knights = new int[8] { pos - 11, pos - 19, pos - 17, pos - 7, pos + 11, pos + 19, pos + 17, pos + 7 };

            for (int i = 0; i < knights.Length; i++)
            {
                int idx = knights[i];

                // If the position is within the board range
                if (idx > 0 && idx < 71 && idx % 9 != 8)
                {
                    if (board[idx] == enemyKnight)
                    {
                        this.Log("In Check by Knight at pos: " + idx);
                        check = true;
                        break;
                    }
                }
            }

            /*
            if (check)
            {
                this.Log("- We are in check by KNIGHTS!");
            }
            */
            return check;
        }

        private bool ColRowCheck(char[] board, bool white, int kingPos)
        {
            bool check = false;
            char enemyRook;
            char enemyQueen;
            string color;
            if (white)
            {
                color = "White";
                enemyRook = 'r';
                enemyQueen = 'q';
            }
            else
            {
                color = "Black";
                enemyRook = 'R';
                enemyQueen = 'Q';
            }

            int[] directions = new int[4] {
                -9, // up
                9,  // down
                -1, // left
                1   // right
            };

            // For each direction
            for (int i = 0; i < directions.Length; i++)
            {
                int shift = directions[i];
                int pos = kingPos + shift;

                // Move in that direction until you hit an edge or another piece
                while (pos > 0 && pos < 71 && pos % 9 != 8)
                {
                    // If you hit a rook or a queen we're in check
                    if (board[pos] == enemyRook || board[pos] == enemyQueen)
                    {
                        this.Log(" - " + color + " is in check on ColRow from " + pos.ToString());
                        check = true;
                        break;
                    }
                    // If you hit something that's not enemy q or r, but isn't blank, then we're safe.
                    else if (board[pos] != '_')
                    {
                        // check is already false
                        break;
                    }
                    pos += shift;
                }
            }

            
            if (check)
            {
                
            }
            
            return check;
        }

        private bool DiagonalCheck(char[] board, bool white, int kingPos)
        {
            bool check = false;
            char enemyBishop;
            char enemyPawn;
            char enemyQueen;
            int[] advanceDiagonals;
            int[] retreatDiagonals;
            string color;
            if (white)
            {
                color = "White";
                this.Log("Checking Diagonals for White");
                enemyBishop = 'b';
                enemyQueen = 'q';
                enemyPawn = 'p';
                enemyKing = 'k';
                advanceDiagonals = new int[2] {
                    -10, // up left
                    -8  // up right
                };

                retreatDiagonals = new int[2] {
                    10, // down left
                    8  // down right
                };
            }
            else
            {
                color = "Black";
                this.Log("Checking Diagonals for Black");
                enemyBishop = 'B';
                enemyQueen = 'Q';
                enemyPawn = 'P';
                enemyKing = 'K';

                advanceDiagonals = new int[2] {
                    10, // down left
                    8  // down right
                };

                retreatDiagonals = new int[2] {
                    -10, // up left
                    -8  // up right
                };
            }

            

            // For each up direction
            for (int i = 0; i < advanceDiagonals.Length; i++)
            {
                int shift = advanceDiagonals[i];
                int pos = kingPos + shift;

                // Check Pawns:
                if (board[pos] == enemyPawn || board[pos] == enemyKing)
                {
                    this.Log(" - " + color + " king is in check by pawn or king from pos " + pos);
                    check = true;
                    break;
                }

                // Move in that direction until you hit an edge or another piece
                while (pos > 0 && pos < 71 && pos % 9 != 8)
                {
                    // If you hit a rook or a queen we're in check
                    if (board[pos] == enemyBishop || board[pos] == enemyQueen)
                    {
                        this.Log(" - " + color + " king (at pos " + kingPos + ") is in check on Diagonals from pos " + pos);
                        check = true;
                        break;
                    }
                    // If you hit something that's not enemy q or r, but isn't blank, then we're safe.
                    else if (board[pos] != '_')
                    {
                        this.Log("We hit a piece @ " + pos.ToString());
                        // check is already false
                        break;
                    }
                    pos += shift;
                }
            }

            // For each up direction
            for (int i = 0; i < retreatDiagonals.Length; i++)
            {
                int shift = retreatDiagonals[i];
                int pos = kingPos + shift;

                // Move in that direction until you hit an edge or another piece
                while (pos > 0 && pos < 71 && pos % 9 != 8)
                {
                    // If you hit a rook or a queen we're in check
                    if (board[pos] == enemyBishop || board[pos] == enemyQueen)
                    {
                        this.Log(" - " + color + " king (at pos " + kingPos + ")  is in check on Diagonals from pos " + pos);
                        check = true;
                        break;
                    }
                    // If you hit something that's not enemy q or r, but isn't blank, then we're safe.
                    else if (board[pos] != '_')
                    {
                        // check is already false
                        break;
                    }
                    pos += shift;
                }
            }

            return check;
        }

        private int GetKingPos(char[] board, bool white)
        {
            char kingChar = white ? 'K' : 'k';
            for (int i = 0; i < 71; ++i)
            {
                if (board[i] == kingChar)
                {
                    return i;
                }
            }
            return 0;
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
            //this.Log("HEURISTIC");
            //this.Log("Color: " + color);
            //bool self.InCheck(boardState, )
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

        private Dictionary<char, int> pieceValues = new Dictionary<char, int>
        {
               {'p', 1},
               {'n', 3},
               {'b', 3},
               {'r', 5},
               {'q', 10}
        };
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
        public AILoggerCallback Log { get; set; }

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
