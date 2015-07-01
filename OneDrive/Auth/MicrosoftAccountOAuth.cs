﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using WPFOneDriveSDKTest;

namespace MicrosoftAccount
{
    public static class MicrosoftAccountOAuth
    {

        public static async Task<string> LoginOneTimeAuthorizationAsync(string clientId, string[] scopes, System.Windows.Window owner = null)
        {
            return await WPFMicrosoftAccountAuth.GetAuthenticationToken(clientId, scopes, OAuthFlow.ImplicitGrant, owner);
        }

        public static async Task<AppTokenResult> LoginAuthorizationCodeFlowAsync(string clientId, string clientSecret, string[] scopes, System.Windows.Window owner = null)
        {
            var authorizationCode = await WPFMicrosoftAccountAuth.GetAuthenticationToken(clientId, scopes, OAuthFlow.AuthorizationCodeGrant, owner);
            if (string.IsNullOrEmpty(authorizationCode))
                return null;
            try
            {
                var tokens = await RedeemAuthorizationCodeAsync(clientId, WPFMicrosoftAccountAuth.OAuthDesktopEndPoint, clientSecret, authorizationCode);
                return tokens;
            }
            catch (Exception ex)
            {
                return null;
            }
           
        }

        public static async Task<AppTokenResult> RedeemRefreshTokenAsync(string clientId, string clientSecret, string refreshToken)
        {
            var queryBuilder = new QueryStringBuilder();
            queryBuilder.Add("client_id", clientId);
            queryBuilder.Add("redirect_uri", WPFMicrosoftAccountAuth.OAuthDesktopEndPoint);
            queryBuilder.Add("client_secret", clientSecret);
            queryBuilder.Add("refresh_token", refreshToken);
            queryBuilder.Add("grant_type", "refresh_token");

            return await PostToTokenEndPoint(queryBuilder);
        }

        private static async Task<AppTokenResult> PostToTokenEndPoint(QueryStringBuilder queryBuilder)
        {
            HttpWebRequest request = WebRequest.CreateHttp(WPFMicrosoftAccountAuth.OAuthMSATokenService);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            using (StreamWriter requestWriter = new StreamWriter(await request.GetRequestStreamAsync()))
            {
                await requestWriter.WriteAsync(queryBuilder.ToString());
                await requestWriter.FlushAsync();
            }

            HttpWebResponse httpResponse;
            try
            {
                var response = await request.GetResponseAsync();
                httpResponse = response as HttpWebResponse;
            }
            catch (WebException webex)
            {
                httpResponse = webex.Response as HttpWebResponse;
            }
            catch (Exception)
            {
                return null;
            }

            // TODO: better error handling

            if (httpResponse == null) 
                return null;

            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                httpResponse.Dispose();
                return null;
            }

            using (var responseBodyStreamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseBody = await responseBodyStreamReader.ReadToEndAsync();
                var tokenResult = Newtonsoft.Json.JsonConvert.DeserializeObject<AppTokenResult>(responseBody);

                httpResponse.Dispose();
                return tokenResult;
            }
        }

        private static async Task<AppTokenResult> RedeemAuthorizationCodeAsync(string clientId, string redirectUrl, string clientSecret, string authCode)
        {
            QueryStringBuilder queryBuilder = new QueryStringBuilder();
            queryBuilder.Add("client_id", clientId);
            queryBuilder.Add("redirect_uri", redirectUrl);
            queryBuilder.Add("client_secret", clientSecret);
            queryBuilder.Add("code", authCode);
            queryBuilder.Add("grant_type", "authorization_code");

            return await PostToTokenEndPoint(queryBuilder);
        }


    }
}
