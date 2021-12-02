using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.TeamCity
{
    public class LastBuild
    {
        public int id { get; set; }
        public string buildTypeId { get; set; }
        public string number { get; set; }
        public string status { get; set; }
        public string state { get; set; }
        public string branchName { get; set; }
        public bool defaultBranch { get; set; }
        public string href { get; set; }
        public string webUrl { get; set; }
        public string statusText { get; set; }
        public Buildtype buildType { get; set; }
        public string queuedDate { get; set; }
        public string startDate { get; set; }
        public string finishDate { get; set; }
        public Triggered triggered { get; set; }
        public Lastchanges lastChanges { get; set; }
        public Changes changes { get; set; }
        public Revisions revisions { get; set; }
        public Agent agent { get; set; }
        public Artifacts artifacts { get; set; }
        public Relatedissues relatedIssues { get; set; }
        public Statistics statistics { get; set; }
        public object[] vcsLabels { get; set; }
        public string finishOnAgentDate { get; set; }
        public Customization customization { get; set; }

        public class Buildtype
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string projectName { get; set; }
            public string projectId { get; set; }
            public string href { get; set; }
            public string webUrl { get; set; }
        }

        public class Triggered
        {
            public string type { get; set; }
            public string date { get; set; }
            public User user { get; set; }
        }

        public class User
        {
            public string username { get; set; }
            public string name { get; set; }
            public int id { get; set; }
            public string href { get; set; }
        }

        public class Lastchanges
        {
            public int count { get; set; }
            public Change[] change { get; set; }
        }

        public class Change
        {
            public int id { get; set; }
            public string version { get; set; }
            public string username { get; set; }
            public string date { get; set; }
            public string href { get; set; }
            public string webUrl { get; set; }
        }

        public class Changes
        {
            public int count { get; set; }
            public string href { get; set; }
        }

        public class Revisions
        {
            public int count { get; set; }
            public Revision[] revision { get; set; }
        }

        public class Revision
        {
            public string version { get; set; }
            public string vcsBranchName { get; set; }
            public VcsRootInstance vcsrootinstance { get; set; }
        }

        public class VcsRootInstance
        {
            public string id { get; set; }
            public string vcsrootid { get; set; }
            public string name { get; set; }
            public string href { get; set; }
        }

        public class Agent
        {
            public int id { get; set; }
            public string name { get; set; }
            public int typeId { get; set; }
            public string href { get; set; }
        }

        public class Artifacts
        {
            public string href { get; set; }
        }

        public class Relatedissues
        {
            public string href { get; set; }
        }

        public class Statistics
        {
            public string href { get; set; }
        }

        public class Customization
        {
        }


    }
}
