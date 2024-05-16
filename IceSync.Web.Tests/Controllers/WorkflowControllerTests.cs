using AutoMapper;
using IceSync.ApiClient;
using IceSync.Web.Controllers;
using IceSync.Web.Models;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace IceSync.Web.Tests.Controllers
{
    [TestFixture]
    public class WorkflowControllerTests
    {
        private IApiClient _apiClient;
        private IMapper _mapper;
        private WorkflowController _controller;

        [SetUp]
        public void SetUp()
        {
            _apiClient = Substitute.For<IApiClient>();
            _mapper = Substitute.For<IMapper>();
            _controller = new WorkflowController(_apiClient, _mapper);
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithListOfWorkflows()
        {
            var apiWorkflows = new List<ApiClient.ResponseModels.Workflow>
            {
                new() { Id = 1, Name = "Test Workflow", MultiExecBehavior = "Skip" }
            };
            var viewModelWorkflows = new List<WorkflowViewModel>
            {
                new() { Id = 1, Name = "Test Workflow", MultiExecBehavior = "Skip" }
            };

            _apiClient.GetWorkflowsAsync().Returns(apiWorkflows);
            _mapper.Map<IEnumerable<WorkflowViewModel>>(apiWorkflows).Returns(viewModelWorkflows);

            var viewResult = await _controller.Index() as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult!.Model as IEnumerable<WorkflowViewModel>;
            Assert.That(model, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(model!.Count(), Is.EqualTo(1));
                Assert.That(model!.First().Name, Is.EqualTo("Test Workflow"));
            });
        }

        [Test]
        public async Task Run_ReturnsOkResult_WhenApiClientReturnsTrue()
        {
            var workflowId = 1;
            _apiClient.RunWorkflowAsync(workflowId).Returns(true);

            var result = await _controller.Run(workflowId);

            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        [Test]
        public async Task Run_ReturnsBadRequest_WhenApiClientReturnsFalse()
        {
            var workflowId = 1;
            _apiClient.RunWorkflowAsync(workflowId).Returns(false);

            var result = await _controller.Run(workflowId);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }
    }
}
