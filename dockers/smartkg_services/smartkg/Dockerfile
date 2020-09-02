FROM aspnetcore

ARG DOCKER_HOST

COPY requirements.txt /tmp/
RUN pip install -r /tmp/requirements.txt

COPY SmartKGLocalBase.zip /tmp/
RUN unzip /tmp/SmartKGLocalBase.zip -d /app

COPY smartkg.zip /tmp/
RUN unzip /tmp/smartkg.zip -d /app

COPY appsettings.json /app/smartkg/

RUN sed -i "s/localhost/${DOCKER_HOST}/g" /app/smartkg/appsettings.json

ENV PATH=/opt/rh/rh-python36/root/usr/bin:$PATH  \
    LD_LIBRARY_PATH=/app/smartkg/lib64:$LD_LIBRARY_PATH


CMD cd /app/smartkg && \
    dotnet SmartKG.KGBot.dll
