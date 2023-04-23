#!/bin/bash

#get a token here https://github.com/settings/tokens?type=beta  

mkdir gitsetup && cd gitsetup

wget https://github.com/git-ecosystem/git-credential-manager/releases/download/v2.0.935/gcm-linux_amd64.2.0.935.tar.gz -O gcm.tar.gz

tar -xzvf gcm.tar.gz

mv git-credential-manager /usr/local/bin/

# Install pass
sudo apt-get install -y pass

# Generate a GPG key
gpg --batch --generate-key <(echo "Key-Type: RSA
Key-Length: 4096
Name-Real: Your Name
Name-Email: yourname@example.com
Expire-Date: 0
%no-protection
")

# Initialize the password store
gpg_id=$(gpg --list-secret-keys --keyid-format LONG | grep "^sec" -A1 | tail -n1 | awk '{print $1}' | cut -d'/' -f2)
pass init "$gpg_id"
