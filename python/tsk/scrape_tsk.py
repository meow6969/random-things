import datetime
import os
import io
import zipfile
import requests
import json
import pathlib


def get_project_info(page_content):
    page = page_content.decode()
    for line in page.splitlines():
        if "var tsProjectInfos = " in line:
            read_index = 0
            for i, char in enumerate(line):
                if char == "{":
                    read_index = i
                    break

            the_dict_text = line[read_index:-1]
            return json.loads(the_dict_text)

    print(page)
    print(f"could not find project info for project id: {project_id}")
    raise Exception()


def get_project_download_url(page_content):
    page = page_content.decode()
    for line in page.splitlines():
        if "var tsProjectDownloadUrl = " in line:
            read_index = 0
            for i, char in enumerate(line):
                if char == "\"":
                    read_index = i + 1
                    break

            return "https://platform.techsmart.codes" + line[read_index:-2]
    print(page)
    raise Exception()


def unzip_from_memory(zip_bytes, the_epic_project_id: int, edit_time: datetime.datetime):
    zip_file = zipfile.ZipFile(io.BytesIO(zip_bytes))
    project_name = pathlib.Path(zip_file.namelist()[0]).parent.name
    project_dir = f"./my_projects/{project_name}-{the_epic_project_id}"
    # if not os.path.exists("./my_projects"):
    #     os.mkdir("./my_projects")
    # if not os.path.exists(project_dir):
    #     os.mkdir(project_dir)
    print(project_dir)
    print(zip_file.namelist())
    for name in zip_file.namelist():
        file_save_path = f"{project_dir}{name[len(project_name):]}"
        pathlib.Path(file_save_path).parent.mkdir(parents=True, exist_ok=True, )
        with open(file_save_path, "wb") as ff:
            ff.write(zip_file.read(name))
            os.utime(file_save_path, (edit_time.timestamp(), edit_time.timestamp()))
    os.utime(project_dir, (edit_time.timestamp(), edit_time.timestamp()))


with open("config.json", "r") as f:
    tsk_cookie = json.load(f)["cookie"]


headers = {
    "accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,"
              "application/signed-exchange;v=b3;q=0.7",
    "accept-language": "en-US,en;q=0.9",
    "cookie": tsk_cookie,
    "user-agent": "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) "
                  "Chrome/130.0.0.0 Safari/537.36"
}

page_num = 1
all_projects = json.loads(requests.get(
    f"https://platform.techsmart.codes/code/my_code_data/{page_num}/last_modified/", headers=headers).content)
total_pages = all_projects["num_pages"]
# print(json.dumps(all_projects, indent=2))
# print(total_pages)
if not os.path.exists("successful.json"):
    successful_download_ids = []
else:
    with open("successful.json", "r") as f:
        successful_download_ids = json.load(f)

if not os.path.exists("skip.json"):
    skip_ids = []
else:
    with open("skip.json", "r") as f:
        skip_ids = json.load(f)

for i in range(1, total_pages + 1):
    all_projects = json.loads(requests.get(
        f"https://platform.techsmart.codes/code/my_code_data/{i}/last_modified/", headers=headers).content)
    the_project_infos: list[dict] = all_projects["page_code_project_infos"]
    for project_info in the_project_infos:
        the_project_id = int(project_info["id"])
        if the_project_id in successful_download_ids:
            continue
        if the_project_id in skip_ids:
            continue
        the_content = requests.get(
            f"https://platform.techsmart.codes/code/{the_project_id}/", headers=headers).content
        try:
            the_dl_url = get_project_download_url(the_content)
            dl_url_content = requests.get(the_dl_url, headers=headers).content
        except Exception as e:
            print(i)
            print(the_project_id)
            print(project_info["title"])
            print(f"https://platform.techsmart.codes/code/{the_project_id}/")
            print(e)
            do_skip = input("do you want to skip? (y/n) ")
            if do_skip == "y":
                skip_ids.append(the_project_id)
                with open("skip.json", "w+") as f:
                    json.dump(skip_ids, f, indent=2)
                continue
            else:
                exit(0)
        try:
            unzip_from_memory(dl_url_content, the_project_id,
                              datetime.datetime.strptime(project_info["last_modified"], "%b %d, %Y"))
        except Exception as e:
            print(the_project_id)
            print(f"https://platform.techsmart.codes/code/{the_project_id}/")
            print(the_dl_url)
            print(e)
            # print(dl_url_content)
            do_skip = input("do you want to skip? (y/n) ")
            if do_skip == "y":
                skip_ids.append(the_project_id)
                with open("skip.json", "w+") as f:
                    json.dump(skip_ids, f, indent=2)
                continue
            else:
                exit(0)
        successful_download_ids.append(the_project_id)
        with open("successful.json", "w+") as f:
            json.dump(successful_download_ids, f, indent=2)

# project_id = 5259642
# r = requests.get(f"https://platform.techsmart.codes/code/{project_id}/", headers=headers)
# dl_url = get_project_download_url(r.content)
# r = requests.get(dl_url, headers=headers)
# unzip_from_memory(r.content, project_id)

