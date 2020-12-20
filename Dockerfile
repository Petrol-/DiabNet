FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build-env
WORKDIR /app

COPY . ./
RUN dotnet publish DiabNet -r linux-musl-x64 -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["./DiabNet"]
