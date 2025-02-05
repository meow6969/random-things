import json
import os
from typing import Tuple

import natsort

from ihatecircularimport import CCs, ensure_constants_py_exists
from showuploaderbotclasses import FILTER_COMPLEX_BUILDER, VideoObject
from showuploaderbotfuncs import (ensure_config_json_exists, fix_season_folder_structure, is_file_video,
                                  verify_show_folder_structure_from_path)


def extract_video_converter_fields_from_config() -> tuple[str, str, str]:
    ensure_config_json_exists()
    with open("config.json") as meow:
        the_config = json.load(meow)
        video_converter_settings: dict[str, str] = the_config["video-converter-settings"]
        return (
            video_converter_settings["files-to-convert-dir"],
            video_converter_settings["output-files-dir"],
            video_converter_settings["files-converted-dir"]
        )


def convert_all_files(files_to_convert_dir, output_files_dir, files_converted_dir):
    print(f"{CCs.HEADER}{CCs.OKGREEN}converting all files in {files_to_convert_dir} to {output_files_dir}{CCs.ENDC}")

    for show_folder_name in natsort.natsorted(os.listdir(files_to_convert_dir)):
        if show_folder_name.startswith("SKIP"):
            continue
        show_folder_path = os.path.join(files_to_convert_dir, show_folder_name)
        if show_folder_name == "MOVIES":
            for movie_file_name in os.listdir(show_folder_path):
                movie_file_path = os.path.join(show_folder_path, movie_file_name)
                if is_file_video(movie_file_path):
                    print(f"{CCs.OKGREEN}{movie_file_path} -> "
                          f"{os.path.join(output_files_dir, 'MOVIES', movie_file_name)}{CCs.ENDC}")
                    movie_filter_complex = FILTER_COMPLEX_BUILDER.get_filter_complex(show_folder_name)
                    video = VideoObject(movie_file_path, movie_filter_complex)
                    video.convert_file(os.path.join(output_files_dir, "MOVIES", movie_file_name),
                                       os.path.join(files_converted_dir, "MOVIES",
                                                    movie_file_name))
            continue
        if not verify_show_folder_structure_from_path(show_folder_path, show_folder_name):
            input(f"{CCs.FAIL}show {show_folder_path} has invalid folder structure, please check!\n"
                  f"press ctrl+c to stop or enter to automatically fix season folder structure{CCs.ENDC}")
            fix_season_folder_structure(show_folder_path)
        for season_folder_name in natsort.natsorted(os.listdir(show_folder_path)):
            show_filter_complex = FILTER_COMPLEX_BUILDER.get_filter_complex(show_folder_name)
            season_folder_path = os.path.join(show_folder_path, season_folder_name)
            for episode_file_name in natsort.natsorted(os.listdir(season_folder_path)):
                episode_file_path = os.path.join(season_folder_path, episode_file_name)
                if is_file_video(episode_file_path):
                    VideoObject(episode_file_path, show_filter_complex).convert_file(
                        os.path.join(output_files_dir, show_folder_name, season_folder_name, episode_file_name),
                        os.path.join(files_converted_dir, show_folder_name, season_folder_name, episode_file_name)
                    )
    print(f"{CCs.OKGREEN}done converting{CCs.ENDC}")


# -filter_complex "[0:v][0:s:0]overlay[v]" -map "[v]" -map 0:a:1
# -filter_complex "[0:v] [0:a] [1:v] [1:a] concat=n=2:v=1:a=1 [v] [a]" -map "[v]" -map "[a]"


def main():
    files_to_convert_dir, output_files_dir, files_converted_dir = extract_video_converter_fields_from_config()

    convert_all_files(files_to_convert_dir, output_files_dir, files_converted_dir)


if __name__ == "__main__":
    main()
