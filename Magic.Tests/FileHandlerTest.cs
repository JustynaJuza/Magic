using System;
using System.Collections.Generic;
using System.IO;
using Magic.Helpers;
using NSubstitute;
using Ploeh.AutoFixture;
using Xunit;

namespace Magic.Tests
{
    public class FileHandlerTest
    {
        [Fact]
        public void File_NotSavedOn_PathTooLongException_ButExceptionHandled()
        {
            var fileHandler = new FileHandler(Substitute.For<IPathProvider>());
            var fileStream = Substitute.For<Stream>();
            var fileName = new string('f', 255);

            var result = fileHandler.SaveFileAsync(fileStream, fileName);
            Assert.Equal(false, result.Result);
        }
    }
}
