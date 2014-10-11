using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

  

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
