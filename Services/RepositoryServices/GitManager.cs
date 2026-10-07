using System.IO;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using RainbusToolbox.Models.Managers;
using Version = System.Version;


namespace RainbusToolbox.Services.RepositoryServices;

public sealed class GitManager(PersistentDataManager persistentDataManager)
{
    // TODO: review logic later, i believe none of those should be actually nullable because of initialization, but currently this is super weird so i'll see later
    public Repository? Repository { get; private set; }

    public void SetRepository(Repository repository)
    {
        Repository = repository;
    }

    public static Signature GetLocalSignature(Repository repo)
    {
        var name = repo.Config.Get<string>("user.name")?.Value;
        var email = repo.Config.Get<string>("user.email")?.Value;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Git user.name or user.email not set. Please configure Git.");

        return new Signature(name, email, DateTimeOffset.Now);
    }

    public string GetCurrentRepoDisplayName()
    {
        var remoteName = Repository?.Head?.RemoteName;
        if (string.IsNullOrWhiteSpace(remoteName)) remoteName = Repository?.Network.Remotes.FirstOrDefault()?.Name;

        if (string.IsNullOrWhiteSpace(remoteName)) return string.Empty;

        var remote = Repository?.Network.Remotes[remoteName];
        if (remote == null) return string.Empty;

        var url = remote.Url;

        if (url.Contains(':') && !url.StartsWith("http")) url = url.Split(':').Last();

        var name = Path.GetFileNameWithoutExtension(url);
        return name;
    }

    private FetchOptions CreateFetchOptions()
    {
        return new FetchOptions
        {
            CredentialsProvider = (_, _, _) =>
                new UsernamePasswordCredentials
                {
                    Username = "token",
                    Password = persistentDataManager.Settings.GitHubToken
                }
        };
    }

    private Branch? GetTrackedBranch(Branch branch, Remote remote)
    {
        if (Repository == null)
            return null;

        var tracked = branch.TrackedBranch;

        if (tracked != null)
            return tracked;

        Log.Debug("Tracked branch is null! Trying to get remote branch manually");
        tracked = Repository.Branches[$"{remote.Name}/{branch.FriendlyName}"];

        return tracked;
    }

    public (int Behind, int Ahead) CheckRepositoryChanges()
    {
        if (Repository == null)
            return (0, 0);

        var branch = Repository.Head;
        Log.Debug("Current branch: {BranchFriendlyName}", branch.FriendlyName);

        if (string.IsNullOrWhiteSpace(branch.RemoteName))
        {
            Log.Debug("No remote set for the current branch");
            return (0, 0);
        }

        var remote = Repository.Network.Remotes[branch.RemoteName];
        Log.Debug("Remote: {RemoteName}", remote.Name);

        var fetchOptions = CreateFetchOptions();

        try
        {
            Log.Debug("Fetching...");
            Repository.Network.Fetch(remote.Name, remote.FetchRefSpecs.Select(x => x.Specification), fetchOptions);
            Log.Debug("Fetch completed");
        }
        catch (Exception ex)
        {
            Log.Debug("Fetch failed: {ExMessage}", ex.Message);
            return (0, 0);
        }

        branch = Repository.Head;
        var tracked = GetTrackedBranch(branch, remote);

        Log.Debug("Local branch tip: ");
        Log.Debug("Tracked branch tip: {TipSha}", tracked.Tip.Sha);

        var divergence = Repository.ObjectDatabase.CalculateHistoryDivergence(branch.Tip, tracked.Tip);
        Log.Debug("Divergence: AheadBy {DivergenceAheadBy}, BehindBy {DivergenceBehindBy}", divergence?.AheadBy,
            divergence?.BehindBy);

        return (divergence?.BehindBy ?? 0, divergence?.AheadBy ?? 0);
    }

    public async Task SynchronizeWithOriginAsync()
    {
        if (Repository == null)
            return;

        try
        {
            if (Repository.RetrieveStatus().IsDirty)
            {
                _ = App.Current.HandleNonFatalExceptionAsync(
                    new Exception("В проекте найдены несохранённые изменения, сначала сделай коммит."));
                return;
            }

            Log.Debug("Starting synchronization with origin...");

            await Task.Yield();

            await Task.Run(() =>
            {
                FetchFromOrigin();
                Log.Debug("Fetch completed");
            });

            var divergence = await Task.Run(CheckRepositoryChanges);
            var behind = divergence.Behind;
            var ahead = divergence.Ahead;

            var didRebase = false;

            if (behind > 0)
            {
                didRebase = true;
                Log.Debug("Local branch is behind by {Behind} commit(s). Rebasing...", behind);

                var identity = new Identity(
                    Repository.Config.Get<string>("user.name")?.Value,
                    Repository.Config.Get<string>("user.email")?.Value
                );

                if (string.IsNullOrWhiteSpace(identity.Name) ||
                    string.IsNullOrWhiteSpace(identity.Email))
                {
                    _ = App.Current.HandleNonFatalExceptionAsync(
                        new Exception("Git user.name / user.email не настроены."));
                    return;
                }

                var rebaseStatus = await Task.Run(() =>
                {
                    var upstream = Repository.Branches[$"origin/{Repository.Head.FriendlyName}"];

                    var result = Repository.Rebase.Start(
                        Repository.Head,
                        upstream,
                        null,
                        identity,
                        new RebaseOptions()
                    );

                    return result.Status;
                });

                if (rebaseStatus == RebaseStatus.Conflicts)
                {
                    _ = App.Current.HandleNonFatalExceptionAsync(
                        new Exception("Обнаружены конфликты. Синхронизация остановлена.")
                    );

                    await Task.Run(() => Repository.Rebase.Abort());
                    return;
                }

                if (rebaseStatus != RebaseStatus.Complete)
                {
                    _ = App.Current.HandleNonFatalExceptionAsync(
                        new Exception($"Rebase failed: {rebaseStatus}")
                    );

                    await Task.Run(() => Repository.Rebase.Abort());
                    return;
                }

                Log.Debug("Rebase completed");
            }
            else
            {
                Log.Debug("No remote commits to rebase");
            }

            if (ahead > 0 || didRebase)
            {
                Log.Debug("Local branch is ahead by {Ahead} commit(s). Pushing...", ahead);

                await Task.Run(() => PushToOrigin(true));

                Log.Debug("Push completed");
            }
            else
            {
                Log.Debug("No local commits to push");
            }

            Log.Debug("Synchronization with origin completed successfully");
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleGlobalExceptionAsync(
                new Exception($"Synchronization failed: {ex.Message}", ex)
            );
        }
    }


    public void FetchFromOrigin()
    {
        if (Repository == null)
            return;

        var remote = Repository.Network.Remotes["origin"];
        var fetchOptions = CreateFetchOptions();
        Commands.Fetch(Repository, remote.Name, remote.FetchRefSpecs.Select(x => x.Specification), fetchOptions,
            "Fetching from origin");
    }


    public void CommitLocalChanges(string comment)
    {
        if (Repository == null)
            return;

        Commands.Stage(Repository, "*");

        if (!Repository.RetrieveStatus().Any(entry => entry.State.HasFlag(FileStatus.NewInIndex) ||
                                                      entry.State.HasFlag(FileStatus.ModifiedInIndex) ||
                                                      entry.State.HasFlag(FileStatus.DeletedFromIndex) ||
                                                      entry.State.HasFlag(FileStatus.RenamedInIndex) ||
                                                      entry.State.HasFlag(FileStatus.TypeChangeInIndex)))
            return;

        var author = GetLocalSignature(Repository);
        Repository.Commit(comment, author, author);
    }

    public void PushToOrigin(bool force = false)
    {
        if (Repository == null)
            return;

        try
        {
            var currentBranch = Repository.Head;
            if (currentBranch == null)
                throw new Exception("No HEAD is set.");

            if (currentBranch.Tip == null)
                throw new Exception("Current branch has no commits.");

            var remote = Repository.Network.Remotes["origin"];
            if (remote == null)
                throw new Exception("Remote 'origin' not found.");

            var trackingBranch = currentBranch.TrackedBranch;

            if (trackingBranch?.Tip != null)
                if (currentBranch.Tip.Sha == trackingBranch.Tip.Sha)
                {
                    Log.Debug("Already up to date with remote");
                    return;
                }

            var pushOptions = new PushOptions
            {
                CredentialsProvider = (_, _, _) =>
                    new UsernamePasswordCredentials
                    {
                        Username = "token",
                        Password = persistentDataManager.Settings.GitHubToken
                    }
            };

            var prefix = force ? "+" : "";
            var refSpec = $"{prefix}refs/heads/{currentBranch.FriendlyName}:refs/heads/{currentBranch.FriendlyName}";

            Log.Debug("Pushing branch '{FriendlyName}' to origin...", currentBranch.FriendlyName);

            Repository.Network.Push(remote, refSpec, pushOptions);

            Log.Debug("Successfully pushed branch '{FriendlyName}' to origin", currentBranch.FriendlyName);
        }
        catch (NonFastForwardException ex)
        {
            _ = App.Current.HandleGlobalExceptionAsync(
                new Exception(
                    "Push rejected (non-fast-forward). Remote contains changes you don’t have.\n" +
                    "If this happened after rebase, retry with force push enabled.",
                    ex
                )
            );
        }
        catch (LibGit2SharpException ex) when (
            ex.Message.Contains("authentication") ||
            ex.Message.Contains("401") ||
            ex.Message.Contains("403"))
        {
            _ = App.Current.HandleGlobalExceptionAsync(
                new Exception(
                    "Authentication failed during push. Check your GitHub token and permissions.",
                    ex
                )
            );
        }
        catch (LibGit2SharpException ex) when (
            ex.Message.Contains("network") ||
            ex.Message.Contains("timeout"))
        {
            _ = App.Current.HandleGlobalExceptionAsync(
                new Exception(
                    "Network error during push. Check your connection.",
                    ex
                )
            );
        }
        catch (LibGit2SharpException ex) when (
            ex.Message.Contains("permission") ||
            ex.Message.Contains("access"))
        {
            _ = App.Current.HandleGlobalExceptionAsync(
                new Exception(
                    "Permission denied during push. Check repository access rights.",
                    ex
                )
            );
        }
        catch (LibGit2SharpException ex)
        {
            _ = App.Current.HandleGlobalExceptionAsync(
                new Exception(
                    $"Git push failed: {ex.Message}",
                    ex
                )
            );
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleGlobalExceptionAsync(
                new Exception($"Unexpected error during push: {ex.Message}", ex)
            );
        }
    }

    public string GetLatestReleaseSemantic()
    {
        if (Repository == null)
            return "1.0.0";

        try
        {
            var semanticVersionPattern = @"(\d+)\.(\d+)\.(\d+)";

            // Get all tags with valid semantic versions
            var tags = Repository.Tags
                .Select(t => new
                {
                    Tag = t,
                    Match = Regex.Match(t.FriendlyName, semanticVersionPattern)
                })
                .Where(m => m.Match.Success)
                .Select(t =>
                {
                    try
                    {
                        return new
                        {
                            t.Tag,
                            Version = new Version(
                                int.Parse(t.Match.Groups[1].Value),
                                int.Parse(t.Match.Groups[2].Value),
                                int.Parse(t.Match.Groups[3].Value)
                            ),
                            VersionString = t.Match.Groups[0].Value,
                            IsValid = true
                        };
                    }
                    catch
                    {
                        Log.Debug("Skipping tag {FriendlyName} - invalid version format", t.Tag.FriendlyName);
                        return new { t.Tag, Version = new Version(), VersionString = "", IsValid = false };
                    }
                })
                .Where(t => t.IsValid)
                .OrderByDescending(v => v.Version)
                .ToList();

            if (tags.Count == 0)
            {
                Log.Debug("No tags found in repository. Defaulting to 1.0.0");
                return "1.0.0";
            }

            var latest = tags.First();
            Log.Debug("Found latest release: {FriendlyName} as {VersionString}", latest.Tag.FriendlyName,
                latest.VersionString);
            return latest.VersionString;
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to get latest release: {ExMessage}. Defaulting to 1.0.0", ex.Message);
            return "1.0.0";
        }
    }
}