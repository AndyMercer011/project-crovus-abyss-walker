FROM mono:latest

# set noninteractive installation
ENV DEBIAN_FRONTEND=noninteractive

# Download DocFX and unzip it
RUN apt-get -qq update && apt-get -qq install -y wget unzip
RUN wget -q https://github.com/dotnet/docfx/releases/download/v2.59.4/docfx.zip
RUN mkdir /opt/docfx && \
    unzip -q docfx.zip -d /opt/docfx && \
    rm docfx.zip
RUN chmod +x /opt/docfx/docfx.exe

# Install git
RUN apt-get -qq update && apt-get -qq install -y git