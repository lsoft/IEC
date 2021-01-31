namespace IEC.Common.Publish
{
    public delegate void PublishedDelegate(
        PublishedFrameCollection[] collections,
        int actualCount
        );
}
