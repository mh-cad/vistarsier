FROM debian:buster
FROM python:3.7-buster
# Basic tools
RUN echo 'deb http://deb.debian.org/debian buster main contrib non-free' >> /etc/apt/sources.list && cat /etc/apt/sources.list
RUN apt-get update -qq \
    && apt-get install -y -q --no-install-recommends \
           apt-utils \
           bzip2 \
           ca-certificates \
           curl \
           locales \
           unzip \
           git \
           cmake \
    && apt-get clean
# ANTs build and install
ENV ANTSPATH="/opt/ants" \
    PATH="/opt/ants:$PATH" \
    CMAKE_INSTALL_PREFIX=$ANTSPATH

RUN echo "Cloning ANTs repo..." \
    && mkdir ~/code \
    && cd ~/code \
    && git clone --branch v2.3.1 https://github.com/ANTsX/ANTs.git

RUN echo "Building ANTs..." \
    && mkdir -p ~/bin/antsBuild \
    && cd ~/bin/antsBuild \
    && cmake ~/code/ANTs
RUN cd ~/bin/antsBuild/ \
    && make
RUN cd ~/bin/antsBuild/ANTS-build \
    && make install

# FSL installation
RUN apt-get install -y fsl

# dotnet install
RUN wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg \
  && mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/ \
  && wget -q https://packages.microsoft.com/config/debian/10/prod.list \
  && mv prod.list /etc/apt/sources.list.d/microsoft-prod.list \
  && chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg \
  && chown root:root /etc/apt/sources.list.d/microsoft-prod.list

RUN apt-get update \
  && apt-get install apt-transport-https -y \
  && apt-get update \
  && apt-get install dotnet-sdk-3.0 -y \
  && apt-get install dotnet-runtime-3.0 -y

# Extra libs needed
RUN apt-get install libc6-dev libgdiplus -y

ENV DOTNET_CLI_TELEMETRY_OPTOUT = 1

RUN mkdir ~/code/vistarsier 

COPY ./VisTarsier.Common/ /root/code/vistarsier/VisTarsier.Common/
COPY ./VisTarsier.Config/ /root/code/vistarsier/VisTarsier.Config/
COPY ./VisTarsier.Console/ /root/code/vistarsier/VisTarsier.Console/
COPY ./VisTarsier.Extensions/ /root/code/vistarsier/VisTarsier.Extensions/
COPY ./VisTarsier.MS/ /root/code/vistarsier/VisTarsier.MS/
COPY ./VisTarsier.NiftiLib/ /root/code/vistarsier/VisTarsier.NiftiLib/

RUN ls ~/code/vistarsier 
RUN ls ~/code/vistarsier/VisTarsier.Console

RUN echo "Building VisTarsier..." \
  && cd ~/code/vistarsier/VisTarsier.Console \
  && dotnet build ~/code/vistarsier/VisTarsier.Console --runtime linux-x64 -c Release -o /usr/share/vistarsier/cmd

COPY ./vtdocker/betasbse.sh /usr/share/vistarsier/betasbse.sh
COPY ./vtdocker/config.json /usr/share/vistarsier/cfg/config.json

WORKDIR /usr/share/vistarsier/cmd/

ENTRYPOINT ["dotnet", "/usr/share/vistarsier/cmd/VisTarsier.CommandLineTool.dll"]
CMD []
