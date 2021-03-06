using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikiled.YiScanner.Client;
using Wikiled.YiScanner.Destinations;
using Wikiled.YiScanner.Monitoring.Source;

namespace Wikiled.YiScanner.Tests.Destinations
{
    [TestFixture]
    public class PictureFileDestinationTests
    {
        private string outPath;

        private PictureFileDestination instance;

        private VideoHeader header;

        private Stream stream;

        [SetUp]
        public void SetUp()
        {
            outPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "out");
            if(Directory.Exists(outPath))
            {
                Directory.Delete(outPath, true);
            }

            header = new VideoHeader(new Host("Camera", IPAddress.Any), "test.mov");
            instance = CreatePictureFileDestination();
            stream = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "data", "centaur_1.mpg"));
        }

        [TearDown]
        public void TestCleanup()
        {
            stream.Dispose();
        }

        [Test]
        public void ResolveName()
        {
            var result = instance.ResolveName(header);
            Assert.AreEqual(Path.Combine(outPath, "Camera", "test.png"), result);
        }

        [Test]
        public async Task Transfer()
        {
            var result = instance.IsDownloaded(header);
            Assert.IsFalse(result);
            result = File.Exists(Path.Combine(outPath, "Camera", "test.png"));
            Assert.IsFalse(result);

            await instance.Transfer(header, stream).ConfigureAwait(false);

            result = File.Exists(Path.Combine(outPath, "Camera", "test.png"));
            Assert.IsTrue(result);
            result = instance.IsDownloaded(header);
            Assert.IsTrue(result);
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new PictureFileDestination(null));
        }

        private PictureFileDestination CreatePictureFileDestination()
        {
            return new PictureFileDestination(outPath);
        }
    }
}