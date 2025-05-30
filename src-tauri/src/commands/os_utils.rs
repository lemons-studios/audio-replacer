use tauri::command;
use users::get_current_username;

#[command(async)]
pub fn get_username() -> String {
    match get_current_username() {
        Some(uname) => uname.to_string_lossy().into_owned(),
        None => String::from("User"),
    }
}
