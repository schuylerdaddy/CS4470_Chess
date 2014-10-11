using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StudentAI
{
    public class NodeMove
    {
        public byte[] board;
        public int h;
        // public NodeMove parent;
        public NodeMove[] children;
        public NodeMove()
        {
        }
        public NodeMove(int _h)
        {
            h = _h;
        }
        // public NodeMove(char[] _board, NodeMove _parent)
        // {
        // board = _board;
        // parent = _parent;
        // children = new NodeMove[100];
        // }
        /* public NodeMove(char[] _board, int _h, NodeMove _parent)
        {
        board = _board;
        h = _h;
        parent = _parent;
        children = new NodeMove[100];
        }*/
        public NodeMove(byte[] _board)
        {
            board = _board;
            // parent = null;
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
}

namespace ShallowRed
{
    public class LightList
    {
        public int Count = 0;
        private byte[][] list = new byte[150][];
        public void Add(byte[] board)
        {
            list[Count++] = board;
        }
        public byte[] this[int i]
        {
            get { return list[i]; }
            private set { }
        }

        public void Empty()
        {
            Count = 0;
        }
        public void Replace(byte[] board, int pos)
        {
            list[pos] = board;
        }
    }

}
