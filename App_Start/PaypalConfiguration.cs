using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PayPal.Api;
using System.Configuration;

namespace NhomProject.App_Start
{
    public static class PaypalConfiguration
    {
        // Static properties for API credentials
        public readonly static string ClientId;
        public readonly static string ClientSecret;
        public readonly static string Mode;

        static PaypalConfiguration()
        {
            var config = GetConfig();
            ClientId = ConfigurationManager.AppSettings["PayPalClientId"];
            ClientSecret = ConfigurationManager.AppSettings["PayPalClientSecret"];
            Mode = ConfigurationManager.AppSettings["paypal:Mode"];
        }

        // Returns a Dictionary of configuration settings
        public static Dictionary<string, string> GetConfig()
        {
            return new Dictionary<string, string>
            {
                { "mode", ConfigurationManager.AppSettings["paypal:Mode"] }
            };
        }

        // Retrieves an access token
        private static string GetAccessToken()
        {
            string accessToken = new OAuthTokenCredential(ClientId, ClientSecret, GetConfig()).GetAccessToken();
            return accessToken;
        }

        // Returns the API context
        public static APIContext GetAPIContext()
        {
            APIContext apiContext = new APIContext(GetAccessToken());
            apiContext.Config = GetConfig();
            return apiContext;
        }
    }
}