using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.Remoting.Messaging;
using Editor.Gvoid.UcbToHockeyApp.Content.UnityCloudBuildApi.Remote;
using UnityEditor;
using UnityEngine;

namespace Editor.Gvoid.UcbToHockeyApp.Content.UnityCloudBuildApi
{
    public class CloudBuildApi_DownloadArtifact
    {
        public static int ProgressPercentage;
        private bool _downloadFileComplete;
        private bool _doFinalCall;
        private string _resultLocation;
        private string _downloadFileError;
        private WebClient _client;

        public delegate void ErrorAction(string message);

        public event ErrorAction OnError;

        public delegate void ProgressAction(float precent);

        public event ProgressAction OnProgress;

        public delegate void SuccessAction(string artifactUrl);

        public event SuccessAction OnSuccess;

        private delegate string AsyncDownloadDelegator(string url, string temp);


        public void StartAsyncDownload(string artifacturl, string targetfile)
        {
            ServicePointManager.ServerCertificateValidationCallback =
                RemoteCertification.MyRemoteCertificateValidationCallback;

            AsyncDownloadDelegator myDelegate = AsyncDownload;
            myDelegate.BeginInvoke(artifacturl, targetfile, DownloadResponse, null);

            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
        }

        private void EditorUpdate()
        {
            SafeProgressUpdate(ProgressPercentage);

            if (_doFinalCall)
            {
                EditorApplication.update -= EditorUpdate;
                if (_downloadFileError == null)
                {
                    SafeSuccess(_resultLocation);
                }
                else
                {
                    SafeError(_downloadFileError);
                }
            }
        }

        public void DownloadResponse(IAsyncResult result)
        {
            AsyncResult res = (AsyncResult) result;
            AsyncDownloadDelegator myDelegate = (AsyncDownloadDelegator) res.AsyncDelegate;
            _resultLocation = myDelegate.EndInvoke(result);
            _doFinalCall = true;
        }

        public string AsyncDownload(string url, string temp)
        {
            _downloadFileComplete = false;
            _downloadFileError = null;
            _resultLocation = null;
            _doFinalCall = false;

            _client = new WebClient();
            _client.DownloadFileCompleted += DownloadFileCompleted;
            _client.DownloadProgressChanged += DownloadFileProgress;
            _client.DownloadFileAsync(new Uri(url), temp);

            while (!_downloadFileComplete)
            {
                System.Threading.Thread.Sleep(1000);
            }

            return temp;
        }

        private void DownloadFileProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressPercentage = e.ProgressPercentage;
        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs asynEvent)
        {
            if (asynEvent.Error == null)
            {
                //Debug.Log("Download Success");
            }
            else if (string.IsNullOrEmpty(asynEvent.Error.Message))
            {
                //Debug.Log("Download Completed with error");
            }
            else
            {
                Debug.Log("Error downloading " + asynEvent.Error);
                _downloadFileError = asynEvent.Error.Message;
            }

            _downloadFileComplete = true;
        }

        //
        private void SafeError(string message)
        {
            if (OnError != null)
            {
                OnError.Invoke(message);
            }
        }

        private void SafeSuccess(string location)
        {
            if (OnSuccess != null)
            {
                OnSuccess.Invoke(location);
            }
        }

        private void SafeProgressUpdate(int percent)
        {
            if (OnProgress != null)
            {
                OnProgress.Invoke(percent*0.01f);
            }
        }
    }
}