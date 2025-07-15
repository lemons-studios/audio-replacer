use velopack::{sources::HttpSource, *};
use tauri::command;
#[command]
pub fn are_updates_available() -> bool {
    let source = get_update_source();
    let um = create_update_manager(source);
    if let UpdateCheck::UpdateAvailable(updates) = um.check_for_updates().unwrap() {
        return true;
    }
    false
}

// Mostly a clone of the above because function because I just cannot bother finding a more efficient solution. It'll have to do
// Besides, No harm in double-checking if updates ARE available
#[command]
pub fn update_app() {
    let source = get_update_source();
    let um = create_update_manager(source);
    if let UpdateCheck::UpdateAvailable(updates) = um.check_for_updates().unwrap() {
        um.download_updates(&updates, None).unwrap();
        um.apply_updates_and_restart(&updates).unwrap();
    }
}

fn get_update_source() -> HttpSource {
    sources::HttpSource::new("https://f004.backblazeb2.com/file/audio-replacer-updates/")
}

fn create_update_manager(source: HttpSource) -> UpdateManager {
    UpdateManager::new(source, None, None).unwrap()
}
