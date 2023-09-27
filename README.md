# Draw together

## Made with

![Svelte](https://img.shields.io/badge/svelte-%23f1413d.svg?style=for-the-badge&logo=svelte&logoColor=white) ![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white) ![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white) ![Nginx](https://img.shields.io/badge/nginx-%23009639.svg?style=for-the-badge&logo=nginx&logoColor=white)

## Run the project locally

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/)

### Steps

1. Clone the repository
2. Run `docker-compose up --build` in the root directory

## Run the project locally without Docker

### Prerequisites

- [.NET 7.0 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [Node.js](https://nodejs.org/en/download/)

### Steps

1. Clone the repository
2. Run `npm install` in the `client` directory
3. Run `npm run dev` in the `client` directory
4. Run `dotnet run` in the `server` directory

## Deploy the project

### Prerequisites

- [Docker](https://docs.docker.com/engine/install/ubuntu/)
- Nginx `sudo apt install nginx`
- firewalld `sudo apt install firewalld`

### Steps

1. Clone the repository
2. Run `docker-compose up --build` in the root directory
3. Create a new file in `/etc/nginx/conf.d/` with the following content:

```nginx
# /etc/nginx/conf.d/reverse-proxy.conf

server {
    # HTTP Port: 80
    listen 80;
    
    # External IP Adress
    server_name xxx.xxx.xxx.xxx;
 
    location / {
        # Internal node server ip and port
        proxy_pass http://127.0.0.1:5173;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }

    location /ws/canvas {
        # Internal c# server ip and port
        proxy_pass http://127.0.0.1:5143;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "Upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
}
```

4. Run `sudo nginx -t` to check if the configuration is valid
5. Run `sudo systemctl restart nginx` to restart the nginx service
6. Run `sudo firewall-cmd --permanent --zone=public --add-port=80/tcp` to open the port 80
7. Run `sudo firewall-cmd --reload` to reload the firewall
9. Run `sudo firewall-cmd --list-all` to check if the port 80 is open

### Add https (optional)

You can do this with [certbot](#certbot) or create a [self signed certificate](#self-signed-certificate) yourself.

#### Certbot [(certbot.eff.org)](https://certbot.eff.org/instructions?ws=nginx&os=ubuntufocal)

For this to work you need to have a domain name and a DNS record pointing to the server.

##### Steps

1. Install snapd with `sudo apt install snapd` if you haven't already
2. Install certbot with `sudo snap install --classic certbot`
3. Run `sudo ln -s /snap/bin/certbot /usr/bin/certbot`
4. Run `sudo certbot --nginx` and follow the instructions
5. Run `sudo certbot renew --dry-run` to check if the certificate will be renewed automatically

#### Self signed certificate [(digialocean.com)](https://www.digitalocean.com/community/tutorials/how-to-create-a-self-signed-ssl-certificate-for-nginx-in-ubuntu-20-04-1)

##### Steps

1. Run `sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout /etc/ssl/private/nginx-selfsigned.key -out /etc/ssl/certs/nginx-selfsigned.crt` to create the certificate
2. Run `sudo openssl dhparam -out /etc/nginx/dhparam.pem 4096` to create the Diffie-Hellman group
3. Create a new file in `/etc/nginx/snippets/self-signed.conf` with the following content:

```nginx
# /etc/nginx/snippets/self-signed.conf

ssl_certificate /etc/ssl/certs/nginx-selfsigned.crt;
ssl_certificate_key /etc/ssl/private/nginx-selfsigned.key;
```

4. Create a new file in `/etc/nginx/snippets/ssl-params.conf` with the following content:

```nginx
# /etc/nginx/snippets/ssl-params.conf

ssl_protocols TLSv1.3;
ssl_prefer_server_ciphers on;
ssl_dhparam /etc/nginx/dhparam.pem; 
ssl_ciphers EECDH+AESGCM:EDH+AESGCM;
ssl_ecdh_curve secp384r1;
ssl_session_timeout  10m;
ssl_session_cache shared:SSL:10m;
ssl_session_tickets off;
ssl_stapling on;
ssl_stapling_verify on;
resolver 8.8.8.8 8.8.4.4 valid=300s;
resolver_timeout 5s;
# Disable strict transport security for now. You can uncomment the following
# line if you understand the implications.
# add_header Strict-Transport-Security "max-age=63072000; includeSubDomains; preload";
add_header X-Frame-Options DENY;
add_header X-Content-Type-Options nosniff;
add_header X-XSS-Protection "1; mode=block";
```

5. Update the `/etc/nginx/conf.d/reverse-proxy.conf` file with the following content:

```nginx
# /etc/nginx/conf.d/reverse-proxy.conf

server {
    # HTTPS Port: 443
    listen 443 ssl;

    # External IP Adress
    server_name xxx.xxx.xxx.xxx;

    # Include conf files
    include snippets/self-signed.conf;
    include snippets/ssl-params.conf;
 
    location / {
        # Internal node server ip and port
        proxy_pass http://127.0.0.1:5173;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }

    location /ws/canvas {
        # Internal c# server ip and port
        proxy_pass http://127.0.0.1:5143;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "Upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
}

server {
    # HTTP Port: 80
    listen 80;
    
    # External IP Adress
    server_name xxx.xxx.xxx.xxx;
 
    return 301 https://$server_name$request_uri;
}
```

6. Run `sudo nginx -t` to check if the configuration is valid
7. Run `sudo systemctl restart nginx` to restart the nginx service
8. Run `sudo firewall-cmd --permanent --zone=public --add-port=443/tcp`
9. Run `sudo firewall-cmd --reload` to reload the firewall
10. Run `sudo firewall-cmd --list-all` to check if the port 443 is open
