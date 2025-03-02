FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
COPY ./src /src
RUN dotnet publish /src/Api/Api.csproj -c "Release" -r linux-musl-x64 --no-self-contained -o /app/publish



FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

COPY --from=build /app/publish /app

ENV ASPNETCORE_ENVIRONMENT=Production
CMD ["sh", "-c", "dotnet /app/Api.dll --no-launch-profile && tail -f /dev/null"]