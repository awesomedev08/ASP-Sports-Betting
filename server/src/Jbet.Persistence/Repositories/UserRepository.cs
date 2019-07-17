﻿using Jbet.Domain;
using Jbet.Domain.Entities;
using Jbet.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Optional;
using Optional.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jbet.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;

        public UserRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Option<Unit, Error>> RegisterAsync(User user, string password)
        {
            var creationResult = (await _userManager.CreateAsync(user, password))
                .SomeWhen(
                    x => x.Succeeded,
                    x => Error.Validation(x.Errors.Select(e => e.Description)));

            return creationResult
                .Map(_ => Unit.Value);
        }

        public async Task<Option<User>> GetAsync(Guid id) =>
            (await _userManager
                .FindByIdAsync(id.ToString()))
            .SomeNotNull();

        public async Task<Unit> ReplaceClaimAsync(User account, string claimType, string claimValue)
        {
            var claimToReplace = (await _userManager.GetClaimsAsync(account))
                .FirstOrDefault(c => c.Type == claimType);

            var claimToAdd = new Claim(claimType, claimValue);

            if (claimToReplace != null)
            {
                await _userManager.ReplaceClaimAsync(account, claimToReplace, claimToAdd);
            }
            else
            {
                await _userManager.AddClaimAsync(account, claimToAdd);
            }

            return Unit.Value;
        }

        public async Task<Option<User>> GetByEmailAsync(string email) =>
            (await _userManager
                .FindByEmailAsync(email))
                .SomeNotNull();

        public Task<bool> CheckPasswordAsync(User user, string password) =>
            _userManager
                .CheckPasswordAsync(user, password);

        public Task<IList<Claim>> GetClaimsAsync(User user) =>
            _userManager
                .GetClaimsAsync(user);
    }
}