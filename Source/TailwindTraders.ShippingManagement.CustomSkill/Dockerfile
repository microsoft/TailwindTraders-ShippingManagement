FROM mcr.microsoft.com/azure-functions/python

COPY ./source /home/site/wwwroot

RUN mkdir -p /usr/share/man/man1

RUN apt-get update

RUN cd /home/site/wwwroot && pip install -r requirements.txt