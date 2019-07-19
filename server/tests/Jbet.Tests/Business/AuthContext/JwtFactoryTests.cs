﻿using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using Jbet.Business.AuthContext;
using Jbet.Core.AuthContext.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Jbet.Tests.Business.AuthContext
{
    public class JwtFactoryTests
    {
        private readonly JwtFactory _jwtFactory;
        private readonly JwtConfiguration _jwtConfiguration;

        public JwtFactoryTests()
        {
            var fixture = new Fixture();

            var signingKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(fixture.Create<string>()));

            _jwtConfiguration = fixture
                .Build<JwtConfiguration>()
                .With(config => config.SigningCredentials, new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256))
                .Create();

            var configuration = Options.Create(_jwtConfiguration);

            _jwtFactory = new JwtFactory(configuration);
        }

        [Theory]
        [AutoData]
        public void CanGenerateProperEncodedTokenWithExtraClaims(string userId, string email, Fixture claimsFixture)
        {
            // Arrange
            // To enable AutoFixture to generate claims
            claimsFixture.Behaviors.Add(new OmitOnRecursionBehavior());
            claimsFixture.Customize<Claim>(
                c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));

            var claims = claimsFixture.Create<List<Claim>>();

            // Act
            var result = _jwtFactory.GenerateEncodedToken(userId, email, claims);

            // Assert
            var jwt = new JwtSecurityToken(result);

            jwt.Claims.ShouldContain(c => c.Type == JwtRegisteredClaimNames.Iss &&
                                          c.Value == _jwtConfiguration.Issuer);
            jwt.Claims.ShouldContain(c => c.Type == JwtRegisteredClaimNames.Sub &&
                                          c.Value == userId);
            jwt.Claims.ShouldContain(c => c.Type == JwtRegisteredClaimNames.Email &&
                                          c.Value == email);
            jwt.Claims.ShouldContain(c => c.Type == JwtRegisteredClaimNames.Aud &&
                                          c.Value == _jwtConfiguration.Audience);

            // The jwt should contain all of the extra claims
            claims.ShouldAllBe(c => jwt.Claims.Any(x => x.Type == c.Type &&
                                                        x.Value == c.Value));

            jwt.ValidFrom.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(10));
            jwt.ValidTo.ShouldBe(DateTime.UtcNow.Add(_jwtConfiguration.ValidFor), TimeSpan.FromSeconds(10));
        }

        [Theory]
        [AutoData]
        public void CanGenerateProperEncodedToken(string userId, string email)
        {
            // Arrange
            // Act
            var result = _jwtFactory.GenerateEncodedToken(userId, email, new List<Claim>());

            // Assert
            var jwt = new JwtSecurityToken(result);

            jwt.Claims.ShouldContain(c => c.Type == JwtRegisteredClaimNames.Iss && c.Value == _jwtConfiguration.Issuer);
            jwt.Claims.ShouldContain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId);
            jwt.Claims.ShouldContain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
            jwt.Claims.ShouldContain(c => c.Type == JwtRegisteredClaimNames.Aud && c.Value == _jwtConfiguration.Audience);

            jwt.ValidFrom.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(10));
            jwt.ValidTo.ShouldBe(DateTime.UtcNow.Add(_jwtConfiguration.ValidFor), TimeSpan.FromSeconds(10));
        }

        [Fact]
        public void CannotBeInitializedWithNullOptions() =>
            ShouldThrowForConfiguration(null, typeof(ArgumentNullException));

        [Fact]
        public void CannotBeInitializedWithInvalidValidity() =>
            ShouldThrowForConfiguration(
                new JwtConfiguration
                {
                    ValidFor = TimeSpan.Zero.Subtract(TimeSpan.FromDays(1))
                },
                typeof(ArgumentException));

        [Fact]
        public void CannotBeInitializedWithNullSigningCredentials() =>
            ShouldThrowForConfiguration(
                new JwtConfiguration
                {
                    SigningCredentials = null
                },
                typeof(ArgumentNullException));

        private static void ShouldThrowForConfiguration(JwtConfiguration configuration, Type expectedExceptionType)
        {
            var options = Options.Create(configuration);

            Should.Throw(
                () => new JwtFactory(options),
                expectedExceptionType);
        }

    }
}