﻿using System;
using System.Net;
using FakeItEasy;
using Jbet.Api.Filters;
using Jbet.Domain;
using Jbet.Tests.Customizations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using Xunit;

namespace Jbet.Tests.Api.Filters
{
    public class ExceptionFilterTests
    {
        [Theory]
        [CustomizedAutoData]
        public void ShouldReturnSerializedExceptionWhenInDevelopment(Exception exception)
        {
            // Arrange
            var environmentMock = new Mock<IHostingEnvironment>();

            environmentMock
                .Setup(e => e.EnvironmentName)
                .Returns("Development");

            var filter = new ExceptionFilter(environmentMock.Object);
            var filterContext = FilterContextProvider.GetExceptionContext(exception);

            // Act
            filter.OnException(filterContext);

            // Assert
            filterContext.HttpContext.Response.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);

            var result = filterContext.Result as JsonResult;

            result.ShouldNotBeNull();
            result.Value.ShouldBeOfType<Exception>();
            ((Exception)result.Value).Message.ShouldBe(exception.Message);
        }

        [Theory]
        [CustomizedAutoData]
        public void ShouldReturnGenericErrorWhenInProduction(Exception exception)
        {
            // Arrange
            var environment = A.Fake<IHostingEnvironment>(opts =>
            {
                opts.ConfigureFake(env => env.EnvironmentName = "Production");
            });

            var filter = new ExceptionFilter(environment);
            var filterContext = FilterContextProvider.GetExceptionContext(exception);

            // Act
            filter.OnException(filterContext);

            // Assert
            filterContext.HttpContext.Response.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);

            var result = filterContext.Result as JsonResult;

            result.ShouldNotBeNull();
            result.Value.ShouldBeOfType<Error>();

            var error = (Error)result.Value;

            error.Type.ShouldBe(ErrorType.Critical);
            error.Messages.ShouldNotBeEmpty();
        }
    }
}