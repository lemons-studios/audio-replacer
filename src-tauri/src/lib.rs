use tauri::Manager;
use crate::commands::app_functions::{get_install_directory, get_username, in_dev_env};
use crate::commands::project_manager::{delete_empty_subdirectories, get_all_files};
use crate::commands::whisper_utils::transcribe_file;

mod commands;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    // Holy moly that's a lot of plugins
    tauri::Builder::default()
        .plugin(tauri_plugin_updater::Builder::new().build())
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_single_instance::init(|_app, _args, _cwd| {
            if let Some(window) = _app.get_webview_window("main") {
                window.unminimize().ok();
                window.set_focus().ok();
            }
        }))
        .plugin(tauri_plugin_global_shortcut::Builder::new().build())
        .plugin(tauri_plugin_upload::init())
        .plugin(tauri_plugin_process::init())
        .plugin(tauri_plugin_shell::init())
        .plugin(tauri_plugin_fs::init())
        .plugin(tauri_plugin_os::init())
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_log::Builder::new().build())
        .plugin(tauri_plugin_drpc::init()) // This line missing was causing me such a headache holy hell
        .invoke_handler(tauri::generate_handler![
            get_all_files,
            delete_empty_subdirectories,
            transcribe_file,
            get_install_directory,
            in_dev_env,
            get_username,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
