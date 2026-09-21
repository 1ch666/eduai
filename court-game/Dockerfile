FROM nginx:stable-alpine
COPY docker/nginx.conf /etc/nginx/nginx.conf
COPY --chown=nginx:nginx Builds/WebGL/ /usr/share/nginx/html/
# Fail honestly if Unity has not produced a playable build.
RUN test -s /usr/share/nginx/html/index.html && \
    find /usr/share/nginx/html/Build -name '*.wasm' | grep -q .
USER nginx
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s \
  CMD wget -q -O /dev/null http://127.0.0.1:8080/healthz || exit 1
CMD ["nginx", "-g", "daemon off;"]
