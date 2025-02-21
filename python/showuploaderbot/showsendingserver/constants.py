import json


from sharedmodels import ServerResponse


VERSION = "0.0.1"
with open("users.json", "r") as f:
    USERS_DICT: dict[str, str] = json.load(f)

# singletons
r = ServerResponse(VERSION)

