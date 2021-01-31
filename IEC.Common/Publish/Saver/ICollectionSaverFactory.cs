namespace IEC.Common.Publish.Saver
{
    /// <summary>
    /// Фабрика сейверов объектов
    /// </summary>
    public interface ICollectionSaverFactory
    {
        ICollectionSaver CreateCollectionSaver();
    }
}