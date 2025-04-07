import http
import pathlib
import uuid
from aiohttp import web
from enum import Enum
from typing import Self
import sys
import json

sys.path.append("../")

# from constants import r, USERS_DICT, FILENAME_ILLEGAL_CHARS, MIN_FILENAME_LENGTH
import constants
from ihatecircularimport import CCs
from showuploaderbotfuncs import season_number_to_folder, get_episode_file_name
# from showserverfuncs import check_if_name_is_ok
import showserverfuncs


class ClientUser:
    name: str
    token: str

    def __init__(self, token: str):
        self.name = constants.SHOW_SENDING_SERVER.users_dict[token]
        self.token = token

    def to_dict(self):
        return {
            "name": self.name
        }


class UploadStatus(Enum):
    INITIALIZED = 0
    UPLOADING = 1
    UPLOADED = 2
    FAILED = 3


class ResponseError(Enum):
    MISC_ERROR = 0
    SHOW_MISSING = 1
    SEASON_MISSING = 2
    EPISODE_MISSING = 3
    UPLOAD_ID_MISSING = 4
    INTERNAL_ERROR = 5
    AUTHORIZATION_MISSING = 6
    AUTHORIZATION_INVALID = 7
    AUTHORIZATION_SCHEME_INVALID = 8
    AUTHORIZATION_TOKEN_INVALID = 9
    MOVIE_MISSING = 10
    INVALID_UPLOAD_ID = 11
    FILE_NAME_MISSING = 12
    INVALID_FILE_NAME = 13
    BODY_MISSING = 14
    CONTENT_TOO_LONG = 15
    INVALID_FILE_TYPE = 16
    UPLOAD_ERROR = 17

    def __str__(self):
        match self:
            case self.SHOW_MISSING:
                return "show field was not found in response body"
            case self.SEASON_MISSING:
                return "season field was not found in response body"
            case self.EPISODE_MISSING:
                return "episode field was not found in response body"
            case self.UPLOAD_ID_MISSING:
                return "upload_id field was not found in response body"
            case self.INTERNAL_ERROR:
                return "internal error with server, try again later"
            case self.MOVIE_MISSING:
                return "movie field was not found in response body"
            case self.INVALID_UPLOAD_ID:
                return "given upload id is not found on the server, try registering a new upload"
            case self.FILE_NAME_MISSING:
                return "file_name field was not found in response body"
            case self.INVALID_FILE_NAME:
                return (f"invalid file name field was found in response body\n"
                        f"file names must be {constants.MIN_FILENAME_LENGTH} characters long, "
                        f"made up of ASCII characters, "
                        f"cannot have these characters: {constants.FILENAME_ILLEGAL_CHARS}, "
                        f"and must be of these case-sensitive extensions: {constants.FILENAME_SUPPORTED_EXTENSIONS}")
            case self.BODY_MISSING:
                return "body field was not found in response body"
            case self.CONTENT_TOO_LONG:
                return "request content length is too long"
            case self.INVALID_FILE_TYPE:
                return "given file type is not acceptable"
            case self.UPLOAD_ERROR:
                return "upload error with server, try again later"
            case _:
                return self.name

    def get_status_code(self) -> int:
        match self:
            case self.SHOW_MISSING | self.UPLOAD_ID_MISSING | self.EPISODE_MISSING | self.SEASON_MISSING | \
                    self.UPLOAD_ID_MISSING | self.MISC_ERROR | self.MOVIE_MISSING | self.INVALID_UPLOAD_ID | \
                    self.FILE_NAME_MISSING | self.INVALID_FILE_NAME | self.BODY_MISSING | self.CONTENT_TOO_LONG | \
                    self.INVALID_FILE_TYPE:
                return http.HTTPStatus.BAD_REQUEST
            case self.INTERNAL_ERROR:
                return http.HTTPStatus.INTERNAL_SERVER_ERROR
            case self.AUTHORIZATION_MISSING | self.AUTHORIZATION_INVALID | self.AUTHORIZATION_SCHEME_INVALID | \
                    self.AUTHORIZATION_TOKEN_INVALID:
                return http.HTTPStatus.UNAUTHORIZED
            case _:
                return http.HTTPStatus.BAD_REQUEST


class ServerResponse:
    headers: dict
    version: str

    def __init__(self, server_version: str):
        self.headers = {
            "Content-Type": "application/json",
            "ShowServer": f"showserver/{server_version}"
        }

    def new(self, message: str | None = None, error: ResponseError | None = None, error_message: str | None = None,
            status_code: int = 200, extras: dict | None = None) -> web.Response:
        did_something = False
        body = extras if extras else {}
        if extras:
            did_something = True
        if message:
            did_something = True
            body["message"] = message
        if error:
            did_something = True
            body["error_id"] = error.value
            if error_message:
                body["error_message"] = error_message
            else:
                body["error_message"] = f"{error}"
            status_code = error.get_status_code()

        if not did_something:
            body = {
                "message": "error",
                "error_id": ResponseError.INTERNAL_ERROR,
                "error_message": f"{ResponseError.INTERNAL_ERROR}"
            }
            status_code = ResponseError.INTERNAL_ERROR.get_status_code()
            print(f"{CCs.FAIL}ServerResponse.new() did nothing!\n"
                  f"{ResponseError.INTERNAL_ERROR.name}{CCs.ENDC}")

        return web.Response(body=json.dumps(body, indent=2), headers=self.headers, status=status_code)


class ClientUpload:
    user: ClientUser
    id: str
    file_path: pathlib.Path
    status: UploadStatus = UploadStatus.INITIALIZED

    def __init__(self, user: ClientUser, file_path: pathlib.Path, upload_id: str | None):
        self.user = user
        if upload_id is None:
            self.id = uuid.uuid4().hex
        else:
            self.id = upload_id
        # we assume the file path has been sanitized as input
        self.file_path = file_path

    def to_dict(self) -> dict:
        return {
            "user": self.user.to_dict(),
            "id": self.id,
            "file_path": str(self.file_path),
            "status": self.status.name,
        }

    def get_savepath(self, *args) -> pathlib.Path:
        raise NotImplementedError


class ClientEpisodeUpload(ClientUpload):
    show: str
    season: int
    episode: int

    def __init__(self, show: str, episode: int, season: int, user: ClientUser, file_name: str,
                 upload_id: str | None):
        self.show = show
        self.episode = episode
        self.season = season
        super().__init__(user, self.get_savepath(file_name), upload_id)

    def get_savepath(self, file_name: str) -> pathlib.Path:
        return (pathlib.Path(f"{constants.FILES_TO_CONVERT_DIR}")
                .joinpath(self.show)
                .joinpath(season_number_to_folder(self.season))
                .joinpath(get_episode_file_name(self.season, self.episode, file_name)))

    def to_dict(self) -> dict:
        m = super().to_dict()
        m["show"] = self.show
        m["season"] = self.season
        m["episode"] = self.episode
        return m

    @staticmethod
    async def from_request(request: web.Request) -> web.Response | Self:
        try:
            data = await request.json()
            if "show" in data:
                show = str(data["show"])
                if not showserverfuncs.check_if_name_is_ok(show) or show == "MOVIES":
                    return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.INVALID_FILE_NAME)
            else:
                return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.SHOW_MISSING)
            print("show success")
            if "episode" in data:
                episode = int(data["episode"])
            else:
                return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.EPISODE_MISSING)
            print("episode success")
            if "season" in data:
                season = int(data["season"])
            else:
                return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.SEASON_MISSING)
            print("season success")
            if "upload_id" in data:
                try:
                    upload_id = uuid.UUID(data["upload_id"]).hex
                except ValueError:
                    return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.INVALID_UPLOAD_ID)
            else:
                upload_id = uuid.uuid4().hex
                # return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.UPLOAD_ID_MISSING)
            print("upload_id success")
            if "file_name" in data:
                file_name = data["file_name"]
                if not showserverfuncs.check_if_name_is_ok(file_name, require_extension=True):
                    return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.INVALID_FILE_NAME)
            else:
                return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.FILE_NAME_MISSING)
            print("file_name success")
            return ClientEpisodeUpload(show, episode, season, request.user, file_name, upload_id)
        except Exception as e:
            return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.MISC_ERROR, str(e))


class ClientMovieUpload(ClientUpload):
    movie: str

    def __init__(self, movie: str, user: ClientUser, upload_id: str | None):
        self.movie = movie
        super().__init__(user, self.get_savepath(), upload_id)

    def get_savepath(self) -> pathlib.Path:
        return (pathlib.Path(f"{constants.FILES_TO_CONVERT_DIR}")
                .joinpath("MOVIES")
                .joinpath(self.movie))

    def to_dict(self) -> dict:
        m = super().to_dict()
        m["movie"] = self.movie
        return m

    @staticmethod
    async def from_request(request: web.Request) -> web.Response | Self:
        try:
            data = await request.json()
            if "movie" in data:
                movie = str(data["movie"])
                if not showserverfuncs.check_if_name_is_ok(movie, require_extension=True):
                    # print("movie name not ok")
                    return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.INVALID_FILE_NAME)
            else:
                return r.new("error", ResponseError.MOVIE_MISSING)
            if "upload_id" in data:
                try:
                    upload_id = uuid.UUID(data["upload_id"]).hex
                except ValueError:
                    return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.INVALID_UPLOAD_ID)
            else:
                upload_id = uuid.uuid4().hex
            return ClientMovieUpload(movie, request.user, upload_id)
        except Exception as e:
            return constants.SHOW_SENDING_SERVER.r.new("error", ResponseError.MISC_ERROR, str(e))
