import aiohttp
import aiofiles
from aiohttp import web
import asyncio
import json
import logging
import functools
from typing import Callable, Coroutine, Any
from discord.ext import commands
import pathlib

from sharedmodels import *
# from constants import PORT, VERSION, CLIENT_MAX_SIZE
import constants


routes: web.RouteTableDef = web.RouteTableDef()


class ShowSendingServer:
    uploads: list[ClientUpload] = []
    r: ServerResponse = ServerResponse(constants.VERSION)

    users_dict: dict[str, str]
    # client: commands.Bot
    app: web.Application

    def __init__(self) -> None:
        logger = logging.getLogger(__name__)
        logging.basicConfig(format="[%(asctime)s.%(msecs)03d] p%(process)s {%(pathname)s: %(funcName)s: %(lineno)d}: "
                                   "%(levelname)s: %(message)s", datefmt="%Y-%m-%d %p %I:%M:%S")
        logger.setLevel(10)
        # constants.BOT_CLIENT = client
        constants.SHOW_SENDING_SERVER = self
        # constants.FILES_TO_CONVERT_DIR = pathlib.Path(client.video_converter_settings['files-to-convert-dir'])
        # self.ev_loop = asyncio.get_event_loop()

        with open(pathlib.Path(__file__).parent.resolve().joinpath("users.json"), "r") as f:
            self.users_dict = json.load(f)
        # self.client = client
        # self.client.loop.
        self.app = web.Application(middlewares=[self.token_auth_middleware(self.user_loader)],
                                   client_max_size=constants.CLIENT_MAX_SIZE)
        self.app.add_routes(routes)

    def start(self) -> None:
        web.run_app(self.app, port=constants.PORT)
        # i think this is ok ???????????????????
        # ??????????????????????????????????????????????????????????????????????????????????????????????
        # await web._run_app(self.app, port=PORT)
        # im a fricking idiot i can just do client.loop
        # await self.client.loop.run_in_executor(None, web.run_app, )
        # await constants.BOT_CLIENT.loop.run_in_executor(None, functools.partial(web.run_app,
        #                                                                         self.app, port=constants.PORT))

        # asyncio.get_running_loop().run_in_executor(None, functools.partial(web.run_app, self.app, port=constants.PORT))

    async def get_upload_from_id(self, u_id: str) -> ClientUpload | None:
        for u in self.uploads:
            if str(u.id) == u_id:
                return u
        return None

    async def user_loader(self, token: str) -> ClientUser:
        user = ClientUser(token) if token in self.users_dict.keys() else None
        return user

    # https://superkogito.github.io/blog/2021/12/31/aiohttp_server_with_token.html
    def token_auth_middleware(self, u_loader: Callable, auth_scheme: str = "Token") \
            -> Callable[[web.Request, Callable], Coroutine[Any, Any, Any]]:

        @web.middleware
        async def middleware(request: web.Request, handler):
            if not request.path.startswith("/authed"):
                return await handler(request)

            try:
                scheme, token = request.headers['Authorization'].strip().split(' ')
            except KeyError:
                # raise web.HTTPUnauthorized(reason='Missing authorization header', )
                return self.r.new("error", ResponseError.AUTHORIZATION_MISSING)
            except ValueError:
                return self.r.new("error", ResponseError.AUTHORIZATION_INVALID)

            if auth_scheme.lower() != scheme.lower():
                return self.r.new("error", ResponseError.AUTHORIZATION_SCHEME_INVALID)

            user = await u_loader(token)
            if user:
                # setattr(request, request_property, user)
                request.user = user
                # request[request_property] = user
            else:
                return self.r.new("error", ResponseError.AUTHORIZATION_TOKEN_INVALID)
            return await handler(request)

        return middleware


constants.SHOW_SENDING_SERVER = ShowSendingServer()


@routes.get("/")
async def about(request: web.Request):
    print(type(request))
    return constants.SHOW_SENDING_SERVER.r.new("hello!")


@routes.get("/authed")
async def about_auth(request: web.Request):
    return constants.SHOW_SENDING_SERVER.r.new(f"hello, {request.user.name}!")


@routes.get("/authed/")
async def about_auth2(request: web.Request):
    return await about_auth(request)


@routes.get("/authed/uploads")
async def get_uploads(request: web.Request):
    r_list = []
    for cu in constants.SHOW_SENDING_SERVER.uploads:
        r_list.append(cu.to_dict())
    return constants.SHOW_SENDING_SERVER.r.new("ok", extras={"uploads": r_list})


@routes.post("/authed/new_episode")
async def create_upload(request: web.Request):
    upload = await ClientEpisodeUpload.from_request(request)
    if isinstance(upload, web.Response):
        return upload
    constants.SHOW_SENDING_SERVER.uploads.append(upload)
    return constants.SHOW_SENDING_SERVER.r.new("ok", extras=upload.to_dict())


@routes.post("/authed/new_movie")
async def create_movie_upload(request: web.Request):
    upload = await ClientMovieUpload.from_request(request)
    if isinstance(upload, web.Response):
        return upload
    constants.SHOW_SENDING_SERVER.uploads.append(upload)
    return constants.SHOW_SENDING_SERVER.r.new("ok", extras=upload.to_dict())


@routes.post("/authed/upload/{id}")
async def upload_file(request: web.Request):
    up = None
    # print(request.can_read_body())
    try:
        up_id = request.match_info["id"]
        up = await constants.SHOW_SENDING_SERVER.get_upload_from_id(up_id)
        if up is None:
            return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.INVALID_UPLOAD_ID)
        assert isinstance(up, ClientUpload)
        if not request.can_read_body:
            return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.BODY_MISSING)
        if request.content_length >= constants.CLIENT_MAX_SIZE:
            return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.CONTENT_TOO_LONG)
        if not request.content_type.startswith("video/"):
            return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.INVALID_FILE_TYPE)
        up.status = UploadStatus.UPLOADING
        up.file_path.parent.mkdir(parents=True, exist_ok=True)
        async with aiofiles.open(up.file_path, mode="wb+") as f:
            await f.write(await request.read())
        up.status = UploadStatus.UPLOADED
    except Exception as e:
        if up is not None:
            up.status = UploadStatus.FAILED
        return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.UPLOAD_ERROR, f"{e}")
    return constants.SHOW_SENDING_SERVER.r.new("ok")


if __name__ == "__main__":
    show_server = ShowSendingServer()
    show_server.start()

