FROM python36

COPY aspnetcore-runtime-2.1.16-linux-x64.tar.gz /data/

RUN mkdir -p /dotnet && \
    tar zxf /data/aspnetcore-runtime-2.1.16-linux-x64.tar.gz -C /dotnet
ENV PATH=/dotnet:$PATH \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

EXPOSE 8080

CMD ["/bin/bash"]

