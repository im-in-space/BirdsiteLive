using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.ActivityPub.Models;
using BirdsiteLive.Common.Regexes;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Domain;
using BirdsiteLive.Models;
using BirdsiteLive.Tools;
using BirdsiteLive.Twitter;
using BirdsiteLive.Twitter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace BirdsiteLive.Controllers
{
    public class UsersController : Controller
    {
        private readonly ITwitterUserService _twitterUserService;
        private readonly ITwitterTweetsService _twitterTweetService;
        private readonly IUserService _userService;
        private readonly IStatusService _statusService;
        private readonly InstanceSettings _instanceSettings;
        private readonly IActivityPubService _activityPubService;
        private readonly ILogger<UsersController> _logger;

        #region Ctor
        public UsersController(ITwitterUserService twitterUserService, IUserService userService, IStatusService statusService, InstanceSettings instanceSettings, ITwitterTweetsService twitterTweetService, IActivityPubService activityPubService, ILogger<UsersController> logger)
        {
            _twitterUserService = twitterUserService;
            _userService = userService;
            _statusService = statusService;
            _instanceSettings = instanceSettings;
            _twitterTweetService = twitterTweetService;
            _activityPubService = activityPubService;
            _logger = logger;
        }
        #endregion

        [Route("/users")]
        public IActionResult Index()
        {
            var acceptHeaders = Request.Headers["Accept"];
            if (acceptHeaders.Any())
            {
                var r = acceptHeaders.First();
                if (r.Contains("application/activity+json")) return NotFound();
            }
            return View("UserNotFound");
        }

        [Route("/@{id}")]
        [Route("/users/{id}")]
        [Route("/users/{id}/remote_follow")]
        public IActionResult Index(string id)
        {
            _logger.LogTrace("User Index: {Id}", id);

            id = id.Trim(new[] { ' ', '@' }).ToLowerInvariant();

            TwitterUser user = null;
            var isSaturated = false;
            var notFound = false;

            // Ensure valid username 
            // https://help.twitter.com/en/managing-your-account/twitter-username-rules
            if (!string.IsNullOrWhiteSpace(id) && UserRegexes.TwitterAccount.IsMatch(id) && id.Length <= 15)
            {
                try
                {
                    user = _twitterUserService.GetUser(id);
                }
                catch (UserNotFoundException)
                {
                    notFound = true;
                }
                catch (UserHasBeenSuspendedException)
                {
                    notFound = true;
                }
                catch (RateLimitExceededException)
                {
                    isSaturated = true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception getting {Id}", id);
                    throw;
                }
            }
            else
            {
                notFound = true;
            }

            //var isSaturated = _twitterUserService.IsUserApiRateLimited();

            var acceptHeaders = Request.Headers["Accept"];
            if (acceptHeaders.Any())
            {
                var r = acceptHeaders.First();
                if (r.Contains("application/activity+json"))
                {
                    if (isSaturated) return new ObjectResult("Too Many Requests") { StatusCode = 429 };
                    if (notFound) return NotFound();
                    var apUser = _userService.GetUser(user);
                    var jsonApUser = JsonConvert.SerializeObject(apUser, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    return Content(jsonApUser, "application/activity+json; charset=utf-8");
                }
            }

            if (isSaturated) return View("ApiSaturated");
            if (notFound) return View("UserNotFound");

            var displayableUser = new DisplayTwitterUser
            {
                Name = user.Name,
                Description = user.Description,
                Acct = user.Acct.ToLowerInvariant(),
                Url = user.Url,
                ProfileImageUrl = user.ProfileImageUrl,
                Protected = user.Protected,

                InstanceHandle = $"@{user.Acct.ToLowerInvariant()}@{_instanceSettings.Domain}"
            };
            return View(displayableUser);
        }

        [Route("/@{id}/{statusId}")]
        [Route("/users/{id}/statuses/{statusId}")]
        public IActionResult Tweet(string id, string statusId)
        {
            var acceptHeaders = Request.Headers["Accept"];
            if (acceptHeaders.Any())
            {
                var r = acceptHeaders.First();
                if (r.Contains("application/activity+json"))
                {
                    if (!long.TryParse(statusId, out var parsedStatusId))
                        return NotFound();

                    if (_instanceSettings.MaxStatusFetchAge > 0)
                    {
                        // I hate bitwise operators, corn syrup, and the antichrist
                        // shift 22 bits to the right to get milliseconds, add the twitter epoch, then divide by 1000 to get seconds
                        long secondsAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (((parsedStatusId >> 22) + 1288834974657) / 1000);

                        if ( secondsAgo > _instanceSettings.MaxStatusFetchAge*60*60*24 )
                        {
                            return new StatusCodeResult(StatusCodes.Status410Gone);
                        }
                    }

                    var tweet = _twitterTweetService.GetTweet(parsedStatusId);
                    if (tweet == null)
                        return NotFound();

                    //var user = _twitterService.GetUser(id);
                    //if (user == null) return NotFound();

                    var status = _statusService.GetStatus(id, tweet);
                    var jsonApUser = JsonConvert.SerializeObject(status);
                    return Content(jsonApUser, "application/activity+json; charset=utf-8");
                }
            }

            return Redirect($"https://{_instanceSettings.TwitterDomain}/{id}/status/{statusId}");
        }

        [Route("/users/{id}/inbox")]
        [HttpPost]
        public async Task<IActionResult> Inbox()
        {
            try
            {
                var r = Request;
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();

                    _logger.LogTrace("User Inbox: {Body}", body);
                    //System.IO.File.WriteAllText($@"C:\apdebug\{Guid.NewGuid()}.json", body);

                    var activity = ApDeserializer.ProcessActivity(body);
                    var signature = r.Headers["Signature"].First();

                    switch (activity?.type)
                    {
                        case "Follow":
                        {
                            var succeeded = await _userService.FollowRequestedAsync(signature, r.Method, r.Path,
                                r.QueryString.ToString(), HeaderHandler.RequestHeaders(r.Headers),
                                activity as ActivityFollow, body);
                            if (succeeded) return Accepted();
                            else return Unauthorized();
                        }
                        case "Undo":
                            if (activity is ActivityUndoFollow)
                            {
                                var succeeded = await _userService.UndoFollowRequestedAsync(signature, r.Method, r.Path,
                                    r.QueryString.ToString(), HeaderHandler.RequestHeaders(r.Headers),
                                    activity as ActivityUndoFollow, body);
                                if (succeeded) return Accepted();
                                else return Unauthorized();
                            }

                            return Accepted();
                        case "Delete":
                        {
                            var succeeded = await _userService.DeleteRequestedAsync(signature, r.Method, r.Path,
                                r.QueryString.ToString(), HeaderHandler.RequestHeaders(r.Headers),
                                activity as ActivityDelete, body);
                            if (succeeded) return Accepted();
                            else return Unauthorized();
                        }
                        default:
                            return Accepted();
                    }
                }
            }
            catch (FollowerIsGoneException)  //TODO: check if user in DB
            {
                return Accepted();
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
            catch (UserHasBeenSuspendedException)
            {
                return NotFound();
            }
            catch (RateLimitExceededException)
            {
                return new ObjectResult("Too Many Requests") { StatusCode = 429 };
            }
        }

        [Route("/users/{id}/followers")]
        [HttpGet]
        public IActionResult Followers(string id)
        {
            var r = Request.Headers["Accept"].First();
            if (!r.Contains("application/activity+json")) return NotFound();

            var followers = new Followers
            {
                id = $"https://{_instanceSettings.Domain}/users/{id}/followers"
            };
            var jsonApUser = JsonConvert.SerializeObject(followers);
            return Content(jsonApUser, "application/activity+json; charset=utf-8");
        }

        [Route("/users/{actor}/remote_follow")]
        [HttpPost]
        public async Task<IActionResult> RemoteFollow(string actor)
        {
            StringValues webfingerValues;

            if (!Request.Form.TryGetValue("webfinger", out webfingerValues)) return BadRequest();

            var webfinger = webfingerValues.First();

            if (webfinger.Length < 1 || actor.Length < 1) return BadRequest();

            if (webfinger[0] == '@') webfinger = webfinger[1..];

            if (webfinger.IndexOf("@") < 0 || ! new Regex("^[A-Za-z0-9_]*$").IsMatch(webfinger.Split('@')[0]) || ! new Regex("^[A-Za-z0-9_]*$").IsMatch(actor) || Uri.CheckHostName(webfinger.Split('@')[1]) == UriHostNameType.Unknown)
            {
                return BadRequest();
            }

            WebFingerData webfingerData;

            try
            {
                webfingerData = await _activityPubService.WebFinger(webfinger);
            }
            catch(Exception e)
            {
                _logger.LogError("Could not WebFinger {user}: {exception}", webfinger, e);
                return NotFound();
            }

            string redirectLink = "";

            foreach(var link in webfingerData.links)
            {
                if(link.rel == "http://ostatus.org/schema/1.0/subscribe" && link.template.Length > 0)
                {
                    redirectLink = link.template.Replace("{uri}", "https://" + _instanceSettings.Domain + "/users/" + actor);
                }
            }

            if (redirectLink == "") return NotFound();

            return Redirect(redirectLink);
        }
    }
}
