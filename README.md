This project is a *fork* of [the original BirdsiteLIVE from NicolasConstant](https://github.com/NicolasConstant/BirdsiteLive). This fork runs in production on [a large BirdsiteLIVE instance](https://twtr.plus). Changes made in this fork include:

* Rework About page entirely - also disclose unlisted accounts and federation restrictions
* Cache Tweets so that, for example, Announces do not hit rate limits
* Allow replacing and redirecting to twitter.com in Tweets to other domains (i.e. Nitter instances)
* Verified checkmarks on [verified](https://twitter.com/verified) Twitter users
* Proper remote follow form on user pages
* Mark individual Tweets as potentially sensitive
* Define and enforce a maximum follow count limit
* Define and enforce a maximum Tweet fetch age using snowflakes
* (Optional) send quote-RTs as Soapbox-style quote posts

This fork is also available as a Docker image as `pasture/birdsitelive`.

The project's original README is as follows:

![Test](https://github.com/NicolasConstant/BirdsiteLive/workflows/.NET%20Core/badge.svg?branch=master&event=push)

# BirdsiteLIVE

## About

BirdsiteLIVE is an ActivityPub bridge from Twitter, it's mostly a pet project/playground for me to handle ActivityPub concepts. Feel free to deploy your own instance (especially if you plan to follow a lot of users) since it use a proper Twitter API key and therefore will have limited calls ceiling (it won't scale, and it's by design).

## State of development

The code is pretty messy and far from a good state, since it's a playground for me the aim was to understand some AP concepts, not provide a good state-of-the-art codebase. But I might refactor it to make it cleaner. 

## Official instance 

You can find an official (and temporary) instance here: [beta.birdsite.live](https://beta.birdsite.live). This instance can disappear at any time, if you want a long term instance you should install your own or use another one. 

## Installation

I'm providing a [docker build](https://hub.docker.com/r/nicolasconstant/birdsitelive). To install it on your own server, please follow [those instructions](https://github.com/NicolasConstant/BirdsiteLive/blob/master/INSTALLATION.md). More [options](https://github.com/NicolasConstant/BirdsiteLive/blob/master/VARIABLES.md) are also available.

Also a [CLI](https://github.com/NicolasConstant/BirdsiteLive/blob/master/BSLManager.md) is available for adminitrative tasks.

## License

This project is licensed under the AGPLv3 License - see [LICENSE](https://github.com/NicolasConstant/BirdsiteLive/blob/master/LICENSE) for details.

## Contact

You can contact me via ActivityPub <a rel="me" href="https://fosstodon.org/@BirdsiteLIVE">here</a>.


