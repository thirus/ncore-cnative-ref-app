# Build image
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS builder

# Install mono for Cake
ENV MONO_VERSION 5.20.1.19

RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF

RUN echo "deb http://download.mono-project.com/repo/debian stretch/snapshots/$MONO_VERSION main" > /etc/apt/sources.list.d/mono-official.list \
  && apt-get update \
  && apt-get install -y mono-runtime \
  && rm -rf /var/lib/apt/lists/* /tmp/*

RUN apt-get update \
  && apt-get install -y binutils curl mono-devel ca-certificates-mono fsharp mono-vbnc nuget referenceassemblies-pcl \
  && rm -rf /var/lib/apt/lists/* /tmp/*

WORKDIR /src

COPY ./build.sh ./build.cake ./

# Install Cake, and compile the Cake build script
RUN ./build.sh -t Clean

# Copy all the csproj files and restore to cache the layer for faster builds
# The dotnet_build.sh script does this anyway, so superfluous, but docker can 
# cache the intermediate images so _much_ faster
COPY ./*.csproj ./

RUN ./build.sh -t Restore

COPY . /src

# Build, Test, and Publish
RUN ./build.sh -t Build && ./build.sh -t Test && ./build.sh -t Publish

#App image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT Production
COPY --from=builder /src/dist .
ENTRYPOINT ["dotnet", "ReferenceApp.dll"]