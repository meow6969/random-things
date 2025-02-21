import aiohttp
from aiohttp import web
import asyncio
import json
import logging
from typing import Callable, Coroutine, Any

from sharedmodels import *
from constants import r


logger = logging.getLogger(__name__)
logging.basicConfig(format="[%(asctime)s.%(msecs)03d] p%(process)s {%(pathname)s: %(funcName)s: %(lineno)d}: "
                           "%(levelname)s: %(message)s", datefmt="%Y-%m-%d %p %I:%M:%S")
logger.setLevel(10)


routes = web.RouteTableDef()
uploads: list[ClientEpisodeUpload] = []


async def user_loader(token: str) -> ClientUser:
    user = ClientUser(token) if token in USERS_DICT.keys() else None
    return user


# https://superkogito.github.io/blog/2021/12/31/aiohttp_server_with_token.html
def token_auth_middleware(u_loader: Callable,
                          request_property: str = "user",
                          auth_scheme: str = "Token",
                          exclude_routes: Tuple = tuple("/"),
                          exclude_methods: Tuple = tuple()) -> Callable[[{headers}, Any], Coroutine[Any, Any, Any]]:
    @web.middleware
    async def middleware(request: web.Request, handler):
        if request.path in exclude_routes:
            return await handler(request)

        try:
            scheme, token = request.headers['Authorization'].strip().split(' ')
        except KeyError:
            # raise web.HTTPUnauthorized(reason='Missing authorization header', )
            return r.new("error", ResponseError.AUTHORIZATION_MISSING)
        except ValueError:
            return r.new("error", ResponseError.AUTHORIZATION_INVALID)

        if auth_scheme.lower() != scheme.lower():
            return r.new("error", ResponseError.AUTHORIZATION_SCHEME_INVALID)

        user = await u_loader(token)
        if user:
            request[request_property] = user
        else:
            return r.new("error", ResponseError.AUTHORIZATION_TOKEN_INVALID)
        return await handler(request)

    return middleware


@routes.get("/")
async def about(request: web.Request):
    return r.new("hello!")


@routes.post("/authed/new_episode")
async def create_upload(request: web.Request):
    upload = ClientEpisodeUpload.from_request(request)


@routes.post("/authed/new_movie")
async def create_upload(request: web.Request):
    upload = ClientUpload.from_request(request)


if __name__ == "__main__":
    app = web.Application(middlewares=[token_auth_middleware(user_loader)])
    app.add_routes(routes)
    web.run_app(app, port=6950)

