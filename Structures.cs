using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShallowRed
{
    public class LightList
    {
        public int Count = 0;

        private char[][] list = new char[150][];
        public void Add(char[] board)
        {
            list[Count++] = board;
        }

        public char[] this[int i]
        {
            get { return list[i]; }
            private set { }
        }

        public void Empty()
        {

            Count = 0;

        }

        public void Replace(char[] board, int pos)
        {
            list[pos] = board;
        }
}
