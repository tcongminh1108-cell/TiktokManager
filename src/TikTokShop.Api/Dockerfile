# Build context: repo root
# Usage: docker build -f src/TikTokShop.Api/Dockerfile -t tiktokshop-api .

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY TikTokShop.sln .
COPY src/TikTokShop.Domain/TikTokShop.Domain.csproj                 src/TikTokShop.Domain/
COPY src/TikTokShop.Application/TikTokShop.Application.csproj       src/TikTokShop.Application/
COPY src/TikTokShop.Infrastructure/TikTokShop.Infrastructure.csproj src/TikTokShop.Infrastructure/
COPY src/TikTokShop.Api/TikTokShop.Api.csproj                       src/TikTokShop.Api/

RUN dotnet restore src/TikTokShop.Api/TikTokShop.Api.csproj

COPY src/ src/

RUN dotnet publish src/TikTokShop.Api/TikTokShop.Api.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN groupadd -r appgroup && useradd -r -g appgroup --no-create-home appuser

COPY --from=build /app/publish .
RUN mkdir -p logs && chown -R appuser:appgroup /app

USER appuser
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TikTokShop.Api.dll"]
