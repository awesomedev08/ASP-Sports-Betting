﻿using Jbet.Api.Hateoas.Resources.Base;
using Jbet.Core.Base;
using Jbet.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jbet.Api.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Base controller.
    /// </summary>
    [Route("[controller]")]
    public class ApiController : Controller
    {
        public ApiController(IMediator mediator, IResourceMapper resourceMapper)
        {
            Mediator = mediator;
            ResourceMapper = resourceMapper;
        }

        protected IMediator Mediator { get; }

        protected IResourceMapper ResourceMapper { get; }

        protected Guid CurrentUserId => TryGetGuidClaim(ClaimTypes.NameIdentifier).ValueOr(Guid.Empty);

        protected IActionResult Error(Error error)
        {
            switch (error.Type)
            {
                case ErrorType.Validation:
                    return BadRequest(error);

                case ErrorType.NotFound:
                    return NotFound(error);

                case ErrorType.Unauthorized:
                    return Unauthorized(error);

                case ErrorType.Conflict:
                    return Conflict(error);

                case ErrorType.Critical:
                    // This shouldn't really happen as critical errors are there to be used by the generic exception filter
                    return new ObjectResult(error)
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };

                default:
                    return BadRequest(error);
            }
        }

        protected IActionResult NotFound(Error error) =>
            NotFound((object)error);

        protected async Task<IActionResult> ResourceContainerResult<TResponse, TResource, TContainer>(IQuery<IList<TResponse>> query)
            where TResource : Resource
            where TContainer : ResourceContainer<TResource>, new()
        {
            var result = await Mediator.Send(query);
            var resource = await ToResourceContainerAsync<TResponse, TResource, TContainer>(result);
            return Ok(resource);
        }

        protected Task<TContainer> ToResourceContainerAsync<T, TResource, TContainer>(IEnumerable<T> models)
            where TContainer : ResourceContainer<TResource>, new()
            where TResource : Resource =>
            ResourceMapper.MapContainerAsync<T, TResource, TContainer>(models);

        protected Task<TResource> ToResourceAsync<T, TResource>(T obj)
            where TResource : Resource =>
            ResourceMapper.MapAsync<T, TResource>(obj);

        protected Task<TResource> ToEmptyResourceAsync<TResource>(Unit unit = default(Unit))
            where TResource : Resource, new() =>
            ResourceMapper.CreateEmptyResourceAsync<TResource>();

        protected Task<TResource> ToEmptyResourceAsync<TResource>(Action<TResource> beforeMap)
            where TResource : Resource, new() =>
            ResourceMapper.CreateEmptyResourceAsync(beforeMap);

        private Option<Guid> TryGetGuidClaim(string claimType)
        {
            var claimValue = User
                .Claims
                .FirstOrDefault(c => c.Type == claimType)?
                .Value;

            return claimValue
                .SomeNotNull()
                .Filter(v => Guid.TryParse(v, out _))
                .Map(v => new Guid(v));
        }
    }
}