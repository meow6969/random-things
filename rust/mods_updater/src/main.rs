use std::path::Path;
use std::fs::File;
use std::io::Write;
use std::fs;
use std::io::Read;
use std::io;
use sha2::{Sha256, Digest};
use data_encoding::HEXLOWER;

use reqwest;
use serde::{Deserialize, Serialize};
use regex::Regex;

#[cfg_attr(target_os = "linux", path = "linux.rs")]
#[cfg_attr(windows, path = "windows.rs")]
mod gui;

#[derive(Serialize, Deserialize)]
struct FileInfo {
    name: String,
    hash: String
}

#[derive(Serialize, Deserialize)]
struct Infos {
    filedata: Vec<FileInfo>,
    removedata: Vec<String>
}


fn get_infos(infos_url: String) -> Infos {
    let r = match reqwest::blocking::get(infos_url) {
        Ok(r) => r.text().unwrap(),
        Err(e) => panic!("error: {}", e)
    };
    println!("infos_json={}", r);
    let i: Infos = serde_json::from_str(&r).expect("MEOW");
    return i;
}


// returns the file name   --- idk wut im doing 
fn create_all_file_dirs(f: &String) -> String {
    let re: Regex = Regex::new(r"([\s\S]*)/([\s\S]*$)").unwrap();
    let caps = re.captures(&f).unwrap();
    let file_name: String = caps.get(0).unwrap().as_str().trim().to_string();
    let dir: String = caps.get(1).unwrap().as_str().trim().to_string();
    // println!("dir=\"{}\", file_name=\"{}\"", &dir, &file_name);
    if dir != "" {
        fs::create_dir_all(&dir).expect("couldnt create dirs");
    }
    return file_name;
}


fn remove_files(removedata: &Vec<String>) {
    for f in removedata.into_iter() {
        if Path::new(&f).exists() {
            println!("removing file=\"{}\"", &f);
            fs::remove_file(&f).expect("cant remove data file");
        }
    }
}


fn download_file(url: String, f: &FileInfo) {
    if Path::new(&f.name).exists() {
        // println!("f.name=\"{}\"", &f.name);
        let input = File::open(&f.name).expect("couldnt open file");
        let mut reader = io::BufReader::new(input);

        let digest = {
            let mut hasher = Sha256::new();
            let mut buffer = [0; 1024];
            loop {
                let count = reader.read(&mut buffer).expect("cant read buffer !!");
                if count == 0 { break }
                hasher.update(&buffer[..count]);
            }
            hasher.finalize()
        };
        let hash: String = HEXLOWER.encode(digest.as_ref());
        // println!("file=\"{}\", hash=\"{}\", fs_hash=\"{}\"", f.name, f.hash, hash);
        if f.hash == hash {
            println!("file \"{}\" hash == hash!! skip!!", f.name);
            return;
        }
        fs::remove_file(&f.name).expect("cant remove filee");
    }
    let file_url: String = url + "/" + &f.hash;
    let bytes = match reqwest::blocking::get(file_url) {
        Ok(bytes) => bytes.bytes().unwrap(),
        Err(e) => panic!("error: {}", e)
    };
    
    create_all_file_dirs(&f.name);
    // println!("craeteing file \"{}\"", &f.name);
    let mut file = File::create(&f.name)
        .expect("failed to create file thingy");
    file.write_all(&bytes)
        .expect("failed to write file bytes");
    println!("successfully wrote file: {}", &f.name);
}


fn main() {
    if !Path::new("mods").exists() || !Path::new("options.txt").exists() {
        gui::display_error(&"MeowUpdater: Error! not a minecraft installation!".to_string(), &"make sure the executable is stored in the main folder NOT the \"mods\" folder!, the same folder with the \"options.txt\" file !!".to_string());
        return;
    }


    let url: String = String::from("https://jestershelter.xyz/files/mcserver/updater");
    let infos_url: String = url.clone() + "/files.json";
    println!("infos_url={}", infos_url);
    let i: Infos = get_infos(infos_url);
    remove_files(&i.removedata);
    // println!("infos.filedata[0].name=\"{}\"", &i.filedata[0].name);
    for fd in i.filedata.into_iter() {
        download_file(url.clone(), &fd);
    }

    let title: String = "MeowUpdater: all done".to_string();
    let msg: String = "files all synced!!".to_string();
    println!("{}", title);
    println!("{}", msg);
    println!("all done! u can now close dis window!");

    gui::display_message(&title, &msg);
}


