FROM ubuntu:20.04

# Install wget and unzip
RUN  apt-get update && apt-get install -y wget unzip

# Install Unity 2020.3.35f1c1
RUN wget https://download.unity3d.com/download_unity/3d0d3c5c3b2a/UnityDownloadAssistant-2020.3.35f1c1-linux.deb

