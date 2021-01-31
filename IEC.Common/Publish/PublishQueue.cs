using System;
using System.Collections.Concurrent;
using System.Threading;
using IEC.Common.Other;
using IEC.Common.Publish.Saver;

namespace IEC.Common.Publish
{
    public sealed class PublishQueue : IPublishQueue
    {
        //максимальный размер очереди, после которого эвенты не добавляеются в нее, а игнорируются
        private const int MaximumQueueLength = 5000;

        //интервал через который записывать диагностику размера очереди
        private const int DiagnosticWriteTimeIntervalInSeconds = 30;

        //максимальное количество итемов, которое может быть записано одним item writer'ом
        //(иногда это означает - в рамках одной транзакции, которые желательно время от времени закрывать)
        private const int BatchSize = 100;

        //время ожидания, пока наберется количество итемов в размере как минимум 1 батч
        private const int BatchWaitTimeoutMsec = 1000;



        //фабрика итем сейверов
        private readonly ICollectionSaverFactory _collectionSaverFactory;

        //логгер
        private readonly IIECLogger _logger;

        //признак подавления ошибок телеметрии
        private readonly bool _suppressExceptions;

        //очередь для записи должна быть потокозащищенной, так как она наполняется из одного трида, а опустошается - из другого
        private readonly ConcurrentQueue<PublishedFrameCollection> _recordQueue = new();

        //событие, сигнализирующее о том, что сейверу пора завершаться
        private readonly ManualResetEvent _shouldStop = new(false);

        //событие, сигнализирующее о том, что появилась новая запись на сохранение
        private readonly AutoResetEvent _doProcess = new(false);

        //массив объектов, подготовленных для сохранения, чтобы не пересоздавать массив постоянно
        //(просто небольшая оптимизация)
        private readonly PublishedFrameCollection[] _batch = new PublishedFrameCollection[BatchSize];



        //время последней записи в лог размера очереди
        private DateTime _lastDiagnosticMessageTime = DateTime.Now;

        //количество ошибок при сохранении
        private int _errorCounter = 0;

        //признак, что сейвер еще не стартовал
        private int _started = 0;

        //признак, что сейвер завершил работу
        private long _disposed = 0L;

        //рабочий поток сохранения
        private Thread? _workerThread;



        public event PublishedDelegate? PublishedEvent;


        public PublishQueue(
            ICollectionSaverFactory collectionSaverFactory,
            IIECLogger logger,
            bool suppressExceptions = true
            )
        {
            if (collectionSaverFactory == null)
            {
                throw new ArgumentNullException(nameof(collectionSaverFactory));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }


            _collectionSaverFactory = collectionSaverFactory;
            _logger = logger;
            _suppressExceptions = suppressExceptions;
        }

        public void AppendToQueue(
            PublishedFrameCollection collection
            )
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            //проверяем, не переполнилась ли очередь
            //при ливне событий, мы можем не успевать записывать
            var rc = _recordQueue.Count;
            if (rc >= MaximumQueueLength)
            {
                return;
            }

            //очередь не переполнилась

            //если еще не стартовали - стартуем
            if (Interlocked.CompareExchange(ref _started, 1, 0) == 0)
            {
                this.WorkStart();
            }

            _recordQueue.Enqueue(collection);

            if (rc + 1 >= BatchSize)
            {
                //итемов набралось на батч, пробуждаем рабочий поток

                _doProcess.Set();
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1L) != 0L)
            {
                return;
            }

            if (_workerThread is not null)
            {
                _shouldStop.Set();
                _workerThread.Join();
            }

            _shouldStop.Dispose();
            _doProcess.Dispose();
        }

        #region private code

        private void WorkStart()
        {
            var t = new Thread(WorkThread);
            _workerThread = t;

            _workerThread.Start();
        }

        private void WorkThread()
        {
            while (true)
            {
                var waitIndex = WaitHandle.WaitAny(
                    new WaitHandle[]
                    {
                        _shouldStop,
                        _doProcess
                    },
                    BatchWaitTimeoutMsec //иногда надо сбрасывать очередь в хранилище, даже если не набралось на 1 полный батч
                    );                   //при смене порядка эвентов надо менять код, которые юзает waitIndex

                //после срабатывания события, сначала пытаемся сохранить всё, а уже потом проверяем условие выхода
                //это нужно, так как если срабатывают оба события очень быстро:
                //using (var saver = new SomeSaver(...))
                //{
                //  saver.Save(...); <-- рейзим _doProcess евент
                //} <-- почти мгновенно рейзим _shouldStop евент
                //то трид не успевает проснуться между активацией _shouldStop и _doProcess
                //а просыпается уже после сработки обоих ивентов, и сразу выходит, не сохранив события
                //это в принципе не очень страшная ситуация в продакшене, так как потеря одного события, которое
                //произошло в последние мгновения перед закрытием телеметрии, не страшна.
                //но такое поведение проваливает тесты, а это уже хуже и раздражает

                //поэтому вне зависимости от того, какое событие пробудило нас, сначала пытаемся сохранить,
                //потом - выходим (если надо)

                try
                {
                    ProcessQueue();
                }
                catch (Exception excp)
                {
                    //если при сохранении рекорда возникла ошибка, лучше этот батч "потерять"
                    //так как иначе ошибка сохранения может быть постоянной, а это приведет
                    //к разбуханию очереди и OutOfMemory

                    if (_suppressExceptions)
                    {
                        //при ошибке инсерта телеметрии ничего не делаем кроме спама в лог
                        //не надо падать и прочее, так как это второстепенная функция

                        _errorCounter++;

                        if (_errorCounter < 100 || (_errorCounter % 100) == 0)
                        {
                            //может быть такая ситуация, что НИ ОДНО сообщение телеметрии не сможет быть закомичено в базу
                            //например что-то с базой или таблицей
                            //в этом случае телеметрия завалит лог сообщениями, в которых потеряется ВСЁ и лог станет нечитаемым
                            //поэтому первые сто сообщений записываются в лог, а потом записываемся КАЖДОЕ СОТОЕ

                            _logger.LogHandledException("Error occured during record save", excp);
                        }
                    }
                    else
                    {
                        //маловероятная ситуация, что
                        //1) сработали два евента (сохранения итема и выхода),
                        //2) при попытке сохранить итемы произошла ошибка
                        //3) не включен режим суппресса исключений
                        //то в этом случае ошибку все равно давим и выходим
                        if (waitIndex == 0)
                        {
                            return;
                        }

                        throw;
                    }

                    //в случае ошибки, необходимо сделать таймаут, чтобы не сжирать весь процессор
                    Thread.Yield();
                }

                //выходим после попытки сохранить всё, если конечно было приказано
                if (waitIndex == 0)
                {
                    return;
                }

            }
        }

        private void ProcessQueue()
        {
            do
            {
                #region запись в лог размера очереди

                var now = DateTime.Now;
                if ((now - _lastDiagnosticMessageTime).TotalSeconds >= DiagnosticWriteTimeIntervalInSeconds)
                {
                    _lastDiagnosticMessageTime = now;

                    var rc = _recordQueue.Count;

                    var message = $"Telemetry stat:{Environment.NewLine}Error count: {_errorCounter}{Environment.NewLine}Queue size: {rc}";

                    _logger.LogMessage(message);
                }

                #endregion

                #region опустошаем очередь

                //набраем батч на сохранение
                var cnt = 0;

                while (_recordQueue.TryDequeue(out var collection) && cnt < BatchSize)
                {
                    _batch[cnt] = collection;

                    cnt++;
                }

                if (cnt > 0)
                {
                    //есть что-то для сохранения

                    using (var saver = _collectionSaverFactory.CreateCollectionSaver())
                    {
                        saver.SaveCollections(_batch, cnt);

                        saver.Commit();
                    }

                    OnPublished(_batch, cnt);
                }

                #endregion

            } while (_recordQueue.Count >= BatchSize && !_shouldStop.WaitOne(0));
        }

        private void OnPublished(
            PublishedFrameCollection[] collections,
            int actualCount
            )
        {
            PublishedEvent?.Invoke(collections, actualCount);
        }

        #endregion
    }
}