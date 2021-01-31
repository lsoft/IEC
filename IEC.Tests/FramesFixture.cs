using System.IO;
using System.Text;
using IEC.Common;
using Xunit;

namespace IEC.Tests
{
    public class FramesFixture
    {
        [Fact]
        public void Test1()
        {
            var tfs = new ThreadsFrames();

            try
            {
                int a = 0;
                using (var scope0 = tfs.CreateImmutableScope(a))
                {
                    string b = "1";
                    using (var scope1 = tfs.CreateImmutableScope(b))
                    {
                        throw new TestExpectedException();
                    }
                }
            }
            catch (TestExpectedException excp)
            {
                var frames = tfs.ExtractFrames();
                Assert.Equal(2, frames.Count);
                Assert.Equal(0, frames[0].GetElementaries().OnlyFirst<int>());
                Assert.Equal("1", frames[1].GetElementaries().OnlyFirst<string>());
            }
        }

        [Fact]
        public void Test2()
        {
            var tfs = new ThreadsFrames();

            try
            {
                int a = 0;
                using (var scope0 = tfs.CreateImmutableScope(a))
                {
                    string b = "1";
                    using (var scope1 = tfs.CreateImmutableScope(b))
                    {
                    }

                    ulong c = 2u;
                    using (var scope2 = tfs.CreateImmutableScope(c))
                    {
                        throw new TestExpectedException();
                    }
                }
            }
            catch (TestExpectedException excp)
            {
                var frames = tfs.ExtractFrames();
                Assert.Equal(2, frames.Count);
                Assert.Equal(0, frames[0].GetElementaries().OnlyFirst<int>());
                Assert.Equal(2u, frames[1].GetElementaries().OnlyFirst<ulong>());
            }
        }
    }
}
