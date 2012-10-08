//using DotNetOpenAuth.AspNet;
//using DotNetOpenAuth.AspNet.Clients;
//using DotNetOpenAuth.Messaging;
//using DotNetOpenAuth.OAuth;
//using DotNetOpenAuth.OAuth.ChannelElements;
//using DotNetOpenAuth.OAuth.Messages;
//using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
//using DotNetOpenAuth.OpenId.RelyingParty;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Reflection;
//using System.Text;
//using System.Xml;
//using System.Xml.Linq;

//namespace FilmTrove.OAuthClients
//{
//    /// <summary>
//    /// Represents a Netflix client
//    /// </summary>
//    public class NetflixClient : IAuthenticationClient
//    {
//        #region Constants and Fields


//        /// <summary>
//        /// Gets the name of the provider which provides authentication service.
//        /// </summary>
//        public string ProviderName { get; private set; }

//        /// <summary>
//        /// Gets the OAuthWebConsumer instance which handles constructing requests to the OAuth providers.
//        /// </summary>
//        protected IOAuthWebWorker WebWorker { get; private set; }

//        /// <summary>
//        /// The description of Netflix's OAuth protocol URIs.
//        /// </summary>
//        public static readonly ServiceProviderDescription NetflixServiceDescription = new ServiceProviderDescription
//        {
//            ProtocolVersion = DotNetOpenAuth.OAuth.ProtocolVersion.V10,
//            RequestTokenEndpoint =
//                new MessageReceivingEndpoint(
//                    "http://api-public.netflix.com/oauth/request_token",
//                    HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
//            UserAuthorizationEndpoint =
//                new MessageReceivingEndpoint(
//                    "https://api-user.netflix.com/oauth/login",
//                    HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
//            AccessTokenEndpoint =
//                new MessageReceivingEndpoint(
//                    "http://api-public.netflix.com/oauth/access_token",
//                    HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
//            TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
//        };
//        #endregion

//        #region Constructors and Destructors

//        /// <summary>
//        /// Initializes a new instance of the <see cref="NetflixClient"/> class with the specified consumer key and consumer secret.
//        /// </summary>
//        /// <remarks>
//        /// Tokens exchanged during the OAuth handshake are stored in cookies.
//        /// </remarks>
//        /// <param name="consumerKey">
//        /// The consumer key. 
//        /// </param>
//        /// <param name="consumerSecret">
//        /// The consumer secret. 
//        /// </param>
//        public NetflixClient(string consumerKey, string consumerSecret)
//            : this(consumerKey, consumerSecret, new AuthenticationOnlyCookieOAuthTokenManager()) { }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="NetflixClient"/> class.
//        /// </summary>
//        /// <param name="consumerKey">The consumer key.</param>
//        /// <param name="consumerSecret">The consumer secret.</param>
//        /// <param name="tokenManager">The token manager.</param>
//        public NetflixClient(string consumerKey, string consumerSecret, IOAuthTokenManager tokenManager)
//        {
//            this.ProviderName = "netflix";
//            this.WebWorker = new NetflixOAuthWebConsumer(NetflixServiceDescription, new SimpleConsumerTokenManager(consumerKey, consumerSecret, tokenManager));
//        }

//        #endregion

//        #region Methods

//        /// <summary>
//        /// Check if authentication succeeded after user is redirected back from the service provider.
//        /// </summary>
//        /// <param name="response">
//        /// The response token returned from service provider 
//        /// </param>
//        /// <returns>
//        /// Authentication result 
//        /// </returns>
//        protected AuthenticationResult VerifyAuthenticationCore(AuthorizedTokenResponse response)
//        {
//            string accessToken = response.AccessToken;
//            string userId = response.ExtraData["user_id"];
//            //string userName = response.ExtraData["screen_name"];

//            var profileRequestUrl = new Uri("http://api-public.netflix.com/users/"
//                                       + EscapeUriDataStringRfc3986(userId));
//            var profileEndpoint = new MessageReceivingEndpoint(profileRequestUrl, HttpDeliveryMethods.GetRequest);
//            HttpWebRequest request = this.WebWorker.PrepareAuthorizedRequest(profileEndpoint, accessToken);

//            var extraData = new Dictionary<string, string>();
//            extraData.Add("accesstoken", accessToken);
//            try
//            {
//                using (WebResponse profileResponse = request.GetResponse())
//                {
//                    using (Stream responseStream = profileResponse.GetResponseStream())
//                    {
//                        XDocument document = LoadXDocumentFromStream(responseStream);
//                        extraData.AddDataIfNotEmpty(document, "name");
//                        extraData.AddDataIfNotEmpty(document, "location");
//                        extraData.AddDataIfNotEmpty(document, "description");
//                        extraData.AddDataIfNotEmpty(document, "url");
//                    }
//                }
//            }
//            catch (Exception)
//            {
//                // At this point, the authentication is already successful.
//                // Here we are just trying to get additional data if we can.
//                // If it fails, no problem.
//            }

//            return new AuthenticationResult(
//                isSuccessful: true, provider: this.ProviderName, providerUserId: userId, userName: userId, extraData: extraData);
//        }

//        public AuthenticationResult VerifyAuthentication(System.Web.HttpContextBase context)
//        {
//            AuthorizedTokenResponse response = this.WebWorker.ProcessUserAuthorization();
//            if (response == null)
//            {
//                return AuthenticationResult.Failed;
//            }

//            AuthenticationResult result = this.VerifyAuthenticationCore(response);
//            if (result.IsSuccessful && result.ExtraData != null)
//            {
//                // add the access token to the user data dictionary just in case page developers want to use it
//                var wrapExtraData = result.ExtraData.IsReadOnly
//                    ? new Dictionary<string, string>(result.ExtraData)
//                    : result.ExtraData;
//                wrapExtraData["accesstoken"] = response.AccessToken;

//                AuthenticationResult wrapResult = new AuthenticationResult(
//                    result.IsSuccessful,
//                    result.Provider,
//                    result.ProviderUserId,
//                    result.UserName,
//                    wrapExtraData);

//                result = wrapResult;
//            }

//            return result;
//        }

//        public void RequestAuthentication(System.Web.HttpContextBase context, Uri returnUrl)
//        {
//            Uri callback = returnUrl.StripQueryArgumentsWithPrefix("oauth_");
//            this.WebWorker.RequestAuthentication(callback);
//        }

//        #endregion

//        #region Interal stuff that I had to add back
//        internal static XDocument LoadXDocumentFromStream(Stream stream)
//        {
//            const int MaxChars = 0x10000; // 64k

//            XmlReaderSettings settings = new XmlReaderSettings()
//            {
//                MaxCharactersInDocument = MaxChars
//            };
//            return XDocument.Load(XmlReader.Create(stream, settings));
//        }
//        private static readonly string[] UriRfc3986CharsToEscape = new[] { "!", "*", "'", "(", ")" };
//        internal static string EscapeUriDataStringRfc3986(string value)
//        {
//            // Start with RFC 2396 escaping by calling the .NET method to do the work.
//            // This MAY sometimes exhibit RFC 3986 behavior (according to the documentation).
//            // If it does, the escaping we do that follows it will be a no-op since the
//            // characters we search for to replace can't possibly exist in the string.
//            StringBuilder escaped = new StringBuilder(Uri.EscapeDataString(value));

//            // Upgrade the escaping to RFC 3986, if necessary.
//            for (int i = 0; i < UriRfc3986CharsToEscape.Length; i++)
//            {
//                escaped.Replace(UriRfc3986CharsToEscape[i], Uri.HexEscape(UriRfc3986CharsToEscape[i][0]));
//            }

//            // Return the fully-RFC3986-escaped string.
//            return escaped.ToString();
//        }
//        #endregion

//    }

//    public class NetflixOAuthWebConsumer : IOAuthWebWorker
//    {
//        /// <summary>
//        /// The _web consumer.
//        /// </summary>
//        private readonly NetflixWebConsumer webConsumer;

//        public NetflixOAuthWebConsumer(ServiceProviderDescription serviceDescription, IConsumerTokenManager tokenManager)
//        {
//            this.webConsumer = new NetflixWebConsumer(serviceDescription, tokenManager);
//        }

//        public HttpWebRequest PrepareAuthorizedRequest(MessageReceivingEndpoint profileEndpoint, string accessToken)
//        {
//            Dictionary<String, String> d = new Dictionary<String, String>();
//            d.Add("application_name", "FilmTrove");
//            d.Add("oauth_consumer_key", "7qf3845qydavuucmhj96b6hd");
//            return this.webConsumer.PrepareAuthorizedRequest(profileEndpoint, accessToken, d);
//        }

//        public AuthorizedTokenResponse ProcessUserAuthorization()
//        {
//            return this.webConsumer.ProcessUserAuthorization();
//        }

//        public void RequestAuthentication(Uri callback)
//        {
//            Dictionary<String, String> d = new Dictionary<String, String>();
//            d.Add("application_name", "FilmTrove");
            
//            var redirectParameters = new Dictionary<string, string> { { "force_login", "false" } };
//            UserAuthorizationRequest request = this.webConsumer.PrepareRequestUserAuthorization(
//                callback, d, redirectParameters);
//            this.webConsumer.Channel.PrepareResponse(request).Send();
//        }
//    }

//    public class NetflixWebConsumer : WebConsumer
//    {
//        public NetflixWebConsumer(ServiceProviderDescription serviceDescription, IConsumerTokenManager tokenManager)
//            : base(serviceDescription, tokenManager) { }

//        protected new UserAuthorizationRequest PrepareRequestUserAuthorization(Uri callback, IDictionary<string, string> requestParameters, IDictionary<string, string> redirectParameters, out string requestToken)
//        {
//            Assembly asm = Assembly.GetExecutingAssembly();

//            // Obtain an unauthorized request token.  Assume the OAuth version given in the service description.
//            var token = (UnauthorizedTokenRequest)asm.CreateInstance("DotNetOpenAuth.OAuth.Messages.UnauthorizedTokenRequest", false,
//                 BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Instance,
//                 null, new Object[] { this.ServiceProvider.RequestTokenEndpoint, new Version(1, 0) }, null, null);
//            token.ConsumerKey = this.ConsumerKey;
//            token.Callback = callback;

//            //var token = new UnauthorizedTokenRequest(this.ServiceProvider.RequestTokenEndpoint, this.ServiceProvider.Version)
//            //{
//            //    ConsumerKey = this.ConsumerKey,
//            //    Callback = callback,
//            //};
//            //Type mdc = asm.GetType("DotNetOpenAuth.Messaging.Reflection.MessageDescriptionCollection");
            
//            var tokenAccessor = this.Channel.GetType().GetProperty("MessageDescriptions").GetValue(this.Channel, null);
//            //var tokenAccessor = this.Channel.MessageDescriptions.GetAccessor(token);
//            tokenAccessor.AddExtraParameters(requestParameters);

//            var requestTokenResponse = this.Channel.Request<UnauthorizedTokenResponse>(token);
//            this.TokenManager.StoreNewRequestToken(token, requestTokenResponse);

//            // Fine-tune our understanding of the SP's supported OAuth version if it's wrong.
//            //if (this.ServiceProvider.Version != requestTokenResponse.Version)
//            //{
//            //    this.ServiceProvider.ProtocolVersion = Enum.Parse(typeof(ProtocolVersion), requestTokenResponse.Version.ToString());
//            //}

//            // Request user authorization.  The OAuth version will automatically include 
//            // or drop the callback that we're setting here.
//            ITokenContainingMessage assignedRequestToken = requestTokenResponse;

//            var requestAuthorization = (UserAuthorizationRequest)asm.CreateInstance("DotNetOpenAuth.OAuth.Messages.UserAuthorizationRequest", false,
//                BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Instance,
//                null, new Object[] { this.ServiceProvider.UserAuthorizationEndpoint, assignedRequestToken.Token, new Version(1, 0) }, null, null);
//            requestAuthorization.Callback = callback;
//            //var requestAuthorization = new UserAuthorizationRequest(this.ServiceProvider.UserAuthorizationEndpoint, assignedRequestToken.Token, requestTokenResponse.Version)
//            //{
//            //    Callback = callback,
//            //};
//            var requestAuthorizationAccessor = tokenAccessor.GetAccessor(requestAuthorization);
//            requestAuthorizationAccessor.AddExtraParameters(redirectParameters);
//            requestToken = requestAuthorization.RequestToken;
//            return requestAuthorization;
//        }
//    }

//    //public class UnauthorizedTokenRequest : SignedMessageBase
//    //{
//    //    /// <summary>
//    //    /// Initializes a new instance of the <see cref="UnauthorizedTokenRequest"/> class.
//    //    /// </summary>
//    //    /// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
//    //    /// <param name="version">The OAuth version.</param>
//    //    public UnauthorizedTokenRequest(MessageReceivingEndpoint serviceProvider, Version version)
//    //        : base(MessageTransport.Direct, serviceProvider, version)
//    //    {
//    //    }

//    //    /// <summary>
//    //    /// Gets or sets the absolute URL to which the Service Provider will redirect the
//    //    /// User back when the Obtaining User Authorization step is completed.
//    //    /// </summary>
//    //    /// <value>
//    //    /// The callback URL; or <c>null</c> if the Consumer is unable to receive
//    //    /// callbacks or a callback URL has been established via other means.
//    //    /// </value>
//    //    [MessagePart("oauth_callback", IsRequired = true, AllowEmpty = false, MinVersion = Protocol.V10aVersion, Encoder = typeof(UriOrOobEncoding))]
//    //    public Uri Callback { get; set; }

//    //    /// <summary>
//    //    /// Gets the extra, non-OAuth parameters that will be included in the message.
//    //    /// </summary>
//    //    public new IDictionary<string, string> ExtraData
//    //    {
//    //        get { return base.ExtraData; }
//    //    }
//    //}

//    internal static class DictionaryExtensions
//    {
//        /// <summary>
//        /// Adds the value from an XDocument with the specified element name if it's not empty.
//        /// </summary>
//        /// <param name="dictionary">
//        /// The dictionary. 
//        /// </param>
//        /// <param name="document">
//        /// The document. 
//        /// </param>
//        /// <param name="elementName">
//        /// Name of the element. 
//        /// </param>
//        public static void AddDataIfNotEmpty(
//            this Dictionary<string, string> dictionary, XDocument document, string elementName)
//        {
//            var element = document.Root.Element(elementName);
//            if (element != null)
//            {
//                dictionary.AddItemIfNotEmpty(elementName, element.Value);
//            }
//        }

//        /// <summary>
//        /// Adds a key/value pair to the specified dictionary if the value is not null or empty.
//        /// </summary>
//        /// <param name="dictionary">
//        /// The dictionary. 
//        /// </param>
//        /// <param name="key">
//        /// The key. 
//        /// </param>
//        /// <param name="value">
//        /// The value. 
//        /// </param>
//        public static void AddItemIfNotEmpty(this IDictionary<string, string> dictionary, string key, string value)
//        {
//            if (key == null)
//            {
//                throw new ArgumentNullException("key");
//            }

//            if (!string.IsNullOrEmpty(value))
//            {
//                dictionary[key] = value;
//            }
//        }
//    }
//}
