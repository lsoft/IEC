using IEC.Common.Publish.Saver;

namespace IEC.Target.Console
{
    public class ConsoleCollectionSaverFactory : ICollectionSaverFactory
    {
        /// <inheritdoc />
        public ICollectionSaver CreateCollectionSaver()
        {
            return new ConsoleCollectionSaver(
                );
        }
    }
}
