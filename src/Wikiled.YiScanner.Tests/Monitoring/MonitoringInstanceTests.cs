using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Wikiled.YiScanner.Client;
using Wikiled.YiScanner.Client.Archive;
using Wikiled.YiScanner.Monitoring;

namespace Wikiled.YiScanner.Tests.Monitoring
{
    [TestFixture]
    public class MonitoringInstanceTests : ReactiveTest
    {
        private MonitoringConfig monitoringConfig;

        private Mock<IDestinationFactory> mockDestinationFactory;

        private Mock<IFtpDownloader> ftpDownloader;

        private Mock<IDeleteArchiving> archiving;

        private MonitoringInstance instance;

        private TestScheduler scheduler;

        [SetUp]
        public void SetUp()
        {
            scheduler = new TestScheduler();
            monitoringConfig = new MonitoringConfig();
            monitoringConfig.Scan = 1;
            archiving = new Mock<IDeleteArchiving>();
            mockDestinationFactory = new Mock<IDestinationFactory>();
            ftpDownloader = new Mock<IFtpDownloader>();
            mockDestinationFactory.Setup(item => item.GetDestinations())
                                  .Returns(new[] { ftpDownloader.Object });
            instance = CreateMonitoringInstance();
        }

        [Test]
        public void Download()
        {
            instance.Start();
            scheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks);
            ftpDownloader.Verify(item => item.Download(), Times.Exactly(10));
        }

        [Test]
        public void DownloadSlow()
        {
            ftpDownloader.Setup(item => item.Download()).Returns(() => Observable.Return(true).Delay(TimeSpan.FromSeconds(5), scheduler).ToTask());
            instance.Start();
            scheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks);
            ftpDownloader.Verify(item => item.Download(), Times.Exactly(2));
        }

        [Test]
        public void Archive()
        {
            monitoringConfig.Archive = 2;
            archiving.Setup(item => item.Archive(It.IsAny<string>(), It.IsAny<TimeSpan>())).Returns(() => Observable.Return(true).Delay(TimeSpan.FromSeconds(5), scheduler).ToTask());
            instance.Start();
            scheduler.AdvanceBy(TimeSpan.FromDays(2).Ticks);
            archiving.Verify(item => item.Archive(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Exactly(2));
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new MonitoringInstance(
                null,
                monitoringConfig,
                mockDestinationFactory.Object,
                archiving.Object));

            Assert.Throws<ArgumentNullException>(() => new MonitoringInstance(
                scheduler,
                null,
                mockDestinationFactory.Object,
                archiving.Object));

            Assert.Throws<ArgumentNullException>(() => new MonitoringInstance(
                scheduler,
                monitoringConfig,
                null,
                archiving.Object));

            Assert.Throws<ArgumentNullException>(() => new MonitoringInstance(
                scheduler,
                monitoringConfig,
                mockDestinationFactory.Object,
                null));
        }

        private MonitoringInstance CreateMonitoringInstance()
        {
            return new MonitoringInstance(
                scheduler,
                monitoringConfig,
                mockDestinationFactory.Object,
                archiving.Object);
        }
    }
}