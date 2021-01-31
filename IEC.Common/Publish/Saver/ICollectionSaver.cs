using System;

namespace IEC.Common.Publish.Saver
{
    /// <summary>
    /// Сейвер объектов в постоянное хранилище
    /// </summary>
    public interface ICollectionSaver : IDisposable
    {
        void SaveCollections(
            PublishedFrameCollection[] collection,
            int actualCollectionCount
            );

        void Commit();
    }
}