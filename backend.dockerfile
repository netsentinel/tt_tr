FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
COPY ./backend /backend
RUN dotnet publish /backend/Api/Api.csproj -c "Release" -r linux-musl-x64 --no-self-contained -o /app/publish



FROM alpine:3 AS final

RUN apk add -q --no-progress aspnetcore8-runtime
COPY --from=build /app/publish /app

ENV ASPNETCORE_ENVIRONMENT=Production
CMD ["sh", "-c", "dotnet /app/Api.dll --no-launch-profile && tail -f /dev/null"]