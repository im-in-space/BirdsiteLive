This project is a *fork* of [the original BirdsiteLIVE from NicolasConstant](https://github.com/NicolasConstant/BirdsiteLive) and [this fork from Pasture](https://git.gamers.exposed/pasture/BirdsiteLIVE). It runs in production at [bird.im-in.space](https://bird.im-in.space).  
Changes made from the original or fork include:

* Rework About page entirely - also disclose unlisted accounts and federation restrictions
* Cache Tweets so that, for example, Announces do not hit rate limits
* Allow replacing and redirecting to twitter.com in Tweets to other domains (i.e. Nitter instances)
* Proper remote follow form on user pages
* Mark individual Tweets as potentially sensitive
* Define and enforce a maximum follow count limit
* Define and enforce a maximum Tweet fetch age using snowflakes
* (Optional) send quote-RTs as Soapbox-style quote posts
* Updated to .NET 6.0 and updated dependencies to stay within currently supported versions

Removed features from the original or fork:

* ~~Verified checkmarks on [verified](https://twitter.com/verified) Twitter users~~

This fork is also available as a Docker image at [`ghcr.io/im-in-space/birdsitelive`](https://github.com/im-in-space/BirdsiteLive/pkgs/container/birdsitelive) (linux/amd64, linux/arm64).  
Or at Docker Hub as [`mkody/birdsitelive`](https://hub.docker.com/r/mkody/birdsitelive) (linux/amd64).

[![ASP.NET Core Build & Tests](https://github.com/im-in-space/BirdsiteLive/actions/workflows/dotnet-core.yml/badge.svg?branch=im-in-space&event=push)](https://github.com/im-in-space/BirdsiteLive/actions/workflows/dotnet-core.yml)

The project's original README is as follows:

---

# BirdsiteLIVE

## About

BirdsiteLIVE is an ActivityPub bridge from Twitter, it's mostly a pet project/playground for me to handle ActivityPub concepts. Feel free to deploy your own instance (especially if you plan to follow a lot of users) since it use a proper Twitter API key and therefore will have limited calls ceiling (it won't scale, and it's by design).

## State of development

The code is pretty messy and far from a good state, since it's a playground for me the aim was to understand some AP concepts, not to provide a good state-of-the-art codebase. But I might refactor it to make it cleaner. 

## Official instance 

There's none! Please read [here why I've stopped it](https://write.as/nicolas-constant/closing-the-official-bsl-instance).

## Installation

I'm providing a [docker build](https://hub.docker.com/r/nicolasconstant/birdsitelive) (linux/amd64 only). To install it on your own server, please follow [those instructions](https://github.com/NicolasConstant/BirdsiteLive/blob/master/INSTALLATION.md). More [options](https://github.com/NicolasConstant/BirdsiteLive/blob/master/VARIABLES.md) are also available.

Also a [CLI](https://github.com/NicolasConstant/BirdsiteLive/blob/master/BSLManager.md) is available for adminitrative tasks.

## License

This project is licensed under the AGPLv3 License - see [LICENSE](https://github.com/NicolasConstant/BirdsiteLive/blob/master/LICENSE) for details.

## Contact

You can contact me via ActivityPub <a rel="me" href="https://fosstodon.org/@BirdsiteLIVE">here</a>.


