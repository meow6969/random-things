import json
import pathlib
import requests
import shutil
import sys

sys.path.append("../")

import showuploaderbotfuncs


class Config:
    token: str
    to_upload: pathlib.Path
    uploaded: pathlib.Path
    server_ip: str

    def __init__(self, config_path: pathlib.Path | str):
        with open(config_path, "r") as f:
            ff = json.load(f)
        self.token = ff["token"]
        self.to_upload = pathlib.Path(ff["files-to-upload-dir"])
        self.uploaded = pathlib.Path(ff["uploaded-files-dir"])
        self.to_upload.mkdir(parents=True, exist_ok=True)
        self.uploaded.mkdir(parents=True, exist_ok=True)
        self.server_ip = ff["server-ip"]


def upload_movie(config: Config, movie: pathlib.Path):
    filename = movie.name
    r = requests.post(
        f"http://{config.server_ip}/authed/new_movie",
        headers={"Authorization": f"Token {config.token}"},
        json={"movie": filename}
    )
    if not r.ok:
        print(f"error in initializing uploading movie {movie}!")
        print(r.content.decode())
        exit(r.status_code)
    with open(movie, "rb") as m_f:
        g = requests.post(
            f"http://{config.server_ip}/authed/upload/{r.json()['id']}",
            headers={"Authorization": f"Token {config.token}", "Content-Type": "video/"},
            data=m_f.read()
        )
    if not g.ok:
        print(f"error in uploading movie {movie}!")
        print(g.content.decode())
        exit(g.status_code)
    move_dir = config.uploaded.joinpath("MOVIES")
    move_dir.mkdir(parents=True, exist_ok=True)
    shutil.move(movie, move_dir.joinpath(filename))
    print(f"successfully uploaded movie {movie}!")


def upload_episode(config: Config, episode: pathlib.Path, show: pathlib.Path, season: int, ep_num: int):
    r = requests.post(
        f"http://{config.server_ip}/authed/new_episode",
        headers={"Authorization": f"Token {config.token}"},
        json={"show": show.name, "season": season, "episode": ep_num, "file_name": episode.name}
    )
    if not r.ok:
        print(f"error in initializing uploading episode {episode}!")
        print(r.content.decode())
        exit(r.status_code)
    with open(episode, "rb") as e_f:
        g = requests.post(
            f"http://{config.server_ip}/authed/upload/{r.json()['id']}",
            headers={"Authorization": f"Token {config.token}", "Content-Type": "video/"},
            data=e_f.read()
        )
    if not g.ok:
        print(f"error in uploading episode {episode}!")
        print(g.content.decode())
        exit(g.status_code)
    move_dir = config.uploaded.joinpath(show.name).joinpath(showuploaderbotfuncs.season_number_to_folder(season))
    move_dir.mkdir(parents=True, exist_ok=True)
    shutil.move(episode, move_dir.joinpath(episode.name))
    print(f"successfully uploaded episode {episode}!")


def main():
    config = Config("./clientconfig.json")
    # pathlib.Path.iterdir()

    for folder in config.to_upload.iterdir():
        if not folder.is_dir():
            continue
        # print(type(folder))
        if folder.name == "MOVIES":
            for movie in folder.iterdir():
                if movie.is_dir():
                    continue
                upload_movie(config, movie)
            continue
        show = folder.name
        for season_folder in folder.iterdir():
            if not season_folder.is_dir():
                continue
            s_num = showuploaderbotfuncs.extract_season_number_from_folder_name(season_folder.name)
            for episode in season_folder.iterdir():
                e_s_num, e_num = showuploaderbotfuncs.extract_season_and_episode_number_from_video_name(episode.name)
                if e_s_num != s_num:
                    print(f"season number for {episode} does not match with season folder number {season_folder.name}!")
                    exit(1)
                upload_episode(config, episode, folder, s_num, e_num)



if __name__ == "__main__":
    main()
