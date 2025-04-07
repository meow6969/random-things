import sys
import math
import copy
from enum import Enum
from typing import Optional, Sequence, Callable, Final, Annotated


mlist: Final[list[int]] = [
 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 
 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'y', 'x', 'z', ' '
]
n: Final[int] = 1
program_name: Final[str] = "skidcode"
NEXTLINESTARTINDEXHERE: Final[str] = "{{NEXTLINESTARTINDENTHERE}}"


class SpecialReturn(Enum):
    NONE               = 0
    HELPMESSAGE        = 1
    HELPVALUE          = 2
    HELPCOMPATIBLE     = 3
    NEEDSTOKEN         = 4


def find_cmd_options_for_method(the_method: Callable[[Optional[str], Optional[SpecialReturn]], str]) -> list[str]:
    found_options: list[str] = []
    options = get_options()
    for a_key in options.keys():
        if options[a_key] is the_method:
            found_options.append(a_key)
    return found_options


def list_str_to_str(the_list: list[str], 
                    separator: Optional[str] = ", ", 
                    surrounds: Optional[Annotated[Sequence[str], 2] | str] = ("", ""), 
                    array_surrounds: Optional[Annotated[Sequence[str], 2] | str] = ("", ""),
                    array_surrounds_min_list_len: int = 0) -> str:
    return_str = ""
    if isinstance(surrounds, str):
        surrounds = (surrounds, surrounds)
    if isinstance(array_surrounds, str):
        array_surrounds = (array_surrounds, array_surrounds)
    for str_from_list in the_list:
        return_str += f"{surrounds[0]}{str_from_list}{surrounds[1]}{separator}"
    if len(return_str) <= len(separator) and array_surrounds_min_list_len <= len(the_list):
        if array_surrounds[0] == "" and array_surrounds[1] == "":
            return_str = "[]"
        elif len(the_list) >= array_surrounds_min_list_len:
            return_str = f"{array_surrounds[0]}{array_surrounds[1]}"
    else:
        return_str = f"{array_surrounds[0]}{return_str[:-len(separator)]}{array_surrounds[1]}"
    return return_str


def method_to_cmd_options_str(the_method: Callable[[Optional[str], Optional[SpecialReturn]], str], 
                              separator: Optional[str] = None, surrounds: Optional[Annotated[Sequence[str], 2] | str] = None, array_surrounds: Optional[Annotated[Sequence[str], 2] | str] = None) -> str:
    return list_str_to_str(find_cmd_options_for_method(the_method), separator=separator, surrounds=surrounds, array_surrounds=array_surrounds)


def collect_option_aliases() -> list[tuple[Callable[[Optional[str], Optional[SpecialReturn]], str], list[str]]]:
    options = get_options()
    the_aliases: list[tuple[Callable[[Optional[str], Optional[SpecialReturn]], str], list[str]]]
    the_aliases = []

    # returns the index, or -1 if it is not in the aliases list
    def option_method_in_aliases(meow_option: str) -> int:
        for i, alias in enumerate(the_aliases):
            if options[meow_option] is alias[0]:
                return i
        return -1

    for option in options.keys():
        the_index = option_method_in_aliases(option)
        if the_index == -1:
            the_aliases.append((options[option], [option]))
        elif option not in the_aliases[the_index][1]:
            the_aliases[the_index][1].append(option)
            
    return the_aliases


def ensure_option_aliases(option_aliases: list[tuple[Callable[[Optional[str], Optional[SpecialReturn]], str], list[str]]] | None):
    if option_aliases is not None:
        return option_aliases
    return collect_option_aliases()


def ensure_option_alias(the_alias: tuple[Callable[[Optional[str], Optional[SpecialReturn]], str], list[str]] | str | Callable[[Optional[str], Optional[SpecialReturn]], str],
                        option_aliases: list[tuple[Callable[[Optional[str], Optional[SpecialReturn]], str], list[str]]] | None) -> tuple[Callable[[Optional[str], Optional[SpecialReturn]], str], list[str]]:
    option_aliases = ensure_option_aliases(option_aliases)
    if isinstance(the_alias, tuple):
        return the_alias
    if isinstance(the_alias, str):
        return option_alias_from_option_key(the_alias, option_aliases)
    if callable(the_alias):
        return option_alias_from_option_method(the_alias, option_aliases)
    else:
        raise Exception(f"internal error: alias: {the_alias} is incompatible type: {type(the_alias)}")

def option_alias_from_option_key(the_option: str,
                                 option_aliases: Optional[list[tuple[Callable[[Optional[str], Optional[SpecialReturn]], str], list[str]]]] = None) -> tuple[Callable[[Optional[str], Optional[SpecialReturn]], str], list[str]]:
    option_aliases = ensure_option_aliases(option_aliases)
    for thingy in option_aliases:
        if the_option in thingy[1]:
            return thingy
    raise Exception(f"internal error: could not get option alias for option: {the_option} from option aliases: {option_aliases}")


def option_alias_from_option_method(the_method: Callable[[Optional[str], Optional[SpecialReturn]], str],
                                    option_aliases: list[tuple[Callable[[Optional[str], Optional[SpecialReturn]], str], list[str]]] | None) -> tuple[Callable[[Optional[str], Optional[SpecialReturn]], str], list[str]]:
    option_aliases = ensure_option_aliases(option_aliases)
    return option_alias_from_option_key(get_option_key_from_option_method(the_method), option_aliases)


def get_option_key_from_option_method(the_method: Callable[[Optional[str], Optional[SpecialReturn]], str]) -> str:
    options = get_options()
    for the_key in options.keys():
        if options[the_key] is the_method:
            return the_key
    raise Exception(f"internal error: key (the_key) {the_key} does not exist in dict (options) {options}")


def get_option_keys_from_option_methods(the_methods: list[Callable[[Optional[str], Optional[SpecialReturn]], str]]) -> list[str]:
    new_keys: list[str] = []
    for meow_method in the_methods:
        new_keys.append(get_option_key_from_option_method(meow_method))
    return new_keys


def get_option_key_string_from_option_methods(the_methods: list[Callable[[Optional[str], Optional[SpecialReturn]], str]]) -> str:
    return list_str_to_str(get_option_keys_from_option_methods(the_methods), separator="|")


def get_option_keys_from_option_key_string(the_string: str) -> list[str]:
    the_split = the_string.split("|")
    if len(the_split) == 1 and the_split[0] == "":
        return []
    return the_split


def get_option_methods_from_option_key_string(the_string: str) -> list[Callable[[Optional[str], Optional[SpecialReturn]], str]]:
    nya_list: list[Callable[[Optional[str], Optional[SpecialReturn]], str]]
    nya_list = []
    the_keys = get_option_keys_from_option_key_string(the_string)
    options = get_options()
    for the_key in the_keys:
        nya_list.append(options[the_key])
    return nya_list
        

def encrypt(x: Optional[str] = None, __get_help_text: Optional[SpecialReturn] = SpecialReturn.NONE) -> str:
    if __get_help_text != SpecialReturn.NONE:
        if __get_help_text == SpecialReturn.HELPVALUE:
            return "PLAINTEXT"
        if __get_help_text == SpecialReturn.HELPMESSAGE:
            help_value = encrypt(__get_help_text=SpecialReturn.HELPVALUE)
            return f"encrypts the given {help_value}"
        if __get_help_text == SpecialReturn.HELPCOMPATIBLE:
            compats = [decrypt]
            return get_option_key_string_from_option_methods(compats)
        if __get_help_text == SpecialReturn.NEEDSTOKEN:
            return "Y"
        return ""
    if x is None or x.strip() == "":
        raise Exception(f"invalid text for {method_to_cmd_options_str(encrypt, separator=" / ", surrounds="'")}: no text given to encrypt")
    global mlist
    encrypted_text = ""
    for i in x.lower():
        if i.isdigit():
            options_error(f"invalid text for {method_to_cmd_options_str(encrypt, separator=" / ", surrounds="'")}: encrypt does not accept numbers as input")
            return
        if i == "z":
            encrypted_text += "$"
        elif i != " " and i != "a":
            encrypted_text += mlist[mlist.index(i) + n]
        else:
            encrypted_text += i

    return encrypted_text


def decrypt(y: Optional[str] = None, __get_help_text: Optional[SpecialReturn] = SpecialReturn.NONE) -> str:
    if __get_help_text != SpecialReturn.NONE:
        if __get_help_text == SpecialReturn.HELPVALUE:
            return "CIPHERTEXT"
        if __get_help_text == SpecialReturn.HELPMESSAGE:
            help_value = decrypt(__get_help_text=SpecialReturn.HELPVALUE)
            return f"decrypts the given {help_value}"
        if __get_help_text == SpecialReturn.HELPCOMPATIBLE:
            compats = [encrypt]
            return get_option_key_string_from_option_methods(compats)
        if __get_help_text == SpecialReturn.NEEDSTOKEN:
            return "Y"
        return ""
    if y is None or y.strip() == "":
        raise Exception(f"invalid text for {method_to_cmd_options_str(decrypt, separator=" / ", surrounds="'")}: no text given to encrypt")
    decrypted_text = ""
    for i in y.lower():
        if i.isdigit():
            raise Exception(f"invalid text for {method_to_cmd_options_str(decrypt, separator=" / ", surrounds="'")}: decrypt does not accept numbers as input")
        if i == "$":
            decrypted_text += "z"
        elif i != " " and i != "a":
            decrypted_text += mlist[mlist.index(i) - n]
        else:
            decrypted_text += i

    return decrypted_text


def get_individual_option_usage(meow_alias: tuple[Callable[[Optional[str], Optional[SpecialReturn]], str]]) -> str:
    the_usage = f"{list_str_to_str(meow_alias[1], separator="|", surrounds="", array_surrounds=("[", "]"))}"
    # if len(meow_alias[1]) > 1:
    #     the_usage = f"({the_usage})"
    help_value_option = meow_alias[0](__get_help_text=SpecialReturn.HELPVALUE)
    the_usage += "\t"
    if help_value_option.strip() != "":
        the_usage += f" {help_value_option}"
    the_usage += "\t"
    return the_usage


def list_of_lists_to_sets(the_list: list[list], parent_stays_list=False) -> set[set] | list[set]:
    new_list = []
    for item in the_list:
        if isinstance(item, list):
            new_list.append(list_of_lists_to_sets(item))
            continue
        new_list.append(item)
    if parent_stays_list:
        return new_list
    return set(new_list)


def get_option_usage(option: tuple[Callable[[Optional[str], Optional[SpecialReturn]], str], list[str]] | str | Callable[[Optional[str], Optional[SpecialReturn]], str],
                     option_aliases: Optional[list[tuple[Callable[[Optional[str], Optional[SpecialReturn]], str], list[str]]] | None] = None,
                     compat_options_gotten: Optional[list[Callable[[Optional[str], Optional[SpecialReturn]], str]] | None] = None) -> str:
    if compat_options_gotten is None:    
        compat_options_gotten = []

    return_strings = [f"{get_individual_option_usage(option)}"]
    current_methods = [option[0]]
    option = ensure_option_alias(option, option_aliases)
    option_aliases = ensure_option_aliases(option_aliases)    
    option_compats = get_option_methods_from_option_key_string(option[0](__get_help_text=SpecialReturn.HELPCOMPATIBLE))
    for compat_method in option_compats:
        if current_methods + [compat_method] in compat_options_gotten: #  or current_methods in compat_options_gotten:
            continue
        
        current_methods.append(compat_method)
        new_thingy = get_individual_option_usage(option_alias_from_option_method(compat_method, option_aliases))
        return_strings.append(new_thingy)
    compat_options_gotten.append(current_methods)
    if len(return_strings) == 1:
        return ""
    return list_str_to_str(return_strings, separator=" ")


def usage_str() -> str:    
    option_aliases = collect_option_aliases()
    
    return_str = "usage: \n"
    option_usages: list[str] = []
    gotten_usages: list[list[Callable[[Optional[str], Optional[SpecialReturn]], str]]] = []
    for alias in option_aliases:
        if [alias[0]] not in gotten_usages:
            option_usages.append(get_individual_option_usage(alias))
            gotten_usages.append([alias[0]])
        the_usage = get_option_usage(alias, option_aliases, gotten_usages)
        if the_usage == "":
            continue
        option_usages.append(the_usage)
        
    all_help_options_usage = list_str_to_str(option_usages, separator="\n", surrounds=(f"  {program_name} ", ""))
    return_str += all_help_options_usage

    return return_str


def description_str() -> str:
    return (f"{program_name}: {NEXTLINESTARTINDEXHERE}a cli based text encryptor/decryptor, originally made by skid (discord=skidgod)\n"
            f"{NEXTLINESTARTINDEXHERE}i randomly found the \"Skid_EDcrypt.exe\" file while looking thru my old files and i was bored\n"
            f"so i made this"
    )


def print_the_output(msg: str):
    def count_initial_string_occurrence(being_searched: str, searching_for: str = " ", get_num_string_before_search: str | None = None):
        token = ""
        num_string_token = ""
        step = len(searching_for)
        search_index = 0
        occurrences_before_search = 0
        for search_index, char in enumerate(being_searched):
            token += char
            if get_num_string_before_search is not None:
                num_string_token += char
                if get_num_string_before_search in num_string_token:
                    num_string_token = ""
                    occurrences_before_search += 1
            
            if len(token) < step:
                continue
            if len(token) > step:
                token = char
            if token != searching_for and len(token) == step:
                break
        search_string_occurrences = math.floor(search_index / step)
        if get_num_string_before_search:
            if search_string_occurrences == 0:
                return 0
            return occurrences_before_search
        return search_string_occurrences
        
    longest_columns = [[]]
    
    current_num_indents = count_initial_string_occurrence(msg)
    for line in msg.split("\n"):

        line_num_indents = count_initial_string_occurrence(line)
        if line_num_indents != current_num_indents:
            longest_columns.append([])
            current_num_indents = line_num_indents
        for i, column in enumerate(line.split("\t")):
            column_len = len(column)
            if NEXTLINESTARTINDEXHERE in column:
                column_len -= len(NEXTLINESTARTINDEXHERE)
            if i > len(longest_columns[-1]) - 1:
                longest_columns[-1].append(column_len)
                continue
            longest_columns[-1][i] = max(longest_columns[-1][i], column_len)
    new_msg = ""
    current_indent_index = 0
    current_num_indents = 0
    line_indent_offset = -1
    for line in msg.split("\n"):
        line_num_indents = count_initial_string_occurrence(line)
        line_prefix = ""
        #newline = line
        if line_indent_offset != -1:  # if this line is effected by NEXTLINESTARTINDEXHERE
            line = " " * line_indent_offset + line
        if line_indent_offset == -1 and line_num_indents != current_num_indents:
            current_indent_index += 1
            current_num_indents = line_num_indents
        line_indent_offset = line.find(NEXTLINESTARTINDEXHERE)
        line = line.replace(NEXTLINESTARTINDEXHERE, "", 1)
        
        i = 0
        line_columns = line.split("\t")
        while "\t" in line:
            
            line = line.replace("\t", " " * (longest_columns[current_indent_index][i] - len(line_columns[i]) + 1), 1)
            i += 1
        new_msg += f"{line}\n"
            
    print(new_msg)


def get_program_info() -> str:
    return (
        f"{description_str()}\n"
        f"\n"
        f"{usage_str()}"
    )


def options_error(msg: str):
    print_str = (
        f"{get_program_info()}\n"
        f"try '{program_name} --help' for help\n"
        f"\n"
        f"error: {msg}")
    print_the_output(print_str)


def print_help_options() -> str:
    return_str = f"options:\n"
    option_aliases = collect_option_aliases()
    for alias in option_aliases:
        return_str += f"  {list_str_to_str(alias[1])} {alias[0](__get_help_text=SpecialReturn.HELPVALUE)} \t{alias[0](__get_help_text=SpecialReturn.HELPMESSAGE)}\n"
    return return_str[:-1]


def get_options() -> dict[str, Callable[[Optional[str], Optional[SpecialReturn]], str]]:
    options = {
        "h"         : help,
        "help"      : help,
        "-h"        : help,
        "--help"    : help,
        "-e"        : encrypt,
        "--encrypt" : encrypt,
        "-d"        : decrypt,
        "--decrypt" : decrypt
    }
    return options


def help(_: Optional[str] = None, __get_help_text: Optional[SpecialReturn] = SpecialReturn.NONE) -> str:
    if __get_help_text != SpecialReturn.NONE:
        if __get_help_text == SpecialReturn.HELPMESSAGE:
            return "show this message and exit\n"
        return ""
    help_text = (
        f"{get_program_info()}\n"
        f"{print_help_options()}"
    )
    return help_text


def main():
    options = get_options()
    current_method = None
    getting_token = False
    token = ""

    if len(sys.argv) <= 1:
        raise Exception("not enough arguments given")
    for arg in sys.argv[1:]:
        if arg in options.keys():
            if getting_token:
                print_the_output(current_method(token))
                getting_token = False
                token = ""
                current_method = None
            current_method = options[arg]
            getting_token = options[arg](__get_help_text=SpecialReturn.NEEDSTOKEN) == "Y"
            if not getting_token:
                print_the_output(current_method())
                current_method = None
        else:
            if not getting_token:
                raise Exception(f"invalid argument passed: {arg}")
            token += arg
    if token != "":
        print_the_output(current_method(token))


if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        options_error(str(e))
