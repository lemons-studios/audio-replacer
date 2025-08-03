use tauri::Manager;

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
        .plugin(tauri_plugin_log::Builder::new().build())
        .plugin(tauri_plugin_shell::init())
        .plugin(tauri_plugin_fs::init())
        .plugin(tauri_plugin_os::init())
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_drpc::init())
        .invoke_handler(tauri::generate_handler![
            commands::project_manager::get_all_files,
            commands::project_manager::get_subdirectories,
            commands::project_manager::calculate_completion,
            commands::project_manager::count_files,
            commands::project_manager::delete_empty_subdirectories,
            commands::whisper_utils::transcribe_file,
            commands::app_functions::get_install_directory,
            commands::app_functions::get_relative_path,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
