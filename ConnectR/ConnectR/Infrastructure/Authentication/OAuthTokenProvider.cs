﻿// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConnectR.Infrastructure.Authentication
{
    /// <summary>
    /// We can't use Authorization Headers for a Websocket-scenario with SignalR. This only works in the long-polling scenario.
    /// 
    /// Instead we'll look for the Bearer Token in multiple locations:
    /// 
    ///     http://stackoverflow.com/questions/22989209/web-api-owin-signalr-authorization    
    /// </summary>
    public class OAuthTokenProvider : OAuthBearerAuthenticationProvider
    {
        private List<Func<IOwinRequest, string>> _locations;
        private readonly Regex _bearerRegex = new Regex("((B|b)earer\\s)");
        private const string AuthHeader = "Authorization";

        /// <summary>
        /// By Default the Token will be searched for on the "Authorization" header.
        /// <para> pass additional getters that might return a token string</para>
        /// </summary>
        /// <param name="locations"></param>
        public OAuthTokenProvider(params Func<IOwinRequest, string>[] locations)
        {
            _locations = locations.ToList();
            //Header is used by default
            _locations.Add(x => x.Headers.Get(AuthHeader));
        }

        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            var getter = _locations.FirstOrDefault(x => !String.IsNullOrWhiteSpace(x(context.Request)));
            if (getter != null)
            {
                var tokenStr = getter(context.Request);
                context.Token = _bearerRegex.Replace(tokenStr, "").Trim();
            }
            return Task.FromResult<object>(null);
        }
    }
}
