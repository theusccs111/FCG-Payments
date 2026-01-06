FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Consumer não precisa expor porta, mas vamos deixar para health check se necessário
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Forçar timezone UTC
ENV TZ=UTC
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia todos os .csproj primeiro
COPY ["FCG-Payments.Consumer/FCG-Payments.Consumer.csproj", "FCG-Payments.Consumer/"]
COPY ["FCG-Payments.Application/FCG-Payments.Application.csproj", "FCG-Payments.Application/"]
COPY ["FCG-Payments.Domain/FCG-Payments.Domain.csproj", "FCG-Payments.Domain/"]
COPY ["FCG-Payments.Infrastructure/FCG-Payments.Infrastructure.csproj", "FCG-Payments.Infrastructure/"]

# Restaura o Consumer
RUN dotnet restore "FCG-Payments.Consumer/FCG-Payments.Consumer.csproj"

# Copia todo o resto
COPY . .
WORKDIR "/src/FCG-Payments.Consumer"
RUN dotnet build "FCG-Payments.Consumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FCG-Payments.Consumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FCG-Payments.Consumer.dll"]