import os


# seperated into its own file to prevent circular import errors
class CCs:  # this is just color codes so that printed lines in the console look pretty
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKCYAN = '\033[96m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'


# doesnt have to be here but whatever imports would look ugly in showuploaderbotfuncs.py otherwise
def ensure_constants_py_exists() -> None:
    if not os.path.exists("constants.py"):
        shutil.copy("constants.example.py", "constants.py")
