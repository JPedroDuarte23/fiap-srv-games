FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY FiapSrvGames.sln .

COPY FiapSrvGames.API/*.csproj ./FiapSrvGames.API/
COPY FiapSrvGames.Application/*.csproj ./FiapSrvGames.Application/
COPY FiapSrvGames.Domain/*.csproj ./FiapSrvGames.Domain/
COPY FiapSrvGames.Infrastructure/*.csproj ./FiapSrvGames.Infrastructure/
COPY FiapSrvGames.Test/*.csproj ./FiapSrvGames.Test/

RUN dotnet restore

COPY FiapSrvGames.API/ ./FiapSrvGames.API/
COPY FiapSrvGames.Application/ ./FiapSrvGames.Application/
COPY FiapSrvGames.Domain/ ./FiapSrvGames.Domain/
COPY FiapSrvGames.Infrastructure/ ./FiapSrvGames.Infrastructure/
COPY FiapSrvGames.Test/ ./FiapSrvGames.Test/

RUN dotnet publish FiapSrvGames.API/FiapSrvGames.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Install the agent
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
&& echo 'deb [signed-by=/usr/share/keyrings/newrelic-apt.gpg] http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
&& wget -O- https://download.newrelic.com/NEWRELIC_APT_2DAD550E.public | gpg --import --batch --no-default-keyring --keyring /usr/share/keyrings/newrelic-apt.gpg \
&& apt-get update \
&& apt-get install -y newrelic-dotnet-agent

# Enable the agent
ENV CORECLR_ENABLE_PROFILING=1 \
CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so

RUN adduser --disabled-password --no-create-home appuser

USER appuser

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "FiapSrvGames.API.dll"]
