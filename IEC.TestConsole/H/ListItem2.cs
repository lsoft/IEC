namespace IEC.TestConsole.H
{
    public class ListItem2<T> : ListItem
    {
        /// <inheritdoc />
        public ListItem2(
            bool b,
            int index,
            object? nullableValue = null
            )
            : base(b, index, nullableValue)
        {
        }
    }
}