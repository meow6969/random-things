use win_dialog::{style, Icon, WinDialog};
// use windows::Win32::Foundation::HWND;

pub fn display_message(title: &String, msg: &String) {
    WinDialog::new(title.to_owned() + "\n" + msg)
        .with_header(title)
        .with_style(style::Ok_)
        .with_icon(Icon::Information)
        .show()
        .unwrap();
}

pub fn display_error(title: &String, msg: &String) {
    WinDialog::new(title.to_owned() + "\n" + msg)
        .with_header(title)
        .with_style(style::Ok_)
        .with_icon(Icon::Error)
        .show()
        .unwrap();
}


