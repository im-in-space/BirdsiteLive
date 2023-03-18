namespace BirdsiteLive.Common.Settings
{
    public class InstanceSettings
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        public string AdminEmail { get; set; }
        public bool ResolveMentionsInProfiles { get; set; }
        public bool PublishReplies { get; set; }
        public int MaxUsersCapacity { get; set; }

        public string UnlistedTwitterAccounts { get; set; }

        public string TwitterDomain { get; set; }

        public string InfoBanner { get; set; }

        public string TwitterDomainLabel { get; set; }

        public bool ShowAboutInstanceOnProfiles { get; set; }

        public int MaxFollowsPerUser { get; set; }

        public bool DiscloseInstanceRestrictions { get; set; }

        public string SensitiveTwitterAccounts { get; set; }

        public int FailingTwitterUserCleanUpThreshold { get; set; }

        public int MaxStatusFetchAge { get; set; }

        public bool EnableQuoteRT { get; set; }

        public int FailingFollowerCleanUpThreshold { get; set; } = -1;

        public int UserCacheCapacity { get; set; }
        public string IpWhiteListing { get; set; }
        public bool EnableXRealIpHeader { get; set; }

        public int MaxTweetRetention { get; set; }
    }
}
