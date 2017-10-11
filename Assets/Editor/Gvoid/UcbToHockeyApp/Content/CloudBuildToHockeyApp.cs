using System;
using System.IO;
using Editor.Gvoid.Configurations;
using Editor.Gvoid.UcbToHockeyApp.Content.HockeyApp;
using Editor.Gvoid.UcbToHockeyApp.Content.UnityCloudBuildApi;
using UnityEngine;

namespace Editor.Gvoid.UcbToHockeyApp.Content
{
    public class CloudBuildToHockeyApp : CloudBuildToHockeyAppUi
    {
        public static void RunTransfer(string buildTargetId)
        {
            Running = true;
            ProgressDescription = "Resolving latest artifact location.";
            Progress = 0;

            var getLatestArtifact = new CloudBuildApi_GetLatestArtifactUrl();
            getLatestArtifact.OnSuccess += HandleGotLatestArtifactUrl;
            getLatestArtifact.OnError += HandleError;
            getLatestArtifact.GetLatestBuildUrl(buildTargetId);
        }

        public static void GetLatestVersion(string buildTargetId)
        {
            Running = true;
            ProgressDescription = "Resolving latest build number.";
            Progress = 0;

            var getLatestArtifact = new CloudBuildApi_GetLatestArtifactUrl();
            getLatestArtifact.OnSuccessBuildNumber += HandleGotLatestBuildNumber;
            getLatestArtifact.OnError += HandleError;
            getLatestArtifact.GetLatestBuildUrl(buildTargetId);
        }

        private static void HandleGotLatestBuildNumber(string artifacturl)
        {
            //LastIosVersionSeen = CloudBuildToHockeyAppUi.UploadingVersion;
            LastAndroidVersionUploaded = CloudBuildToHockeyAppUi.UploadingVersion;
            Running = false;
        }

        private static void HandleError(string message)
        {
            Debug.Log("CloudBuildToHockeyApp: " + message);
            Window.Close();
        }

        private static void HandleGotLatestArtifactUrl(string artifacturl)
        {
            StepsCompleted = 0;

            string fileName = GetArtifactFileName(artifacturl);
            ProgressDescription = "Downloading artifact - " + fileName;

            var downloadArtifact = new CloudBuildApi_DownloadArtifact();
            downloadArtifact.OnSuccess += HandleDownloadArtifactComplete;
            downloadArtifact.OnProgress += HandleDownloadProgress;
            downloadArtifact.OnError += HandleError;
            
            string tempfile = artifacturl.Contains(".ipa") ? Application.temporaryCachePath+tempIPAfile : Application.temporaryCachePath+tempAPKfile;

            downloadArtifact.StartAsyncDownload(artifacturl, tempfile);
        }

        private static string GetArtifactFileName(string artifacturl)
        {
            Uri uri = new Uri(artifacturl);
            string fileName = Path.GetFileName(uri.LocalPath);
            return fileName;
        }

        private static void HandleDownloadProgress(float percent)
        {
            float complete = StepsCompleted;
            float done = TotalStepsInProcess;
            Progress = (complete/done) + (percent/done);
        }

        public static void HandleDownloadArtifactComplete(string artifacturl)
        {
            StepsCompleted = 1;

            Debug.Log("Artifact downloaded " + artifacturl);

            StartUploading(artifacturl);
        }

        public static void StartUploading(string artifacturl)
        {
            try
            {
                ProgressDescription = "Uploading to HockeyApp.";
                var uploadArtifact = new HockeyAppApi_UploadArtifact();
                uploadArtifact.OnSuccess += HandleUploadSuccess;
                uploadArtifact.OnProgress += HandleDownloadProgress;
                uploadArtifact.OnError += HandleError;

                var appUploadVo = GetHockeyAppUploadVo();

                UploadingIos = artifacturl.Contains(".ipa");

                var hockeyAppUploadVo = new HockeyAppUploadVo();
                hockeyAppUploadVo.hockeyappAppID = UploadingIos ? HockeyAppInfo.HockeyappAppID_iOS : HockeyAppInfo.HockeyappAppID_Android;
                hockeyAppUploadVo.artifactFilePath = artifacturl;
                hockeyAppUploadVo.status = appUploadVo.status;
                hockeyAppUploadVo.notes = appUploadVo.notes;
                hockeyAppUploadVo.note_type = appUploadVo.note_type;
                hockeyAppUploadVo.notify = appUploadVo.notify;
                //hockeyAppUploadVo.dsymFilePath;
                hockeyAppUploadVo.artifactFilePath = artifacturl;

                uploadArtifact.StartUpload(hockeyAppUploadVo);
            }
            catch (Exception e)
            {
                Debug.LogError("There was a problem trying to upload.");
                Debug.LogError(e);
                Running = false;
            }
        }

        private static void HandleUploadSuccess()
        {
            StepsCompleted = 2;
            Running = false;

            if (UploadingIos)
            {
                LastIosVersionUploaded = UploadingVersion;
            }
            else
            {
                LastAndroidVersionUploaded = UploadingVersion;
            }
        }
    }
}