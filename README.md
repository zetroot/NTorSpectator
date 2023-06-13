# dotNET Tor Spectator

This is a small bot to have fun with several things:
 - Tor daemon interaction
 - Mastodon posting

This small bot loads sites from `sites.txt` into database and tries to get descriptors for them. If hidden service descriptor received - the site is marked as up. Else - down. That's simple.

## Where is the bot

Follow the bot on mastodon [ntorspectator@botsin.space](https://botsin.space/@ntorspectator)

## Prerequisites 

There are some prerequisites for set up. You need Tor deamon running to request hidden service descriptors, you will need a mastodon account to post collected statuses and you'd better have Seq to collect logs from application. All collected data is stored in Postgresql, so you will need it too.

### Tor daemon

Setup tor according to tor manual, configure bridges if connection to tor network fails or is unstable. Tor deamon is communicated through unix domain socket, typically located at `/run/tor/control`. You will also need an auth cookie to execute commands, so you should also be able to read cookie from `/run/tor/control.authcookie`.

### Mastodon account

Get a bot account on some mastodon instance, where you would like to post your statuses. Do respect instance policies for bots and automated posts.
Register application and save API token. You'll need it.
Also, save information on toot length on this instance. It will be used in configuration.

### Seq

Logging to console is great, but i prefer seq for pet projects. It is small, fast, reliable, painless log collector. The Seq Serilog sink is already built in NTorSpectator, pointing to `http://seq:5341`

### PostgreSQL

Database will run in container, for easier maintanence and setup, but you can run it wherever you want.

## Setup

The best way to run applications is docker compose. Try change my mind. So the sample file looks like this:

```yaml
version: "3"

services:    
  tor_spectator:
    image: ghcr.io/zetroot/ntorspectator-observer:master
    platform: linux/arm64
    restart: always
    depends_on:
      seq:
        condition: service_started
      database:
        condition: service_healthy
      migrator:
        condition: service_completed_successfully
    ports:
      - 81:8000
    container_name: ntorspectator
    environment:
      - ASPNETCORE_ENVIRONMENT=Production # this is production environment, is not it?
    volumes:
      - "./sites.txt:/app/sites.txt"  # mount file with site list into application folder
      - "./appsettings.Production.json:/app/appsettings.Production.json" # mount production config into app folder
      - "/run/tor/control:/run/tor/control" # also mount tor unix-socket
      - "/run/tor/control.authcookie:/run/tor/control.authcookie" # and not forget about auth cookie

  migrator:
    image: ghcr.io/zetroot/ntorspectator-efbundle:master
    depends_on:
      database:
        condition: service_healthy
    platform: linux/arm64
    restart: no
    command: /app/bundle --connection "Server=pgdb;Port=5432;Database=ntorspectator;User Id=postgres;Password=password" # connection string to database to apply migrations

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

  database:
    image: 'postgres:15'
    container_name: pg_torspec
    hostname: pgdb
    user: postgres
    healthcheck:
      test: ["CMD-SHELL", "pg_isready"]
      interval: 1s
      timeout: 5s
      retries: 10
    ports:
      - 5432:5432
    environment:
      - POSTGRES_USER=postgres # The PostgreSQL user (useful to connect to the database)
      - POSTGRES_PASSWORD=password # The PostgreSQL password (useful to connect to the database)
      - POSTGRES_DB=postgres # The PostgreSQL default database (automatically created at first launch)
    volumes:
      - "./db-data/:/var/lib/postgresql/data/"

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
  "ConnectionStrings": {
    "SpectatorDatabase":"Server=pgdb;Port=5432;Database=ntorspectator;User Id=postgres;Password=password;"
  },
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
