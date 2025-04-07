# import sys
# sys.path.append("../")

import constants
# from constants import MIN_FILENAME_LENGTH, FILENAME_ILLEGAL_CHARS, MAX_FILENAME_LENGTH
# print(MIN_FILENAME_LENGTH)


def is_ascii(s: str) -> bool:
    try:
        s.encode('ascii')
    except UnicodeDecodeError:
        return False
    return True


def check_if_name_is_ok(file_name: str, require_extension=False) -> bool:
    # print("check_if_name_is_ok")
    # __FILENAME_ILLEGAL_CHARS = [
    #     ".",
    #     "/",
    #     "<",
    #     ">",
    #     ":",
    #     "\"",
    #     "'",
    #     "\\",
    #     "|",
    #     "?",
    #     "*"
    # ]
    #
    # __MIN_FILENAME_LENGTH = 5
    # __MAX_FILENAME_LENGTH = 256

    for char in constants.FILENAME_ILLEGAL_CHARS:
        if char in file_name:
            return False
    if file_name.strip()[0] == ".":
        return False
    if len(file_name) < constants.MIN_FILENAME_LENGTH:
        # print("check_if_name_is_ok_return")
        return False
    if len(file_name) > constants.MAX_FILENAME_LENGTH:
        # print("check_if_name_is_ok_return")
        return False
    fi_split = file_name.split('.')
    # print("48")
    # print(fi_split)
    if require_extension:
        # print(f"require_extension fi_split={fi_split}, len={len(fi_split)}, [-1]={fi_split[-1]}")
        # print(f"supported_extensions={constants.FILENAME_SUPPORTED_EXTENSIONS}")
        # print(f"{fi_split[-1] in constants.FILENAME_SUPPORTED_EXTENSIONS}")
        if len(fi_split) == 1 or fi_split[-1] not in constants.FILENAME_SUPPORTED_EXTENSIONS:
            # print(f"len fi_split = 1 or fi_split[-1] not in FILENAME_SUPPORTED_EXTENSIONS")
            return False
    if not is_ascii(file_name):
        # print("not ascii")
        return False
    # print("check_if_name_is_ok_return")
    return True
