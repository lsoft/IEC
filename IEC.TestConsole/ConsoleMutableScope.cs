using System;
using IEC.Common;
using IEC.Common.Scope;
using IEC.TestConsole.H;

namespace IEC.TestConsole
{
    internal class ConsoleMutableScope : MutableScope
    {
        public ListItem? Nullable
        {
            get;
            private set;
        } = default;

        public int A
        {
            get;
            private set;
        } = default;

        public string B
        {
            get;
            private set;
        } = string.Empty;

        public DateTime C
        {
            get;
            private set;
        } = default;

        /// <inheritdoc />
        public ConsoleMutableScope(
            Action<IThreadFrame> disposeAction
            )
            : base(disposeAction)
        {
        }

        public void SetData(
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
