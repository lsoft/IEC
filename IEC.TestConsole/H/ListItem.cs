namespace IEC.TestConsole.H
{
    public class ListItem
    {
        public bool B
        {
            get;
        }

        public int Index
        {
            get;
        }

        public object? NullableValue
        {
            get;
        }


        public ListItem(
            bool b,
            int index,
            object? nullableValue = null
            )
        {
            B = b;
            Index = index;
            NullableValue = nullableValue;
        }
    }
}