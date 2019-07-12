﻿using System.Net;
using System.Threading.Tasks;
using Jbet.Api.Hateoas.Resources.Auth;
using Jbet.Api.Hateoas.Resources.Base;
using Jbet.Core.AuthContext;
using Jbet.Core.AuthContext.Commands;
using Jbet.Core.AuthContext.Queries;
using Jbet.Domain;
using Jbet.Domain.Views;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optional.Async.Extensions;

namespace Jbet.Api.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// A controller responsible for users and JWT token. 
    /// </summary>
    public class AuthController : ApiController
    {
        /// <inheritdoc />
        public AuthController(IMediator mediator, IResourceMapper resourceMapper) 
            : base(mediator, resourceMapper)
        {
        }

        /// <summary>
        /// Retrieves the currently logged in user.
        /// </summary>
        [HttpGet(Name = nameof(GetCurrentUser))]
        public async Task<IActionResult> GetCurrentUser() =>
            (await Mediator.Send(new GetUser { Id = CurrentUserId })
                .MapAsync(ToResourceAsync<UserView, UserResource>))
            .Match<IActionResult>(Ok, _ => Unauthorized());

        /// <summary>
        /// Register.
        /// </summary>
        /// <param name="command">The user model.</param>
        /// <returns>A user model.</returns>
        /// <response code="201">A user was created.</response>
        /// <response code="400">Invalid input.</response>
        [HttpPost("register", Name = nameof(Register))]
        [ProducesResponseType(typeof(Error), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Register([FromBody] Register command) =>
            (await Mediator.Send(command)
                .MapAsync(ToEmptyResourceAsync<RegisterResource>))
            .Match(u => CreatedAtAction(nameof(Register), u), Error);

        /// <summary>
        /// Login.
        /// </summary>
        /// <param name="command">The credentials.</param>
        /// <returns>A JWT.</returns>
        /// <response code="200">If the credentials have a match.</response>
        /// <response code="400">If the credentials don't match/don't meet the requirements.</response>
        [HttpPost("login", Name = nameof(Login))]
        [ProducesResponseType(typeof(JwtView), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Error), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Login([FromBody] Login command) =>
            (await Mediator.Send(command)
                .MapAsync(ToResourceAsync<JwtView, LoginResource>))
            .Match(
                jwt =>
                {
                    SetAuthCookie(jwt.TokenString);
                    return Ok(jwt);
                },
                Error);

        /// <summary>
        /// Logout. (unsets the auth cookie)
        /// </summary>
        [Authorize]
        [HttpDelete("logout", Name = nameof(Logout))]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Response.Cookies.Delete(AuthConstants.Cookies.AuthCookieName);
            var resource = await ToEmptyResourceAsync<LogoutResource>();
            return Ok(resource);
        }

        private void SetAuthCookie(string token) =>
            HttpContext.Response.Cookies.Append(AuthConstants.Cookies.AuthCookieName, token);
    }
}