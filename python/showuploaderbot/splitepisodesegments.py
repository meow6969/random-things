from pathlib import Path
import math
import subprocess

import natsort
import cv2
import pytesseract


def frame_number_to_timecode(frame_number, fps):
    secs = frame_number / fps
    mins = math.floor(secs / 60)
    secs = int(secs % 60)
    return f"{mins:02d}:{secs:02d}"


def split_episode(i_file: Path, season_number: int, episode_number: int):
    print(f"processing i_file={i_file}...")
    search_range = 30  # seconds
    # search_text = ["WRITTEN BY", "WRITTEN-BY", "WRITTENBY", "WRITTEN.BY", "WRITTEN", "STORY"]
    search_text = ["WRITTEN", "STORY"]
    out_dir = i_file.parent.joinpath("a")
    out_dir.mkdir(parents=True, exist_ok=True)

    vidcap = cv2.VideoCapture(str(i_file))
    # vidcap.get(cv2.CAP_PROP_)
    fps = int(vidcap.get(cv2.CAP_PROP_FPS))
    frame_count = int(vidcap.get(cv2.CAP_PROP_FRAME_COUNT))
    # time_length = frame_count / fps
    half_point = frame_count / 2
    search_range *= fps
    start_bound = half_point - search_range
    end_bound = half_point + search_range

    if out_dir.joinpath(f'S{season_number:02d}E{episode_number:02d}{i_file.suffix}').exists():
        return

    success = True
    vidcap.set(cv2.CAP_PROP_POS_FRAMES, start_bound)
    frame = int(start_bound)
    # frame = 0
    second_timer = 0
    # vidcap.
    while success:
        success, image = vidcap.read()
        if start_bound <= frame <= end_bound and second_timer == 0:
            print(f"frame={frame}/{frame_count}")
            img_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            ocr_out = pytesseract.image_to_string(img_rgb)
            print(f"ocr=\"{ocr_out}\"")
            for t in search_text:
                if t in ocr_out:
                    timecd = frame_number_to_timecode(frame - (fps * 3), fps)
                    print(f"found match at timecode={timecd}")
                    subprocess.run([
                        "ffmpeg",
                        "-hwaccel", "auto",
                        "-i", f"{i_file}",
                        "-c", "copy",
                        "-to", f"{timecd}",
                        f"{out_dir.joinpath(f'S{season_number:02d}E{episode_number:02d}{i_file.suffix}')}"
                    ])
                    subprocess.run([
                        "ffmpeg",
                        "-hwaccel", "auto",
                        "-i", f"{i_file}",
                        "-c", "copy",
                        "-ss", f"{timecd}",
                        f"{out_dir.joinpath(f'S{season_number:02d}E{episode_number + 1:02d}{i_file.suffix}')}"
                    ])
                    return
        if frame > end_bound:
            raise Exception(f"could not find a match for video={i_file}")

        frame += 1
        second_timer += 1
        if second_timer >= fps:
            # print(f"frame={frame}/{frame_count}")
            second_timer = 0


def main():
    in_dir = Path("/mnt/f/meow/toconvert/amazing-world-of-gumball/s04/")
    fils = list(in_dir.iterdir())
    fils = natsort.natsorted(fils)
    season_number = 3
    episode_number = 1
    for subdir in fils:
        if not subdir.is_file():
            continue
        if subdir.suffix != ".mkv":
            continue
        split_episode(
            subdir,
            season_number,
            episode_number
        )
        episode_number += 2


if __name__ == "__main__":
    main()
