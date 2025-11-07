namespace O2DESNet;

internal class EventIndexer
{
    private static int _eventIndex = 1;

    public static int GenerateIndex()
    {
        _eventIndex++;
        return _eventIndex;
    }

    public static void Reset()
    {
        _eventIndex = 0;
    }
}
