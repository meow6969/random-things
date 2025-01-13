import sqlite3
import time

# this file is just me testing out sql commands
# im really bad at sql
# idk why i admit that here its kinda obvious cause of like everything uhh
# ok 

# set up sqlite database
# conn = sqlite3.connect("nicotine-plus.db")
conn = sqlite3.connect("/home/meow/.local/share/nicotine/plugins/upload_stats/nicotine-plus.db")
cur = conn.cursor()


response = cur.execute(f"""
    SELECT file_location
        FROM file
;""")
retrieved = ["file_location"]

data = []

for column in response:
    for i, row in enumerate(column):
        data.append(row)

new_data = []

for i in data:
    new_i = ''
    if i.startswith("/mnt/h/musics"):
        new_i = "musics" + i[len("/mnt/h/musics"):]
        new_data.append(new_i.replace("/", "\\"))
        continue
    elif i.startswith("/mnt/h/plex-folder/da tv"):
        new_i = "anime" + i[len("/mnt/h/plex-folder/da tv"):]
        new_data.append(new_i.replace("/", "\\"))
        continue
    elif i.startswith("/mnt/h/plex-folder/da animei movei"):
        new_i = "anime movies" + i[len("/mnt/h/plex-folder/da animei movei"):]
        new_data.append(new_i.replace("/", "\\"))
        continue


for i, file_location in enumerate(data):
    # print(i)
    cur.execute("""
        UPDATE file 
            SET virtual_location = :virtual_location WHERE file_location = :file_location
    ;""", {"virtual_location": new_data[i], "file_location": file_location})
    # print(f"UPDATE file SET virtual_location = {new_data[i]} WHERE file_location = {file_location}")
conn.commit()

conn.close()
