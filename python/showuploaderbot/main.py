import json
import math
import os
import threading
import typing
import sys
import asyncio
print(sys.version)

import natsort

import discord
from discord.ext import commands

from convertfilestodiscorduploadable import convert_all_files, extract_video_converter_fields_from_config
from ihatecircularimport import CCs
from showuploaderbotclasses import TvShow
from showuploaderbotfuncs import ensure_config_json_exists, is_file_video, save_progress_tracker, wait_between_uploads
import constants


def setup_bot_client() -> commands.Bot:
    print(f"{CCs.OKCYAN}setting up commands.Bot object as client{CCs.ENDC}")
    ensure_config_json_exists()
    _client = commands.Bot(command_prefix="showbot")
    _client.sync_lock = False
    _client.convert_lock = False

    with open("config.json", "r") as f:
        config = json.load(f)
        _client.owners = config["owners"]
        _client.shows_folder = config["shows-folder"]
        _client.shows_server_id = config["shows-server-id"]
        _client.seconds_to_wait_between_uploads = config["minutes-to-wait-between-uploads"] * 60
        _client.video_converter_settings = config["video-converter-settings"]
        _client.filter_complex_builder_path = os.path.realpath("./filter_complex_builder.json")

    if not os.path.exists(os.path.join(os.getcwd(), "progress_tracker.json")):
        with open("progress_tracker.json", "w+") as f:
            json.dump({}, f)
    with open("progress_tracker.json", "r+") as f:
        progress_tracker = json.load(f)
        _client.progress_tracker = progress_tracker
    return _client


async def sync_all_uploads(shows_folder: str) -> bool:
    if client.sync_lock:
        return False
    client.sync_lock = True
    try:
        for disk_object in natsort.natsorted(os.listdir(shows_folder)):
            disk_object_path = os.path.join(shows_folder, disk_object)
            if not os.path.isdir(disk_object_path):
                continue
            if os.path.basename(disk_object_path) == "MOVIES":
                await upload_movies_to_discord(disk_object_path)
                continue
            show = TvShow(disk_object_path, client)
            if show.show_name.startswith("SKIP"):
                print(f"{CCs.WARNING}skipped {show.show_name[5:]}{CCs.ENDC}")
                continue

            # if show_name != "nichijou":
            #     print(f"{CCs.OKCYAN}skipping show {show_name}{CCs.ENDC}")
            #     continue

            # check that the season names are correctly named

            if not show.verify_show_folder_structure():  # if there was an error in file structure
                continue  # go to the next show

            category = await get_category_for_show(show.show_name)

            upload_channel = await get_channel_of_name(show.show_name, category.id)

            # now we can start uploading episodes
            # await upload_show_to_discord(show, upload_channel)
            await show.upload_to_discord(upload_channel)
        save_progress_tracker(client)
        print(f"{CCs.OKGREEN}done uploading all shows!{CCs.ENDC}")
        client.sync_lock = False
        return True
    except Exception as e:
        print(f"{CCs.FAIL}error in show sync: {e}{CCs.ENDC}")
        client.sync_lock = False
        return False
    # await client.close()


async def get_channel_of_name(channel_name: str, category_id: int | None = None,
                              create_new=True) -> discord.TextChannel | discord.CategoryChannel:
    # if category_id is -1 that means to create a category

    for channel in client.shows_server.channels:
        if channel.name == channel_name:
            return channel
    if category_id == -1:
        return await client.shows_server.create_category(channel_name)
    if category_id is None:  # we are getting a category channel
        chnl = await client.shows_server.create_text_channel(channel_name)
        await sort_show_channels()
        return chnl
    return await client.shows_server.create_text_channel(channel_name, category=client.get_channel(category_id))


async def sort_show_channels():
    all_show_channels = get_all_sorted_show_channels()
    # input(all_show_channels.keys())
    current_channel_index = 0
    for channel in all_show_channels.values():
        if channel.category.name == get_show_category_name_from_show_channel_index(current_channel_index):
            pass
        else:  # this means that the channel is in the wrong category
            await make_sure_channel_in_proper_category(channel, current_channel_index)
        current_channel_index += 1
    all_show_categories: [discord.CategoryChannel] = get_all_show_category_channels()
    for category in all_show_categories:
        await sort_category_channels_by_name(category)
    print(f"{CCs.OKGREEN}all categories sorted!{CCs.ENDC}")


async def sort_category_channels_by_name(category: discord.CategoryChannel):
    all_sorted_channels = get_all_name_sorted_channels_in_category(category)
    for i, channel_name in enumerate(all_sorted_channels.keys()):
        if category.channels[i].name != channel_name:
            print(f"{CCs.FAIL}index={i}: {category.channels[i].name} != {channel_name}")
            print("properly sorted category: ")
            # this category isnt sorted properly
            print_sorted_channels_dict(all_sorted_channels)
            print(f"the category: {category.name} has unsorted channels!{CCs.ENDC}")


def print_sorted_channels_dict(all_sorted_channels: dict):
    for i, show_name in enumerate(all_sorted_channels.keys()):
        print(f"{i}: {show_name}")


async def make_sure_channel_in_proper_category(channel: discord.TextChannel,
                                               current_channel_index: int) -> discord.TextChannel:
    proper_category_channel_name = get_show_category_name_from_show_channel_index(current_channel_index)
    print(f"{proper_category_channel_name}, {current_channel_index}, {channel.name}")
    proper_category = await get_channel_of_name(proper_category_channel_name, -1)
    if proper_category.id == channel.category.id:
        return channel
    # print(proper_category.channels)
    if len(proper_category.channels) == 50:
        # last_channel = proper_category.channels[-1]
        last_channel = list(get_all_name_sorted_channels_in_category(proper_category).values())[-1]
        # proper_last_channel_category = await get_channel_of_name(
        #     get_show_category_name_from_show_channel_index(current_channel_index + 50), -1)
        last_channel_proper_channel_index = get_show_proper_channel_index(last_channel.name)
        await make_sure_channel_in_proper_category(last_channel, last_channel_proper_channel_index)
        await channel.edit(category=proper_category)
        print(f"{channel.name} got reassigned a category")
        # input()
    else:
        await channel.edit(category=proper_category)
        print(f"{channel.name} got reassigned a category")
        # input()
    # print(f"{channel.category.name} {get_show_category_name_from_show_channel_index(current_channel_index)}")
    # input(channel.name)
    return channel


def get_all_sorted_show_channels() -> dict[str, discord.TextChannel]:
    all_show_channels: dict[str, discord.TextChannel] = {}
    for channel in client.shows_server.channels:
        if not isinstance(channel, discord.TextChannel):
            continue
        assert isinstance(channel, discord.TextChannel)
        if channel.name in client.progress_tracker.keys() and channel.category.name.lower().startswith("shows-"):
            all_show_channels[channel.name] = channel
    return dict(sorted(all_show_channels.items()))


def get_all_name_sorted_channels_in_category(category: discord.CategoryChannel) -> dict[str, discord.TextChannel]:
    all_channels: dict[str, discord.TextChannel] = {}
    for channel in category.channels:
        all_channels[channel.name] = channel
    return dict(sorted(all_channels.items()))


def get_all_show_category_channels() -> [discord.CategoryChannel]:
    all_categories: [discord.CategoryChannel] = []
    for channel in client.shows_server.channels:
        if not isinstance(channel, discord.CategoryChannel):
            continue
        assert isinstance(channel, discord.CategoryChannel)
        if channel.name.lower().startswith("shows-"):
            all_categories.append(channel)
    return all_categories


def get_show_proper_channel_index(show_name: str) -> int:
    all_show_channels = sorted(client.progress_tracker.keys())
    proper_index = 0
    for show in all_show_channels:
        if show == show_name:
            return proper_index
        proper_index += 1
    return proper_index


async def get_category_for_show(show_name: str) -> discord.CategoryChannel:
    show_channel_index = 0
    for show in client.progress_tracker.keys():
        if show == show_name:
            break
        show_channel_index += 1
    category_name = get_show_category_name_from_show_channel_index(show_channel_index)
    for channel in client.shows_server.channels:
        if channel.name.lower() == category_name:
            return channel

    return await client.guilds[0].create_category(category_name)


def get_show_category_id_from_show_channel_index(show_channel_index: int) -> int:
    return math.floor(show_channel_index / 50 + 1)  # only 50 channels per category


def get_show_category_name_from_show_channel_index(show_channel_index: int) -> str:
    return f"shows-{get_show_category_id_from_show_channel_index(show_channel_index)}"


async def upload_movies_to_discord(movies_folder: str) -> None:
    upload_channel = await get_channel_of_name("movies")
    for movie_name in os.listdir(movies_folder):
        movie_file = os.path.join(movies_folder, movie_name)
        if not is_file_video(movie_file):
            continue
        if is_movie_already_uploaded(movie_name):
            print(f"{CCs.WARNING}MOVIE {movie_name} ALREADY UPLOADED{CCs.ENDC}")
            continue
        print(f"movie: {movie_name}")
        msg = await upload_channel.send(f"movie: {movie_name}", file=discord.File(movie_file))
        await msg.pin()
        update_progress_tracker_movie(movie_name)
        await wait_between_uploads(client)
    print(f"{CCs.OKGREEN}uploaded all movies!{CCs.ENDC}")


def update_progress_tracker_movie(movie_name: str) -> None:
    if "MOVIES" not in client.progress_tracker.keys():
        client.progress_tracker["MOVIES"] = []
    if movie_name not in client.progress_tracker["MOVIES"]:
        client.progress_tracker["MOVIES"].append(movie_name)
    save_progress_tracker(client)


def is_movie_already_uploaded(movie_name: str) -> bool:
    if "MOVIES" not in client.progress_tracker.keys():
        return False
    if movie_name not in client.progress_tracker["MOVIES"]:
        return False
    return True


def convert_files():
    files_to_convert_dir, output_files_dir, files_converted_dir = extract_video_converter_fields_from_config()

    convert_all_files(files_to_convert_dir, output_files_dir, files_converted_dir)


async def convert_files_async() -> bool:
    if client.sync_lock:
        return False
    if client.convert_lock:
        return False
    client.convert_lock = True

    convert_files_thread = threading.Thread(target=convert_files)
    convert_files_thread.start()
    while convert_files_thread.is_alive():
        await asyncio.sleep(1)
    client.convert_lock = False
    return True


client = setup_bot_client()


@client.event
async def on_ready():
    if client.user.id not in client.owners:
        client.owners.append(client.user.id)

    client.shows_server = client.get_guild(client.shows_server_id)
    print(f"{CCs.OKGREEN}starting up the upload server!{CCs.ENDC}")
    # client.show_sending_server = ShowSendingServer(client)
    # await client.show_sending_server.start()

    print(f"{CCs.OKGREEN}show uploader bot ready!{CCs.ENDC}")
    # await sort_show_channels()
    await sync_all_uploads(client.shows_folder)


@client.event
async def on_message(message: discord.Message):
    if message.author == client.user:
        return
    if message.author.id != constants.SELFBOT_NOTIFIER_WEBHOOK_ID:
        return
    if message.content != "!STARTUPLOAD":
        return
    await convert_files_async()
    await sync_all_uploads(client.shows_folder)


@client.command()
async def about(ctx):
    await ctx.send("hello! i am show uploader bot! i upload ur fav tv shows!!\n"
                   "source code here: https://github.com/meow6969/random-things/tree/main/python/showuploaderbot\n"
                   "check if out if u want!")


@client.command()
async def sync_shows(ctx):
    await ctx.send("syncing shows...")
    r = await sync_all_uploads(client.shows_folder)
    if r:
        return await ctx.send("done syncing shows!")
    elif client.sync_lock:
        return await ctx.send("sync is locked!")
    elif client.convert_lock:
        return await ctx.send("convert is locked!")
    return await ctx.send("error syncing shows!")


def start_bot():
    ensure_config_json_exists()
    with open("config.json") as meow:
        the_config = json.load(meow)
        token = the_config["token"]

    # convert_files()

    client.run(token)


if __name__ == "__main__":
    start_bot()
