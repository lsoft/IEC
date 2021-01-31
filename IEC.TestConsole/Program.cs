using System;
using System.Linq;
using IEC.Common;
using IEC.Common.Publish;
using IEC.Target.SqlServer;
using IEC.TestConsole.H;

namespace IEC.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = args[0];


            var logger = new ConsoleLogger();

            using (var tfs = new ThreadsFrames())
            {
                //var sf = new ConsoleCollectionSaverFactory(
                //    );

                var ftn = new FullTableName("_ss");

                var tfnc = new TargetFileNameController(
                    1,
                    @"_bbin",
                    500
                    );

                var sf = new SqlServerCollectionSaverFactory(
                    connectionString,
                    ftn,
                    tfnc
                    );
                sf.PrepareDatabase();

                var r = new SqlServerReader(
                    connectionString,
                    ftn,
                    tfnc
                    );

                using (var pq = new PublishQueue(
                    sf,
                    logger
                    ))
                {

                    var p = new Publisher(
                        pq,
                        tfs
                        );

                    var iec = new Common.IEC(tfs, p, r);

                    iec.Publisher.PublishedEvent +=
                        (
                            collections,
                            count
                            ) =>
                        {
                            var read = iec.Reader.ReadBetween(
                                new DateTime(2000, 1, 1),
                                new DateTime(2030, 1, 1)
                                );

                            Console.WriteLine(read.Last().Body);
                        };

                    ProcessMutable(iec);
                    //ProcessImmutable(iec);
                }
            }

            Console.WriteLine("Finished!");
        }

        private static void ProcessMutable(
            Common.IEC iec
            )
        {
            try
            {
                int a = 0;
                string b = "1";
                var c = DateTime.Now;
                using (var scope1 = iec.TFS.CreateMutableScope<ConsoleMutableScope>())
                {
                    scope1.SetData(
                        new ListItem2<object>(false, 0, null),
                        a,
                        b,
                        c
                        );

                    scope1.SetData(
                        new ListItem2<object>(true, 1, null),
                        a + 1,
                        b + "1",
                        c.AddDays(1)
                        );

                    throw new ExpectedException();
                }
            }
            catch (ExpectedException excp)
            {
                iec.Publisher.PublishFrames(new InvalidOperationException("exception-wrapper", excp));
            }
        }

        private static void ProcessImmutable(
            Common.IEC iec
            )
        {
            try
            {
                int a = 0;
                string b = "1";
                var c = DateTime.Now;
                var d = Guid.NewGuid();
                var container = new Container(
                    new ListItem2<object>(true, -1, null),
                    a,
                    b,
                    c
                    );
                using (var scope1 = iec.TFS.CreateImmutableScope(
                    (object?) null,
                    a,
                    b,
                    c,
                    d,
                    container,
                    new CompleteList(
                        new ListItem2<object>(true, 0),
                        new ListItem2<object>(false, 1, "1 is not null"),
                        new ListItem(true, 2),
                        new ListItem(false, 3, "3 is not null")
                        )
                    ))
                {
                    throw new ExpectedException();
                }
            }
            catch (ExpectedException excp)
            {
                iec.Publisher.PublishFrames(new InvalidOperationException("exception-wrapper", excp));
            }
        }
    }
}
