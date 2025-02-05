# you shouldnt really need to change anything here, except for VIDEO_FRAMERATE and VIDEO_RESOLUTION

FFMPEG_ARGS = ["ffmpeg", "-hwaccel", "auto", "-v", "error", "-i", "INPUT_FILE", "INSERT_FILTERS",
               "-b:v", "VIDEO_BITRATE", "-b:a", "AUDIO_BITRATE", "-r", "FRAMERATE", "-ac", "1", "-pix_fmt", "yuv420p",
               "OUTPUT_FILE"]
# the -2 means that it will only match the X value of the resolution while
# correctly scaling the Y value based on the video aspect ratio

# this can be flipped, for example (-2, 640) will scale the
# video based on the Y value, keeping the aspect ratio
VIDEO_RESOLUTION = (640, -2)
VIDEO_BITRATE_OFFSET = 9 / 10          # this defines how much we favor the audio bitrate over the video bitrate
VIDEO_FRAMERATE = 16                   # this is in FPS
ALLOWED_VIDEO_OFFSET_VARIANCE = 1 / 10
MAX_FILESIZE_KBITS = 100000 * 8
MAX_AUDIO_BITRATE_KBITS = 128
MIN_AUDIO_BITRATE_KBITS = 48           # literally no reason i picked this just felt it was good ???
FILTER_COMPLEX_BUILDER_JSON_PATH = "filter_complex_builder.json"

# these shouldnt need to be changed
IMAGE_BASED_SUBTITLE_CODECS = ["dvbsub", "dvdsub", "pgssub", "xsub", "hdmv_pgs_subtitle"]
TEXT_BASED_SUBTITLE_CODECS = ["ssa", "ass", "webvtt", "jacosub", "microdvd", "mov_text", "mpl2", "pjs", "realtext",
                              "sami", "stl", "subrip", "subviewer", "subviewer1", "text", "vplayer", "webvtt", "srt"]
