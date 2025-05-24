import shutil

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
import threading
import re
import time
import subprocess

from sharedmodels import *
from showuploaderbotfuncs import hash_dict_or_list
# from constants import PORT, VERSION, CLIENT_MAX_SIZE
import constants


routes: web.RouteTableDef = web.RouteTableDef()


class ShowSendingServer:
    uploads: list[ClientUpload] = []
    r: ServerResponse = ServerResponse(constants.VERSION)

    users_dict: dict[str, str]
    # client: commands.Bot
    app: web.Application
    do_the_upnp_task: bool = False

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

    def do_upnp_task(self):
        self.do_the_upnp_task = True
        verify_ip_regex = re.compile(r"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$")
        get_duration_regex = re.compile(r"(?<=\(duration=)[\d]*?(?=\))")

        local_ip = subprocess.check_output(["hostname", "-i"]).decode().strip()
        if not verify_ip_regex.match(local_ip):
            raise Exception(f"Invalid local ip: {local_ip}")
        do_upnp = ["upnpc", "-e", "show sending server tcp", "-a", local_ip, f"{constants.PORT}", f"{constants.PORT}",
                   "TCP"]

        while self.do_the_upnp_task:
            upnp_out = subprocess.check_output(do_upnp).decode().strip()
            try:
                upnp_rule_duration = int(get_duration_regex.search(upnp_out)[0])
                print(f"applied upnp rule, sleeping for duration={upnp_rule_duration}s")
                time.sleep(upnp_rule_duration)
            except IndexError:
                print(f"error applying upnp rule: invalid upnp_out:\n"
                      f"{upnp_out}")
                exit(1)

    def start(self) -> None:
        upnp_thread = None
        if constants.DO_UPNP:
            upnp_thread = threading.Thread(target=self.do_upnp_task)
            upnp_thread.start()
        web.run_app(self.app, port=constants.PORT)
        self.do_the_upnp_task = False
        if upnp_thread is not None and upnp_thread.is_alive():
            upnp_thread.join()
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


@routes.post("/authed/filter_complex_builder")
async def sync_filter_complex_builder(request: web.Request):
    data = await request.json()

    if constants.FILTER_COMPLEX_BUILDER_JSON_PATH.exists():
        with open(constants.FILTER_COMPLEX_BUILDER_JSON_PATH, "r") as f:
            previous = json.load(f)

        saved = False
        i = 1
        while not saved:
            bk_filepath = constants.FILTER_COMPLEX_BUILDER_JSON_PATH.parent.joinpath(
                f"{i}_{constants.FILTER_COMPLEX_BUILDER_JSON_PATH}")
            if bk_filepath.exists():
                i += 1
                continue
            shutil.move(constants.FILTER_COMPLEX_BUILDER_JSON_PATH, bk_filepath)
            saved = True
        # TODO: make this better this sucks
        for thingy_key in data.keys():
            if thingy_key not in previous:
                previous[thingy_key] = data[thingy_key]
                continue
            if thingy_key == "MOVIES":
                if "filter-complex-regex-filename" not in data[thingy_key]:
                    continue
                if "regex-actions" not in data[thingy_key]["filter-complex-regex-filename"]:
                    continue
                previous_regex_hashes = []
                for regex_action in previous[thingy_key]["filter-complex-regex-actions"]:
                    previous_regex_hashes.append(hash_dict_or_list(regex_action))
                for regex_action in data[thingy_key]["filter-complex-regex-actions"]["regex-actions"]:
                    regex_hash = hash_dict_or_list(regex_action)
                    if regex_hash not in previous_regex_hashes:
                        previous_regex_hashes.append(regex_hash)
                        previous[thingy_key]["filter-complex-regex-actions"].append(regex_action)
        data = previous

    with open(constants.FILTER_COMPLEX_BUILDER_JSON_PATH, "w+") as f:
        json.dump(data, f)
    return constants.SHOW_SENDING_SERVER.r.new("ok")


if __name__ == "__main__":
    show_server = ShowSendingServer()
    show_server.start()

