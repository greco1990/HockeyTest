using System;
using System.Collections.Generic;
using System.IO;
using Editor.Gvoid.Configurations;
using UnityEditor;
using UnityEngine;
using File = System.IO.File;

namespace Editor.Gvoid.UcbToHockeyApp.Content.HockeyApp
{
    public class HockeyAppApi_UploadArtifact
    {
        public delegate void ErrorAction(string message);
        public event ErrorAction OnError;
        public delegate void ProgressAction(float percent);
        public event ProgressAction OnProgress;
        public delegate void SuccessAction();
        public event SuccessAction OnSuccess;

        private WWW _request;

        public void StartUpload(HockeyAppUploadVo hockeyAppUploadVo)
        {
            string artifactFilePath = Path.GetFullPath(hockeyAppUploadVo.artifactFilePath);
            string url = "https://rink.hockeyapp.net/api/2/apps/" + hockeyAppUploadVo.hockeyappAppID + "/app_versions/upload";

            WWWForm form = new WWWForm();
            form.AddField("status", hockeyAppUploadVo.status);
            form.AddField("notify", hockeyAppUploadVo.notify);
            form.AddField("notes", hockeyAppUploadVo.notes);
            form.AddField("note_type", hockeyAppUploadVo.note_type);
            //form.AddField("strategy", hockeyAppUploadVo.strategy);
            
            //
            byte[] fileData = File.ReadAllBytes(artifactFilePath);
            string fileExtension = Path.GetExtension(artifactFilePath).ToLower();

            if (fileExtension == ".apk")
            {
                form.AddBinaryData("ipa", fileData, Path.GetFileName(artifactFilePath), "application/vnd.android.package-archive");
            }
            else if (fileExtension == ".ipa")
            {
                form.AddBinaryData("ipa", fileData, Path.GetFileName(artifactFilePath), "application/octet-stream");
                if (!string.IsNullOrEmpty(hockeyAppUploadVo.dsymFilePath))
                {
                    form.AddBinaryData("dsym", fileData, Path.GetFileName(hockeyAppUploadVo.dsymFilePath),
                        "application/octet-stream");
                }
            }

            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> header in form.headers)
            {
                headers.Add(header.Key, header.Value.Replace("\"", ""));
            }
            headers.Add("X-HockeyAppToken", HockeyAppInfo.HockeyappApiKey);
            
            _request = new WWW(url, form.data, headers);

            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
        }

        public void EditorUpdate()
        {
            if (_request == null)
            {
                Debug.LogError("_request not initialized.");
                EditorApplication.update -= EditorUpdate;
                return;
            }

            while (!_request.isDone)
            {
                SafeProgress(_request.uploadProgress);
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(_request.error))
                {
                    Debug.Log(_request.error);
                    SafeError(_request.error);

                    var responseHeader = "";
                    foreach (var key in _request.responseHeaders.Keys)
                    {
                        responseHeader += string.Format("\t{0} = {1}\n", key, _request.responseHeaders[key]);
                    }
                    Debug.LogError(responseHeader);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                SafeError(e.Message);
            }

            EditorApplication.update -= EditorUpdate;
            SafeSuccess();
        }

        

        //
        private void SafeError(string message)
        {
            if (OnError != null)
            {
                OnError.Invoke(message);
            }
        }

        private void SafeProgress(float progress)
        {
            if (OnProgress != null)
            {
                OnProgress.Invoke(progress);
            }
        }

        private void SafeSuccess()
        {
            if (OnSuccess != null)
            {
                OnSuccess.Invoke();
            }
        }
    }
}