using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using Backend.Controllers.Responses;
using Backend.Infrastructure;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

/// <summary>
/// The Analytic Service is responsible for handling all the logic for the analytic endpoints.
/// </summary>
public class AnalyticService : IAnalyticService
{
    private readonly AppDbContext _db;

    /// <summary>
    /// The constructor for the Analytic Service.
    /// </summary>
    /// <param name="db">AppDbContext to be injected.</param>
    public AnalyticService(AppDbContext db)
    {
        _db = db;
    }

    #region Audience Age

    /// <summary>
    /// Get the average age of the audience for a podcast or episode.
    /// </summary>
    /// <param name="podcastOrEpisodeId">The ID of the podcast or episode.</param>
    /// <param name="user">The user making the request.</param>
    /// <returns>The average age of the audience.</returns>
    /// <exception cref="Exception">Thrown when the podcast or episode does not exist or the user is not the owner.</exception>
    /// <exception cref="Exception">Thrown when there is no audience data available for the given podcast or episode.</exception>
    public async Task<uint> GetAverageAudienceAgeAsync(Guid podcastOrEpisodeId, User user)
    {
        // Check if the podcast exists and the user is the owner
        Podcast? podcast = await _db.Podcasts
        .FirstOrDefaultAsync(p => p.Id == podcastOrEpisodeId && p.PodcasterId == user.Id);

        double avgAge = -1;

        // Get the average age of the audience for the podcast
        if (podcast is not null)
        {
            // Check if there are any interactions for the podcast
            if (await _db.UserEpisodeInteractions.Include(uei => uei.Episode).AnyAsync(uei => uei.Episode.PodcastId == podcast.Id) == false)
                throw new Exception("No audience data available for the given podcast.");

            // Get the average age of the audience for the podcast
            avgAge = await _db.UserEpisodeInteractions
                .Include(uei => uei.User)
                .Include(uei => uei.Episode)
                .Where(uei => uei.Episode.PodcastId == podcast.Id)
                .Select(uei => DateTime.Now.Year - uei.User.DateOfBirth.Year)
                .AverageAsync();
        }
        else
        {
            // Check if the episode exists and the user is the owner
            Episode episode = await _db.Episodes
            .FirstOrDefaultAsync(e => e.Id == podcastOrEpisodeId && e.Podcast.PodcasterId == user.Id) ?? throw new Exception("Podcast or Episode does not exist for the given ID.");

            // Check if there are any interactions for the episode
            if (await _db.UserEpisodeInteractions.AnyAsync(uei => uei.EpisodeId == episode.Id) == false)
                throw new Exception("No audience data available for the given episode.");

            // Get the average age of the audience for the episode
            avgAge = await _db.UserEpisodeInteractions
                .Include(uei => uei.User)
                .Where(uei => uei.EpisodeId == episode.Id)
                .Select(uei => DateTime.Now.Year - uei.User.DateOfBirth.Year)
                .AverageAsync();
        }

        // Return the average age
        return (uint)avgAge;
    }

    /// <summary>
    /// Get the age range of the audience for a podcast or episode.
    /// </summary>
    /// <param name="podcastOrEpisodeId">The ID of the podcast or episode.</param>
    /// <param name="min">The minimum age of the audience.</param>
    /// <param name="max">The maximum age of the audience.</param>
    /// <param name="user">The user making the request.</param>
    /// <returns>The age range of the audience.</returns>
    /// <exception cref="Exception">Thrown when the minimum age is greater than the maximum age.</exception>
    /// <exception cref="Exception">Thrown when the podcast or episode does not exist or the user is not the owner.</exception>
    /// <exception cref="Exception">Thrown when there is no audience data available for the given podcast or episode.</exception>
    public async Task<AgeRangeResponse> GetAgeRangeInfoAsync(Guid podcastOrEpisodeId, uint min, uint max, User user)
    {
        // Check if the minimum age is greater than the maximum age
        if (min > max)
            throw new Exception("Minimum age cannot be greater than maximum age.");

        // Check if the podcast exists and the user is the owner
        Podcast? podcast = await _db.Podcasts
        .FirstOrDefaultAsync(p => p.Id == podcastOrEpisodeId && p.PodcasterId == user.Id);

        AgeRangeResponse ageRangeResponse;
        int totalInteractionsCount = 0;

        // Get the average age of the audience for the podcast
        if (podcast is not null)
        {
            // Get the total interactions count for the podcast
            totalInteractionsCount = await _db.UserEpisodeInteractions
                .Include(uei => uei.User)
                .Include(uei => uei.Episode)
                .Where(uei => uei.Episode.PodcastId == podcast.Id)
                .CountAsync();

            // Check if there are any interactions for the podcast
            if (totalInteractionsCount == 0)
                throw new Exception("No audience data available for the given podcast.");

            // Get the age range of the audience for the podcast
            ageRangeResponse = await _db.UserEpisodeInteractions
                .Include(uei => uei.User)
                .Include(uei => uei.Episode)
                .Where(uei => uei.Episode.PodcastId == podcast.Id && ((DateTime.Now.Year - uei.User.DateOfBirth.Year) >= min) && ((DateTime.Now.Year - uei.User.DateOfBirth.Year) <= max))
                .ToListAsync()
                .ContinueWith(t => new AgeRangeResponse(t.Result, (uint)totalInteractionsCount));
        }
        else
        {
            // Check if the episode exists and the user is the owner
            Episode episode = await _db.Episodes
            .FirstOrDefaultAsync(e => e.Id == podcastOrEpisodeId && e.Podcast.PodcasterId == user.Id) ?? throw new Exception("Podcast or Episode does not exist for the given ID.");

            // Get the total interactions count for the episode
            totalInteractionsCount = await _db.UserEpisodeInteractions
                .Include(uei => uei.User)
                .Where(uei => uei.EpisodeId == episode.Id)
                .CountAsync();

            // Check if there are any interactions for the episode
            if (totalInteractionsCount == 0)
                throw new Exception("No audience data available for the given episode.");

            // Get the age range of the audience for the episode
            ageRangeResponse = await _db.UserEpisodeInteractions
                .Include(uei => uei.User)
                .Where(uei => uei.EpisodeId == episode.Id && ((DateTime.Now.Year - uei.User.DateOfBirth.Year) >= min) && ((DateTime.Now.Year - uei.User.DateOfBirth.Year) <= max))
                .ToListAsync()
                .ContinueWith(t => new AgeRangeResponse(t.Result, (uint)totalInteractionsCount));
        }

        return ageRangeResponse;
    }

    /// <summary>
    /// Get the age range distribution of the audience for a podcast or episode.
    /// </summary>
    /// <param name="podcastOrEpisodeId"> The ID of the podcast or episode.</param>
    /// <param name="ageInterval">The interval for the age range.</param>
    /// <param name="user">The user making the request.</param>
    /// <returns>The age range distribution of the audience.</returns>
    /// <exception cref="Exception">Thrown when the age interval is 0.</exception>
    /// <exception cref="Exception">Thrown when the podcast or episode does not exist or the user is not the owner.</exception>
    /// <exception cref="Exception">Thrown when there is no audience data available for the given podcast or episode.</exception>
    public async Task<List<AgeRangeResponse>> GetAgeRangeDistributionInfoAsync(Guid podcastOrEpisodeId, uint ageInterval, User user)
    {
        // Check if the age interval is 0
        if (ageInterval == 0)
            throw new Exception("Age interval cannot be 0.");

        // Check if the podcast exists and the user is the owner
        Podcast? podcast = await _db.Podcasts
        .FirstOrDefaultAsync(p => p.Id == podcastOrEpisodeId && p.PodcasterId == user.Id);

        List<AgeRangeResponse> ageRangeResponses;
        int totalInteractionsCount = 0;

        if (podcast is not null)
        {
            // Get the total interactions count for the podcast
            totalInteractionsCount = await _db.UserEpisodeInteractions
                .Include(uei => uei.User)
                .Include(uei => uei.Episode)
                .Where(uei => uei.Episode.PodcastId == podcast.Id)
                .CountAsync();

            // Check if there are any interactions for the podcast
            if (totalInteractionsCount == 0)
                throw new Exception("No audience data available for the given podcast.");

            // Get the age range distribution of the audience for the podcast
            ageRangeResponses = await _db.UserEpisodeInteractions
                .Include(uei => uei.User)
                .Include(uei => uei.Episode)
                .Where(uei => uei.Episode.PodcastId == podcast.Id)
                .GroupBy(uei => (DateTime.Now.Year - uei.User.DateOfBirth.Year - ((DateTime.Now.Year - uei.User.DateOfBirth.Year) % ageInterval)) / ageInterval)
                .OrderByDescending(g => g.Count())
                .OrderBy(g => g.Key)
                .Select(g => new AgeRangeResponse(g.ToList(), (uint)totalInteractionsCount))
                .ToListAsync();
        }
        else
        {
            // Check if the episode exists and the user is the owner
            Episode episode = await _db.Episodes
            .FirstOrDefaultAsync(e => e.Id == podcastOrEpisodeId && e.Podcast.PodcasterId == user.Id) ?? throw new Exception("Podcast or Episode does not exist for the given ID.");

            // Get the total interactions count for the episode
            totalInteractionsCount = await _db.UserEpisodeInteractions
                .Include(uei => uei.User)
                .Where(uei => uei.EpisodeId == episode.Id)
                .CountAsync();

            // Check if there are any interactions for the episode
            if (totalInteractionsCount == 0)
                throw new Exception("No audience data available for the given episode.");

            // Get the age range distribution of the audience for the episode
            ageRangeResponses = await _db.UserEpisodeInteractions
                .Include(uei => uei.User)
                .Where(uei => uei.EpisodeId == episode.Id)
                .GroupBy(uei => (DateTime.Now.Year - uei.User.DateOfBirth.Year - ((DateTime.Now.Year - uei.User.DateOfBirth.Year) % ageInterval)) / ageInterval)
                .OrderByDescending(g => g.Count())
                .OrderBy(g => g.Key)
                .Select(g => new AgeRangeResponse(g.ToList(), (uint)totalInteractionsCount))
                .ToListAsync();
        }

        return ageRangeResponses;
    }

    #endregion Audience Age

    #region Watch Time

    /// <summary>
    /// Get the average watch time of the audience for a podcast or episode.
    /// </summary>
    /// <param name="podcastOrEpisodeId">The ID of the podcast or episode.</param>
    /// <param name="user">The user making the request.</param>
    /// <returns>The average watch time of the audience.</returns>
    /// <exception cref="Exception">Thrown when the podcast or episode does not exist or the user is not the owner.</exception>
    /// <exception cref="Exception">Thrown when there is no audience data available for the given podcast or episode.</exception>
    /// <exception cref="Exception">Thrown when the total watch time is 0.</exception>
    /// <exception cref="Exception">Thrown when the total clicks is 0.</exception>
    /// <exception cref="Exception">Thrown when the episode does not exist for the given ID.</exception>
    public async Task<TimeSpan> GetAverageWatchTimeAsync(Guid podcastOrEpisodeId, User user)
    {
        // Check if the podcast exists and the user is the owner
        Podcast? podcast = await _db.Podcasts
        .FirstOrDefaultAsync(p => p.Id == podcastOrEpisodeId && p.PodcasterId == user.Id);

        // Initialize the average watch time
        TimeSpan avgWatchTime = TimeSpan.Zero;

        // Get the average watch time of the audience for the podcast
        if (podcast is not null)
        {
            // Check if there are any interactions for the podcast
            if (await _db.UserEpisodeInteractions.Include(uei => uei.Episode).AnyAsync(uei => uei.Episode.PodcastId == podcast.Id) == false)
                throw new Exception("No audience data available for the given podcast.");

            // Get the total watch time of the audience for the podcast
            TimeSpan totalWatchTime = TimeSpan.FromSeconds(await _db.UserEpisodeInteractions
                .Include(uei => uei.Episode)
                .Where(uei => uei.Episode.PodcastId == podcast.Id)
                .Select(uei => uei.TotalListenTime.TotalSeconds)
                .SumAsync());

            // Get the total amount of clicks for the podcast
            int totalClicks = await _db.UserEpisodeInteractions
                .Where(uei => uei.Episode.PodcastId == podcast.Id)
                .Select(uei => uei.Clicks)
                .SumAsync();

            // Get the average watch time of the audience for the podcast
            avgWatchTime = TimeSpan.FromSeconds((double)totalWatchTime.TotalSeconds / totalClicks);
        }
        else
        {
            // Check if the episode exists and the user is the owner
            Episode episode = await _db.Episodes
            .FirstOrDefaultAsync(e => e.Id == podcastOrEpisodeId && e.Podcast.PodcasterId == user.Id) ?? throw new Exception("Podcast or Episode does not exist for the given ID.");

            // Check if there are any interactions for the episode
            if (await _db.UserEpisodeInteractions.AnyAsync(uei => uei.EpisodeId == episode.Id) == false)
                throw new Exception("No audience data available for the given episode.");

            // Get the total watch time of the audience for the episode
            TimeSpan totalWatchTime = TimeSpan.FromSeconds(await _db.UserEpisodeInteractions
                .Where(uei => uei.EpisodeId == episode.Id)
                .Select(uei => uei.TotalListenTime.Seconds)
                .SumAsync());

            // Get the total amount of clicks for the episode
            int totalClicks = await _db.UserEpisodeInteractions
                .Where(uei => uei.EpisodeId == episode.Id)
                .Select(uei => uei.Clicks)
                .SumAsync();

            // Get the average watch time of the audience for the episode
            avgWatchTime = totalWatchTime / totalClicks;
        }

        return avgWatchTime;
    }

    /// <summary>
    /// Get the total watch time of the audience for a podcast or episode.
    /// </summary>
    /// <param name="podcastOrEpisodeId">The ID of the podcast or episode.</param>
    /// <param name="user">The user making the request.</param>
    /// <returns>The total watch time of the audience.</returns>
    /// <exception cref="Exception">Thrown when the podcast or episode does not exist or the user is not the owner.</exception>
    /// <exception cref="Exception">Thrown when there is no audience data available for the given podcast or episode.</exception>
    /// <exception cref="Exception">Thrown when the episode does not exist for the given ID.</exception>
    /// <exception cref="Exception">Thrown when the total watch time is 0.</exception>
    public async Task<TimeSpan> GetTotalWatchTimeAsync(Guid podcastOrEpisodeId, User user)
    {
        // Check if the podcast exists and the user is the owner
        Podcast? podcast = await _db.Podcasts
            .FirstOrDefaultAsync(p => p.Id == podcastOrEpisodeId && p.PodcasterId == user.Id);

        TimeSpan totalWatchTime = TimeSpan.Zero;

        // Get the total watch time of the audience for the podcast
        if (podcast is not null)
        {
            // Check if there are any interactions for the podcast
            if (await _db.UserEpisodeInteractions.Include(uei => uei.Episode).AnyAsync(uei => uei.Episode.PodcastId == podcast.Id) == false)
                throw new Exception("No audience data available for the given podcast.");

            // Get the total watch time of the audience for the podcast
            totalWatchTime = TimeSpan.FromSeconds(await _db.UserEpisodeInteractions
                .Include(uei => uei.Episode)
                .Where(uei => uei.Episode.PodcastId == podcast.Id)
                .Select(uei => uei.TotalListenTime.TotalSeconds)
                .SumAsync());
        }
        else
        {
            // Check if the episode exists and the user is the owner
            Episode episode = await _db.Episodes
                .FirstOrDefaultAsync(e => e.Id == podcastOrEpisodeId && e.Podcast.PodcasterId == user.Id) ?? throw new Exception("Podcast or Episode does not exist for the given ID.");

            // Check if there are any interactions for the episode
            if (await _db.UserEpisodeInteractions.AnyAsync(uei => uei.EpisodeId == episode.Id) == false)
                throw new Exception("No audience data available for the given episode.");

            // Get the total watch time of the audience for the episode
            totalWatchTime = TimeSpan.FromSeconds(await _db.UserEpisodeInteractions
                .Where(uei => uei.EpisodeId == episode.Id)
                .Select(uei => uei.TotalListenTime.TotalSeconds)
                .SumAsync());
        }

        return totalWatchTime;
    }

    /// <summary>
    /// Get the watch time range of the audience for a podcast or episode.
    /// </summary>
    /// <param name="podcastOrEpisodeId">The ID of the podcast or episode.</param>
    /// <param name="user">The user making the request.</param>
    /// <param name="minTime">The minimum watch time.</param>
    /// <param name="maxTime">The maximum watch time.</param>
    /// <returns>The watch time range of the audience.</returns>
    /// <exception cref="Exception">Thrown when the minimum time is greater than the maximum time.</exception>
    /// <exception cref="Exception">Thrown when the podcast or episode does not exist or the user is not the owner.</exception>
    /// <exception cref="Exception">Thrown when there is no audience data available for the given podcast or episode.</exception>
    public async Task<WatchTimeRangeResponse> GetWatchTimeRangeInfoAsync(Guid podcastOrEpisodeId, User user, TimeSpan minTime, TimeSpan maxTime)
    {
        // Check if the min time is greater than the max time
        if (minTime > maxTime)
            throw new Exception("Minimum time cannot be greater than maximum time.");

        // Check if the podcast exists and the user is the owner
        Podcast? podcast = await _db.Podcasts
            .FirstOrDefaultAsync(p => p.Id == podcastOrEpisodeId && p.PodcasterId == user.Id);

        WatchTimeRangeResponse watchTimeRangeResponse;
        int totalClicks = 0;
        TimeSpan totalWatchTime = TimeSpan.Zero;

        // Get the total watch time of the audience for the podcast
        if (podcast is not null)
        {
            // Check if there are any interactions for the podcast
            if (await _db.UserEpisodeInteractions.Include(uei => uei.Episode).AnyAsync(uei => uei.Episode.PodcastId == podcast.Id) == false)
                throw new Exception("No audience data available for the given podcast.");

            // Get the total watch time of the audience for the podcast
            totalWatchTime = TimeSpan.FromSeconds(await _db.UserEpisodeInteractions
                .Include(uei => uei.Episode)
                .Where(uei => uei.Episode.PodcastId == podcast.Id)
                .Select(uei => uei.TotalListenTime.TotalSeconds)
                .SumAsync());

            // Get the total amount of clicks for the podcast
            totalClicks = await _db.UserEpisodeInteractions
                .Include(uei => uei.Episode)
                .Where(uei => uei.Episode.PodcastId == podcast.Id)
                .Select(uei => uei.Clicks)
                .SumAsync();

            // Get the watch time range of the audience for the podcast
            watchTimeRangeResponse = await _db.UserEpisodeInteractions
                .Include(uei => uei.Episode)
                .Where(uei => uei.Episode.PodcastId == podcast.Id && (uei.TotalListenTime >= minTime) && (uei.TotalListenTime <= maxTime))
                .ToListAsync()
                .ContinueWith(t => new WatchTimeRangeResponse(t.Result, totalClicks, totalWatchTime));
        }
        else
        {
            // Check if the episode exists and the user is the owner
            Episode episode = await _db.Episodes
                .FirstOrDefaultAsync(e => e.Id == podcastOrEpisodeId && e.Podcast.PodcasterId == user.Id) ?? throw new Exception("Podcast or Episode does not exist for the given ID.");

            // Check if there are any interactions for the episode
            if (await _db.UserEpisodeInteractions.AnyAsync(uei => uei.EpisodeId == episode.Id) == false)
                throw new Exception("No audience data available for the given episode.");

            // Get the total watch time of the audience for the episode
            totalWatchTime = TimeSpan.FromSeconds(await _db.UserEpisodeInteractions
                .Where(uei => uei.EpisodeId == episode.Id)
                .Select(uei => uei.TotalListenTime.TotalSeconds)
                .SumAsync());

            // Get the total amount of clicks for the episode
            totalClicks = await _db.UserEpisodeInteractions
                .Where(uei => uei.EpisodeId == episode.Id)
                .Select(uei => uei.Clicks)
                .SumAsync();

            // Get the watch time range of the audience for the episode
            watchTimeRangeResponse = await _db.UserEpisodeInteractions
                .Where(uei => uei.EpisodeId == episode.Id && (uei.TotalListenTime >= minTime) && (uei.TotalListenTime <= maxTime))
                .ToListAsync()
                .ContinueWith(t => new WatchTimeRangeResponse(t.Result, totalClicks, totalWatchTime));
        }

        // Return the watch time range
        return watchTimeRangeResponse;
    }

    /// <summary>
    /// Get the watch time distribution of the audience for a podcast or episode.
    /// </summary>
    /// <param name="podcastOrEpisodeId">The ID of the podcast or episode.</param>
    /// <param name="user">The user making the request.</param>
    /// <param name="timeInterval">The time interval.</param>
    /// <param name="intervalIsInMinutes">Whether the time interval is in minutes.</param>
    /// <returns>The watch time distribution of the audience.</returns>
    /// <exception cref="Exception">Thrown when the time interval is 0.</exception>
    /// <exception cref="Exception">Thrown when the podcast or episode does not exist or the user is not the owner.</exception>
    /// <exception cref="Exception">Thrown when there is no audience data available for the given podcast or episode.</exception>
    /// <exception cref="Exception">Thrown when the episode does not exist for the given ID.</exception>
    public async Task<List<WatchTimeRangeResponse>> GetWatchTimeDistributionInfoAsync(Guid podcastOrEpisodeId, User user, uint timeInterval = 1, bool intervalIsInMinutes = true)
    {
        // Check that the time interval is not 0
        if (timeInterval == 0)
            throw new Exception("Time interval cannot be 0.");

        if (intervalIsInMinutes)
            timeInterval *= 60;

        // Check if the podcast exists and the user is the owner
        Podcast? podcast = await _db.Podcasts
            .FirstOrDefaultAsync(p => p.Id == podcastOrEpisodeId && p.PodcasterId == user.Id);

        List<WatchTimeRangeResponse> watchTimeRangeResponses;
        int totalClicks = 0;
        TimeSpan totalWatchTime = TimeSpan.Zero;

        if (podcast is not null)
        {
            // Check if there are any interactions for the podcast
            if (await _db.UserEpisodeInteractions.Include(uei => uei.Episode).AnyAsync(uei => uei.Episode.PodcastId == podcast.Id) == false)
                throw new Exception("No audience data available for the given podcast.");

            // Get the total watch time of the audience for the podcast
            totalWatchTime = TimeSpan.FromSeconds(await _db.UserEpisodeInteractions
                .Include(uei => uei.Episode)
                .Where(uei => uei.Episode.PodcastId == podcast.Id)
                .Select(uei => uei.TotalListenTime.TotalSeconds)
                .SumAsync());

            // Get the total amount of clicks for the podcast
            totalClicks = await _db.UserEpisodeInteractions
                .Include(uei => uei.Episode)
                .Where(uei => uei.Episode.PodcastId == podcast.Id)
                .Select(uei => uei.Clicks)
                .SumAsync();

            // Get the watch time range of the audience for the podcast
            watchTimeRangeResponses = await _db.UserEpisodeInteractions
                .Include(uei => uei.Episode)
                .Where(uei => uei.Episode.PodcastId == podcast.Id)
                .GroupBy(uei => (uei.TotalListenTime.TotalSeconds - (uei.TotalListenTime.TotalSeconds % timeInterval)) / timeInterval)
                .OrderByDescending(g => g.Count())
                .OrderBy(g => g.Key)
                .Select(g => new WatchTimeRangeResponse(g.ToList(), totalClicks, totalWatchTime))
                .ToListAsync();
        }
        else
        {
            // Check if the episode exists and the user is the owner
            Episode episode = await _db.Episodes
                .FirstOrDefaultAsync(e => e.Id == podcastOrEpisodeId && e.Podcast.PodcasterId == user.Id) ?? throw new Exception("Podcast or Episode does not exist for the given ID.");

            // Check if there are any interactions for the episode
            if (await _db.UserEpisodeInteractions.AnyAsync(uei => uei.EpisodeId == episode.Id) == false)
                throw new Exception("No audience data available for the given episode.");

            // Get the total watch time of the audience for the episode
            totalWatchTime = TimeSpan.FromSeconds(await _db.UserEpisodeInteractions
                .Where(uei => uei.EpisodeId == episode.Id)
                .Select(uei => uei.TotalListenTime.TotalSeconds)
                .SumAsync());

            // Get the total amount of clicks for the episode
            totalClicks = await _db.UserEpisodeInteractions
                .Where(uei => uei.EpisodeId == episode.Id)
                .Select(uei => uei.Clicks)
                .SumAsync();

            // Get the watch time range of the audience for the episode
            watchTimeRangeResponses = await _db.UserEpisodeInteractions
                .Where(uei => uei.EpisodeId == episode.Id)
                .GroupBy(uei => (uei.TotalListenTime.TotalSeconds - (uei.TotalListenTime.TotalSeconds % timeInterval)) / timeInterval)
                .OrderByDescending(g => g.Count())
                .OrderBy(g => g.Key)
                .Select(g => new WatchTimeRangeResponse(g.ToList(), totalClicks, totalWatchTime))
                .ToListAsync();
        }

        return watchTimeRangeResponses;
    }

    #endregion Watch Time

    #region User Engagement Metrics

    /// <summary>
    /// Get the user engagement metrics for a podcast or episode.
    /// </summary>
    /// <param name="podcastOrEpisodeId">The ID of the podcast or episode.</param>
    /// <param name="user">The user making the request.</param>
    /// <returns>The user engagement metrics for the podcast or episode.</returns>
    public async Task<UserEngagementMetricsResponse> GetUserEngagementMetricsAsync(Guid podcastOrEpisodeId, User user)
    {
        // Check if the podcast exists and the user is the owner
        Podcast? podcast = await _db.Podcasts
            .FirstOrDefaultAsync(p => p.Id == podcastOrEpisodeId && p.PodcasterId == user.Id);

        UserEngagementMetricsResponse userEngagementMetricsResponse;
        List<UserEpisodeInteraction> interactions;
        int commentsCount = 0;
        int likesCount = 0;

        if (podcast is not null)
        {
            // Get the number of comments for the podcast
            commentsCount = await _db.Comments
                .Include(c => c.Episode)
                .Where(c => c.Episode.PodcastId == podcast.Id)
                .CountAsync();

            // Get the number of likes for the podcast
            likesCount = await _db.EpisodeLikes
                .Include(l => l.Episode)
                .Where(l => l.Episode.PodcastId == podcast.Id)
                .CountAsync();

            // Get all the interactions for the podcast
            interactions = await _db.UserEpisodeInteractions
                .Include(uei => uei.Episode)
                .Where(uei => uei.Episode.PodcastId == podcast.Id)
                .ToListAsync();

            // Set the user engagement metrics response
            userEngagementMetricsResponse = interactions.Count == 0 ?
                new UserEngagementMetricsResponse { TotalLikes = likesCount, TotalComments = commentsCount } :
                new UserEngagementMetricsResponse(interactions, commentsCount, likesCount);
        }
        else
        {
            // Check if the episode exists and the user is the owner
            Episode episode = await _db.Episodes
                .FirstOrDefaultAsync(e => e.Id == podcastOrEpisodeId && e.Podcast.PodcasterId == user.Id) ?? throw new Exception("Podcast or Episode does not exist for the given ID.");

            // Get the number of comments for the episode
            commentsCount = await _db.Comments
                .Where(c => c.EpisodeId == episode.Id)
                .CountAsync();

            // Get the number of likes for the episode
            likesCount = await _db.EpisodeLikes
                .Where(l => l.EpisodeId == episode.Id)
                .CountAsync();

            // Get all the interactions for the episode
            interactions = await _db.UserEpisodeInteractions
                .Where(uei => uei.EpisodeId == episode.Id)
                .ToListAsync();

            // Set the user engagement metrics response
            userEngagementMetricsResponse = interactions.Count == 0 ?
                new UserEngagementMetricsResponse { TotalLikes = likesCount, TotalComments = commentsCount }
                : new UserEngagementMetricsResponse(interactions, commentsCount, likesCount);
        }

        return userEngagementMetricsResponse;
    }

    /// <summary>
    /// Get the top commented podcasts for a user.
    /// </summary>
    /// <param name="count">The number of podcasts to get.</param>
    /// <param name="getLessCommented">Whether to get the less commented podcasts.</param>
    /// <param name="user">The user making the request.</param>
    /// <param name="domainUrl">The domain URL.</param>
    /// <returns>The top commented podcasts for the user.</returns>
    /// <exception cref="Exception">Thrown when the count is less than or equal to 0.</exception>
    public async Task<List<PodcastResponse>> GetTopCommentedPodcastsAsync(int count, bool getLessCommented, User user, string domainUrl)
    {
        if (count <= 0)
            throw new Exception("Count cannot be less than or equal to 0.");

        // Return the top commented podcasts for the user
        return getLessCommented ?
            await _db.Podcasts
            .Include(p => p.Episodes).ThenInclude(e => e.Comments)
            .Where(p => p.PodcasterId == user.Id)
            .OrderBy(p => p.Episodes.Sum(e => e.Comments.Count))
            .Take(count)
            .Select(p => new PodcastResponse(p, domainUrl))
            .ToListAsync()
            :
            await _db.Podcasts
            .Include(p => p.Episodes).ThenInclude(e => e.Comments)
            .Where(p => p.PodcasterId == user.Id)
            .OrderByDescending(p => p.Episodes.Sum(e => e.Comments.Count))
            .Take(count)
            .Select(p => new PodcastResponse(p, domainUrl))
            .ToListAsync();
    }

    /// <summary>
    /// Get the top commented episodes for a podcast.
    /// </summary>
    /// <param name="podcastId">The ID of the podcast.</param>
    /// <param name="count">The number of episodes to get.</param>
    /// <param name="getLessCommented">Whether to get the less commented episodes.</param>
    /// <param name="user">The user making the request.</param>
    /// <param name="domainUrl">The domain URL.</param>
    /// <returns>The top commented episodes for the podcast.</returns>
    /// <exception cref="Exception">Thrown when the count is less than or equal to 0.</exception>
    public async Task<List<EpisodeResponse>> GetTopCommentedEpisodesAsync(Guid podcastId, int count, bool getLessCommented, User user, string domainUrl)
    {
        if (count <= 0)
            throw new Exception("Count cannot be less than or equal to 0.");

        if(! await _db.Podcasts.AnyAsync(p => p.Id == podcastId && p.PodcasterId == user.Id))
            throw new Exception("Podcast does not exist for the given ID.");

        return getLessCommented ?
            await _db.Episodes
            .Include(e => e.Comments)
            .Include(e => e.Podcast)
            .Where(e => e.PodcastId == podcastId && e.Podcast.PodcasterId == user.Id)
            .OrderBy(e => e.Comments.Count)
            .Take(count)
            .Select(e => new EpisodeResponse(e, domainUrl,false))
            .ToListAsync()
            :
            await _db.Episodes
            .Include(e => e.Comments)
            .Include(e => e.Podcast)
            .Where(e => e.PodcastId == podcastId && e.Podcast.PodcasterId == user.Id)
            .OrderByDescending(e => e.Comments.Count)
            .Take(count)
            .Select(e => new EpisodeResponse(e, domainUrl,false))
            .ToListAsync();
    }

    /// <summary>
    /// Get the top liked podcasts for a user.
    /// </summary>
    /// <param name="count">The number of podcasts to get.</param>
    /// <param name="getLessLiked">Whether to get the less liked podcasts.</param>
    /// <param name="user">The user making the request.</param>
    /// <param name="domainUrl">The domain URL.</param>
    /// <returns>The top liked podcasts for the user.</returns>
    /// <exception cref="Exception">Thrown when the count is less than or equal to 0.</exception>
    public async Task<List<PodcastResponse>> GetTopLikedPodcastsAsync(int count, bool getLessLiked, User user, string domainUrl)
    {
        if (count <= 0)
            throw new Exception("Count cannot be less than or equal to 0.");

        return getLessLiked ? 
            await _db.Podcasts
            .Include(p => p.Episodes).ThenInclude(e => e.Likes)
            .Where(p => p.PodcasterId == user.Id)
            .OrderBy(p => p.Episodes.Sum(e => e.Likes.Count))
            .Take(count)
            .Select(p => new PodcastResponse(p, domainUrl))
            .ToListAsync()
            :
            await _db.Podcasts
            .Include(p => p.Episodes).ThenInclude(e => e.Likes)
            .Where(p => p.PodcasterId == user.Id)
            .OrderByDescending(p => p.Episodes.Sum(e => e.Likes.Count))
            .Take(count)
            .Select(p => new PodcastResponse(p, domainUrl))
            .ToListAsync();
    }

    /// <summary>
    /// Get the top liked episodes for a podcast.
    /// </summary>
    /// <param name="podcastId">The ID of the podcast.</param>
    /// <param name="count">The number of episodes to get.</param>
    /// <param name="getLessLiked">Whether to get the less liked episodes.</param>
    /// <param name="user">The user making the request.</param>
    /// <param name="domainUrl">The domain URL.</param>
    /// <returns>The top liked episodes for the podcast.</returns>
    /// <exception cref="Exception">Thrown when the count is less than or equal to 0.</exception>
    public async Task<List<EpisodeResponse>> GetTopLikedEpisodesAsync(Guid podcastId, int count, bool getLessLiked, User user, string domainUrl)
    {
        if (count <= 0)
            throw new Exception("Count cannot be less than or equal to 0.");

        if (!await _db.Podcasts.AnyAsync(p => p.Id == podcastId && p.PodcasterId == user.Id))
            throw new Exception("Podcast does not exist for the given ID.");

        return getLessLiked ?
            await _db.Episodes
            .Include(e => e.Likes)
            .Include(e => e.Podcast)
            .Where(e => e.PodcastId == podcastId && e.Podcast.PodcasterId == user.Id)
            .OrderBy(e => e.Likes.Count)
            .Take(count)
            .Select(e => new EpisodeResponse(e, domainUrl,false))
            .ToListAsync()
            :
            await _db.Episodes
            .Include(e => e.Likes)
            .Include(e => e.Podcast)
            .Where(e => e.PodcastId == podcastId && e.Podcast.PodcasterId == user.Id)
            .OrderByDescending(e => e.Likes.Count)
            .Take(count)
            .Select(e => new EpisodeResponse(e, domainUrl,false))
            .ToListAsync();
    }

    /// <summary>
    /// Get the top clicked podcasts for a user.
    /// </summary>
    /// <param name="count">The number of podcasts to get.</param>
    /// <param name="getLessClicked">Whether to get the less clicked podcasts.</param>
    /// <param name="user">The user making the request.</param>
    /// <param name="domainUrl">The domain URL.</param>
    /// <returns>The top clicked podcasts for the user.</returns>
    /// <exception cref="Exception">Thrown when the count is less than or equal to 0.</exception>
    public Task<List<PodcastResponse>> GetTopClickedPodcastsAsync(int count, bool getLessClicked, User user, string domainUrl)
    {
        if (count <= 0)
            throw new Exception("Count cannot be less than or equal to 0.");

        return getLessClicked ?
            _db.Podcasts
            .Include(p => p.Episodes).ThenInclude(e => e.UserEpisodeInteractions)
            .Where(p => p.PodcasterId == user.Id)
            .OrderBy(p => p.Episodes.Sum(e => e.UserEpisodeInteractions.Sum(uei => uei.Clicks)))
            .Take(count)
            .Select(p => new PodcastResponse(p, domainUrl))
            .ToListAsync()
            :
            _db.Podcasts
            .Include(p => p.Episodes).ThenInclude(e => e.UserEpisodeInteractions)
            .Where(p => p.PodcasterId == user.Id)
            .OrderByDescending(p => p.Episodes.Sum(e => e.UserEpisodeInteractions.Sum(uei => uei.Clicks)))
            .Take(count)
            .Select(p => new PodcastResponse(p, domainUrl))
            .ToListAsync();
    }

    /// <summary>
    /// Get the top clicked episodes for a podcast.
    /// </summary>
    /// <param name="podcastId">The ID of the podcast.</param>
    /// <param name="count">The number of episodes to get.</param>
    /// <param name="getLessClicked">Whether to get the less clicked episodes.</param>
    /// <param name="user">The user making the request.</param>
    /// <param name="domainUrl">The domain URL.</param>
    /// <returns>The top clicked episodes for the podcast.</returns>
    /// <exception cref="Exception">Thrown when the count is less than or equal to 0.</exception>
    /// <exception cref="Exception">Thrown when the podcast does not exist for the given ID.</exception>
    public async Task<List<EpisodeResponse>> GetTopClickedEpisodesAsync(Guid podcastId, int count, bool getLessClicked, User user, string domainUrl)
    {
        if (count <= 0)
            throw new Exception("Count cannot be less than or equal to 0.");

        if (!await _db.Podcasts.AnyAsync(p => p.Id == podcastId && p.PodcasterId == user.Id))
            throw new Exception("Podcast does not exist for the given ID.");

        return getLessClicked ?
            await _db.Episodes
            .Include(e => e.UserEpisodeInteractions)
            .Include(e => e.Podcast)
            .Where(e => e.PodcastId == podcastId && e.Podcast.PodcasterId == user.Id)
            .OrderBy(e => e.UserEpisodeInteractions.Sum(uei => uei.Clicks))
            .Take(count)
            .Select(e => new EpisodeResponse(e, domainUrl,false))
            .ToListAsync()
            :
            await _db.Episodes
            .Include(e => e.UserEpisodeInteractions)
            .Include(e => e.Podcast)
            .Where(e => e.PodcastId == podcastId && e.Podcast.PodcasterId == user.Id)
            .OrderByDescending(e => e.UserEpisodeInteractions.Sum(uei => uei.Clicks))
            .Take(count)
            .Select(e => new EpisodeResponse(e, domainUrl,false))
            .ToListAsync();
    }

    /// <summary>
    /// Get the top watched podcasts for a user.
    /// </summary>
    /// <param name="count">The number of podcasts to get.</param>
    /// <param name="getLessWatched">Whether to get the less watched podcasts.</param>
    /// <param name="user">The user making the request.</param>
    /// <param name="domainUrl">The domain URL.</param>
    /// <returns>The top watched podcasts for the user.</returns>
    /// <exception cref="Exception">Thrown when the count is less than or equal to 0.</exception>
    public async Task<List<PodcastResponse>> GetTopWatchedPodcastsAsync(int count, bool getLessWatched, User user, string domainUrl)
    {
        if (count <= 0)
            throw new Exception("Count cannot be less than or equal to 0.");

        return getLessWatched ?
            await _db.Podcasts
            .Include(p => p.Episodes).ThenInclude(e => e.UserEpisodeInteractions)
            .Where(p => p.PodcasterId == user.Id)
            .OrderBy(p => p.Episodes.Sum(e => e.UserEpisodeInteractions.Sum(uei => uei.TotalListenTime.TotalSeconds)))
            .Take(count)
            .Select(p => new PodcastResponse(p, domainUrl))
            .ToListAsync()
            :
            await _db.Podcasts
            .Include(p => p.Episodes).ThenInclude(e => e.UserEpisodeInteractions)
            .Where(p => p.PodcasterId == user.Id)
            .OrderByDescending(p => p.Episodes.Sum(e => e.UserEpisodeInteractions.Sum(uei => uei.TotalListenTime.TotalSeconds)))
            .Take(count)
            .Select(p => new PodcastResponse(p, domainUrl))
            .ToListAsync();
    }

    /// <summary>
    /// Get the top watched episodes for a podcast.
    /// </summary>
    /// <param name="podcastId">The ID of the podcast.</param>
    /// <param name="count">The number of episodes to get.</param>
    /// <param name="getLessWatched">Whether to get the less watched episodes.</param>
    /// <param name="user">The user making the request.</param>
    /// <param name="domainUrl">The domain URL.</param>
    /// <returns>The top watched episodes for the podcast.</returns>
    /// <exception cref="Exception">Thrown when the count is less than or equal to 0.</exception>
    /// <exception cref="Exception">Thrown when the podcast does not exist for the given ID.</exception>
    public async Task<List<EpisodeResponse>> GetTopWatchedEpisodesAsync(Guid podcastId, int count, bool getLessWatched, User user, string domainUrl)
    {
        if (count <= 0)
            throw new Exception("Count cannot be less than or equal to 0.");

        if (!await _db.Podcasts.AnyAsync(p => p.Id == podcastId && p.PodcasterId == user.Id))
            throw new Exception("Podcast does not exist for the given ID.");

        return getLessWatched ?
            await _db.Episodes
            .Include(e => e.UserEpisodeInteractions)
            .Include(e => e.Podcast)
            .Where(e => e.PodcastId == podcastId && e.Podcast.PodcasterId == user.Id)
            .OrderBy(e => e.UserEpisodeInteractions.Sum(uei => uei.TotalListenTime.TotalSeconds))
            .Take(count)
            .Select(e => new EpisodeResponse(e, domainUrl,false))
            .ToListAsync()
            :
            await _db.Episodes
            .Include(e => e.UserEpisodeInteractions)
            .Include(e => e.Podcast)
            .Where(e => e.PodcastId == podcastId && e.Podcast.PodcasterId == user.Id)
            .OrderByDescending(e => e.UserEpisodeInteractions.Sum(uei => uei.TotalListenTime.TotalSeconds))
            .Take(count)
            .Select(e => new EpisodeResponse(e, domainUrl,false))
            .ToListAsync();
    }

    #endregion User Engagement Metrics
}