from enum import Enum
import json
import os
import pathlib
import re
import subprocess

import natsort

import discord

from showuploaderbotfuncs import (change_extension_to_mp4, convert_str_or_bytes_to_str,
                                  extract_season_number_from_folder_name, get_ffmpeg_filter_complex_from_values,
                                  get_file_subtitle_from_video_path, is_file_video, update_progress_tracker,
                                  verify_show_folder_structure_from_path, wait_between_uploads,
                                  extract_ep_compare_num_from_video_name)
from ihatecircularimport import CCs, ensure_constants_py_exists
ensure_constants_py_exists()
from constants import *


class TvShowMetadata:
    metadata_dict: dict
    send_episode_number: bool

    def __init__(self, show_path):
        self.metadata_dict = self.get_show_metadata_dict(show_path)
        if "send-episode-number" in self.metadata_dict.keys():
            self.send_episode_number = self.metadata_dict["send-episode-number"]
        else:
            self.send_episode_number = False

    @staticmethod
    def get_show_metadata_dict(show_path) -> dict:
        for file in os.listdir(show_path):
            file_path = os.path.join(show_path, file)
            if file != "metadata.json" or not os.path.isfile(file_path):
                continue
            with open(file_path) as f:
                return json.load(f)
        return {}


class TvShow:
    show_metadata: TvShowMetadata
    show_name: str
    show_path: str
    latest_uploaded_season_number: int
    latest_uploaded_episode_number: int
    client: discord.Client
    total_episode_number: int

    # def __init__(self, show_path: str, client: discord.Client, latest_uploaded_season_number,
    #              latest_uploaded_episode_number):
    def __init__(self, show_path: str, client: discord.Client):
        self.show_path = show_path
        self.show_name = os.path.basename(show_path)
        self.show_metadata = TvShowMetadata(self.show_path)
        self.client = client
        self.latest_uploaded_season_number, self.latest_uploaded_episode_number = (
            self.get_latest_episode_uploaded())
        self.total_episode_number = 1
        # self.latest_uploaded_season_number = latest_uploaded_season_number
        # self.latest_uploaded_episode_number = latest_uploaded_episode_number

    def verify_show_folder_structure(self) -> bool:
        return verify_show_folder_structure_from_path(self.show_path, self.show_name)

    def get_latest_episode_uploaded(self) -> (int, int):
        if self.show_name not in self.client.progress_tracker.keys():
            return -1, -1
        season_number = -1
        for season_string in self.client.progress_tracker[self.show_name]:
            season_number = max(season_number, extract_season_number_from_folder_name(season_string))
        # print(season_number)
        season_string = "s" + str(season_number).zfill(2)
        # print(season_string)
        if season_string not in self.client.progress_tracker[self.show_name].keys():
            return -1, -1
        # print(f"{season_number}, {client.progress_tracker[show_name][season_string]}")
        return season_number, self.client.progress_tracker[self.show_name][season_string]

    async def upload_to_discord(self, upload_channel: discord.TextChannel):
        first_ep = True
        # not doing checking on episodes since this should be done properly by the video converter
        for season_folder_name in natsort.natsorted(os.listdir(self.show_path)):
            season_folder_path = os.path.join(self.show_path, season_folder_name)
            if os.path.isfile(season_folder_path):
                continue
            season = ShowSeason(self, season_folder_path)
            for episode_name in natsort.natsorted(os.listdir(season.season_path)):
                episode_path = os.path.join(season.season_path, episode_name)
                if os.path.isdir(episode_path):
                    continue
                if not is_file_video(episode_path):
                    continue
                episode = ShowEpisode(self, season, episode_path, season.current_episode_number,
                                      self.total_episode_number)
                first_ep = await episode.upload_to_discord(upload_channel, first_ep)

        print(f"{CCs.OKGREEN}done uploading!{CCs.ENDC}")


class ShowSeason:
    show: TvShow
    season_path: str
    season_folder_name: str
    season_number: int
    current_episode_number: int

    def __init__(self, show: TvShow, season_path: str | bytes):
        self.show = show
        self.season_path = convert_str_or_bytes_to_str(season_path)
        self.season_folder_name = os.path.basename(season_path)
        self.season_number = extract_season_number_from_folder_name(self.season_folder_name)
        self.current_episode_number = 1

    def increase_episode_number(self, amount=1):
        self.current_episode_number += amount
        self.show.total_episode_number += amount


class ShowEpisode:
    show: TvShow
    season: ShowSeason
    episode_path: str
    episode_file_name: str
    episode_number: int
    total_episode_number: int

    def __init__(self, show: TvShow, season: ShowSeason, episode_path: str, episode_number: int,
                 total_episode_number: int):
        self.show = show
        self.season = season
        self.episode_path = episode_path
        self.episode_file_name = os.path.basename(self.episode_path)
        self.episode_number = episode_number
        self.total_episode_number = total_episode_number
        self.human_readable_episode_string = \
            f"season {self.season.season_number} episode {self.episode_number} ({self.episode_file_name})"
        if self.show.show_metadata.send_episode_number:
            self.human_readable_episode_string += f", (episode number={self.total_episode_number})"

    def already_uploaded(self) -> bool:
        if self.show.show_name not in self.show.client.progress_tracker.keys():
            return False
        if self.season.season_folder_name not in self.show.client.progress_tracker[self.show.show_name].keys():
            return False
        if self.episode_number > self.show.client.progress_tracker[self.show.show_name][self.season.season_folder_name]:
            return False
        return True

    def later_episode_already_uploaded(self) -> bool:
        if self.show.latest_uploaded_season_number is None or self.show.latest_uploaded_episode_number is None:
            return False
        if (self.season.season_number < self.show.latest_uploaded_season_number or
                (self.season.season_number == self.show.latest_uploaded_season_number and
                 self.episode_number <= self.show.latest_uploaded_episode_number)):
            return True
        return False

    async def upload_to_discord(self, upload_channel: discord.TextChannel, first_ep: bool) -> bool:
        if self.already_uploaded():
            # print(f"{CCs.WARNING}season {season_number} episode {episode_number} ({episode_name}) "
            #       f"ALREADY UPLOADED{CCs.ENDC}")
            self.season.increase_episode_number()
            return False

        if self.later_episode_already_uploaded():
            print(f"{CCs.FAIL} {self.human_readable_episode_string}"
                  f"CANNOT BE UPLOADED AS THE LATEST UPLOADED FILE IS season {latest_uploaded_season_number} "
                  f"episode {latest_uploaded_episode_number}{CCs.ENDC}")
            self.season.increase_episode_number()
            return False
        print(f"{CCs.OKBLUE}uploading {self.human_readable_episode_string}{CCs.ENDC}")
        msg = await self.__upload_episode_to_channel(upload_channel)
        if first_ep:
            await msg.pin()
            first_ep = False
        update_progress_tracker(self.show.show_name, self.season.season_folder_name, self.episode_number,
                                self.show.client)
        self.season.increase_episode_number()
        await wait_between_uploads(self.show.client)
        return first_ep

    async def __upload_episode_to_channel(self, upload_channel: discord.TextChannel) -> discord.Message:
        return await upload_channel.send(self.human_readable_episode_string, file=discord.File(self.episode_path))


class SelectorType(Enum):
    GLOBAL = 0
    SEASON = 1
    REGEX_FILENAME = 2
    EPISODE_RANGE = 3


class FilterComplexBuilder:
    filter_complex_dict: dict
    show_to_filter_converter: dict

    def __init__(self):
        if os.path.exists(FILTER_COMPLEX_BUILDER_JSON_PATH):
            with open(FILTER_COMPLEX_BUILDER_JSON_PATH) as f:
                self.filter_complex_dict = json.load(f)
        else:
            print(f"{CCs.WARNING}"
                  f"could not find a filter complex builder json from path: {FILTER_COMPLEX_BUILDER_JSON_PATH}!\n"
                  f"files with subtitles or multiple video/audio tracks may not be converted correctly!{CCs.ENDC}")
            self.filter_complex_dict = {}
        self.show_to_filter_converter = {}
        for show_name in self.filter_complex_dict.keys():
            self.show_to_filter_converter["show_name"] = self.get_filter_complex(show_name)

    def get_filter_complex(self, show_name: str):
        if show_name not in self.filter_complex_dict.keys():
            return NoFilterComplex({}, "")
        selector_type: str | int = self.filter_complex_dict[show_name]["filter-complex-selector-type"]
        if isinstance(selector_type, str):
            if selector_type.isnumeric():
                selector_type = int(selector_type)

        match self.get_selector_type(selector_type):
            case SelectorType.GLOBAL:
                return GlobalFilterComplex(self.filter_complex_dict[show_name], show_name)
            case SelectorType.REGEX_FILENAME:
                return RegexFilterComplex(self.filter_complex_dict[show_name], show_name)
            case SelectorType.EPISODE_RANGE:
                return EpisodeRangeFilterComplex(self.filter_complex_dict[show_name], show_name)
            case _:
                raise Exception(f"{show_name} does not have a valid filter complex selector type!")

    @staticmethod
    def get_selector_type(selector_type: str | int) -> SelectorType:
        if isinstance(selector_type, str):
            match selector_type:
                case "global":
                    return SelectorType.GLOBAL
                case "season":
                    return SelectorType.SEASON
                case "regex-filename":
                    return SelectorType.REGEX_FILENAME
                case "episode-range":
                    return SelectorType.EPISODE_RANGE
                case _:
                    raise Exception(f"invalid selector_type! got {selector_type}")
        return SelectorType(selector_type)


class ShowFilterComplex:
    show_complex_dict: dict
    filter_complex_options_dict: dict
    show_name: str
    selector_type: SelectorType
    video_stream_id: int = 0
    subtitle_stream_id: int | None = None
    audio_stream_id: int = 0
    x_resolution: int = VIDEO_RESOLUTION[0]
    y_resolution: int = VIDEO_RESOLUTION[1]

    def __init__(self, show_complex_dict: dict, show_name: str):
        self.show_complex_dict = show_complex_dict
        self.show_name = show_name

    def get_ffmpeg_filter_complex(self, video_path: str):
        pass

    def post_video_file_conversion(self, output_destination: str):
        pass

    def set_video_stuff_from_filter_complex(self, filter_complex_dict):
        if "video-stream-id" in filter_complex_dict.keys():
            self.video_stream_id = filter_complex_dict["video-stream-id"]
        if "subtitle-stream-id" in filter_complex_dict.keys():
            self.subtitle_stream_id = filter_complex_dict["subtitle-stream-id"]
        if "audio-stream-id" in filter_complex_dict.keys():
            self.audio_stream_id = filter_complex_dict["audio-stream-id"]
        if "x-resolution" in filter_complex_dict.keys():
            self.x_resolution = filter_complex_dict["x-resolution"]
        if "y-resolution" in filter_complex_dict.keys():
            self.y_resolution = filter_complex_dict["y-resolution"]


class NoFilterComplex(ShowFilterComplex):
    subtitle_file_path: pathlib.Path | None

    def __init__(self, show_complex_dict=None, show_name=None):
        super().__init__({}, "")

    def get_ffmpeg_filter_complex(self, video_path: str = "") -> list[str]:
        self.subtitle_file_path = get_file_subtitle_from_video_path(video_path)
        return get_ffmpeg_filter_complex_from_values(video_path, subtitle_file_path=self.subtitle_file_path)

    def post_video_file_conversion(self, destination_video_file_path: str):
        if self.subtitle_file_path is None:
            return
        subprocess.run(["mv", "-v", str(self.subtitle_file_path),
                        str(pathlib.Path(destination_video_file_path).parent)])


class GlobalFilterComplex(ShowFilterComplex):
    def __init__(self, show_complex_dict: dict, show_name: str):
        super().__init__(show_complex_dict, show_name)
        self.selector_type = SelectorType.GLOBAL
        self.filter_complex_options_dict = show_complex_dict["filter-complex-global"]
        self.set_video_stuff_from_filter_complex(self.filter_complex_options_dict)
        # print(show_name)
        # print(json.dumps(show_complex_dict, indent=2))
        # input()

    def get_ffmpeg_filter_complex(self, video_path: str) -> list[str]:
        return get_ffmpeg_filter_complex_from_values(
            video_path, self.video_stream_id, self.subtitle_stream_id, self.audio_stream_id, None,
            self.x_resolution, self.y_resolution
        )


class RegexFilterComplex(ShowFilterComplex):
    regex_actions: dict

    def __init__(self, show_complex_dict=None, show_name=None):
        super().__init__({}, "")
        self.selector_type = SelectorType.REGEX_FILENAME
        self.filter_complex_options_dict = show_complex_dict["filter-complex-regex-filename"]
        self.regex_actions = self.filter_complex_options_dict["regex-actions"]

    def get_ffmpeg_filter_complex(self, video_path: str) -> list[str]:
        ep_regex_action = self.get_regex_from_video_name(os.path.basename(video_path))
        if ep_regex_action is None:
            print(f"{CCs.WARNING}could not find a regex action for video {video_path}{CCs.ENDC}")
        else:
            self.set_video_stuff_from_filter_complex(ep_regex_action)

        return get_ffmpeg_filter_complex_from_values(
            video_path, self.video_stream_id, self.subtitle_stream_id, self.audio_stream_id, None,
            self.x_resolution, self.y_resolution
        )

    def get_regex_from_video_name(self, video_name: str) -> dict | None:
        for regex_action in self.regex_actions:
            for regex_pattern in regex_action["regex-patterns"]:
                if len(re.findall(regex_pattern, video_name)) == 0:
                    continue
                # print(f"{video_name}: {regex_action}")
                return regex_action
        return None


class EpisodeRangeFilterComplex(ShowFilterComplex):
    range_actions: dict

    def __init__(self, show_complex_dict=None, show_name=None):
        super().__init__({}, "")
        self.selector_type = SelectorType.EPISODE_RANGE
        self.filter_complex_options_dict = show_complex_dict["filter-complex-episode-range"]
        self.range_actions = self.filter_complex_options_dict["range-actions"]

    def get_ffmpeg_filter_complex(self, video_path: str) -> list[str]:
        ep_range_action = self.get_range_from_video_name(os.path.basename(video_path))
        if ep_range_action is None:
            print(f"{CCs.WARNING}could not find a range action for video {video_path}{CCs.ENDC}")
        else:
            self.set_video_stuff_from_filter_complex(ep_range_action)

        return get_ffmpeg_filter_complex_from_values(
            video_path, self.video_stream_id, self.subtitle_stream_id, self.audio_stream_id, None,
            self.x_resolution, self.y_resolution
        )

    def get_range_from_video_name(self, video_name: str) -> dict | None:
        ep_compare_num = extract_ep_compare_num_from_video_name(video_name)
        for range_action in self.range_actions:
            for episode_range in range_action["episode-ranges"]:
                ep_r_cmpre0 = extract_ep_compare_num_from_video_name(episode_range[0])
                ep_r_cmpre1 = extract_ep_compare_num_from_video_name(episode_range[1])
                if ep_r_cmpre0 > ep_r_cmpre1:
                    raise Exception(f"invalid episode_range: {episode_range}")

                if ep_r_cmpre0 <= ep_compare_num <= ep_r_cmpre1:
                    # print(f"{CCs.OKGREEN}{episode_file_name} fits in {episode_range}{CCs.ENDC}")
                    return range_action
        return None


class VideoObject:
    t: float
    audio_bitrate: int
    bitrate: float
    path: str
    name: str
    expected_size: float
    framerate: int
    # resolution: tuple[int, int]
    filter_complex: ShowFilterComplex

    def __init__(self, video_path: str, filter_complex: ShowFilterComplex = None):
        self.t = self.get_video_length(video_path)
        self.audio_bitrate, self.bitrate = self.get_bitrate_from_t(self.t)
        self.path = video_path
        self.name = pathlib.Path(video_path).name
        self.expected_size = self.bitrate * self.t + self.audio_bitrate * self.t
        self.framerate = VIDEO_FRAMERATE
        if filter_complex is None:
            filter_complex = NoFilterComplex()
        self.filter_complex = filter_complex
        # self.resolution = (self.filter_complex.x_resolution, self.filter_complex.y_resolution)

    def convert_file(self, destination_folder: str, destination_converted):
        pathlib.Path(destination_folder).parent.mkdir(parents=True, exist_ok=True)
        pathlib.Path(destination_converted).parent.mkdir(parents=True, exist_ok=True)
        destination_file = os.path.join(os.path.realpath(self.path), os.path.realpath(destination_folder))
        ffmpeg_args = self.get_ffmpeg_arguments(destination_file)
        print(f"{CCs.OKCYAN}\n{self.name}, framerate={self.framerate}, b:v={self.bitrate}k, b:a={self.audio_bitrate}k, "
              f"resolution=({self.filter_complex.x_resolution}, {self.filter_complex.y_resolution}), "
              f"exp_size={self.expected_size / 8 / 1000}MB{CCs.ENDC}")
        print(f"{CCs.OKCYAN}converting: {self.path} -> {change_extension_to_mp4(destination_folder)}{CCs.ENDC}")
        print(CCs.OKGREEN, end="")
        if subprocess.run(ffmpeg_args).returncode == 0:
            subprocess.run(["mv", "-v", self.path, destination_converted])
            self.filter_complex.post_video_file_conversion(destination_converted)
            print(CCs.ENDC, end="")
            return
        print(CCs.ENDC, end="")
        print(f"{CCs.FAIL}could not convert file: {self.path}{CCs.ENDC}")
        input()

        # cmd = ""
        # for arg in self.get_ffmpeg_arguments(destination_file):
        #     cmd += f"{arg} "
        # os.system(cmd)

    def get_ffmpeg_arguments(self, destination_file: str) -> list:
        new_args = []
        for arg in FFMPEG_ARGS:
            match arg:
                case "INPUT_FILE":
                    # new_args.append(f"\"{self.path}\"")
                    new_args.append(self.path)
                case "INSERT_FILTERS":
                    new_args += self.filter_complex.get_ffmpeg_filter_complex(self.path)
                case "VIDEO_BITRATE":
                    new_args.append(str(int(self.bitrate)) + "k")
                case "AUDIO_BITRATE":
                    new_args.append(str(int(self.audio_bitrate)) + "k")
                case "FRAMERATE":
                    new_args.append(str(self.framerate))
                # case "scale=RESOLUTION":
                #     new_args.append(f"scale={self.resolution}")
                case "OUTPUT_FILE":
                    # new_args.append(f"\"{destination_file}\"")
                    new_args.append(change_extension_to_mp4(destination_file))
                case _:
                    new_args.append(arg)
        # print(new_args)
        return new_args

    @staticmethod
    def get_bitrate_from_t(t) -> (int, float):
        # this defines how much we favor the audio bitrate over the video bitrate
        video_bitrate_offset = VIDEO_BITRATE_OFFSET
        max_audio_bitrate = MAX_AUDIO_BITRATE_KBITS
        variance = ALLOWED_VIDEO_OFFSET_VARIANCE
        audio_bitrate = 50
        video_bitrate = 50
        just_decreased_audio_bitrate = -1

        finding_video_bitrate = True
        while finding_video_bitrate:
            video_bitrate = ((t * audio_bitrate) - MAX_FILESIZE_KBITS) / -t
            if audio_bitrate >= max_audio_bitrate:
                break
            elif audio_bitrate <= MIN_AUDIO_BITRATE_KBITS:
                break
            if video_bitrate / video_bitrate_offset < audio_bitrate + variance:
                if just_decreased_audio_bitrate == 0:
                    break
                audio_bitrate -= 1
                just_decreased_audio_bitrate = 1
            if video_bitrate / video_bitrate_offset > audio_bitrate + variance:
                if just_decreased_audio_bitrate == 1:
                    break
                audio_bitrate += 1
                just_decreased_audio_bitrate = 0
        return audio_bitrate, video_bitrate

    @staticmethod
    def get_video_length(filename) -> float:
        result = subprocess.run(
            ["ffprobe", "-v", "error", "-show_entries", "format=duration", "-of",
             "default=noprint_wrappers=1:nokey=1", filename], stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
        str_out = result.stdout.decode()
        for line in str_out.split("\n"):
            try:
                return float(line)
            except ValueError:
                pass
        raise Exception(f"could not find length from output: {str_out}")


FILTER_COMPLEX_BUILDER = FilterComplexBuilder()
