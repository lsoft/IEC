using System;

namespace IEC.TestConsole.H
{
    public class Container
    {
        public ListItem? Nullable
        {
            get;
        }

        public int A
        {
            get;
        }

        public string B
        {
            get;
        }

        public DateTime C
        {
            get;
        }

        public Container(
            ListItem? nullable,
            int a,
            string b,
            DateTime c
            )
        {
            Nullable = nullable;
            A = a;
            B = b;
            C = c;
        }
    }
}