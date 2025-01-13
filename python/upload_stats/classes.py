import os
import sqlite3
import math


class database:
    def __init__(self):
        self.conn = sqlite3.connect(f"{os.path.dirname(__file__)}/nicotine-plus.db")
        self.cur = self.conn.cursor()

    def db_setup(self):
        self.cur.execute("""
            CREATE TABLE IF NOT EXISTS downloaders (
                user_name TEXT PRIMARY KEY,
                is_buddy BOOLEAN NOT NULL,
                time_first_dl INT NOT NULL,  
                time_last_dl INT NOT NULL,
                total_files_downloaded INT NOT NULL,
                total_files_completed INT NOT NULL,
                total_bytes_downloaded INT NOT NULL,
                total_files_shared INT NOT NULL,
                is_leecher BOOLEAN NOT NULL
            )
        ;""")

        self.cur.execute("""
            CREATE TABLE IF NOT EXISTS file (
                file_location TEXT PRIMARY KEY,
                file_name TEXT NOT NULL,
                total_dl INT NOT NULL,
                total_completed_dl INT NOT NULL,
                time_first_dl INT NOT NULL,
                time_last_dl INT NOT NULL,
                virtual_location TEXT NOT NULL
            )
        ;""")
        self.conn.commit()

    def select_downloader_user(self, user_name, is_buddy=False, time_first_dl=False, time_last_dl=False,
                               total_files_downloaded=False, total_files_completed=False, total_bytes_downloaded=False,
                               total_files_shared=False, is_leecher=False) -> dict:

        if not self.check_if_key_exists("user_name", user_name, "downloaders"):
            return {}

        information_to_retrieve = ""
        if is_buddy:
            information_to_retrieve += f"is_buddy, "
        if time_first_dl:
            information_to_retrieve += f"time_first_dl, "
        if time_last_dl:
            information_to_retrieve += f"time_last_dl, "
        if total_files_downloaded:
            information_to_retrieve += f"total_files_downloaded, "
        if total_files_completed:
            information_to_retrieve += f"total_files_completed, "
        if total_bytes_downloaded:
            information_to_retrieve += f"total_bytes_downloaded, "
        if total_files_shared:
            information_to_retrieve += f"total_files_shared, "
        if is_leecher:
            information_to_retrieve += f"is_leecher, "

        if information_to_retrieve.strip() == '':  # no retrieval information was enabled
            return {}
        information_to_retrieve = information_to_retrieve[:-2]  # take off the last ", "

        response = self.cur.execute(f"""
            SELECT {information_to_retrieve} FROM downloaders WHERE user_name=:user_name
        ;""", {"user_name": user_name})

        data_retrieved = []
        for i in information_to_retrieve.split(','):
            data_retrieved.append(i.strip())

        return self.select_to_dict(response, data_retrieved)

    def select_file_location(self, file_location, file_name=False, total_dl=False, total_completed_dl=False,
                             time_first_dl=False, time_last_dl=False) -> dict:

        if not self.check_if_key_exists("file_location", file_location, "file"):
            return {}

        information_to_retrieve = ""
        if file_name:
            information_to_retrieve += f"file_name, "
        if total_dl:
            information_to_retrieve += f"total_dl, "
        if total_completed_dl:
            information_to_retrieve += f"total_completed_dl, "
        if time_first_dl:
            information_to_retrieve += f"time_first_dl, "
        if time_last_dl:
            information_to_retrieve += f"time_last_dl, "

        if information_to_retrieve.strip() == '':  # no retrieval information was enabled
            return {}
        information_to_retrieve = information_to_retrieve[:-2]  # take off the last ", "

        response = self.cur.execute(f"""
            SELECT {information_to_retrieve} FROM file WHERE file_location=:file_location
        ;""", {"file_location": file_location})

        data_retrieved = []
        for i in information_to_retrieve.split(','):
            data_retrieved.append(i.strip())

        return self.select_to_dict(response, data_retrieved)

    def check_if_key_exists(self, key, key_value, db_name) -> bool:
        # print(key)
        # print(key_value)
        # print(db_name)
        # response = self.cur.execute(f"""
        #     SELECT :key FROM :db_name WHERE :key=:key_value
        # ;""", {"key": key, "db_name": db_name, "key_value": key_value})
        response = self.cur.execute(f"""
            SELECT {key} FROM {db_name} WHERE {key} = :key_value
        ;""", {"key_value": key_value})

        if len(list(response)) == 0:
            return False
        else:
            return True

    @staticmethod
    def select_to_dict(response, data_retrieved) -> dict:
        return_dict = {}
        __response = []
        for i in response:
            for y in i:
                __response.append(y)
        for i, info in enumerate(__response):
            return_dict[data_retrieved[i]] = info
        return return_dict

    @staticmethod
    def convert_size(size_bytes):
        if size_bytes == 0:
            return "0B"
        size_name = ("B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB")
        i = int(math.floor(math.log(size_bytes, 1024)))
        p = math.pow(1024, i)
        s = round(size_bytes / p, 2)
        return "%s %s" % (s, size_name[i])
