using LibGit2Sharp;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Diagnostics;

namespace SemanticKernelPlayground.Plugins
{
    public class GitCommand
    {
        [KernelFunction("get_git_commits")]
        [Description("Gets a list of commits from repository")]
        public async Task<string> GetCommits(string repositoryPath)
        {
            if (string.IsNullOrWhiteSpace(repositoryPath))
            {
                throw new ArgumentNullException("The repository path cannot be null or empty", nameof(repositoryPath));
            }

            using (var repo = new Repository(repositoryPath))
            {
                var commits = repo.Commits.Select(commit =>
                    $"{commit.Author.When}: {commit.MessageShort} by {commit.Author.Name}"
                ).ToList();

                return string.Join("\n", commits);
            }
        }

        [KernelFunction("create_git_tag_version")]
        [Description("create a tag version to git commit")]
        public async Task<string> CreateTag(string repositoryPath, string tagName)
        {
            if (string.IsNullOrWhiteSpace(repositoryPath))
            {
                throw new ArgumentNullException("The repository path cannot be null or empty", nameof(repositoryPath));
            }

            if (string.IsNullOrWhiteSpace(tagName))
            {
                throw new ArgumentNullException("The repository path cannot be null or empty", nameof(repositoryPath));
            }

            using (var repo = new Repository(repositoryPath))
            {
                var signature = new Signature("ColdPay", "test@test.com", DateTimeOffset.Now);

                var newTag = repo.Tags.Add(tagName, repo.Head.Tip, signature, $"Realease {tagName}");

                return newTag.FriendlyName;
            }
        }

    }
}
