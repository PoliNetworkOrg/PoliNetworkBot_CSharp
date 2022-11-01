FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

ENV TZ=Europe/Rome
RUN rm -rf /etc/localtime
RUN ln -s /usr/share/zoneinfo/Europe/Rome /etc/localtime

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "PoliNetworkBot_CSharp.dll", "2"]
