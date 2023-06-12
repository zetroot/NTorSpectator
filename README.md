# dotNET Tor Spectator

This is a small bot to have fun with several things:
 - Tor daemon interaction
 - Mastodon posting

This small thing starts and tries to get service descriptors for sites mentioned in `sites.txt`. If hidden service descriptor received - the site is marked as up. Else - down. That's simple.

## Where is the bot

Follow the bot on mastodon. Staging account is [devmacaca@botsin.space](https://botsin.space/@devmacaca)

## Prerequisites 

There are some prerequisites for set up. You need Tor deamon running to request hidden service descriptors, you will need a mastodon account to post collected statuses and you'd better have Seq to collect logs from application.

### Tor daemon

Setup tor according to tor manual, configure bridges if connection to tor network fails or is unstable. Tor deamon is communicated through unix domain socket, typically located at `/run/tor/control`. You will also need an auth cookie to execute commands, so you should also be able to read cookie from `/run/tor/control.authcookie`.

### Mastodon account

Get a bot account on some mastodon instance, where you would like to post your statuses. Do respect instance policies for bots and automated posts.
Register application and save API token. You'll need it.
Also, save information on toot length on this instance. It will be used in configuration.

### Seq

Logging to console is great, but i prefer seq for pet projects. It is small, fast, reliable, painless log collector. The Seq Serilog sink is already built in NTorSpectator, pointing to `http://seq:5341`

## Setup

The best way to run applications is docker compose. So the sample file looks like this:

```yaml
version: "3"

services:
  tor_spectator:
    image: ghcr.io/zetroot/ntorspectator:latest
    platform: linux/arm64
    restart: always
    depends_on:
      seq:
        condition: service_started
    container_name: ntorspectator
    environment:
      - ASPNETCORE_ENVIRONMENT=Production # this is production environment, is not it?
    volumes:
      - "./sites.txt:/app/sites.txt"  # mount file with site list into application folder
      - "./appsettings.Production.json:/app/appsettings.Production.json" # mount production config into app folder
      - "/run/tor/control:/run/tor/control" # also mount tor unix-socket
      - "/run/tor/control.authcookie:/run/tor/control.authcookie" # and not forget about auth cookie
  
  seq:
    image: datalust/seq:latest
    container_name: seq_torspec
    hostname: seq
    restart: unless-stopped
    volumes:
      - "./seq_data:/data" # persist logs to disk
    ports:
      - "8081:80"
      - "5342:5341"
    environment:
      - ACCEPT_EULA=Y
```

`Sites.txt` is a file with tor onion links, one at a line:
```
he5dybnt7sr6cm32.........tm65flqy6irivtflruqfc5ep7eiodiad.onion
ho2hua2hfduv6f7h.........qdn4szgyy2jjnx545v4z3epq7uyrscid.onion
hqfld5smkr4b4xrj.........oqhuuoehjdvoin755iytmpk4sm7cbwad.onion
hxuzjtocnzvv5g2r.........bupmk7rclb6lly3fo4tvqkk5oyrv3nid.onion
hzwjmjimhr7bdmfv.........ibt5ojjmpo3pbp5ctwcg37n3hyk7qzid.onion
```

And provide production config in `appsettings.Production.json`:
```json
{
  "MastodonSettings" : {
    "Instance" : "https://your.cool.mastodon.instance.prod",
    "Token": "YourSuperSecretApiTokenGoesHere"
  },
  "SpectatorSettings": {
    "CooldownInterval":"01:00:00" <<== this is the interval to cool down tor network
  }
}
```

And then just `docker compose pull && docker compose up -d`. You are amazing!
