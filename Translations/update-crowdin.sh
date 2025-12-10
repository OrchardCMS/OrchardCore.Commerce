#!/bin/sh

PROJECT_ID="$1"
PERSONAL_TOKEN="$2"

# Initialize the configuration file from the template.
echo "\"project_id\": \"$PROJECT_ID\"" > crowdin.yml
echo "\"api_token\": \"$PERSONAL_TOKEN\"" >> crowdin.yml
cat crowdin.yml.template >> crowdin.yml

crowdin upload sources
crowdin download
