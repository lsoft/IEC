using IEC.Common.Publish.Saver;

namespace IEC.Target.Console
{
    public class ConsoleCollectionSaver : CollectionSaver
    {
        public ConsoleCollectionSaver()
        {
        }

        protected override void LogCompleteMessage(
            string message
            )
        {
            System.Console.WriteLine(message);
        }

        /// <inheritdoc />
        public override void Commit()
        {
            //nothing to do
        }

        /// <inheritdoc />
        protected override void DoDispose()
        {
            //nothing to do
        }
    }
}