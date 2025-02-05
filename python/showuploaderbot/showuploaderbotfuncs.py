import asyncio
import json
import mimetypes
import os
import pathlib
import random
import subprocess

import natsort

import discord

from ihatecircularimport import CCs, ensure_constants_py_exists
ensure_constants_py_exists()
from constants import *


def change_extension_to_mp4(filename: str) -> str:
    if len(pathlib.Path(filename).name.split(".")) == 0:
        return filename
    return f"{os.path.splitext(filename)[0]}.mp4"


def verify_show_folder_structure_from_path(show_path: str, show_name: str) -> bool:
    season_number = -1
    for season_folder_name in natsort.natsorted(os.listdir(show_path)):
        season_folder_path = os.path.join(show_path, season_folder_name)
        if os.path.isfile(season_folder_path):
            continue
        if not season_folder_name.startswith("s") or len(season_folder_name) != 3 or \
                not season_folder_name[1].isdigit() or not season_folder_name[2].isdigit():
            print(f"{CCs.FAIL}{show_name} has invalid folder structure, "
                  f"found folder of name {season_folder_name}{CCs.ENDC}")
            return False
        if season_number == -1:  # first pass
            season_number = extract_season_number_from_folder_name(season_folder_name)
            if season_number != 0 and season_number != 1:
                print(f"{CCs.FAIL}{show_name} has invalid folder structure, "
                      f"could not find the first season, found season {season_number}{CCs.ENDC}")
                return False
        else:
            new_season_number = extract_season_number_from_folder_name(season_folder_name)
            if new_season_number - 1 != season_number:
                print(f"{CCs.FAIL}{show_name} has invalid folder structure, "
                      f"missing seasons, went from season {season_number} to season {new_season_number}{CCs.ENDC}")
                return False
            season_number = new_season_number
    return True


def convert_str_or_bytes_to_str(txt: str | bytes) -> str:
    try:
        return txt.decode("utf-8")
    except (UnicodeDecodeError, AttributeError):
        return txt


def is_file_video(file_path: str) -> bool:
    return mimetypes.guess_type(file_path)[0].startswith('video')


def fix_season_folder_structure(show_folder_path):
    folders = []
    for i in os.listdir(show_folder_path):
        folder = os.path.join(show_folder_path, i)
        if os.path.isfile(folder):
            continue
        folders.append(folder)
    folders = natsort.natsorted(folders)
    extra_zero = ""
    if (len(folders)) >= 100:
        extra_zero += "0"
    for i, name in enumerate(folders):
        folder = os.path.join(show_folder_path, name)
        if i + 1 < 10:
            os.rename(folder, os.path.join(show_folder_path, f"s{extra_zero}0{i + 1}"))
            print(f"{name} -> s{extra_zero}0{i + 1}")
        elif i + 1 < 100:
            os.rename(folder, os.path.join(show_folder_path, f"s{extra_zero}{i + 1}"))
            print(f"{name} -> s{extra_zero}{i + 1}")
        elif i + 1 >= 100:
            os.rename(folder, os.path.join(show_folder_path, f"s{i + 1}"))
            print(f"{name} -> s{i + 1}")


async def wait_between_uploads(client) -> None:
    print(f"{CCs.OKCYAN}waiting {client.seconds_to_wait_between_uploads} seconds...{CCs.ENDC}")
    await asyncio.sleep(client.seconds_to_wait_between_uploads + random.randint(0, 60))


def save_progress_tracker(client: discord.Client) -> None:
    with open("progress_tracker.json", "w+") as f:
        json.dump(client.progress_tracker, f, indent=2)


def extract_season_number_from_folder_name(season_folder_name: str) -> int:
    # folder structure of the shows is "<show-name>/s<season number (2 digits)>/*episodes*"
    if not season_folder_name.startswith("s") or len(season_folder_name) != 3 or \
            not season_folder_name[1].isdigit() or not season_folder_name[2].isdigit():
        raise Exception(f"season_folder_name invalid, got {season_folder_name}")

    return int(season_folder_name[1:])


def get_subtitle_info_from_video(video_path: str, subtitle_index=0) -> dict:
    result = subprocess.run(
        ["ffprobe", "-v", "error", "-show_entries", "stream", "-of", "json", video_path],
        stdout=subprocess.PIPE, stderr=subprocess.DEVNULL)
    # print(result.stdout)
    stream_entries: dict = json.loads(result.stdout)
    if "streams" not in stream_entries.keys():
        raise Exception(f"could not get subtitle data from video: {video_path}")
    for stream_entry in stream_entries["streams"]:
        current_subtitle_index = 0
        # im pretty sure the output of this is always in order
        if stream_entry["codec_type"] == "subtitle":
            if current_subtitle_index == subtitle_index:
                return stream_entry
            current_subtitle_index += 1
    # raise Exception(f"could not find subtitle index {subtitle_index} from video: {video_path}")
    return {}


def subtitle_is_image_based(video_path: str, subtitle_index=0) -> bool:
    subtitle_data = get_subtitle_info_from_video(video_path, subtitle_index)
    if len(subtitle_data.keys()) == 0:
        return False  # the video has no subtitles probably
    if subtitle_data["codec_name"] in IMAGE_BASED_SUBTITLE_CODECS:
        return True
    if subtitle_data["codec_name"] in TEXT_BASED_SUBTITLE_CODECS:
        return False
    raise Exception(f"invalid subtitle codec name {subtitle_data['codec_name']} for video {video_path}")


def does_video_have_subtitles(video_path: str):
    subtitle_data = get_subtitle_info_from_video(video_path)
    if len(subtitle_data.keys()) == 0:
        return False  # the video has no subtitles probably
    return True


def get_ffmpeg_scale_string():
    if VIDEO_RESOLUTION[0] >= 0:
        width = f"'min({VIDEO_RESOLUTION[0]},iw)'"
    else:
        width = f"{VIDEO_RESOLUTION[0]}"
    if VIDEO_RESOLUTION[1] >= 0:
        height = f"'min({VIDEO_RESOLUTION[1]},ih)'"
    else:
        height = f"{VIDEO_RESOLUTION[1]}"
    return f"{width}:{height}"


def get_file_subtitle_from_video_path(video_path: str) -> pathlib.Path | None:
    vid_file = pathlib.Path(video_path)
    for file in vid_file.parent.iterdir():
        if file.name == vid_file.name:
            continue
        if str(file.name).startswith(os.path.splitext(vid_file.name)[0]):
            if len(file.suffix) == 0:
                continue
            if file.suffix[1:] in TEXT_BASED_SUBTITLE_CODECS:
                # we found a subtitle file for this file
                return file
            elif file.suffix[1:] in IMAGE_BASED_SUBTITLE_CODECS:
                raise NotImplementedError("havent implemented image based subtitles from file yet")
    return None


def get_ffmpeg_filter_complex_from_values(
        video_path: str,
        video_stream_id: int = 0,
        subtitle_stream_id: int | None = None,
        audio_stream_id: int = 0,
        subtitle_file_path: pathlib.Path | None = None) -> list[str]:
    returned_filter_complex = ["-filter_complex"]
    filter_complex_options = ""
    filter_complex_options += f"[0:v:{video_stream_id}]scale={get_ffmpeg_scale_string()}"
    if subtitle_file_path is not None:
        filter_complex_options += "[vs];[vs]"
        if subtitle_file_path.suffix[1:] in TEXT_BASED_SUBTITLE_CODECS:
            filter_complex_options += f"subtitles={subtitle_file_path}"
            # print(filter_complex_options)
        elif subtitle_file_path.suffix[1:] in IMAGE_BASED_SUBTITLE_CODECS:
            raise NotImplementedError("havent implemented image based subtitles from file yet")
    elif subtitle_stream_id is not None and does_video_have_subtitles(video_path):
        filter_complex_options += "[vs];[vs]"
        if subtitle_is_image_based(video_path, subtitle_stream_id):
            filter_complex_options += f"[0:s:{subtitle_stream_id}]overlay"
        else:
            filter_complex_options += f"subtitles={video_path}:si={subtitle_stream_id}"
    filter_complex_options += "[v]"
    returned_filter_complex += [filter_complex_options, "-map", "[v]", "-map", f"0:a:{audio_stream_id}"]
    # input(returned_filter_complex)
    return returned_filter_complex


def update_progress_tracker(show_name: str, season_folder_name: str, episode_number: int, client) -> None:
    if show_name not in client.progress_tracker.keys():
        client.progress_tracker[show_name] = {}
    client.progress_tracker[show_name][season_folder_name] = episode_number
    save_progress_tracker(client)


def ensure_config_json_exists() -> None:
    if not os.path.exists("config.json"):
        print(f"{CCs.FAIL}showuploaderbotfuncs.ensure_config_json_exists(): could not find config.json! "
              f"please copy and paste config.example.json as config.json and fill out the required fields!{CCs.ENDC}")
        exit(1)
