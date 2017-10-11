using System;
using System.Text.RegularExpressions;
using Editor.Gvoid.Configurations;
using Editor.Gvoid.UcbToHockeyApp.Content.UnityCloudBuildApi.JsonFormats;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Editor.Gvoid.UcbToHockeyApp.Content.UnityCloudBuildApi
{
    public class CloudBuildApi_GetLatestArtifactUrl
    {
        public delegate void ErrorAction(string message);

        public event ErrorAction OnError;

        public delegate void SuccessAction(string artifactUrl);

        public event SuccessAction OnSuccess;
        public event SuccessAction OnSuccessBuildNumber;

        private UnityWebRequest _request;

        public void GetLatestBuildUrl(string buildtargetid)
        {
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;

            var listAllBuildsUrl =
                " https://build-api.cloud.unity3d.com/api/v1/" +
                "orgs/" + CloudBuildInfo.OrganizationId +
                "/projects/" + CloudBuildInfo.ProjectId +
                "/buildtargets/" + buildtargetid +
                "/builds" +
                "?" +
                "buildStatus=success" +
                "&" +
                "per_page=" + 1;

            _request = UnityWebRequest.Get(listAllBuildsUrl);
            _request.SetRequestHeader("Content-Type", "application/json");
            _request.SetRequestHeader("Authorization", "Basic " + CloudBuildInfo.CloudBuildApiKey);
            _request.Send();
        }

        public void EditorUpdate()
        {
            while (!_request.isDone)
            {
                return;
            }

            try
            {
                if (_request.isError)
                {
                    Debug.Log(_request.error);
                    SafeError(_request.error);
                }
                else
                {
                    //Debug.Log(request.downloadHandler.text);
                    GetFirstArtifactLink(_request.downloadHandler.text);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                SafeError(e.Message);
            }

            EditorApplication.update -= EditorUpdate;
        }

        private void GetFirstArtifactLink(string json)
        {
            var trimmedJson = RemoveFirstEnclosingSquareBrakets(json);
            var response = JsonUtility.FromJson<ListAllBuildsResponse_RootObject>(trimmedJson);

            var firstArtifact = response.links.artifacts;
            PopulateReleaseNotes(response);
            CloudBuildToHockeyAppUi.UploadingVersion = response.build.ToString();

            if (firstArtifact.Count <= 0)
            {
                Debug.Log("No artifacts from build found.");
            }

            var latestSuccesfullBuildUrl = firstArtifact[0].files[0].href;
            SafeSuccess(latestSuccesfullBuildUrl);
        }

        public static string RemoveFirstEnclosingSquareBrakets(string json)
        {
            string pattern = @"^(\[){1}(.*?)(\]){1}$";
            return Regex.Replace(json, pattern, "$2");
        }

        private void PopulateReleaseNotes(ListAllBuildsResponse_RootObject response)
        {
            var changelog = "";
            foreach (var change in response.changeset)
            {
                changelog += change.message + "\n";
            }

            if (!CloudBuildToHockeyAppUi.AutoReleaseNotes)
            {
                CloudBuildToHockeyAppUi.ReleaseNotes = changelog;
            }
        }

        //
        private void SafeError(string message)
        {
            if (OnError != null)
            {
                OnError.Invoke(message);
            }
        }

        private void SafeSuccess(string url)
        {
            if (OnSuccess != null)
            {
                OnSuccess.Invoke(url);
            }
        }
    }
}