import http
import uuid
from aiohttp import web
from enum import Enum

from constants import r, USERS_DICT
from ..ihatecircularimport import CCs


class ClientUser:
    name: str
    token: str

    def __init__(self, token: str):
        self.name = USERS_DICT[token]
        self.token = token


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

    def __str__(self):
        match self:
            case self.SHOW_MISSING:
                return "show field was not found in response body"
            case self.SEASON_MISSING:
                return "season field was not found in response body"
            case self.EPISODE_MISSING:
                return "show field was not found in response body"
            case self.UPLOAD_ID_MISSING:
                return "upload_id field was not found in response body"
            case self.INTERNAL_ERROR:
                return "internal error with server, try again later"
            case _:
                return self.name

    def get_status_code(self) -> int:
        match self:
            case self.SHOW_MISSING | self.UPLOAD_ID_MISSING | self.EPISODE_MISSING | self.SEASON_MISSING | \
                 self.UPLOAD_ID_MISSING | self.MISC_ERROR:
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
            status_code: int = 200):
        did_something = False
        body = {}
        if message:
            did_something = True
            body = {
                "message": message
            }
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
    id: uuid

    def __init__(self, user: ClientUser, upload_id: uuid | None):
        self.user = user
        if upload_id is None:
            self.id = uuid.uuid4()


class ClientEpisodeUpload(ClientUpload):
    show: str
    season: int
    episode: int

    def __init__(self, show: str, episode: int, season: int, user: ClientUser, upload_id: uuid | None):
        super().__init__(user, upload_id)
        self.show = show
        self.episode = episode
        self.season = season
        if upload_id is None:
            self.upload_id = uuid.uuid4()

    @staticmethod
    def from_request(request: web.Request) -> web.Response | ClientEpisodeUpload:
        try:
            data = request.json()
            if "show" in data:
                show = str(data["show"])
            else:
                return r.new("error", ResponseError.SHOW_MISSING)
            if "episode" in data:
                episode = int(data["episode"])
            else:
                return r.new("error", ResponseError.SHOW_MISSING)
            if "season" in data:
                season = int(data["season"])
            else:
                return r.new("error", ResponseError.SEASON_MISSING)
            if "upload_id" in data:
                upload_id = uuid.UUID(data["upload_id"])
            else:
                return r.new("error", ResponseError.UPLOAD_ID_MISSING)
            return ClientEpisodeUpload(show, episode, season, upload_id)
        except Exception as e:
            return r.new("error", ResponseError.MISC_ERROR, str(e))
