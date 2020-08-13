FROM aspnetcore

COPY SmartKGLocalBase.zip /tmp/
RUN unzip /tmp/SmartKGLocalBase.zip -d /app

COPY smartkg.zip /tmp/
RUN unzip /tmp/smartkg.zip -d /app

COPY appsettings.json /app/smartkg/

ENV LD_LIBRARY_PATH=/app/smartkg/lib64:$LD_LIBRARY_PATH

CMD cd /app/smartkg && \
    dotnet SmartKG.KGBot.dll