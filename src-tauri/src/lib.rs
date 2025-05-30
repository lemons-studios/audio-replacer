use std::{str, vec};

use futures_lite::StreamExt as _;
use mundy::{Interest, Preferences};
use users::get_current_username;
use walkdir::WalkDir;
use tauri::{
    command
};

#[command(async)]
fn get_username() -> String {
    match get_current_username() {
        Some(uname) => uname.to_string_lossy().into_owned(),
        None => String::from("User"),
    }
}

#[command]
async fn get_system_color() -> String {
    let mut stream = Preferences::stream(Interest::AccentColor);
    while let Some(preferences) = stream.next().await {
        let accent_color = preferences.accent_color.to_owned();
        accent_color;
    }
    String::from("g") // test value if other errors out
}

#[command]
fn get_all_files(path: &str, sort: bool) -> Vec<String> {
    let mut files = vec![]; 
    for e in WalkDir::new(path).into_iter().filter_map(Result::ok) {
        if e.metadata().unwrap().is_file() {
            files.push(e.path().display().to_string());
        }
    }
    if sort {
        files.sort()
    }
    files
}


#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_shell::init())
        .plugin(tauri_plugin_fs::init())
        .plugin(tauri_plugin_http::init())
        .plugin(tauri_plugin_os::init())
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![get_username, get_system_color, get_all_files])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
