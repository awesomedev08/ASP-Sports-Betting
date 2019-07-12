﻿using System.Net;
using System.Threading.Tasks;
using Jbet.Core.AuthContext;
using Jbet.Core.AuthContext.Commands;
using Jbet.Domain;
using Jbet.Domain.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jbet.Api.Controllers
{
    public class AuthController : Controller
    {
        /// <summary>
        /// Retrieves the currently logged in user.
        /// </summary>
        [HttpGet(Name = nameof(GetCurrentUser))]
        public async Task<IActionResult> GetCurrentUser()
        {
            return Ok();
        }

        /// <summary>
        /// Register.
        /// </summary>
        /// <param name="command">The user model.</param>
        /// <returns>A user model.</returns>
        /// <response code="201">A user was created.</response>
        /// <response code="400">Invalid input.</response>
        [HttpPost("register", Name = nameof(Register))]
        [ProducesResponseType(typeof(Error), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Register([FromBody] Register command)
        {
            return Ok();
        }

        /// <summary>
        /// Login.
        /// </summary>
        /// <param name="command">The credentials.</param>
        /// <returns>A JWT.</returns>
        /// <response code="200">If the credentials have a match.</response>
        /// <response code="400">If the credentials don't match/don't meet the requirements.</response>
        [HttpPost("login", Name = nameof(Login))]
        [ProducesResponseType(typeof(JwtView), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Error), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Login([FromBody] Login command)
        {
            return Ok();
        }

        /// <summary>
        /// Logout. (unsets the auth cookie)
        /// </summary>
        [Authorize]
        [HttpDelete("logout", Name = nameof(Logout))]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Response.Cookies.Delete(AuthConstants.Cookies.AuthCookieName);
            //var resource = await ToEmptyResourceAsync<LogoutResource>();
            return Ok();
        }
    }
}