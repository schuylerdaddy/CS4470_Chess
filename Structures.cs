using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShallowRed
{
    public class NodeMove
    {
        public char[] board;
        public int h;
 //       public NodeMove parent;
        public NodeMove[] children;
        public NodeMove()
        {

        }
        public NodeMove(int _h)
        {
            h = _h;
        }
   //     public NodeMove(char[] _board, NodeMove _parent)
   //     {
   //         board = _board;
   //         parent = _parent;
     //       children = new NodeMove[100];
   //     }

   /*     public NodeMove(char[] _board, int _h, NodeMove _parent)
        {
            board = _board;
            h = _h;
            parent = _parent;
            children = new NodeMove[100];
        }*/

        public NodeMove(char[] _board)
        {
            board = _board;
          //  parent = null;
            children = new NodeMove[100];
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

    public class LightList
    {
        public int Count = 0;
        private char[][] list = new char[10000][];

        public void Add(char[] board)
        {
            list[Count++] = board;
        }

        public char[] this[int i]
        {
            get { return list[i]; }
            private set { }
        }
    }
}
