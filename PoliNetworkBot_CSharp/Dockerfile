FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /App
COPY --from=build-env /App/out .
COPY --from=build-env /App/static ./static

# Set globalization invariant to false and install Italian locale
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV DEBIAN_FRONTEND=noninteractive
RUN apt update && apt install -y locales && rm -rf /var/lib/apt/lists/* \
    && echo "it_IT.UTF-8 UTF-8" > /etc/locale.gen \
    && locale-gen

# Set Italian culture for the application
ENV LANG=it_IT.UTF-8
ENV LANGUAGE=it_IT.UTF-8
ENV LC_ALL=it_IT.UTF-8

# Install git
RUN apt update && apt install -y git hub wget unzip && rm -rf /var/lib/apt/lists/*

# Set timezone to Italy
RUN sh -c 'rm -rf /etc/localtime; ln -s /usr/share/zoneinfo/Europe/Rome /etc/localtime;'
RUN date

# Change executer to non user
RUN useradd -u 7999 -m botexecuter
RUN chown -R botexecuter .
USER botexecuter

RUN cat ./static/ssh-agent >> /home/botexecuter/.bashrc

ENTRYPOINT ["sh", "-c", "dotnet PoliNetworkBot_CSharp.dll 2"]
