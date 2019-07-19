﻿using System.Collections.Generic;
using System.Linq;
using Shouldly;
using System.Net.Http;
using System.Threading.Tasks;
using Jbet.Api.Hateoas.Resources.Base;

namespace Jbet.Tests.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<TResponse> ShouldDeserializeTo<TResponse>(this HttpResponseMessage response)
        {
            var deserialized = await response?
                .Content?
                .ReadAsAsync<TResponse>();

            return deserialized != null ?
                deserialized :
                throw new ShouldAssertException($"Expected the response to be of type {typeof(TResponse).FullName} but could not deserialize it.");
        }

        public static async Task<TResource> ShouldBeAResource<TResource>(this HttpResponseMessage response, IEnumerable<string> expectedLinks)
            where TResource : Resource
        {
            // We always expect valid resources to be returned with a success status code
            response.IsSuccessStatusCode.ShouldBeTrue(response.StatusCode.ToString());

            var resource = await response.ShouldDeserializeTo<TResource>();

            if (expectedLinks != null)
                resource.Links.ShouldAllBe(l => expectedLinks.Contains(l.Key));

            return resource;
        }
    }
}