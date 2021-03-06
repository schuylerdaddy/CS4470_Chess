using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ShallowRed
{
    public class LightList
    {
        public int Count = 0;
        private byte[][] list = new byte[75][];
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

        public void Remove(byte[] board)
        {
            int position = -99;
            for (int idx = 0; idx < Count; ++idx)
            {
                bool equal = true;
                for (int i = 0; i < FEN.OUTOFBOUNDSHIGH; i++)
                {
                    if (list[idx][i] != board[i])
                    {
                        equal = false;
                        break;
                    }
                }
                if (equal)
                {
                    position = idx;
                    break;
                }
            }
            if (position == -99) return;
            for (int i = position; i < Count - 1; ++i)
            {
                list[i]=list[i+1];
            }
            --Count;
        }

        public static LightList ConvertBuffer(BoardBuffer bf)
        {
            LightList ll = new LightList();

            for (int i = 0; i < bf.capCount; ++i)
            {
                ll.Add(bf.captures[i]);
            }

            for (int i = 0; i < bf.thrCount; ++i)
            {
                ll.Add(bf.threats[i]);
            }

            for (int i = 0; i < bf.forCount; ++i)
            {
                ll.Add(bf.forward[i]);
            }

            for (int i = 0; i < bf.oCount; ++i)
            {
                ll.Add(bf.other[i]);
            }

            for (int i = 0; i < bf.nsCount; ++i)
            {
                ll.Add(bf.notSafe[i]);
            }

            return ll;
        }
    }

    public class BoardBuffer
    {
        public byte[][] captures = new byte[30][];
        public byte[][] threats = new byte[40][];
        public byte[][] forward = new byte[50][];
        public byte[][] other = new byte[50][];
        public byte[][] notSafe = new byte[50][];

        public int capCount = 0;
        public int thrCount = 0;
        public int forCount = 0;
        public int oCount = 0;
        public int nsCount = 0;

        public void AddCapture(byte[] board)
        {
            captures[capCount++] = board;
        }

        public void AddThreat(byte[] board)
        {
            threats[thrCount++] = board;
        }

        public void AddForward(byte[] board)
        {
            forward[forCount++] = board;
        }

        public void Add(byte[] board)
        {
            other[oCount++] = board;
        }

        public void AddUnSafe(byte[] board)
        {
            notSafe[nsCount++] = board;
        }
    }

    public class OLightList
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
    }
