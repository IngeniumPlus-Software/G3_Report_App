#!/bin/bash

echo "----- Setting up environment variables -----"

EC2_AVAIL_ZONE=`curl -s http://169.254.169.254/latest/meta-data/placement/availability-zone`
EC2_REGION="`echo \"$EC2_AVAIL_ZONE\" | sed 's/[a-z]$//'`"

echo "Setting up envs for $SECRET_NAME into appsettings.json"

if [[ -n "$STACK_SECRET_NAME" ]]; then

  # Get secrets from Secret Manager
  SM_ENVS=$(aws secretsmanager get-secret-value --region "$EC2_REGION" --secret-id "$STACK_SECRET_NAME" --query SecretString --output text)

  echo "$SM_ENVS" > /var/www/api/appsettings.json
  echo "export STACK_SECRET_NAME=$STACK_SECRET_NAME" > /etc/environment

else
  echo "No environment vars to set, missing secret"
fi

