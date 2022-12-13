using System;
using BirdsiteLive.Twitter.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BirdsiteLive.Twitter
{
    public interface ICachedTwitterTweetsService : ITwitterTweetsService
    {
        void PurgeTweet(long statusId);
    }

    public class CachedTwitterTweetsService : ICachedTwitterTweetsService
    {
        private readonly ITwitterTweetsService _twitterService;

        private MemoryCache _tweetCache = new MemoryCache(new MemoryCacheOptions()
        {
            SizeLimit = 5000
        });
        private MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSize(1)//Size amount
                       //Priority on removing when reaching size limit (memory pressure)
            .SetPriority(CacheItemPriority.High)
            // Keep in cache for this time, reset time if accessed.
            // We set this lower than a user's in case they delete this Tweet for some reason; we don't need that cached.
            .SetSlidingExpiration(TimeSpan.FromHours(2))
            // Remove from cache after this time, regardless of sliding expiration
            .SetAbsoluteExpiration(TimeSpan.FromDays(7));

        #region Ctor
        public CachedTwitterTweetsService(ITwitterTweetsService twitterService)
        {
            _twitterService = twitterService;
        }

        public ExtractedTweet[] GetTimeline(string username, int nberTweets, long fromTweetId = -1)
        {
            // This sounds like it'd be silly to cache; pass this directly to TwitterService.
            // Theoretically this shouldn't be called more than once every 15 min anyway?
            return _twitterService.GetTimeline(username, nberTweets, fromTweetId);
        }

        public ExtractedTweet GetTweet(long statusId)
        {
            if(!_tweetCache.TryGetValue(statusId, out ExtractedTweet tweet))
            {
                tweet = _twitterService.GetTweet(statusId);

                // Unlike with the user cache, save the null value anyway to prevent (quicker) API exhaustion.
                // It's incredibly unlikely that a tweet with this ID is going to magically appear within 2 hours.
                _tweetCache.Set(statusId, tweet, _cacheEntryOptions); 
            }

            return tweet;
        }
        #endregion

        public void PurgeTweet(long statusId)
        {
            _tweetCache.Remove(statusId);
        }
    }
}