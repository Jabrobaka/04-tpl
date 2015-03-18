namespace JapaneseCrossword
{
    public class LineDescription
    {
        public LineDescription(int index, LineType type)
        {
            Index = index;
            Type = type;
        }

        public LineType Type { get; private set; }
        public int Index { get; private set; }
    }

    public enum LineType
    {
        Row,
        Column
    }
}
