FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Install the new relic agent
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
&& echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
&& wget https://download.newrelic.com/548C16BF.gpg \
&& apt-key add 548C16BF.gpg \
&& apt-get update \
&& apt-get install -y 'newrelic-dotnet-agent' \
&& rm -rf /var/lib/apt/lists/*

# Enable the new relic agent
ENV CORECLR_ENABLE_PROFILING=1 \
CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so \
NEW_RELIC_LICENSE_KEY=82ac8d1e830b8348355a6b72674a9230FFFFNRAL \
NEW_RELIC_APP_NAME="FIAP-Payments"

WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Forçar timezone UTC
ENV TZ=UTC
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia todos os .csproj primeiro
COPY ["FCG-Payments.Api/FCG-Payments.Api.csproj", "FCG-Payments.Api/"]
COPY ["FCG-Payments.Application/FCG-Payments.Application.csproj", "FCG-Payments.Application/"]
COPY ["FCG-Payments.Domain/FCG-Payments.Domain.csproj", "FCG-Payments.Domain/"]
COPY ["FCG-Payments.Infrastructure/FCG-Payments.Infrastructure.csproj", "FCG-Payments.Infrastructure/"]

# Restaura apenas a API
RUN dotnet restore "FCG-Payments.Api/FCG-Payments.Api.csproj"

# Copia todo o resto
COPY . .

WORKDIR "/src/FCG-Payments.Api"
RUN dotnet build "FCG-Payments.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FCG-Payments.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FCG-Payments.Api.dll"]