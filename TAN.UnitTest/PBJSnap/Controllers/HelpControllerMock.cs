using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TANWeb.Controllers;

namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class HelpControllerMock
    {
        private readonly Mock<IConfiguration> _configuration;
        private readonly HelpController _helpController;

        public HelpControllerMock()
        {
            _configuration = new Mock<IConfiguration>();
            _configuration.Setup(c => c.GetSection("UserManualPath")["Server"])
                              .Returns("https://localhost:7032/");
            _configuration.Setup(c => c.GetSection("UserManualPath")["FilePath"])
                              .Returns("assets/usermanual/");
            _configuration.Setup(c => c.GetSection("UserManualPath")["FileName"])
                              .Returns("ThinkAnewUserManualv2");
            _helpController = new HelpController(_configuration.Object);
        }

        [Fact]
        public void Index_ReturnsCorrectViewWithFilePathInViewBag()
        {
            //Arrange
            _configuration.Setup(c => c.GetSection("UserManualPath:Server").Value)
                .Returns("https://localhost:7032/");
            _configuration.Setup(c => c.GetSection("UserManualPath:FilePath").Value)
                .Returns("assets/usermanual/"); 
            _configuration.Setup(c => c.GetSection("UserManualPath:FileName").Value)
                .Returns("ThinkAnewUserManualv2");
            // Act
            var result = _helpController.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);
            Assert.True(result.ViewData.ContainsKey("FilePath"));

            var filePath = result.ViewData["FilePath"] as string;
            Assert.Equal("https://localhost:7032/assets/usermanual/ThinkAnewUserManualv2.pdf", filePath);
        }

    }
}
