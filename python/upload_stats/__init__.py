# GNU GENERAL PUBLIC LICENSE
#    Version 3, 29 June 2007
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program.  If not, see <http://www.gnu.org/licenses/>.

from pynicotine import slskmessages
from pynicotine.pluginsystem import BasePlugin, ResponseThrottle
import classes
import time
import os


class Plugin(BasePlugin):

    def __init__(self, *args, **kwargs):

        super().__init__(*args, **kwargs)

        self.settings = {
            'num_files': 1,
            'num_folders': 1,
        }
        self.metasettings = {
            'message': {
                'description': ('saves statistics for what gets downloaded from your shares and saves some info on'
                                'the users who download from you'),
                'type': 'textview'
            },
            'num_files': {
                'description': 'Require users to have a minimum number of shared files:',
                'type': 'int', 'minimum': 0
            },
            'num_folders': {
                'description': 'Require users to have a minimum number of shared folders:',
                'type': 'int', 'minimum': 1
            }
        }

        self.probed = {}
        self.str_action = ""
        self.db_jobs = {}
        self.throttle = ResponseThrottle(self.core, self.human_name)

        self.database = classes.database()

        self.update_db = True

    def loaded_notification(self):
        min_num_files = self.metasettings['num_files']['minimum']
        min_num_folders = self.metasettings['num_folders']['minimum']

        if self.settings['num_files'] < min_num_files:
            self.settings['num_files'] = min_num_files

        if self.settings['num_folders'] < min_num_folders:
            self.settings['num_folders'] = min_num_folders

        # set up sqlite database
        self.database.db_setup()

        # self.log("Upload Stats plugin loaded successfully!")
        # self.log(__file__)

    def upload_queued_notification(self, user, virtual_path, real_path):
        # self.log(virtual_path) returns something like "anime\Steins;Gate (2011)\Season 1\S01E01.mkv"

        if self.database.check_if_key_exists("user_name", user, "downloaders"):
            response = self.database.select_downloader_user(user, total_files_downloaded=True)
            self.database.cur.execute(f"""
                UPDATE downloaders
                    SET total_files_downloaded = {response["total_files_downloaded"] + 1},
                    time_last_dl = {int(time.time())}
                WHERE user_name=:user_name
            ;""", {"user_name": user})
        else:
            if user not in self.probed:
                self.probed[user] = ['requesting', 1]
                # self.core.queue_network_message(slskmessages.GetUserStats(user))
                slskmessages.GetUserStats(user)
                self.log("Getting statistics from the server for new user %s…", user)
            else:
                self.probed[user][1] += 1
        if self.database.check_if_key_exists("file_location", real_path, "file"):
            response = self.database.select_file_location(real_path, total_dl=True)
            self.database.cur.execute(f"""
                UPDATE file
                    SET total_dl = {response["total_dl"] + 1},
                    time_last_dl = {int(time.time())}
                WHERE file_location=:file_location
            ;""", {"file_location": real_path})
        else:
            # print(real_path)
            # print(os.path.basename(real_path))
            self.database.cur.execute(f"""
                INSERT INTO file(file_location, file_name, total_dl, total_completed_dl, time_first_dl, time_last_dl, 
                                 virtual_location) 
                    VALUES (
                        :file_location,
                        :file_name,
                        1,
                        0,
                        {int(time.time())},
                        {int(time.time())},
                        :virtual_location
                    )
            ;""", {"file_location": real_path, "file_name": os.path.basename(real_path),
                   "virtual_location": virtual_path})
        self.database.conn.commit()

    def user_stats_notification(self, user, stats):
        if user not in self.probed:
            # We did not trigger this notification
            return

        if self.probed[user][0] != 'requesting':
            # We already dealt with this user.
            return

        if user in (i[0] for i in self.config.sections["server"]["userlist"]):
            is_buddy = 1
        else:
            is_buddy = 0

        if stats['files'] >= self.settings['num_files'] and stats['dirs'] >= self.settings['num_folders']:
            is_leecher = 0
        else:
            is_leecher = 1

        time.sleep(1)

        self.database.cur.execute(f"""
            INSERT INTO downloaders(user_name, is_buddy, time_first_dl, time_last_dl, total_files_downloaded,
                                    total_files_completed, total_bytes_downloaded, total_files_shared, is_leecher)
                VALUES (
                    :user_name,
                    {is_buddy},
                    {int(time.time())},
                    {int(time.time())},
                    {self.probed[user][1]},
                    0,
                    0,
                    {stats['files']},
                    {is_leecher}
                )
        ;""", {"user_name": user})
        self.database.conn.commit()

        del self.probed[user]

    def upload_finished_notification(self, user, virtual_path, real_path):

        if self.database.check_if_key_exists("user_name", user, "downloaders"):
            file_size = os.stat(real_path).st_size
            response = self.database.select_downloader_user(user, total_files_completed=True,
                                                            total_bytes_downloaded=True)
            self.database.cur.execute(f"""
                UPDATE downloaders
                    SET total_files_completed = {response["total_files_completed"] + 1},
                    total_bytes_downloaded = {response["total_bytes_downloaded"] + file_size}
                WHERE user_name=:user_name
            ;""", {"user_name": user})
            self.database.conn.commit()
        if self.database.check_if_key_exists("file_location", real_path, "file"):
            response = self.database.select_file_location(real_path, total_completed_dl=True)
            self.database.cur.execute(f"""
                UPDATE file
                    SET total_completed_dl = {response["total_completed_dl"] + 1}
                WHERE file_location = :file_location
            ;""", {"file_location": real_path})

            self.database.conn.commit()

    def incoming_public_chat_notification(self, room, user, line):
        if user != self.core.users.login_username:
            return
        if not self.throttle.ok_to_respond(room, user, line, 10):
            return

        if "!stats" in line.lower().strip():
            response = self.database.cur.execute(f"""
                SELECT total_files_downloaded, total_files_completed, total_bytes_downloaded, is_leecher 
                    FROM downloaders
            ;""")
            retrieved = ["total_files_downloaded", "total_files_completed", "total_bytes_downloaded", "is_leecher"]

            data = {}

            for i in retrieved:
                data[i] = 0
            users = 0
            leecher_upload_amt = 0
            for column in response:
                users += 1
                if column[3] == True:
                    leecher_upload_amt += column[2]
                for i, row in enumerate(column):
                    data[retrieved[i]] += row


            human_readable_size = self.database.convert_size(data[retrieved[2]])
            human_readable_leecher_size = self.database.convert_size(leecher_upload_amt)

            self.send_public(room, f"total files uploaded: {data[retrieved[0]]}, "
                                   f"total files completed: {data[retrieved[1]]}, "
                                   f"total amount uploaded: {human_readable_size}, "
                                   f"total users: {users}, "
                                   f"total leechers: {data[retrieved[3]]}, "
                                   f"leecher percentage: {int(int(data[retrieved[3]]) / users * 100)}%, "
                                   f"amount uploaded to leechers: {human_readable_leecher_size}")
            # print(response)
        # if "!filestats" in line.lower().strip():
        #     response = self.database.cur.execute(f"""
        #         SELECT total_files_downloaded, total_files_completed, total_bytes_downloaded, is_leecher
        #             FROM downloaders
        #     ;""")

    def disable(self):
        self.update_db = False
        self.database.conn.close()
