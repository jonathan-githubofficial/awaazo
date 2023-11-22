using System.Runtime.CompilerServices;
using Backend.Controllers.Requests;
using Backend.Controllers.Responses;
using Backend.Infrastructure;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

/// <summary>
/// Handles all services for the Social Controller.
/// </summary>
public class SocialService : ISocialService
{
    /// <summary>
    /// Current DB instance
    /// </summary>
    private readonly AppDbContext _db;

    /// <summary>
    /// Default Constructor
    /// </summary>
    /// <param name="db"></param>
    public SocialService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Adds a like to an Episode.
    /// </summary>
    /// <param name="episodeId"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<bool> AddLikeToEpisodeAsync(Guid episodeId, User user)
    {
        // Get the episode to be liked
        Episode episode = await _db.Episodes
        .Include(e => e.Likes)
        .Where(e => e.Id.Equals(episodeId))
        .FirstOrDefaultAsync() ?? throw new Exception("Episode does not exist with the given ID.");

        // Check if the user has already liked the episode
        if (episode.Likes.Any(l => l.UserId.Equals(user.Id)))
            throw new Exception("Episode has already been liked.");

        // Create the Like
        EpisodeLike like = new()
        {
            UserId = user.Id,
            EpisodeId = episodeId,
            UpdatedAt = DateTime.Now,
            CreatedAt = DateTime.Now
        };

        // Add the Like to the DB and return the status
        await _db.EpisodeLikes.AddAsync(like);
        return await _db.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// Adds a like to a comment.
    /// </summary>
    /// <param name="commentId"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<bool> AddLikeToCommentAsync(Guid commentId, User user)
    {
        // Get the comment to be liked
        Comment? comment = await _db.Comments
            .Include(c => c.Likes)
            .Include(c => c.Episode)
            .Include(c => c.Comments)
            .Where(c => c.Id.Equals(commentId))
            .FirstOrDefaultAsync();

        CommentReply? commentReply = await _db.CommentReplies
            .Include(c => c.Likes)
            .Where(c => c.Id == commentId)
            .FirstOrDefaultAsync();

        // If the comment exist, add a like
        if (comment is not null)
        {
            // Check that the comment is not already liked
            if (comment.Likes.Any(l => l.UserId.Equals(user.Id)))
                throw new Exception("Comment has already been liked.");

            // Create the Like
            CommentLike like = new()
            {
                UserId = user.Id,
                CommentId = commentId,
                UpdatedAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            await _db.CommentLikes.AddAsync(like);
        }
        else if (commentReply is not null)
        {
            // Check that the comment is not already liked
            if (commentReply.Likes.Any(l => l.UserId.Equals(user.Id)))
                throw new Exception("Comment has already been liked.");

            // Create the like
            CommentReplyLike like = new()
            {
                UserId = user.Id,
                CommentReplyId = commentId,
                UpdatedAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            await _db.CommentReplyLikes.AddAsync(like);
        }
        else
            throw new Exception("Given ID does not belong to any commment and/or episode.");

        // Save changes to the DB and return the status
        return await _db.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// Adds a comment to an episode or a comment.
    /// </summary>
    /// <param name="episodeOrCommentId"></param>
    /// <param name="user"></param>
    /// <param name="commentText"></param>
    /// <returns></returns>
    public async Task<bool> AddCommentAsync(Guid episodeOrCommentId, User user, string commentText)
    {
        // Check if the comment is a reply to a comment or an episode
        return await _db.Episodes.AnyAsync(e => e.Id == episodeOrCommentId) ?
            await AddCommentToEpisodeAsync(episodeOrCommentId, user, commentText) :
            await AddCommentToCommentAsync(episodeOrCommentId, user, commentText);
    }

    /// <summary>
    /// Adds a comment to an episode.
    /// </summary>
    /// <param name="episodeId"></param>
    /// <param name="user"></param>
    /// <param name="commentText"></param>
    /// <returns></returns>
    public async Task<bool> AddCommentToEpisodeAsync(Guid episodeId, User user, string commentText)
    {
        // Check if the episode exists
        Episode episode = await _db.Episodes
        .FirstOrDefaultAsync(e => e.Id.Equals(episodeId)) ?? throw new Exception("Episode does not exist for the given ID.");

        // Create the comment
        Comment comment = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            EpisodeId = episodeId,
            Text = commentText,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        // Save the comment to the DB and return the status
        await _db.Comments.AddAsync(comment);
        return await _db.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// Adds a comment to a comment.
    /// </summary>
    /// <param name="commentId"></param>
    /// <param name="user"></param>
    /// <param name="commentText"></param>
    /// <returns></returns>
    public async Task<bool> AddCommentToCommentAsync(Guid commentId, User user, string commentText)
    {
        // Check if the comment exists
        Comment comment = await _db.Comments
        .FirstOrDefaultAsync(c => c.Id == commentId) ?? throw new Exception("Comment does not exist for the given ID.");

        // Create the comment
        CommentReply commentReply = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ReplyToCommentId = commentId,
            Text = commentText,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        // Save the comment to the DB and return the status
        await _db.CommentReplies.AddAsync(commentReply);
        return await _db.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// Removes a comment.
    /// </summary>
    /// <param name="commentId"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<bool> RemoveCommentAsync(Guid commentId, User user)
    {
        // Check if the comment exists
        Comment? comment = await _db.Comments
        .Include(c => c.Comments).ThenInclude(c => c.Likes)
        .Include(c => c.Likes)
        .FirstOrDefaultAsync(c => c.Id == commentId);

        // If it does not, check if its a comment reply
        if (comment is null)
        {
            CommentReply commentReply = await _db.CommentReplies
            .Include(c => c.Likes)
            .FirstOrDefaultAsync(c => c.Id == commentId) ?? throw new Exception("No Comment exist for the given comment ID.");

            if (commentReply.UserId != user.Id)
                throw new Exception("User is not the owner of the comment.");

            // Remove all likes
            _db.CommentReplyLikes.RemoveRange(commentReply.Likes);

            // If it is, remove it.
            _db.CommentReplies.Remove(commentReply);
        }
        else
        {
            if (comment.UserId != user.Id)
                throw new Exception("User is not the owner of the comment.");

            // Remove all replies and likes
            foreach(CommentReply reply in comment.Comments)
                _db.CommentReplyLikes.RemoveRange(reply.Likes);

            _db.CommentLikes.RemoveRange(comment.Likes);
            _db.CommentReplies.RemoveRange(comment.Comments);

            // Remove the top comment last
            _db.Comments.Remove(comment);
        }


        // Save the changes and return the status.
        return await _db.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// Adds a like.
    /// </summary>
    /// <param name="episodeOrCommentId"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<bool> AddLikeAsync(Guid episodeOrCommentId, User user)
    {
        // Check if the like is for a comment or an episode
        return await _db.Episodes.AnyAsync(e => e.Id == episodeOrCommentId) ?
            await AddLikeToEpisodeAsync(episodeOrCommentId, user) :
            await AddLikeToCommentAsync(episodeOrCommentId, user);
    }

    /// <summary>
    /// Removes a like.
    /// </summary>
    /// <param name="episodeOrCommentId"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<bool> RemoveLikeAsync(Guid episodeOrCommentId, User user)
    {
        // Get the episodeLike, commentLike or commentReplyLike
        EpisodeLike? episodeLike = await _db.EpisodeLikes
        .FirstOrDefaultAsync(l => l.EpisodeId == episodeOrCommentId && l.UserId == user.Id);

        CommentLike? commentLike = await _db.CommentLikes
        .FirstOrDefaultAsync(l => l.CommentId == episodeOrCommentId && l.UserId == user.Id);

        CommentReplyLike? commentReplyLike = await _db.CommentReplyLikes
        .FirstOrDefaultAsync(l => l.CommentReplyId == episodeOrCommentId && l.UserId == user.Id);

        // Remove the like
        if (episodeLike is not null)
            _db.EpisodeLikes.Remove(episodeLike);
        else if (commentLike is not null)
            _db.CommentLikes.Remove(commentLike);
        else if (commentReplyLike is not null)
            _db.CommentReplyLikes.Remove(commentReplyLike);
        else
            throw new Exception("Like does not exist for the given ID.");

        // Save the changes and return the status.
        return await _db.SaveChangesAsync() > 0;
    }

    #region Rating

    /// <summary>
    /// Adds a rating to a podcast.
    /// </summary>
    /// <param name="podcastId"></param>
    /// <param name="user"></param>
    /// <param name="rating"></param>
    /// <returns></returns>
    public async Task<bool> AddRatingToPodcastAsync(Guid podcastId, User user, uint rating)
    {
        // Check if the podcast exists
        Podcast podcast = _db.Podcasts!.FirstOrDefault(podcast => podcast.Id.Equals(podcastId)) ?? throw new Exception("Podcast does not exist with the given ID.");

        // Check if the user has already rated the podcast
        PodcastRating? existingRating = await _db.PodcastRatings.FirstOrDefaultAsync(rating => rating.PodcastId.Equals(podcastId) && rating.UserId.Equals(user.Id));

        // Check if the rating is valid
        if (rating > PodcastRating.MAX_RATING || rating < PodcastRating.MIN_RATING)
            throw new Exception(string.Format("Rating must be between {0} and {1}.", PodcastRating.MIN_RATING, PodcastRating.MAX_RATING));

        // If the rating does not exist, create it, otherwise update it
        if (existingRating is null)
        {
            // Create the Rating
            PodcastRating podcastRating = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PodcastId = podcastId,
                Rating = rating
            }; 

            // Add the Rating to the DB
            await _db.PodcastRatings.AddAsync(podcastRating);
        }
        else
        {
            // Update the Rating in the DB
            existingRating.Rating = rating;
            _db.PodcastRatings.Update(existingRating);
        }

        // Return the status
        return await _db.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// Removes a rating from a podcast.
    /// </summary>
    /// <param name="podcastId"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<bool> RemoveRatingFromPodcastAsync(Guid podcastId, User user)
    {
        // Check if the Podcast Rating exists
        PodcastRating podcastRating = await _db.PodcastRatings.FirstOrDefaultAsync(rating => rating.PodcastId.Equals(podcastId) && rating.UserId.Equals(user.Id)) ?? throw new Exception("Rating does not exist for the given User ID and Podcast ID.");

        // If the Podcast Rating has a review, update it, otherwise remove it
        if (podcastRating.Review != string.Empty)
        {
            podcastRating.Rating = 0;
            _db.PodcastRatings.Update(podcastRating);
        }
        else
        {
            _db.PodcastRatings.Remove(podcastRating);
        }

        // Return the status
        return await _db.SaveChangesAsync() > 0;
    }

    #endregion

    #region Review

    /// <summary>
    /// Adds a review to a podcast. 
    /// </summary>
    /// <param name="podcastId"></param>
    /// <param name="user"></param>
    /// <param name="review"></param>
    /// <returns></returns>
    public async Task<bool> AddReviewToPodcastAsync(Guid podcastId, User user, string review)
    {
        // Check if the podcast exists
        Podcast podcast = _db.Podcasts!.FirstOrDefault(podcast => podcast.Id.Equals(podcastId)) ?? throw new Exception("Podcast does not exist with the given ID.");

        // Check if the user has already rated the podcast
        PodcastRating? existingRating = await _db.PodcastRatings.FirstOrDefaultAsync(rating => rating.PodcastId.Equals(podcastId) && rating.UserId.Equals(user.Id));

        // If the rating does not exist, create it, otherwise update it
        if (existingRating is null)
        {
            // Create the Rating
            PodcastRating podcastRating = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PodcastId = podcastId,
                Review = review
            };
            
            // Add the Rating to the DB
            await _db.PodcastRatings.AddAsync(podcastRating);
        }
        else
        {
            // Update the Rating in the DB
            existingRating.Review = review;
            _db.PodcastRatings.Update(existingRating);
        }

        // Return the status
        return await _db.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// Removes a review from a podcast.
    /// </summary>
    /// <param name="podcastId"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<bool> RemoveReviewFromPodcastAsync(Guid podcastId, User user)
    {
        // Check if the Podcast Rating exists
        PodcastRating podcastRating = await _db.PodcastRatings.FirstOrDefaultAsync(rating => rating.PodcastId.Equals(podcastId) && rating.UserId.Equals(user.Id)) ?? throw new Exception("Rating does not exist for the given User ID and Podcast ID.");

        // If the Podcast Rating has a rating, update it, otherwise remove it
        if (podcastRating.Rating != 0)
        {
            podcastRating.Review = string.Empty;
            _db.PodcastRatings.Update(podcastRating);
        }
        else
        {
            _db.PodcastRatings.Remove(podcastRating);
        }

        // Return the status
        return await _db.SaveChangesAsync() > 0;
    }

    #endregion
}