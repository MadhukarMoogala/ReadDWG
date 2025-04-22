using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Threading.Tasks;
using WebApi;
using Xunit;
using static WebApi.DrawingsController;

namespace WebApi.Tests.Controllers
{
    public class DrawingsControllerTests
    {
        private readonly Mock<IJobQueue> _mockJobQueue = new();
        private readonly Mock<IWebHostEnvironment> _mockEnv = new();
        private readonly Mock<ILogger<DrawingsController>> _mockLogger = new();
        private readonly DrawingsController _controller;

        public DrawingsControllerTests()
        {
            _mockEnv.Setup(e => e.ContentRootPath).Returns("/test/path");
            _controller = new DrawingsController(_mockJobQueue.Object, _mockEnv.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Upload_ValidDwgFile_ReturnsJobId_AndEnqueuesJob()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.dwg");
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns(Task.CompletedTask);

            DrawingJob? enqueuedJob = null;
            _mockJobQueue.Setup(q => q.Enqueue(It.IsAny<DrawingJob>()))
                         .Callback<DrawingJob>(job => enqueuedJob = job);

            var controller = new DrawingsController(_mockJobQueue.Object, _mockEnv.Object, _mockLogger.Object);

            // Act
            var result = await controller.Upload(mockFile.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value!;
            var jobId = value.GetType().GetProperty("jobId")?.GetValue(value)?.ToString();

            Assert.False(string.IsNullOrEmpty(jobId));
            Assert.NotNull(enqueuedJob);
            Assert.Equal(jobId, enqueuedJob!.Id);

            _mockJobQueue.Verify(q => q.Enqueue(It.IsAny<DrawingJob>()), Times.Once);
        }



        [Fact]
        public async Task Upload_InvalidFileType_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.pdf");

            // Act
            var result = await _controller.Upload(mockFile.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Only .dwg files are supported.", badRequestResult.Value);
        }

        [Fact]
        public void Status_ExistingJob_ReturnsStatus()
        {
            // Arrange
            var testJob = new DrawingJob { Id = "test123", Status = JobStatus.Pending };
            _mockJobQueue.Setup(q => q.GetJob("test123")).Returns(testJob);

            // Act
            var result = _controller.Status("test123");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<JobStatusResponse>(okResult.Value);

            Assert.Equal("Pending", response.Status);
            Assert.Null(response.ErrorMessage);
        }


        [Fact]
        public void Status_NonExistentJob_ReturnsNotFound()
        {
            // Arrange
            _mockJobQueue.Setup(q => q.GetJob(It.IsAny<string>())).Returns((DrawingJob?)null);

            // Act
            var result = _controller.Status("invalid");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        
        [Fact]
        public void Result_CompletedJob_ReturnsParsedOutput()
        {
            // Arrange
            var testJob = new DrawingJob
            {
                Id = "test123",
                Status = JobStatus.Completed,
                OutputPath = Path.GetFullPath("output_test.txt")
            };

            _mockJobQueue.Setup(q => q.GetJob("test123")).Returns(testJob);

            var fileContent = """
                            Layer:Walls
                            Layer:Furniture
                            Block:Table
                            Block:Chair
                            """;

            System.IO.File.WriteAllText(testJob.OutputPath, fileContent);

            // Act
            var result = _controller.Result("test123");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<JobResultResponse>(okResult.Value);

            if(response == null)
            {
                Assert.Fail("Response is null");
                return;
            }
            if(response.Output == null)
            {
                Assert.Fail("Response.Output is null");
                return;
            }
            Assert.NotNull(response.Output.Layers);
            Assert.NotNull(response.Output.Blocks);

            Assert.Contains("Walls", response.Output.Layers );
            Assert.Contains("Table", response.Output.Blocks );       
            Assert.Contains("Furniture", response.Output.Layers);
            Assert.Contains("Chair", response.Output.Blocks);

            // Cleanup
            System.IO.File.Delete(testJob.OutputPath);
        }


        [Fact]
        public void Result_IncompleteJob_ReturnsBadRequest()
        {
            // Arrange
            var testJob = new DrawingJob { Id = "test123", Status = JobStatus.Processing };
            _mockJobQueue.Setup(q => q.GetJob("test123")).Returns(testJob);

            // Act
            var result = _controller.Result("test123");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
