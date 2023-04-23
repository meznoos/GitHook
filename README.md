## This tool is used for auto pull your github repositories when you push new code.

1. download GitHook from release
2. run gitkey.sh on your server
3. get a token here https://github.com/settings/tokens?type=beta
4. clone your code, select `2. Personal access token` , and paste the token
5. run `./GitHook /path/to/you/code`
6. create a github webhook in settings
    ```
   Payload URL: http://yourip:10822/
   Content type: application/json
   Secret: no
   Just the push event.
   ```

After that, when you push your code to github, your server will auto pull the code.

You can add it to systemd:

```bash
cat > /etc/systemd/system/YourGithookName.service  <<EOF
[Unit]
Description=githook
[Service]
User=root
Group=root
ExecStart=/usr/local/bin/GitHook /path/to/you/code
Restart=always
RestartSec=2
[Install]
WantedBy=multi-user.target
EOF
```