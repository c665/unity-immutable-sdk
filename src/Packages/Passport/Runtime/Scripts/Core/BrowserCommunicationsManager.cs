using System.Net;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using VoltstroStudios.UnityWebBrowser.Core;
using Immutable.Passport.Model;
using UnityEngine;
using Newtonsoft.Json;

namespace Immutable.Passport.Core {
    public class BrowserCommunicationsManager 
    {
        private const string TAG = "[Browser Communications Manager]";
        private IDictionary<string, TaskCompletionSource<string>> requestTaskMap = new Dictionary<string, TaskCompletionSource<string>>();
        private IWebBrowserClient webBrowserClient;

        public BrowserCommunicationsManager(IWebBrowserClient webBrowserClient) {
            this.webBrowserClient = webBrowserClient;
            this.webBrowserClient.OnUnityPostMessage += OnUnityPostMessage;
        }

        #region Unity to Browser

        public Task<string> Call(string fxName, string? data = null) {
            var t = new TaskCompletionSource<string>();
            string requestId = Guid.NewGuid().ToString();
            // Add task completion source to the map so we can return the response
            requestTaskMap.Add(requestId, t);
            CallFunction(requestId, fxName, data);
            return t.Task;
        }

        private void CallFunction(string requestId, string fxName, string? data = null) {
            Debug.Log($"{TAG} Call {fxName} (request ID: {requestId})");

            Request request = new Request(fxName, requestId, data);
            string requestJson = JsonConvert.SerializeObject(request).Replace("\\", "\\\\").Replace("\"", "\\\"");

            // Call the function on the JS side
            string js = @$"callFunction(""{requestJson}"")";
            webBrowserClient.ExecuteJs(js);
        }

        #endregion

        #region Browser to Unity

        private void OnUnityPostMessage(string message) {
            Debug.Log($"[Unity] OnUnityPostMessage: {message}");
            HandleResponse(message);
        }

        private void HandleResponse(string message) {
            Response? response = JsonUtility.FromJson<Response>(message);

            // Check if the reponse returned is valid and the task to return the reponse exists
            if (response == null || response.responseFor == null || response.requestId == null)
                return;

            string requestId = response.requestId;
            PassportException? exception = ParseError(response);

            if (requestTaskMap.ContainsKey(requestId)) {
                NotifyRequestResult(requestId, message, exception);
            } else {
                throw new PassportException($"No TaskCompletionSource for request id {requestId} found.");
            }
        }

        private PassportException? ParseError(Response response) {
            if (response.success == false || !String.IsNullOrEmpty(response.error)) {
                // Failed or error occured
                try {
                    if (response.errorType != null) {
                        PassportErrorType type = (PassportErrorType) System.Enum.Parse(typeof(PassportErrorType), response.errorType);
                        return new PassportException(response.error, type);
                    }
                } catch (Exception ex) {
                    Debug.Log($"{TAG} Parse passport type error: {ex.Message}");
                }
                return new PassportException(response.error);
            } else {
                // No error
                return null;
            }
        }
        
        private void NotifyRequestResult(string requestId, string result, PassportException? e)
        {
            TaskCompletionSource<string>? completion = requestTaskMap[requestId] as TaskCompletionSource<string>;
            try {
                if (e != null) {
                    if (!completion.TrySetException(e))
                        throw new PassportException($"Unable to set exception for for request id {requestId}. Task has already been completed.");
                } else {
                    if(!completion.TrySetResult(result))
                        throw new PassportException($"Unable to set result for for request id {requestId}. Task has already been completed.");
                }
            } catch (ObjectDisposedException exception) {
                throw new PassportException($"Task for request id {requestId} has already been disposed and can't be updated.");
            }

            requestTaskMap.Remove(requestId);
        }

        #endregion

    }
}