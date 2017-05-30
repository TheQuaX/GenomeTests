using System;
using System.IO;

namespace Interpreter
{
    class BFInterpreter
    {
        private static readonly int BUFFER = 65535;
        private int pointer { get; set; }
        private bool print { get; set; }
        private int[] buffer = new int[BUFFER];

        public BFInterpreter()
        {
            this.pointer = 0;
            this.Reset();

        }

        public void Reset()
        {
            Array.Clear(this.buffer, 0, this.buffer.Length);
        }

        public void Interpret(string src)
        {
            int i = 0;
            int max = src.Length;
            while (i < max)
            {
                //Simple Brainfuck logic
                switch (src[i])
                {
                    case '>':
                        this.pointer++;
                        if (this.pointer >= BUFFER)
                        {
                            this.pointer = 0;
                        }
                        break;

                    case '<':
                        this.pointer--;
                        if (this.pointer < 0)
                        {
                            this.pointer = 0;
                        }
                        break;

                    case '.': //Output
                        Console.Write((char)this.buffer[this.pointer]);
                        break;

                    case '+':
                        this.buffer[this.pointer]++;
                        break;

                    case '-':
                        this.buffer[this.pointer]--;
                        break;

                    case '[':
                        if (this.buffer[this.pointer] == 0)
                        {
                            int loopa = 1;
                            while (loopa > 0)
                            {
                                i++;
                                char c = src[i];
                                if (c == '[')
                                {
                                    loopa++;
                                }
                                else if (c == ']')
                                {
                                    loopa--;
                                }
                            }
                        }
                        break;

                    case ']':
                        int loopb = 1;
                        while (loopb > 0)
                        {
                            i--;
                            char c = src[i];
                            if (c == '[')
                            {
                                loopb--;
                            }
                            else if (c == ']')
                            {
                                loopb++;
                            }
                        }
                        i--;
                        break;

                    case ',':
                        ConsoleKeyInfo key = Console.ReadKey(this.print);
                        this.buffer[this.pointer] = (int)key.KeyChar;
                        break;

                    default:
                        break;
                }
                i++;
            }
        }
    }
}