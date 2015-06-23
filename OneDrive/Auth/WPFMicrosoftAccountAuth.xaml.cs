using MicrosoftAccount;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;


namespace WPFOneDriveSDKTest
{
    /// <summary>
    /// Interaction logic for WPFMicrosoftAccountAuth.xaml
    /// </summary> 
    public partial class WPFMicrosoftAccountAuth : Window
    {

        public const string OAuthDesktopEndPoint = "https://login.live.com/oauth20_desktop.srf";
        public const string OAuthMSAAuthorizeService = "https://login.live.com/oauth20_authorize.srf";
        public const string OAuthMSATokenService = "https://login.live.com/oauth20_token.srf";


        #region Properties
        public string StartUrl { get; private set; }
        public string EndUrl { get; private set; }
        public AuthResult AuthResult { get; private set; }
        public OAuthFlow AuthFlow { get; private set; }

        public String result;

        public CancellationTokenSource cts;

        #endregion

        public WPFMicrosoftAccountAuth(string startUrl, string endUrl, OAuthFlow flow = OAuthFlow.AuthorizationCodeGrant)
        {
            InitializeComponent();

            this.StartUrl = startUrl;
            this.EndUrl = endUrl;
            this.AuthFlow = flow;

            this.webBrowser.Navigated += webBrowser_Navigated;

            System.Diagnostics.Debug.WriteLine("Navigating to start URL: " + this.StartUrl);
            this.webBrowser.Navigate(this.StartUrl);

            cts = new CancellationTokenSource();

        }





        #region Private Methods

        void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Navigated to: " + webBrowser.Source.AbsoluteUri.ToString());

            if (this.webBrowser.Source.AbsoluteUri.StartsWith(EndUrl))
            {
                this.AuthResult = new AuthResult(this.webBrowser.Source, this.AuthFlow);
                result = "OK";
                cts.Cancel();
                this.Close();
            }    
          
        }




        public void ShowWithToke()
        {
            this.Show();

        }


        private void CloseWindow()
        {
            const int interval = 100;
            //var t = new System.Threading.Timer(new System.Threading.TimerCallback((state) =>
            //{
            //    //this.DialogResult = System.Windows.Forms.DialogResult.OK;
            //    this.BeginInvoke(new MethodInvoker(() => this.Close()));
            //}), null, interval, System.Threading.Timeout.Infinite);
        }


        #endregion


        //public Task<String> ShowDialogAsync(Window owner = null)
        //{
        //    this.Show();
        //    TaskCompletionSource<String> tcs = new TaskCompletionSource<String>();
        //    this.Closed += (s, e) => { tcs.SetResult(result); };
        //    //{
        //    //    tcs.SetResult(this.DialogResult);
        //    //};
        //    //if (owner == null)
        //    //    this.Show();
        //    //else
        //    //    this.Show(owner);

        //    return tcs.Task;
        //}

        #region Static Methods



        private static string GenerateScopeString(string[] scopes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var scope in scopes)
            {
                if (sb.Length > 0)
                    sb.Append(" ");
                sb.Append(scope);
            }
            return sb.ToString();
        }

        private static string BuildUriWithParameters(string baseUri, Dictionary<string, string> queryStringParameters)
        {
            var sb = new StringBuilder();
            sb.Append(baseUri);
            sb.Append("?");
            foreach (var param in queryStringParameters)
            {
                if (sb[sb.Length - 1] != '?')
                    sb.Append("&");
                sb.Append(param.Key);
                sb.Append("=");
                sb.Append(Uri.EscapeDataString(param.Value));
            }
            return sb.ToString();
        }

        public static void GenerateUrlsForOAuth(string clientId, string[] scopes, OAuthFlow flow, out string startUrl, out string completeUrl, string redirectUrl = OAuthDesktopEndPoint)
        {
            Dictionary<string, string> urlParam = new Dictionary<string, string>();
            urlParam.Add("client_id", clientId);
            urlParam.Add("scope", GenerateScopeString(scopes));
            urlParam.Add("redirect_uri", redirectUrl);
            urlParam.Add("display", "popup");

            switch (flow)
            {
                case OAuthFlow.ImplicitGrant:
                    urlParam.Add("response_type", "token");
                    break;
                case OAuthFlow.AuthorizationCodeGrant:
                    urlParam.Add("response_type", "code");
                    break;
                default:
                    throw new NotSupportedException("flow value not supported");
            }

            startUrl = BuildUriWithParameters(OAuthMSAAuthorizeService, urlParam);
            completeUrl = redirectUrl;
        }

        public static async Task<string> GetAuthenticationToken(string clientId, string[] scopes, OAuthFlow flow, Window owner = null)
        {
            string startUrl, completeUrl;
            GenerateUrlsForOAuth(clientId, scopes, flow, out startUrl, out completeUrl);

            WPFMicrosoftAccountAuth authForm = new WPFMicrosoftAccountAuth(startUrl, completeUrl, flow);


            authForm.ShowWithToke();

            Task t1 = Task.Factory.StartNew(() =>
            {
                authForm.cts.Token.ThrowIfCancellationRequested();

                bool moreToDo = true;
                while (moreToDo)
                {
                    // Poll on this property if you have to do
                    // other cleanup before throwing.
                    if ( authForm.cts.Token.IsCancellationRequested)
                    {
                        // Clean up here, then...
                         authForm.cts.Token.ThrowIfCancellationRequested();
                    }

                }
            }, authForm.cts.Token); // Pass same token to StartNew.

            await Task.WhenAny(t1);

            if ("OK" == authForm.result)
            {
                return OnAuthComplete(authForm.AuthResult);
            }
            return null;
        }



        private static string OnAuthComplete(AuthResult authResult)
        {
            switch (authResult.AuthFlow)
            {
                case OAuthFlow.ImplicitGrant:
                    return authResult.AccessToken;
                case OAuthFlow.AuthorizationCodeGrant:
                    return authResult.AuthorizeCode;
                default:
                    throw new ArgumentOutOfRangeException("Unsupported AuthFlow value");
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            webBrowser.GoBack();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            webBrowser.GoForward();
        }
        #endregion

        public static async Task<string> GetUserId(string authToken)
        {
            if (string.IsNullOrEmpty(authToken)) throw new ArgumentNullException("authToken");

            string requestUrl = "https://apis.live.net/v5.0/me?access_token=" + System.Uri.EscapeUriString(authToken);
            HttpWebRequest request = HttpWebRequest.CreateHttp(requestUrl);

            HttpWebResponse response;
            try
            {

                WebResponse wr = await request.GetResponseAsync();
                response = wr as HttpWebResponse;
            }
            catch (WebException webex)
            {
                response = webex.Response as HttpWebResponse;
            }

            if (null == response) return null;

            UserObject user = null;
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream);
                user = Newtonsoft.Json.JsonConvert.DeserializeObject<UserObject>(await reader.ReadToEndAsync());
            }

            response.Dispose();

            if (null != user)
                return user.Id;
            else
                return null;
        }


        internal class UserObject
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }

    }



}
