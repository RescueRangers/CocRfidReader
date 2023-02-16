using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CocRfidReader.Services;
using CocRfidReader.WPF.Services;
using Impinj.OctaneSdk;
using Microsoft.Extensions.Logging;
using Moq;

namespace CocRfidReader.Tests
{
    public class TagsServiceTests
    {
        private TagsService service;

        [SetUp]
        public void Setup()
        {
            var mockLogger = new Mock<ILogger<TagsService>>();
            var mockCocReader = new Mock<ICocReader>();
            mockCocReader.Setup
            (
                c => c.GetAsync(It.IsAny<string>(),
                It.IsAny<CancellationToken>())).ReturnsAsync
                (
                    new Model.Coc 
                    { 
                        AccountNumber = "7777", 
                        EPC = "12", 
                        ItemNumber = "8888", 
                        ItemText = "Test Item", 
                        Name = "Test", 
                        PRODUKTIONSNR = 54125 
                    }
                );

            service = new TagsService(mockLogger.Object, mockCocReader.Object);
        }

        [Test]
        public async Task TestCorrectAccount()
        {
            var tag = CreateInstance<Tag>();
            var coc = await service.GetCocFromTag(tag, "7777");

            Assert.That(coc.IsAccountCorrect, Is.True);
        }

        [Test]
        public async Task TestOneOffAccount()
        {
            var tag = CreateInstance<Tag>();
            var coc = await service.GetCocFromTag(tag, "7778");

            Assert.That(coc.IsAccountCorrect, Is.True);
        }

        [Test]
        public async Task TestIncorrectAccount()
        {
            var tag = CreateInstance<Tag>();
            var coc = await service.GetCocFromTag(tag, "7879");

            Assert.That(coc.IsAccountCorrect, Is.False);
        }

        public T CreateInstance<T>(params object[] args)
        {
            var type = typeof(T);
            var instance = type.Assembly.CreateInstance(
                type.FullName, false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, args, null, null);
            return (T)instance;
        }
    }
}
