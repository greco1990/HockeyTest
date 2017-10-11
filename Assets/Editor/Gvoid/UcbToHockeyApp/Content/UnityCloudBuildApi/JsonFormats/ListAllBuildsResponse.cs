using System.Collections.Generic;

namespace Editor.Gvoid.UcbToHockeyApp.Content.UnityCloudBuildApi.JsonFormats
{
    [System.Serializable]
    public class Author
    {
        public string fullName;
        public string absoluteUrl;
    }

    [System.Serializable]
    public class Changeset
    {
        public string commitId;
        public string message;
        public string timestamp;
        public string _id;
        public Author author;
        public int numAffectedFiles;
    }

    [System.Serializable]
    public class ProjectVersion
    {
        public string name;
        public string filename;
        public string projectName;
        public string platform;
        public int size;
        public string created;
        public string lastMod;
        public string bundleId;
        public List<object> udids;
    }

    [System.Serializable]
    public class Self
    {
        public string method;
        public string href;
    }

    [System.Serializable]
    public class Log
    {
        public string method;
        public string href;
    }

    [System.Serializable]
    public class Auditlog
    {
        public string method;
        public string href;
    }

    [System.Serializable]
    public class Meta
    {
        public string type;
    }

    [System.Serializable]
    public class DownloadPrimary
    {
        public string method;
        public string href;
        public Meta meta;
    }

    [System.Serializable]
    public class CreateShare
    {
        public string method;
        public string href;
    }

    [System.Serializable]
    public class RevokeShare
    {
        public string method;
        public string href;
    }

    [System.Serializable]
    public class Icon
    {
        public string method;
        public string href;
    }

    [System.Serializable]
    public class File
    {
        public string filename;
        public int size;
        public string href;
    }

    [System.Serializable]
    public class Artifact
    {
        public string key;
        public string name;
        public bool primary;
        public bool show_download;
        public List<File> files;
    }

    [System.Serializable]
    public class Links
    {
        public Self self;
        public Log log;
        public Auditlog auditlog;
        public DownloadPrimary download_primary;
        public CreateShare create_share;
        public RevokeShare revoke_share;
        public Icon icon;
        public List<Artifact> artifacts;
    }

    [System.Serializable]
    public class ListAllBuildsResponse_RootObject
    {
        public int build;
        public string buildtargetid;
        public string buildTargetName;
        public string buildStatus;
        public string platform;
        public int workspaceSize;
        public string created;
        public string finished;
        public string checkoutStartTime;
        public int checkoutTimeInSeconds;
        public string buildStartTime;
        public double buildTimeInSeconds;
        public string publishStartTime;
        public double publishTimeInSeconds;
        public double totalTimeInSeconds;
        public string lastBuiltRevision;
        public List<Changeset> changeset;
        public bool favorited;
        public bool deleted;
        public string cooldownDate;
        public string scmBranch;
        public string unityVersion;
        public int auditChanges;
        public ProjectVersion projectVersion;
        public string projectName;
        public string projectId;
        public string orgId;
        public Links links;
    }
    
}