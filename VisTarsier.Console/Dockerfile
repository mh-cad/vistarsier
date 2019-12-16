FROM debian:buster

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

RUN	wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg \
	&& sudo mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/ \
	&& wget -q https://packages.microsoft.com/config/debian/10/prod.list \
	&& sudo mv prod.list /etc/apt/sources.list.d/microsoft-prod.list \
	&& sudo chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg \
	&& sudo chown root:root /etc/apt/sources.list.d/microsoft-prod.list

RUN echo "Cloning VT repo..." \
    && mkdir ~/code \
    && cd ~/code \
    && git clone https://github.com/mh-cad/vistarsier.git
RUN echo "Building VT Nifti Commandline..." \
RUN mkdir ~/vt \
	&& cd ~/vt \
	&& dotnet build ~/code/vistarsier/VisTarsier.Console/ -o . -c Release
	