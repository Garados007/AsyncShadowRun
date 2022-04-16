FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder
WORKDIR /src
COPY ./AsyncShadowRun.sln ./AsyncShadowRun.sln
COPY ./AsyncShadowRun ./AsyncShadowRun
RUN mkdir -p /app && \
    dotnet build --nologo -c RELEASE \
        AsyncShadowRun/AsyncShadowRun.csproj && \
    dotnet publish --nologo -c RELEASE -o /app \
        AsyncShadowRun/AsyncShadowRun.csproj

FROM mikefarah/yq:4 as resource
WORKDIR /data
COPY setup-resources.sh /data/setup-resources.sh
COPY --chown=yq ./resource ./resource/
RUN ./setup-resources.sh
    
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=builder /app /app
COPY --chown=root --from=resource /data/resource /app/resource
CMD [ "dotnet", "/app/AsyncShadowRun.dll" ]
