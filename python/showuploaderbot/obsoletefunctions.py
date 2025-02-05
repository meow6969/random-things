import os

import natsort


def generate_uploaded_list() -> None:  # this just used to generate the message in #upload-suggestions
    path = "/mnt/g/archivestuff/visualmediaarchives/discord50mbtvshows"
    show_number = 0
    for folder_name in natsort.natsorted(os.listdir(path)):
        folder_path = os.path.join(path, folder_name)
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
        show_folder = input("enter show folder\n")
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


def verify_one_piece_links():
    cur_ep = 0
    one_piece_folder = "/mnt/h/plex-folder/da tv/One Piece (1999)"
    for season_folder in sorted(os.listdir(one_piece_folder)):
        season_path = os.path.join(one_piece_folder, season_folder)
        for episode_file_name in sorted(os.listdir(season_path)):
            cur_ep += 1
            episode_file_path = os.path.join(season_path, episode_file_name)
            # print(os.path.realpath(episode_file_path))
            if (str(cur_ep) not in os.path.realpath(episode_file_path) and
                    f"0{cur_ep}" not in os.path.realpath(episode_file_path) and
                    f"00{cur_ep}" not in os.path.realpath(episode_file_path)):
                print(f"wrong: {episode_file_name}, expect: {cur_ep}")
            else:
                print(f"{episode_file_name}, expect: {cur_ep}")


if __name__ == "__main__":
    # verify_one_piece_links()
    rename_show_folder_season_folders()
