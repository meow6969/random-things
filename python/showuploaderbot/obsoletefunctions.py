import json
import os
import time

import natsort

from ihatecircularimport import CCs
from showuploaderbotfuncs import (extract_season_and_episode_number_from_video_name, get_all_subtitles_info_from_video,
                                  get_list_of_dict_keys_as_dict_list, hash_dict_or_list,
                                  get_all_codec_type_info_from_video)


def generate_uploaded_list(show_folder: str) -> None:  # this just used to generate the message in #upload-suggestions
    show_number = 0
    for folder_name in natsort.natsorted(os.listdir(show_folder)):
        folder_path = os.path.join(show_folder, folder_name)
        if os.path.isfile(folder_path):
            continue
        if folder_name.startswith("SKIP"):
            continue
        if folder_name == "MOVIES":
            continue
        show_number += 1
        print(f"{show_number}: ~~{folder_name.replace('-', ' ')}~~ UPLOADED")


def rename_show_folder_season_folders():
    while True:
        show_folder = input("enter show folder or just press enter to stop:\n")
        if show_folder.strip() == "":
            print(f"{CCs.OKGREEN}bye bye nya{CCs.ENDC}")
            return
        if not os.path.isdir(show_folder):
            print(f"{CCs.FAIL}\"{show_folder}\" is not a valid directory, try again{CCs.ENDC}")
            continue
        folders = []
        for i in os.listdir(show_folder):
            folder = os.path.join(show_folder, i)
            if os.path.isfile(folder):
                continue
            folders.append(folder)
        folders = natsort.natsorted(folders)
        print(folders)
        for i, name in enumerate(folders):
            folder = os.path.join(show_folder, name)
            if i + 1 < 10:
                os.rename(folder, os.path.join(show_folder, f"s0{i + 1}"))
                print(f"{name} -> s0{i + 1}")
            elif i + 1 > 10:
                os.rename(folder, os.path.join(show_folder, f"s{i + 1}"))
                print(f"{name} -> s{i + 1}")
            # if i < 10:
            #     os.rename(folder, os.path.join(show_folder, f"s0{i}"))
            #     print(f"{name} -> s0{i}")
            # elif i > 10:
            #     os.rename(folder, os.path.join(show_folder, f"s{i}"))
            #     print(f"{name} -> s{i}")


def verify_episode_numbered_series_links(show_folder: str):
    cur_ep = 0
    for season_folder in sorted(os.listdir(show_folder)):
        season_path = os.path.join(show_folder, season_folder)
        for episode_file_name in sorted(os.listdir(season_path)):
            cur_ep += 1
            episode_file_path = os.path.join(season_path, episode_file_name)

            episode_link_file_name = os.path.basename(os.path.realpath(episode_file_path))
            # print(episode_link_file_name)
            if (str(cur_ep) not in episode_link_file_name and
                    f"0{cur_ep}" not in episode_link_file_name and
                    f"00{cur_ep}" not in episode_link_file_name):
                print(f"{CCs.FAIL}wrong: {episode_file_name}, expect: {cur_ep}{CCs.ENDC}")
            else:
                print(f"{CCs.OKGREEN}correct: {episode_file_name}, expect: {cur_ep}{CCs.ENDC}")


def check_for_codec_type_changes_for_show(show_folder: str, codec_type: str):
    last_codec_types_for_video: list[dict] | None = None
    last_codec_types_special_keys: dict[str, list] | None = None
    last_codec_types_special_keys_hash: int | None = None
    last_video_path: str | None = None
    last_video_name: str | None = None
    special_keys = ("tags/language", "index")
    video_codec_types_types: dict[int, dict] = {}
    current_episode_range: list[str] = []
    total_codec_type_changes = 0
    start_time = time.time_ns()
    time_spent_getting_codec_type_info = 0
    for season_folder in sorted(os.listdir(show_folder)):
        season_path = os.path.join(show_folder, season_folder)
        for episode_file_name in sorted(os.listdir(season_path)):
            episode_file_path = os.path.join(season_path, episode_file_name)
            s_time = time.time_ns()
            cur_episode_codec_types = get_all_codec_type_info_from_video(episode_file_path, codec_type)
            time_spent_getting_codec_type_info += time.time_ns() - s_time
            if last_codec_types_for_video is None:  # this is only for very first ep
                last_codec_types_for_video = cur_episode_codec_types
                last_codec_types_special_keys = get_list_of_dict_keys_as_dict_list(
                    cur_episode_codec_types, *special_keys)
                last_video_path = episode_file_path
                last_video_name = episode_file_name
                last_codec_types_special_keys_hash = hash_dict_or_list(last_codec_types_special_keys)
                video_codec_types_types[last_codec_types_special_keys_hash] = {
                    "special-keys": last_codec_types_special_keys,
                    "episode-ranges": []
                }
                current_episode_range.append(episode_file_name)
                continue
            # if len(last_subtitles_for_video) != len(cur_episode_subtitles):
            #     print(f"\n{CCs.BOLD}{CCs.FAIL}differing subtitles detected!\n"
            #           f"{last_video_path} got {len(last_subtitles_for_video)} subtitle entries\n"
            #           f"{episode_file_path} got {len(cur_episode_subtitles)} subtitle entries{CCs.ENDC}\n")
            #     total_subtitle_changes += 1
            #     last_subtitles_for_video = cur_episode_subtitles
            #     last_subtitles_special_keys = get_list_of_dict_keys_as_dict_list(cur_episode_subtitles,
            #                                                                      *special_keys)
            #     last_video_path = episode_file_path
            #     continue
            cur_episode_special_keys = get_list_of_dict_keys_as_dict_list(cur_episode_codec_types, *special_keys)
            if cur_episode_special_keys != last_codec_types_special_keys:
                cur_episode_special_keys_hash = hash_dict_or_list(cur_episode_special_keys)
                print(f"\n\n{CCs.FAIL}differing {codec_type}s detected!\n"
                      f"last video ({last_video_path}) got\n"
                      f"{last_codec_types_special_keys_hash}: {json.dumps(last_codec_types_special_keys, indent=2)}\n"
                      f"special key values\n"
                      f"current video ({episode_file_path}) got\n"
                      f"{cur_episode_special_keys_hash}: {json.dumps(cur_episode_special_keys, indent=2)}\n"
                      f"special key values{CCs.ENDC}\n\n")
                current_episode_range.append(last_video_name)
                # print(current_episode_range)
                total_codec_type_changes += 1

                video_codec_types_types[last_codec_types_special_keys_hash]["episode-ranges"].append(current_episode_range)
                if cur_episode_special_keys_hash not in video_codec_types_types.keys():
                    video_codec_types_types[cur_episode_special_keys_hash] = {
                        "special-keys": cur_episode_special_keys,
                        "episode-ranges": []
                    }
                last_codec_types_for_video = cur_episode_codec_types
                last_codec_types_special_keys = cur_episode_special_keys
                last_codec_types_special_keys_hash = hash_dict_or_list(last_codec_types_special_keys)
                last_video_path = episode_file_path
                current_episode_range = [episode_file_name]
                last_video_name = episode_file_name
                continue
            last_video_name = episode_file_name
            last_video_path = episode_file_path
    current_episode_range.append(last_video_name)
    video_codec_types_types[last_codec_types_special_keys_hash]["episode-ranges"].append(current_episode_range)

    for codec_type_type in video_codec_types_types:
        print(f"{codec_type_type}: {json.dumps(video_codec_types_types[codec_type_type]['episode-ranges'])}")

    print(f"\n\nall {codec_type} types:\n"
          f"{json.dumps(video_codec_types_types, indent=2)}\n\n")
    print(f"{CCs.OKBLUE}"
          f"total {codec_type} changes:           {CCs.ENDC}", end="")
    if total_codec_type_changes == 0:
        print(f"{CCs.OKGREEN}{total_codec_type_changes}{CCs.ENDC} {CCs.OKCYAN}:3{CCs.ENDC}")
    elif total_codec_type_changes < 6:
        print(f"{CCs.UNDERLINE}{CCs.WARNING}{total_codec_type_changes}{CCs.ENDC}{CCs.OKCYAN} :({CCs.ENDC}")
    else:
        print(f"{CCs.UNDERLINE}{CCs.FAIL}{total_codec_type_changes}{CCs.ENDC}{CCs.OKCYAN} :c{CCs.ENDC}")
    total_time_spent = time.time_ns() - start_time
    print(f"{CCs.OKBLUE}" 
          f"total time spent:                 {int(total_time_spent / 1000000000)}s\n"
          f"time spent getting {codec_type} info: {time_spent_getting_codec_type_info / 1000000000}s\n"
          f"difference:                       {(total_time_spent - time_spent_getting_codec_type_info) / 1000000000}s"
          f"{CCs.ENDC}")


def check_for_subtitle_changes_for_show(show_folder: str):
    check_for_codec_type_changes_for_show(show_folder, "subtitle")


def check_for_audio_changes_for_show(show_folder: str):
    check_for_codec_type_changes_for_show(show_folder, "audio")


def test_get_list_of_dict_keys_as_dict_list():
    cur_time = time.time_ns()
    subs1 = get_all_subtitles_info_from_video("/mnt/f/meow/toconvert/hokuto-no-ken/s01/S01E02.mkv")
    subs2 = get_all_subtitles_info_from_video("/mnt/f/meow/toconvert/hokuto-no-ken/s01/S01E03.mkv")
    subs3 = get_all_subtitles_info_from_video("/mnt/f/meow/toconvert/hokuto-no-ken/s01/S01E04.mkv")
    print(json.dumps(subs1, indent=2))
    print("\n\n\n\n")
    print(json.dumps(subs2, indent=2))
    print("\n\n\n\n")
    print(json.dumps(subs3, indent=2))
    print(f"\n{CCs.OKGREEN}took {int((time.time_ns() - cur_time) / 1000)}ms to get subtitle info{CCs.ENDC}\n")

    cur_time = time.time_ns()
    subs1_keys_dict = get_list_of_dict_keys_as_dict_list(subs1, "tags/language", "r_frame_rate")
    print(json.dumps(subs1_keys_dict, indent=2))
    print("\n")
    subs2_keys_dict = get_list_of_dict_keys_as_dict_list(subs2, "tags/language", "r_frame_rate")
    print(json.dumps(subs2_keys_dict, indent=2))
    print("\n")
    special_keys = ("tags/language", "r_frame_rate")
    subs3_keys_dict = get_list_of_dict_keys_as_dict_list(subs3, *special_keys)
    print(json.dumps(subs3_keys_dict, indent=2))
    print("\n")
    print(f"subs1_keys_dict == subs2_keys_dict: {subs1_keys_dict == subs2_keys_dict}")
    print(f"subs1_keys_dict == subs3_keys_dict: {subs1_keys_dict == subs3_keys_dict}")
    print(f"subs2_keys_dict == subs3_keys_dict: {subs2_keys_dict == subs3_keys_dict}")
    print("\n")
    print(json.dumps(get_list_of_dict_keys_as_dict_list([
        {
            "kit": 2,
            "meow": {
                "nyacat": 1
            }
        },
        {
            "kit": 3,
            "meow": {
                "nyakitty": 1
            }
        }
    ], "kit", "meow/nyakitty"), indent=2))
    print(f"\n{CCs.OKGREEN}took {int((time.time_ns() - cur_time) / 1000)}ms to get subtitle keys info")


def check_subtitle_ranges_for_show(show_name: str, show_path: str) -> None:
    with open("filter_complex_builder.json") as f:
        range_actions = json.load(f)[show_name]["filter-complex-episode-range"]["range-actions"]
    print(range_actions)
    for season_folder in sorted(os.listdir(show_path)):
        season_path = os.path.join(show_path, season_folder)
        for episode_file_name in sorted(os.listdir(season_path)):
            s_num, ep_num = extract_season_and_episode_number_from_video_name(episode_file_name)
            ep_compare_num = int(f"{s_num}{ep_num:03d}")
            for range_action in range_actions:
                for episode_range in range_action["episode-ranges"]:
                    ep_r_s0, ep_r_ep0 = extract_season_and_episode_number_from_video_name(episode_range[0])
                    ep_r_cmpre0 = int(f"{ep_r_s0}{ep_r_ep0:03d}")
                    ep_r_s1, ep_r_ep1 = extract_season_and_episode_number_from_video_name(episode_range[1])
                    ep_r_cmpre1 = int(f"{ep_r_s1}{ep_r_ep1:03d}")
                    if ep_r_cmpre0 > ep_r_cmpre1:
                        raise Exception(f"invalid episode_range: {episode_range}")

                    if ep_r_cmpre0 <= ep_compare_num <= ep_r_cmpre1:
                        print(f"{CCs.OKGREEN}{episode_file_name} fits in {episode_range}{CCs.ENDC}")


if __name__ == "__main__":
    # verify_episode_numbered_series_links("/mnt/f/meow/toconvert/keroro-gunsou")
    # rename_show_folder_season_folders()
    # test_get_list_of_dict_keys_as_dict_list()
    check_for_subtitle_changes_for_show("/mnt/f/meow/toconvert/konosuba-rashii-sekai-ni-shukufuku-wo")
    check_for_audio_changes_for_show("/mnt/f/meow/toconvert/konosuba-rashii-sekai-ni-shukufuku-wo")
    # check_for_subtitle_changes_for_show("/mnt/f/meow/converted/hokuto-no-ken")
    # check_for_audio_changes_for_show("/mnt/f/meow/converted/hokuto-no-ken")
    # check_subtitle_ranges_for_show("hokuto-no-ken",
    #                                "/mnt/h/plex-folder/da tv/Fist of the North Star (1984)")
    # season_number, episode_number, season_number_length, episode_number_length = (
    #     extract_season_and_episode_number_from_video_name("S00001000E00002323.mkv"))
    # print(f"{season_number}, {episode_number}, {season_number_length}, {episode_number_length}")
