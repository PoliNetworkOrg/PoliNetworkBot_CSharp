FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /App
COPY --from=build-env /App/out .
RUN TZ='Europe/Rome' date > build-date.txt
RUN ls -la
ENTRYPOINT ["sh", "-c", "rm -rf /etc/localtime; ln -s /usr/share/zoneinfo/Europe/Rome /etc/localtime; dotnet PoliNetworkBot_CSharp.dll 2"]
