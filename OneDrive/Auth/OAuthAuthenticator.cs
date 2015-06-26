﻿using System;
using System.Threading.Tasks;
using OneDrive;
using MicrosoftAccount;


namespace OneDrive
{
    public static class OAuthAuthenticator
    {
        private const string msa_client_id = "0000000044128B55";
        private const string msa_client_secret = "amw-eMF4Ps-jzDVv6qwL4scqp2iFI29l";

        public static async Task<ODConnection> SignInToMicrosoftAccount()
        {
            string oldRefreshToken = Properties.ODSettings.Default.RefreshToken;
            AppTokenResult appToken = null;
            if (!string.IsNullOrEmpty(oldRefreshToken))
            {
                appToken = await MicrosoftAccountOAuth.RedeemRefreshTokenAsync(msa_client_id, msa_client_secret, oldRefreshToken);
            }

            if (null == appToken)
            {
                appToken = await MicrosoftAccountOAuth.LoginAuthorizationCodeFlowAsync(msa_client_id,
                    msa_client_secret,
                    new[] { "wl.offline_access", "wl.basic", "wl.signin", "onedrive.readwrite" });
            }                       

            if (null != appToken)
            {
                SaveRefreshToken(appToken.RefreshToken);

                return new ODConnection("https://api.onedrive.com/v1.0", new OAuthTicket(appToken));
            }

            return null;
        }

        private static void SaveRefreshToken(string refreshToken)
        {
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var settings = Properties.ODSettings.Default;
                settings.RefreshToken = refreshToken;
                settings.Save();
            }
        }

        public static async Task<AppTokenResult> RenewAccessTokenAsync(OAuthTicket ticket)
        {
            string oldRefreshToken = ticket.RefreshToken;
            AppTokenResult appToken = null;

            if (!string.IsNullOrEmpty(oldRefreshToken))
            {
                appToken = await MicrosoftAccountOAuth.RedeemRefreshTokenAsync(msa_client_id, msa_client_secret, oldRefreshToken);
                SaveRefreshToken(appToken.RefreshToken);
            }

            return appToken;
        }
    }
}
