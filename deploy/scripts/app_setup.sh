#!/usr/bin/env bash
set -e

rm -rf /var/www/api
ln -s /var/www/api-latest/build_output /var/www/api

chmod -R 775 /var/www/api-latest/build_output

cd /var/www/api-latest
cp deploy/conf/kestrel-g3.service /etc/systemd/system/kestrel-g3.service
systemctl enable kestrel-g3.service