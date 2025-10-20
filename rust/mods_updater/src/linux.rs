use std::process::Command;


pub fn display_message(title: &String, msg: &String) {
    Command::new("zenity")
        .arg("--info")
        .arg("--text")
        .arg(title.to_owned() + "\n" + msg)
        .spawn()
        .expect("couldnt open zenity");
}

pub fn display_error(title: &String, msg: &String) {
    Command::new("zenity")
        .arg("--warning")
        .arg("--text")
        .arg(title.to_owned() + "\n" + msg)
        .spawn()
        .expect("couldnt open zenity");
}


