<p align="center">
	<b>Toki</b><br>
	<span>n. (toki pona) - communication, speech</span>
</p>

<hr>

A C# ActivityPub Fediverse-compatible server. Very early in development, probably never will be good enough for a huge instance.

I'm working on it as sort of a personal challenge, considering I've always wanted to implement the ActivityPub protocol.

I don't really plan on making a frontend (no design skills) for this, but it will support the Mastodon API set, so you'll be able to connect to it with any of your favorite Fediverse clients.

## Modules

**Toki** contains the server & Web backend. 

**Toki.ActivityPub** contains all of the ActivityPub related logic, like the models, and various resolvers. 

**Toki.ActivityStreams** contains the ActivityStreams implementation.

## Requirements

* .NET 8
* PostgreSQL (as the database)
* Redis (for the job queue)