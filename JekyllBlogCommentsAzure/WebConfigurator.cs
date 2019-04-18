using System.Configuration;

namespace JekyllBlogCommentsAzure
{
    public class WebConfigurator
    {
        public string CommentWebsiteUrl => ConfigurationManager.AppSettings["CommentWebsiteUrl"];

        public string GitHubToken => ConfigurationManager.AppSettings["GitHubToken"];

        public string PullRequestRepository => ConfigurationManager.AppSettings["PullRequestRepository"];

        public string CommentFallbackCommitEmail => ConfigurationManager.AppSettings["CommentFallbackCommitEmail"];
    }
}
