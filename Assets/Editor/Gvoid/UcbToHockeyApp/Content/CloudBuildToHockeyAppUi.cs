using Editor.Gvoid.Configurations;
using Editor.Gvoid.UcbToHockeyApp.Content.HockeyApp;
using UnityEditor;
using UnityEngine;

namespace Editor.Gvoid.UcbToHockeyApp.Content
{
    public class CloudBuildToHockeyAppUi : EditorWindow
    {
        public static string tempAPKfile = "/UCBArtifact.apk";
        public static string tempIPAfile = "/UCBArtifact.ipa";

        protected static EditorWindow Window;
        protected static int TotalStepsInProcess = 2;
        protected static int StepsCompleted;
        protected static float Progress;

        protected static string ProgressBarTitle = "Cloud -> Hockeyapp";
        protected static string ProgressDescription = "Unity Cloud to HockeyApp Progress.";

        
        public static bool Running;



        // ---------
        [MenuItem("Window/UnityCloudToHockeyApp")]
        private static void UnityCloudToHockeyApp()
        {
            SetupWindow();
        }

        public static void UploadAPk()
        {
            CloudBuildToHockeyApp.StartUploading(tempAPKfile);
        }

        public static void UploadIpa()
        {
            CloudBuildToHockeyApp.StartUploading(tempIPAfile);
        }

        protected static void SetupWindow()
        {
            Window = GetWindow(typeof(CloudBuildToHockeyApp));
            Window.title = "Cloud Build to Hockeyapp";
            Window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Unity Cloud to Hockeyapp Status", EditorStyles.boldLabel);
            GUILayout.Label("Last iOS uploaded: " + LastIosVersionUploaded);
            GUILayout.Label("Last Android uploaded: " + LastAndroidVersionUploaded);
//            GUILayout.Label("iOS version on UCB: " + LastIosVersionSeen);
//            GUILayout.Label("Android version on UCB: " + LastAndroidVersionSeen);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Label("Unity Cloud Build Settings", EditorStyles.boldLabel);
            CloudBuildInfo.CloudBuildApiKey = EditorGUILayout.TextField("CloudBuildApiKey", CloudBuildInfo.CloudBuildApiKey);
            CloudBuildInfo.OrganizationId = EditorGUILayout.TextField("OrganizationId", CloudBuildInfo.OrganizationId.ToLower());
            CloudBuildInfo.ProjectId = EditorGUILayout.TextField("ProjectId", CloudBuildInfo.ProjectId.ToLower());
            CloudBuildInfo.iOSBuildTargetId = EditorGUILayout.TextField("iOS Build Id", CloudBuildInfo.iOSBuildTargetId.ToLower());
            CloudBuildInfo.AndroidBuildTargetId = EditorGUILayout.TextField("Android Build Id", CloudBuildInfo.AndroidBuildTargetId.ToLower());

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Label("Hockeyapp Settings", EditorStyles.boldLabel);
            HockeyAppInfo.HockeyappAppID_Android = EditorGUILayout.TextField("AppID Android", HockeyAppInfo.HockeyappAppID_Android);
            HockeyAppInfo.HockeyappAppID_iOS = EditorGUILayout.TextField("AppID iOS", HockeyAppInfo.HockeyappAppID_iOS);
            HockeyAppInfo.HockeyappApiKey = EditorGUILayout.TextField("HockeyappApiKey", HockeyAppInfo.HockeyappApiKey);
            status = (HockeyAppStatus)EditorGUILayout.EnumPopup("Status", status);
            //HockeyAppStrategyMethod = (HockeyAppStrategy)EditorGUILayout.EnumPopup("Strategy", HockeyAppStrategyMethod);
            HockeyAppNotifyMethod = (HockeyAppNotify)EditorGUILayout.EnumPopup("NotifyMethod", HockeyAppNotifyMethod);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Label("Release notes", EditorStyles.boldLabel);
            AutoReleaseNotes = EditorGUILayout.Toggle("Automatic release notes", AutoReleaseNotes);
            EditorGUI.BeginDisabledGroup(AutoReleaseNotes);
            if (!AutoReleaseNotes)
            {
                ReleaseNotes = EditorGUILayout.TextField("Release Notes", ReleaseNotes, GUILayout.Width(Window.position.width * 0.99f), GUILayout.Height(100));
                note_type = (HockeyAppNoteType) EditorGUILayout.EnumPopup("note_type", note_type);
            }
            EditorGUI.EndDisabledGroup();

//            EditorGUILayout.Space();
//            EditorGUILayout.Space();
//            if (!Running)
//            {
//                if (GUILayout.Button("Sync", GUILayout.Width(Window.position.width * 0.99f)))
//                {
//                    CloudBuildToHockeyApp.GetLatestVersion(CloudBuildInfo.iOSBuildTargetId);
//                }
//            }
//            else
//            {
//                GUI.enabled = false;
//                if (GUILayout.Button("Sync", GUILayout.Width(Window.position.width * 0.99f)))
//                {
//                }
//            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (!Running)
            {
                if (GUILayout.Button("Transfer iOS", GUILayout.Width(Window.position.width * 0.99f)))
                {
                    CloudBuildToHockeyApp.RunTransfer(CloudBuildInfo.iOSBuildTargetId);
                }
            }
            else
            {
                GUI.enabled = false;
                if (GUILayout.Button("Transfer iOS", GUILayout.Width(Window.position.width * 0.99f)))
                {
                }
            }

            if (!Running)
            {
                if (GUILayout.Button("Transfer Android", GUILayout.Width(Window.position.width*0.99f)))
                {
                    CloudBuildToHockeyApp.RunTransfer(CloudBuildInfo.AndroidBuildTargetId);
                }
            }
            else
            {
                GUI.enabled = false;
                if (GUILayout.Button("Transfer Android", GUILayout.Width(Window.position.width * 0.99f)))
                {
                }
            }

            if (Running)
            {
                EditorUtility.DisplayProgressBar(ProgressBarTitle, ProgressDescription, Progress);
            }
            else
            {
                EditorUtility.ClearProgressBar();
            }
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        protected static HockeyAppUploadVo GetHockeyAppUploadVo()
        {
            var appUploadVo = new HockeyAppUploadVo();

            appUploadVo.status = ((int)status).ToString();
            appUploadVo.notes = ReleaseNotes;
            appUploadVo.note_type = ((int)note_type).ToString();
            appUploadVo.notify = ((int)HockeyAppNotifyMethod).ToString();
            //appUploadVo.strategy = ((int)HockeyAppStrategyMethod).ToString();

            return appUploadVo;
        }


        // Hockeyapp config
        private enum HockeyAppStatus
        {
            DoNotAllowDownloadOrInstall = 1,
            AllowDownloadOrInstall = 2,
        }

        private enum HockeyAppNoteType
        {
            Textile = 0,
            Markdown = 1,
        }

        private enum HockeyAppNotify
        {
            DoNotNotifyTesters = 0,
            NotifyAllTestersThatCanInstallTheApp = 1,
            NotifyAllTesters = 2,
        }

        private enum HockeyAppStrategy
        {
            add = 0,
            replace = 1,
        }

        public static bool AutoReleaseNotes = true;
        private static HockeyAppStatus status = HockeyAppStatus.AllowDownloadOrInstall;
        private static HockeyAppNoteType note_type = HockeyAppNoteType.Textile;
        private static HockeyAppNotify HockeyAppNotifyMethod = HockeyAppNotify.DoNotNotifyTesters;
        private static HockeyAppStrategy HockeyAppStrategyMethod = HockeyAppStrategy.replace;

        public static string ReleaseNotes = "No message";

        public static string UploadingVersion;
        protected static bool UploadingIos;
        public static string LastIosVersionUploaded = "No message";
        public static string LastAndroidVersionUploaded = "No message";
//        public static string LastIosVersionSeen = "No message";
//        public static string LastAndroidVersionSeen = "No message";
    }
}