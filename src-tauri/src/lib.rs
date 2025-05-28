use users::get_current_username;
use mundy::{ColorScheme, Interest, Preferences};
use futures_lite::StreamExt as _;

#[tauri::command]
fn get_username() -> String {
    match get_current_username() {
        Some(uname) => uname.to_string_lossy().into_owned(),
        None => String::from("User"),
    }
}


// Work-In-Progress Functions!!

#[tauri::command]
async fn get_system_color() -> String {
    let mut stream = Preferences::stream(Interest::AccentColor);
    String::from("g")
}

#[tauri::command]
async fn is_dark_mode() -> bool {
    let mut stream = Preferences::stream(Interest::ColorScheme);
    true
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_os::init())
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![get_username])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
