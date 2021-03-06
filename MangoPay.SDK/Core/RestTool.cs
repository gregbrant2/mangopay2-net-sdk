﻿using Common.Logging;
using MangoPay.SDK.Entities;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MangoPay.SDK.Core
{
    /// <summary>Class used to build HTTP request, call the request and handle response.</summary>
    internal class RestTool
    {
        // root/parent instance that holds the OAuthToken and Configuration instance
        private MangoPayApi _root;

        // enable/disable log debugging
        //private bool _debugMode;

        // variable to flag that in request authentication data are required
        private bool _authRequired;

        // array with HTTP header to send with request
        private Dictionary<String, String> _requestHttpHeaders;

        // request type for current request
        private string _requestType;

        /// <summary>Whether to include ClientId in the API url or not</summary>
        private bool _includeClientId;

        // key-value collection pass to the request
        private Dictionary<String, String> _requestData;

        // code get from response
        private int _responseCode;

        // pagination object
        private Pagination _pagination;

        // logger object
        private ILog _log;

        /// <summary>Instantiates new RestTool object.</summary>
        /// <param name="root">Root/parent instance that holds the OAuthToken and Configuration instance.</param>
        /// <param name="authRequired">Defines whether request authentication is required.</param>
        public RestTool(MangoPayApi root, bool authRequired)
        {
            this._root = root;
            this._authRequired = authRequired;
            LogManager.Adapter = this._root.Config.LoggerFactoryAdapter;
            this._log = LogManager.GetLogger(this._root.Config.LoggerFactoryAdapter.GetType());
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        /// <summary>Adds HTTP headers as name/value pairs into the request.</summary>
        /// <param name="httpHeader">Collection of headers name/value pairs.</param>
        internal void AddRequestHttpHeader(Dictionary<String, String> httpHeader)
        {
            if (this._requestHttpHeaders == null)
                this._requestHttpHeaders = new Dictionary<String, String>();

            foreach (KeyValuePair<string, string> item in httpHeader)
            {
                this._requestHttpHeaders.Add(item.Key, item.Value);
            }
        }

        /// <summary>Adds HTTP header into the request.</summary>
        /// <param name="key">Header name.</param>
        /// <param name="value">Header value.</param>
        internal void AddRequestHttpHeader(String key, String value)
        {
            AddRequestHttpHeader(new Dictionary<String, String> { { key, value } });
        }

        /// <summary>Checks the HTTP response and if it's neither 200 nor 204 throws a ResponseException.</summary>
        /// <param name="restResponse">Rest response object</param>
        private void CheckResponseCode(IRestResponse restResponse)
        {
            var responseCode = (int)restResponse.StatusCode;

            if (responseCode != 200 && responseCode != 204)
            {
                if (responseCode == 401)
                    throw new UnauthorizedAccessException(restResponse.Content);
                else if (restResponse.ResponseStatus == ResponseStatus.TimedOut)
                    throw new TimeoutException(restResponse.ErrorMessage);
                else
                    throw new ResponseException(restResponse.Content, responseCode);
            }
        }

        /// <summary>Makes a call to the MangoPay API.
        /// This generic method handles calls targeting single 
        /// DTO instances. In order to process collections of objects, 
        /// use <code>RequestList</code> method instead.
        /// </summary>
        /// <typeparam name="U">Return type.</typeparam>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="idempotencyKey">Idempotency key for this request.</param>
        /// <param name="endPoint">An instance of <see cref="ApiEndPoint"/> that specifies API url and method to call</param>
        /// <param name="requestData">Collection of key-value pairs of request parameters.</param>
        /// <param name="pagination">Pagination object.</param>
        /// <param name="entity">Instance of DTO class that is going to be sent in case of PUTting or POSTing.</param>
        /// <returns>The DTO instance returned from API.</returns>
        public U Request<U, T>(String idempotencyKey, ApiEndPoint endPoint, Dictionary<String, String> requestData, Pagination pagination, T entity)
            where U : new()
        {
            this._requestType = endPoint.RequestType;
            this._includeClientId = endPoint.IncludeClientId;
            this._requestData = requestData;

            U responseResult = this.DoRequest<U, T>(idempotencyKey, endPoint.GetUrl(), pagination, entity);

            return responseResult;
        }

        public async Task<U> RequestAsync<U, T>(String idempotencyKey, ApiEndPoint endPoint, Dictionary<String, String> requestData, Pagination pagination, T entity)
            where U : new()
        {
            this._requestType = endPoint.RequestType;
            this._includeClientId = endPoint.IncludeClientId;
            this._requestData = requestData;

            U responseResult = await this.DoRequestAsync<U, T>(idempotencyKey, endPoint.GetUrl(), pagination, entity);

            return responseResult;
        }

        /// <summary>Makes a call to the MangoPay API.
        /// This generic method handles calls targeting single 
        /// DTO instances. In order to process collections of objects, 
        /// use <code>RequestList</code> method instead.
        /// </summary>
        /// <typeparam name="U">Return type.</typeparam>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="endPoint">An instance of <see cref="ApiEndPoint"/> that specifies API url and method to call</param>
        /// <returns>The DTO instance returned from API.</returns>
        public U Request<U, T>(ApiEndPoint endPoint)
            where U : new()
        {
            return Request<U, T>(null, endPoint, null, null, default(T));
        }

        public async Task<U> RequestAsync<U, T>(ApiEndPoint endPoint)
            where U : new()
        {
            return await RequestAsync<U, T>(null, endPoint, null, null, default(T));
        }

        /// <summary>Makes a call to the MangoPay API.
        /// This generic method handles calls targeting single 
        /// DTO instances. In order to process collections of objects, 
        /// use <code>RequestList</code> method instead.
        /// </summary>
        /// <typeparam name="U">Return type.</typeparam>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="idempotencyKey">Idempotency key for this request.</param>
        /// <param name="endPoint">An instance of <see cref="ApiEndPoint"/> that specifies API url and method to call</param>
        /// <returns>The DTO instance returned from API.</returns>
        public U Request<U, T>(String idempotencyKey, ApiEndPoint endPoint)
            where U : new()
        {
            return Request<U, T>(idempotencyKey, endPoint, null, null, default(T));
        }

        public async Task<U> RequestAsync<U, T>(String idempotencyKey, ApiEndPoint endPoint)
            where U : new()
        {
            return await RequestAsync<U, T>(idempotencyKey, endPoint, null, null, default(T));
        }

        /// <summary>Makes a call to the MangoPay API.
        /// This generic method handles calls targeting single 
        /// DTO instances. In order to process collections of objects, 
        /// use <code>RequestList</code> method instead.
        /// </summary>
        /// <typeparam name="U">Return type.</typeparam>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="endPoint">An instance of <see cref="ApiEndPoint"/> that specifies API url and method to call</param>
        /// <param name="requestData">Collection of key-value pairs of request parameters.</param>
        /// <returns>The DTO instance returned from API.</returns>
        public U Request<U, T>(ApiEndPoint endPoint, Dictionary<String, String> requestData)
            where U : new()
        {
            return Request<U, T>(null, endPoint, requestData, null, default(T));
        }

        public async Task<U> RequestAsync<U, T>(ApiEndPoint endPoint, Dictionary<String, String> requestData)
            where U : new()
        {
            return await RequestAsync<U, T>(null, endPoint, requestData, null, default(T));
        }

        /// <summary>Makes a call to the MangoPay API.
        /// This generic method handles calls targeting single 
        /// DTO instances. In order to process collections of objects, 
        /// use <code>RequestList</code> method instead.
        /// </summary>
        /// <typeparam name="U">Return type.</typeparam>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="endPoint">An instance of <see cref="ApiEndPoint"/> that specifies API url and method to call</param>
        /// <param name="requestType">HTTP request term, one of the GET, PUT or POST.</param>
        /// <param name="requestData">Collection of key-value pairs of request parameters.</param>
        /// <returns>The DTO instance returned from API.</returns>
        public U Request<U, T>(String idempotencyKey, ApiEndPoint endPoint, Dictionary<String, String> requestData)
            where U : new()
        {
            return Request<U, T>(idempotencyKey, endPoint, requestData, null, default(T));
        }

        public async Task<U> RequestAsync<U, T>(String idempotencyKey, ApiEndPoint endPoint, Dictionary<String, String> requestData)
            where U : new()
        {
            return await RequestAsync<U, T>(idempotencyKey, endPoint, requestData, null, default(T));
        }

        /// <summary>Makes a call to the MangoPay API.
        /// This generic method handles calls targeting single 
        /// DTO instances. In order to process collections of objects, 
        /// use <code>RequestList</code> method instead.
        /// </summary>
        /// <typeparam name="U">Return type.</typeparam>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="endPoint">An instance of <see cref="ApiEndPoint"/> that specifies API url and method to call</param>
        /// <param name="requestData">Collection of key-value pairs of request parameters.</param>
        /// <param name="pagination">Pagination object.</param>
        /// <returns>The DTO instance returned from API.</returns>
        public U Request<U, T>(ApiEndPoint endPoint, Dictionary<String, String> requestData, Pagination pagination)
            where U : new()
        {
            return Request<U, T>(null, endPoint, requestData, pagination, default(T));
        }

        public async Task<U> RequestAsync<U, T>(ApiEndPoint endPoint, Dictionary<String, String> requestData, Pagination pagination)
            where U : new()
        {
            return await RequestAsync<U, T>(null, endPoint, requestData, pagination, default(T));
        }

        /// <summary>Makes a call to the MangoPay API.
        /// This generic method handles calls targeting single 
        /// DTO instances. In order to process collections of objects, 
        /// use <code>RequestList</code> method instead.
        /// </summary>
        /// <typeparam name="U">Return type.</typeparam>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="idempotencyKey">Idempotency key for this request.</param>
        /// <param name="endPoint">An instance of <see cref="ApiEndPoint"/> that specifies API url and method to call</param>
        /// <param name="requestData">Collection of key-value pairs of request parameters.</param>
        /// <param name="pagination">Pagination object.</param>
        /// <returns>The DTO instance returned from API.</returns>
        public U Request<U, T>(String idempotencyKey, ApiEndPoint endPoint, Dictionary<String, String> requestData, Pagination pagination)
            where U : new()
        {
            return Request<U, T>(idempotencyKey, endPoint, requestData, pagination, default(T));
        }

        public async Task<U> RequestAsync<U, T>(String idempotencyKey, ApiEndPoint endPoint, Dictionary<String, String> requestData, Pagination pagination)
            where U : new()
        {
            return await RequestAsync<U, T>(idempotencyKey, endPoint, requestData, pagination, default(T));
        }

        /// <summary>Makes a call to the MangoPay API. 
        /// This generic method handles calls targeting collections of 
        /// DTO instances. In order to process single objects, 
        /// use <code>Request</code> method instead.
        /// </summary>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="endPoint">An instance of <see cref="ApiEndPoint"/> that specifies API url and method to call</param>
        /// <param name="requestData">Collection of key-value pairs of request parameters.</param>
        /// <param name="pagination">Pagination object.</param>
        /// <param name="additionalUrlParams"></param>
        /// <returns>Collection of DTO instances returned from API.</returns>
        public ListPaginated<T> RequestList<T>(ApiEndPoint endPoint, Dictionary<String, String> requestData, Pagination pagination, Dictionary<String, String> additionalUrlParams)
            where T : new()
        {
            this._requestType = endPoint.RequestType;
            this._includeClientId = endPoint.IncludeClientId;
            this._requestData = requestData;

            ListPaginated<T> responseResult = this.DoRequestList<T>(endPoint.GetUrl(), pagination, additionalUrlParams);

            return responseResult;
        }

        public async Task<ListPaginated<T>> RequestListAsync<T>(ApiEndPoint endPoint, Dictionary<String, String> requestData, Pagination pagination, Dictionary<String, String> additionalUrlParams)
            where T : new()
        {
            this._requestType = endPoint.RequestType;
            this._includeClientId = endPoint.IncludeClientId;
            this._requestData = requestData;

            ListPaginated<T> responseResult = await this.DoRequestListAsync<T>(endPoint.GetUrl(), pagination, additionalUrlParams);

            return responseResult;
        }

        /// <summary>Makes a call to the MangoPay API. 
        /// This generic method handles calls targeting collections of 
        /// DTO instances. In order to process single objects, 
        /// use <code>Request</code> method instead.
        /// </summary>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="endPoint">An instance of <see cref="ApiEndPoint"/> that specifies API url and method to call</param>
        /// <returns>Collection of DTO instances returned from API.</returns>
        public ListPaginated<T> RequestList<T>(ApiEndPoint endPoint)
            where T : new()
        {
            return RequestList<T>(endPoint, null, null, null);
        }

        public async Task<ListPaginated<T>> RequestListAsync<T>(ApiEndPoint endPoint)
            where T : new()
        {
            return await RequestListAsync<T>(endPoint, null, null, null);
        }

        /// <summary>Makes a call to the MangoPay API. 
        /// This generic method handles calls targeting collections of 
        /// DTO instances. In order to process single objects, 
        /// use <code>Request</code> method instead.
        /// </summary>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="endPoint">An instance of <see cref="ApiEndPoint"/> that specifies API url and method to call</param>
        /// <param name="requestData">Collection of key-value pairs of request parameters.</param>
        /// <returns>Collection of DTO instances returned from API.</returns>
        public ListPaginated<T> RequestList<T>(ApiEndPoint endPoint, Dictionary<String, String> requestData)
            where T : new()
        {
            return RequestList<T>(endPoint, requestData, null, null);
        }

        public async Task<ListPaginated<T>> RequestListAsync<T>(ApiEndPoint endPoint, Dictionary<String, String> requestData)
            where T : new()
        {
            return await RequestListAsync<T>(endPoint, requestData, null, null);
        }

        /// <summary>Makes a call to the MangoPay API. 
        /// This generic method handles calls targeting collections of 
        /// DTO instances. In order to process single objects, 
        /// use <code>Request</code> method instead.
        /// </summary>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="endPoint">An instance of <see cref="ApiEndPoint"/> that specifies API url and method to call</param>
        /// <param name="requestData">Collection of key-value pairs of request parameters.</param>
        /// <param name="pagination">Pagination object.</param>
        /// <returns>Collection of DTO instances returned from API.</returns>
        public ListPaginated<T> RequestList<T>(ApiEndPoint endPoint, Dictionary<String, String> requestData, Pagination pagination)
            where T : new()
        {
            return RequestList<T>(endPoint, requestData, pagination, null);
        }

        public async Task<ListPaginated<T>> RequestListAsync<T>(ApiEndPoint endPoint, Dictionary<String, String> requestData, Pagination pagination)
            where T : new()
        {
            return await RequestListAsync<T>(endPoint, requestData, pagination, null);
        }

        private U DoRequest<U, T>(String idempotencyKey, String urlMethod, Pagination pagination)
            where T : new()
            where U : new()
        {
            return DoRequest<U, T>(idempotencyKey, urlMethod, pagination, default(T));
        }

        private async Task<U> DoRequestAsync<U, T>(String idempotencyKey, String urlMethod, Pagination pagination)
            where T : new()
            where U : new()
        {
            return await DoRequestAsync<U, T>(idempotencyKey, urlMethod, pagination, default(T));
        }

        private U DoRequest<U, T>(String idempotencyKey, String urlMethod, Pagination pagination, T entity)
            where U : new()
        {
            U responseObject = default(U);

            UrlTool urlTool = new UrlTool(_root);
            String restUrl = urlTool.GetRestUrl(urlMethod, this._authRequired && this._includeClientId, pagination, null, _root.Config.ApiVersion);

            string fullUrl = urlTool.GetFullUrl(restUrl);
            RestClient client = new RestClient(fullUrl);

            client.AddHandler(Constants.APPLICATION_JSON, () => { return new MangoPayJsonDeserializer(); });

            _log.Debug("FullUrl: " + urlTool.GetFullUrl(restUrl));

            Method method = (Method)Enum.Parse(typeof(Method), this._requestType, false);
            RestRequest restRequest = new RestRequest(method)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new MangoPayJsonSerializer()
            };
            restRequest.JsonSerializer.ContentType = Constants.APPLICATION_JSON;

            if (_root.Config.Timeout > 0)
            {
                client.Timeout = _root.Config.Timeout;
                restRequest.Timeout = _root.Config.Timeout;
            }

            var headers = this.GetHttpHeaders(restUrl);
            foreach (KeyValuePair<string, string> h in headers)
            {
                restRequest.AddHeader(h.Key, h.Value);

                if (h.Key != Constants.AUTHORIZATION)
                    _log.Debug("HTTP Header: " + h.Key + ": " + h.Value);
            }

            if (!String.IsNullOrWhiteSpace(idempotencyKey))
                restRequest.AddHeader(Constants.IDEMPOTENCY_KEY, idempotencyKey);

            if (pagination != null)
            {
                this._pagination = pagination;
            }

            _log.Debug("RequestType: " + this._requestType);

            if (this._requestData != null || entity != null)
            {
                if (entity != null)
                {
                    restRequest.AddJsonBody(entity);
                }
                if (this._requestData != null)
                {
                    foreach (KeyValuePair<String, String> entry in this._requestData)
                    {
                        restRequest.AddParameter(entry.Key, entry.Value);
                    }
                }

                Parameter body = restRequest.Parameters.Where(p => p.Type == ParameterType.RequestBody).FirstOrDefault();
                IEnumerable<Parameter> parameters = restRequest.Parameters.Where(p => p.Type == ParameterType.GetOrPost);
                foreach (Parameter p in parameters)
                {
                    _log.Debug(p.Name + ": " + p.Value);
                }

                if (body != null)
                {
                    _log.Debug("CurrentBody: " + body.Value);
                }
                else
                {
                    _log.Debug("CurrentBody: /body is null/");
                }
            }

            IRestResponse<U> restResponse = client.Execute<U>(restRequest);
            responseObject = restResponse.Data;

            this._responseCode = (int)restResponse.StatusCode;

            if (restResponse.StatusCode == HttpStatusCode.OK || restResponse.StatusCode == HttpStatusCode.NoContent)
            {
                _log.Debug("Response OK: " + restResponse.Content);
            }
            else
            {
                _log.Debug("Response ERROR: " + restResponse.Content);
            }

            if (this._responseCode == 200)
            {
                _log.Debug("Response object: " + responseObject.ToString());
            }

            SetLastRequestInfo(restRequest, restResponse);

            this.CheckResponseCode(restResponse);

            return responseObject;
        }

        private async Task<U> DoRequestAsync<U, T>(String idempotencyKey, String urlMethod, Pagination pagination, T entity)
            where U : new()
        {
            U responseObject = default(U);

            UrlTool urlTool = new UrlTool(_root);
            String restUrl = urlTool.GetRestUrl(urlMethod, this._authRequired && this._includeClientId, pagination, null, _root.Config.ApiVersion);

            string fullUrl = urlTool.GetFullUrl(restUrl);
            RestClient client = new RestClient(fullUrl);

            client.AddHandler(Constants.APPLICATION_JSON, () => { return new MangoPayJsonDeserializer(); });

            _log.Debug("FullUrl: " + urlTool.GetFullUrl(restUrl));

            Method method = (Method)Enum.Parse(typeof(Method), this._requestType, false);
            RestRequest restRequest = new RestRequest(method)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new MangoPayJsonSerializer()
            };
            restRequest.JsonSerializer.ContentType = Constants.APPLICATION_JSON;

            if (_root.Config.Timeout > 0)
            {
                client.Timeout = _root.Config.Timeout;
                restRequest.Timeout = _root.Config.Timeout;
            }

            var headers = await this.GetHttpHeadersAsync(restUrl);
            foreach (KeyValuePair<string, string> h in headers)
            {
                restRequest.AddHeader(h.Key, h.Value);

                if (h.Key != Constants.AUTHORIZATION)
                    _log.Debug("HTTP Header: " + h.Key + ": " + h.Value);
            }

            if (!String.IsNullOrWhiteSpace(idempotencyKey))
                restRequest.AddHeader(Constants.IDEMPOTENCY_KEY, idempotencyKey);

            if (pagination != null)
            {
                this._pagination = pagination;
            }

            _log.Debug("RequestType: " + this._requestType);

            if (this._requestData != null || entity != null)
            {
                if (entity != null)
                {
                    restRequest.AddJsonBody(entity);
                }
                if (this._requestData != null)
                {
                    foreach (KeyValuePair<String, String> entry in this._requestData)
                    {
                        restRequest.AddParameter(entry.Key, entry.Value);
                    }
                }

                Parameter body = restRequest.Parameters.Where(p => p.Type == ParameterType.RequestBody).FirstOrDefault();
                IEnumerable<Parameter> parameters = restRequest.Parameters.Where(p => p.Type == ParameterType.GetOrPost);
                foreach (Parameter p in parameters)
                {
                    _log.Debug(p.Name + ": " + p.Value);
                }

                if (body != null)
                {
                    _log.Debug("CurrentBody: " + body.Value);
                }
                else
                {
                    _log.Debug("CurrentBody: /body is null/");
                }
            }

            IRestResponse<U> restResponse = await client.ExecuteAsync<U>(restRequest);
            responseObject = restResponse.Data;

            this._responseCode = (int)restResponse.StatusCode;

            if (restResponse.StatusCode == HttpStatusCode.OK || restResponse.StatusCode == HttpStatusCode.NoContent)
            {
                _log.Debug("Response OK: " + restResponse.Content);
            }
            else
            {
                _log.Debug("Response ERROR: " + restResponse.Content);
            }

            if (this._responseCode == 200)
            {
                _log.Debug("Response object: " + responseObject.ToString());
            }

            SetLastRequestInfo(restRequest, restResponse);

            this.CheckResponseCode(restResponse);

            return responseObject;
        }

        private void SetLastRequestInfo(IRestRequest request, IRestResponse response)
        {
            _root.LastRequestInfo = new LastRequestInfo() { Request = request, Response = response };

            _root.LastRequestInfo.RateLimitingCallsAllowed =
                response.Headers.FirstOrDefault(h => h.Name == "X-RateLimit-Limit")?.Value?.ToString();
            _root.LastRequestInfo.RateLimitingCallsRemaining =
                response.Headers.FirstOrDefault(h => h.Name == "X-RateLimit-Remaining")?.Value?.ToString();
            _root.LastRequestInfo.RateLimitingTimeTillReset =
                response.Headers.FirstOrDefault(h => h.Name == "X-RateLimit-Reset")?.Value?.ToString();
        }

        private ListPaginated<T> DoRequestList<T>(string urlMethod, Pagination pagination, Dictionary<String, String> additionalUrlParams)
        {
            ListPaginated<T> responseObject = null;

            UrlTool urlTool = new UrlTool(_root);
            string restUrl = urlTool.GetRestUrl(urlMethod, this._authRequired && this._includeClientId, pagination, null, _root.Config.ApiVersion);

            if (this._requestData != null)
            {
                string parameters = "";
                foreach (KeyValuePair<String, String> entry in this._requestData)
                {
                    parameters += String.Format("&{0}={1}", Uri.EscapeDataString(entry.Key), Uri.EscapeDataString(entry.Value));
                }
                if (pagination == null)
                    parameters = parameters.Remove(0, 1).Insert(0, Constants.URI_QUERY_SEPARATOR);

                restUrl += parameters;
            }

            string fullUrl = urlTool.GetFullUrl(restUrl);
            RestClient client = new RestClient(fullUrl);

            client.AddHandler(Constants.APPLICATION_JSON, () => { return new MangoPayJsonDeserializer(); });

            _log.Debug("FullUrl: " + urlTool.GetFullUrl(restUrl));

            Method method = (Method)Enum.Parse(typeof(Method), this._requestType, false);
            RestRequest restRequest = new RestRequest(method)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new MangoPayJsonSerializer()
            };
            restRequest.JsonSerializer.ContentType = Constants.APPLICATION_JSON;

            var headers = this.GetHttpHeaders(restUrl);
            foreach (KeyValuePair<string, string> h in headers)
            {
                restRequest.AddHeader(h.Key, h.Value);

                if (h.Key != Constants.AUTHORIZATION)
                    _log.Debug("HTTP Header: " + h.Key + ": " + h.Value);
            }

            if (pagination != null)
            {
                this._pagination = pagination;
            }

            _log.Debug("RequestType: " + this._requestType);

            IRestResponse<List<T>> restResponse = client.Execute<List<T>>(restRequest);
            responseObject = new ListPaginated<T>(restResponse.Data);

            this._responseCode = (int)restResponse.StatusCode;

            if (restResponse.StatusCode == HttpStatusCode.OK || restResponse.StatusCode == HttpStatusCode.NoContent)
            {
                _log.Debug("Response OK: " + restResponse.Content);
            }
            else
            {
                _log.Debug("Response ERROR: " + restResponse.Content);
            }

            if (this._responseCode == 200)
            {
                responseObject = this.ReadResponseHeaders<T>(restResponse.Headers, responseObject);

                _log.Debug("Response object: " + responseObject.ToString());
            }

            SetLastRequestInfo(restRequest, restResponse);

            this.CheckResponseCode(restResponse);

            return responseObject;
        }

        private async Task<ListPaginated<T>> DoRequestListAsync<T>(string urlMethod, Pagination pagination, Dictionary<String, String> additionalUrlParams)
        {
            ListPaginated<T> responseObject = null;

            UrlTool urlTool = new UrlTool(_root);
            string restUrl = urlTool.GetRestUrl(urlMethod, this._authRequired && this._includeClientId, pagination, null, _root.Config.ApiVersion);

            if (this._requestData != null)
            {
                string parameters = "";
                foreach (KeyValuePair<String, String> entry in this._requestData)
                {
                    parameters += String.Format("&{0}={1}", Uri.EscapeDataString(entry.Key), Uri.EscapeDataString(entry.Value));
                }
                if (pagination == null)
                    parameters = parameters.Remove(0, 1).Insert(0, Constants.URI_QUERY_SEPARATOR);

                restUrl += parameters;
            }

            string fullUrl = urlTool.GetFullUrl(restUrl);
            RestClient client = new RestClient(fullUrl);

            client.AddHandler(Constants.APPLICATION_JSON, () => { return new MangoPayJsonDeserializer(); });

            _log.Debug("FullUrl: " + urlTool.GetFullUrl(restUrl));

            Method method = (Method)Enum.Parse(typeof(Method), this._requestType, false);
            RestRequest restRequest = new RestRequest(method)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new MangoPayJsonSerializer()
            };
            restRequest.JsonSerializer.ContentType = Constants.APPLICATION_JSON;

            var headers = await this.GetHttpHeadersAsync(restUrl);
            foreach (KeyValuePair<string, string> h in headers)
            {
                restRequest.AddHeader(h.Key, h.Value);

                if (h.Key != Constants.AUTHORIZATION)
                    _log.Debug("HTTP Header: " + h.Key + ": " + h.Value);
            }

            _log.Debug("RequestType: " + this._requestType);

            var restResponse = await client.ExecuteAsync<List<T>>(restRequest);

            responseObject = new ListPaginated<T>(restResponse.Data);

            this._responseCode = (int)restResponse.StatusCode;

            if (restResponse.StatusCode == HttpStatusCode.OK || restResponse.StatusCode == HttpStatusCode.NoContent)
            {
                _log.Debug("Response OK: " + restResponse.Content);
            }
            else
            {
                _log.Debug("Response ERROR: " + restResponse.Content);
            }

            if (this._responseCode == 200)
            {
                responseObject = this.ReadResponseHeaders<T>(restResponse.Headers, responseObject);

                _log.Debug("Response object: " + responseObject.ToString());
            }

            SetLastRequestInfo(restRequest, restResponse);

            this.CheckResponseCode(restResponse);

            return responseObject;
        }

        /// <summary>Reads and parses response headers (pagination etc.)</summary>
        /// <param name="headers">The original response headers</param>
        /// <param name="listPaginated">The list</param>
        private ListPaginated<T> ReadResponseHeaders<T>(IList<Parameter> headers, ListPaginated<T> listPaginated)
        {
            headers = headers.Where(x => x.Name != null).ToList();
            foreach (var header in headers)
            {
                var value = header.Value.ToString();

                if (header.Name.ToLower().Contains(Constants.X_NUMBER_OF_PAGES.ToLower()))
                {
                    listPaginated.TotalPages = int.Parse(value);
                    continue;
                }

                if (header.Name.ToLower().Contains(Constants.X_NUMBER_OF_ITEMS.ToLower()))
                {
                    listPaginated.TotalItems = int.Parse(value);
                    continue;
                }

                if (header.Name.ToLower().Contains(Constants.LINK.ToLower()))
                {

                    var links = CustomSplit(value, ',');

                    if (links.Count <= 0) continue;

                    SetLinksForList(listPaginated, links);
                }
            }

            return listPaginated;
        }

        private List<string> CustomSplit(string input, char delim)
        {
            var list = new List<string>();
            var pos = new List<int> {0};

            for (var i = 0; i < input.Length; i++)
            {
                if (input[i] == delim)
                {
                    pos.Add(i + 1);
                }
            }

            pos.Add(input.Length + 1);

            for (var i = 1; i < pos.Count; i++)
            {
                var length = pos[i] - pos[i - 1] - 1;
                var charArray = new char[length];
                var count = 0;
                for (var j = pos[i - 1]; j < pos[i] - 1; j++)
                {
                    charArray[count++] = input[j];
                }

                list.Add(new string(charArray));
            }

            return list;
        }

        private string SubstractFromRel(string rel, char delim)
        {
            var pos = new List<int>();

            for (var i = 0; i < rel.Length; i++)
            {
                if (rel[i] == delim)
                {
                    pos.Add(i + 1);
                }
            }

            var length = pos[1] - pos[0] - 1;
            var charArr = new char[length];
            var count = 0;
            for (var i = pos[0]; i < pos[1] - 1; i++)
            {
                charArr[count++] = rel[i];
            }

            return new string(charArr);
        }

        private void SetLinksForList<T>(ListPaginated<T> listPaginated, List<string> links)
        {
            foreach (var l in links)
            {
                var oneLink = CustomSplit(l, ';');
                oneLink[1] = SubstractFromRel(oneLink[1], '"');

                if (oneLink[0] != null && oneLink[1] != null)
                {
                    if (oneLink[1] == Constants.LINKS_FIRST_ITEM) listPaginated.Links[0] = oneLink[0];
                    if (oneLink[1] == Constants.LINKS_PREVIOUS_ITEM) listPaginated.Links[1] = oneLink[0];
                    if (oneLink[1] == Constants.LINKS_NEXT_ITEM) listPaginated.Links[2] = oneLink[0];
                    if (oneLink[1] == Constants.LINKS_LAST_ITEM) listPaginated.Links[3] = oneLink[0];
                }
            }
        }

        /// <summary>Reads and parses response headers (pagination etc.)</summary>
        /// <param name="conn">Response object.</param>
        private ListPaginated<T> ReadResponseHeadersOld<T>(IRestResponse restResponse, ListPaginated<T> listPaginated = null)
        {
            foreach (Parameter k in restResponse.Headers)
            {
                String v = (string)k.Value;
                _log.Debug("Response header: " + k.Name + ":" + v);

                if (k.Name == null) continue;

                if (k.Name.Equals(Constants.X_NUMBER_OF_PAGES))
                {
                    listPaginated.TotalPages = Int32.Parse(v);
                }
                if (k.Name.Equals(Constants.X_NUMBER_OF_ITEMS))
                {
                    listPaginated.TotalItems = Int32.Parse(v);
                }
                if (k.Name.Equals(Constants.LINK))
                {
                    String linkValue = v;
                    String[] links = linkValue.Split(',');

                    if (links != null && links.Length > 0)
                    {
                        foreach (String l in links)
                        {
                            String link = l;
                            link = link.Replace("<\"", "");
                            link = link.Replace("\">", "");
                            link = link.Replace(" rel=\"", "");
                            link = link.Replace("\"", "");

                            String[] oneLink = link.Split(';');

                            if (oneLink != null && oneLink.Length > 1)
                            {
                                if (oneLink[0] != null && oneLink[1] != null)
                                {
                                    if (oneLink[1] == Constants.LINKS_FIRST_ITEM) listPaginated.Links[0] = oneLink[0];
                                    if (oneLink[1] == Constants.LINKS_PREVIOUS_ITEM) listPaginated.Links[1] = oneLink[0];
                                    if (oneLink[1] == Constants.LINKS_NEXT_ITEM) listPaginated.Links[2] = oneLink[0];
                                    if (oneLink[1] == Constants.LINKS_LAST_ITEM) listPaginated.Links[3] = oneLink[0];
                                }
                            }
                        }
                    }
                }
            }

            return listPaginated;
        }

        /// <summary>Gets HTTP header to use in request.</summary>
        /// <param name="restUrl">The REST API URL.</param>
        /// <returns>Collection of headers name-value pairs.</returns>
        private Dictionary<String, String> GetHttpHeaders(String restUrl)
        {
            // return if already created...
            if (this._requestHttpHeaders != null)
                return this._requestHttpHeaders;

            // ...or initialize with default headers
            Dictionary<String, String> httpHeaders = new Dictionary<String, String>
            {
                // content type
                { Constants.CONTENT_TYPE, Constants.APPLICATION_JSON },

                // User agent header
                { Constants.USER_AGENT, $"MangoPay V2 SDK .NET {_root.GetVersion()}" }
            };

            // AuthenticationHelper http header
            if (this._authRequired)
            {
                AuthenticationHelper authHlp = new AuthenticationHelper(_root);
                var httpHelper = authHlp.GetHttpHeaderKey();
                foreach (KeyValuePair<string, string> item in httpHelper)
                {
                    httpHeaders.Add(item.Key, item.Value);
                }
            }

            return httpHeaders;
        }

        private async Task<Dictionary<String, String>> GetHttpHeadersAsync(String restUrl)
        {
            // return if already created...
            if (this._requestHttpHeaders != null)
                return this._requestHttpHeaders;

            // ...or initialize with default headers
            Dictionary<String, String> httpHeaders = new Dictionary<String, String>
            {
                // content type
                { Constants.CONTENT_TYPE, Constants.APPLICATION_JSON },

                // User agent header
                { Constants.USER_AGENT, $"MangoPay V2 SDK .NET {_root.GetVersion()}" }
            };

            // AuthenticationHelper http header
            if (this._authRequired)
            {
                AuthenticationHelper authHlp = new AuthenticationHelper(_root);
                var httpHelper = await authHlp.GetHttpHeaderKeyAsync();
                foreach (KeyValuePair<string, string> item in httpHelper)
                {
                    httpHeaders.Add(item.Key, item.Value);
                }
            }

            return httpHeaders;
        }
    }
}
