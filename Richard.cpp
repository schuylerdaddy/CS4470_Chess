// This file has two parts:
//  1 - The "InCheck" function that takes a bard and a bool
//      indicating white (true) or black, and returns whether they are in check
//  2 - Below is my entire StudentAI.cs, where I have a simplified (greedy) algorith
//      being used in place of MiniMax to work through bugs on things like move validation


 /////////////////////////////////////////
 // Here's all the code needed for "is this color in check"
 /////////////////////////////////////////
 public bool InCheck(char[] board, bool white)
        {
            //this.Log("Is Anyone In Check?");
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
            bool check = false;
            char enemyKnight;

            if (white) {
                enemyKnight = 'n';
            }
            else
            {
                enemyKnight = 'N';
            }

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
                        //this.Log("In Check by Knight at pos: " + idx);
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
            if (white)
            {
                enemyRook = 'r';
                enemyQueen = 'q';
            }
            else
            {
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

            /*
            if (check)
            {
                this.Log("- We are in check on Col/Row!!");
            }
            */
            return check;
        }

        private bool DiagonalCheck(char[] board, bool white, int kingPos)
        {
            int idx = kingPos + 8;
            int idx2 = idx + 2;

            if (idx < 71)
            {
                if (idx % 9 != 8)
                {
                    if (white)
                    {
                        if (board[idx] == 'q' || board[idx] == 'b' || board[idx] == 'k')
                            return true;
                    }
                    else if (board[idx] == 'Q' || board[idx] == 'B' || board[idx] == 'K' || board[idx] == 'P')
                        return true;
                }

                if (idx2 % 9 != 8)
                {
                    if (white)
                    {
                        if (board[idx2] == 'q' || board[idx2] == 'b' || board[idx2] == 'k')
                            return true;
                    }
                    else if (board[idx2] == 'Q' || board[idx2] == 'B' || board[idx2] == 'K' || board[idx2] == 'P')
                        return true;

                }

                idx += 8;
                idx2 += 10;

                while (idx < 71)
                {
                    if (idx % 9 != 8)
                    {
                        if (white)
                        {
                            if (board[idx] == 'q' || board[idx] == 'b' || board[idx] == 'k')
                                return true;
                        }
                        else if (board[idx] == 'Q' || board[idx] == 'B' || board[idx] == 'K')
                            return true;
                    }

                    if (idx2 < 71 && idx2 % 9 != 8)
                    {
                        if (white)
                        {
                            if (board[idx2] == 'q' || board[idx2] == 'b' || board[idx2] == 'k')
                                return true;
                        }
                        else if (board[idx2] == 'Q' || board[idx2] == 'B' || board[idx2] == 'K')
                            return true;
                    }

                    idx += 8;
                    idx2 += 10;
                }
            }

            idx = kingPos - 10;
            idx2 = idx + 2;

            if (idx > -1)
            {
                if (idx % 9 != 8)
                {
                    if (white)
                    {
                        if (board[idx] == 'q' || board[idx] == 'b' || board[idx] == 'k' || board[idx] == 'p')
                            return true;
                    }
                    else if (board[idx] == 'Q' || board[idx] == 'B' || board[idx] == 'K')
                        return true;
                }

                if (idx2 % 9 != 8)
                {
                    if (white)
                    {
                        if (board[idx2] == 'q' || board[idx2] == 'b' || board[idx2] == 'k' || board[idx2] == 'p')
                            return true;
                    }
                    else if (board[idx2] == 'Q' || board[idx2] == 'B' || board[idx2] == 'K')
                        return true;
                }

                idx -= 10;
                idx2 -= 8;

                while (idx > -1)
                {
                    if (idx % 9 != 8)
                    {
                        if (white)
                        {
                            if (board[idx] == 'q' || board[idx] == 'b' || board[idx] == 'k')
                                return true;
                        }
                        else if (board[idx] == 'Q' || board[idx] == 'B' || board[idx] == 'K')
                            return true;
                    }

                    if (idx > -1 && idx2 % 9 != 8)
                    {
                        if (white)
                        {
                            if (board[idx2] == 'q' || board[idx2] == 'b' || board[idx2] == 'k')
                                return true;
                        }
                        else if (board[idx2] == 'Q' || board[idx2] == 'B' || board[idx2] == 'K')
                            return true;
                    }

                    idx -= 10;
                    idx2 -= 8;
                }
            }
            return false;
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
        
  /////////////////////////////////////////
  // Below is my entire StudentAI.cs
  /////////////////////////////////////////
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
            this.Log("Let the Game Begin!");
            //convert Fen for to shallow red fen form
            String originalFenBoard=board.ToPartialFenBoard();
            char[] SRfen= FENExtensions.ToShallowRedFEN(originalFenBoard);

            char[] boardAfterMove= greedy(SRfen, myColor);
            //char[] boardAfterMove = miniMax(SRfen, myColor);

            ChessMove move = FENExtensions.GenerateMove(SRfen, boardAfterMove);
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

            bool white=false;
            if (colorOfPlayerMoving==ChessColor.White) white=true;
            string stdFen = boardBeforeMove.ToPartialFenBoard();
            char[] SRfen = FENExtensions.ToShallowRedFEN(stdFen);
            int to= (moveToCheck.To.Y * 9) + moveToCheck.To.X;
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
            this.Log("### Greedy v2 ###");
            Char[] chosenBoard;
            bool white;
            //if (color == ChessColor.White) white = true;
            //else white = false;
           // List<Char[]> legalBoards= FEN.GetAvailableMoves (SRFen, white); //AvailableMoves defined by Greg
            //need to build tree
            NodeMove init = new NodeMove(SRFen);
            GameTree Tree=new GameTree(init);

            //const int MAXDEPTH = 4;
            List<Char[]> LegalBoard = FEN.GetAvailableMoves(SRFen, color);
            List<Char[]> EqualBoard = new List<Char[]>();
            int numMax = 0;
            int max = 0;
            int finalHeuristic = 0;
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
            //this.Log("Is Anyone In Check?");
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
            bool check = false;
            char enemyKnight;

            if (white) {
                enemyKnight = 'n';
            }
            else
            {
                enemyKnight = 'N';
            }

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
                        //this.Log("In Check by Knight at pos: " + idx);
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
            if (white)
            {
                enemyRook = 'r';
                enemyQueen = 'q';
            }
            else
            {
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

            /*
            if (check)
            {
                this.Log("- We are in check on Col/Row!!");
            }
            */
            return check;
        }

        private bool DiagonalCheck(char[] board, bool white, int kingPos)
        {
            int idx = kingPos + 8;
            int idx2 = idx + 2;

            if (idx < 71)
            {
                if (idx % 9 != 8)
                {
                    if (white)
                    {
                        if (board[idx] == 'q' || board[idx] == 'b' || board[idx] == 'k')
                            return true;
                    }
                    else if (board[idx] == 'Q' || board[idx] == 'B' || board[idx] == 'K' || board[idx] == 'P')
                        return true;
                }

                if (idx2 % 9 != 8)
                {
                    if (white)
                    {
                        if (board[idx2] == 'q' || board[idx2] == 'b' || board[idx2] == 'k')
                            return true;
                    }
                    else if (board[idx2] == 'Q' || board[idx2] == 'B' || board[idx2] == 'K' || board[idx2] == 'P')
                        return true;

                }

                idx += 8;
                idx2 += 10;

                while (idx < 71)
                {
                    if (idx % 9 != 8)
                    {
                        if (white)
                        {
                            if (board[idx] == 'q' || board[idx] == 'b' || board[idx] == 'k')
                                return true;
                        }
                        else if (board[idx] == 'Q' || board[idx] == 'B' || board[idx] == 'K')
                            return true;
                    }

                    if (idx2 < 71 && idx2 % 9 != 8)
                    {
                        if (white)
                        {
                            if (board[idx2] == 'q' || board[idx2] == 'b' || board[idx2] == 'k')
                                return true;
                        }
                        else if (board[idx2] == 'Q' || board[idx2] == 'B' || board[idx2] == 'K')
                            return true;
                    }

                    idx += 8;
                    idx2 += 10;
                }
            }

            idx = kingPos - 10;
            idx2 = idx + 2;

            if (idx > -1)
            {
                if (idx % 9 != 8)
                {
                    if (white)
                    {
                        if (board[idx] == 'q' || board[idx] == 'b' || board[idx] == 'k' || board[idx] == 'p')
                            return true;
                    }
                    else if (board[idx] == 'Q' || board[idx] == 'B' || board[idx] == 'K')
                        return true;
                }

                if (idx2 % 9 != 8)
                {
                    if (white)
                    {
                        if (board[idx2] == 'q' || board[idx2] == 'b' || board[idx2] == 'k' || board[idx2] == 'p')
                            return true;
                    }
                    else if (board[idx2] == 'Q' || board[idx2] == 'B' || board[idx2] == 'K')
                        return true;
                }

                idx -= 10;
                idx2 -= 8;

                while (idx > -1)
                {
                    if (idx % 9 != 8)
                    {
                        if (white)
                        {
                            if (board[idx] == 'q' || board[idx] == 'b' || board[idx] == 'k')
                                return true;
                        }
                        else if (board[idx] == 'Q' || board[idx] == 'B' || board[idx] == 'K')
                            return true;
                    }

                    if (idx > -1 && idx2 % 9 != 8)
                    {
                        if (white)
                        {
                            if (board[idx2] == 'q' || board[idx2] == 'b' || board[idx2] == 'k')
                                return true;
                        }
                        else if (board[idx2] == 'Q' || board[idx2] == 'B' || board[idx2] == 'K')
                            return true;
                    }

                    idx -= 10;
                    idx2 -= 8;
                }
            }
            return false;
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

        private  Dictionary<char, int> pieceValues = new Dictionary<char, int>
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
