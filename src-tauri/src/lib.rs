extern crate core;

use crate::commands::app_functions::{close_app, get_install_directory, get_username, in_dev_env};
use crate::commands::project_manager::{
    delete_empty_subdirectories, get_all_directories, get_all_files, randomize_file_order,
};
use crate::commands::whisper_utils::transcribe_file;
use core::option::Option::Some;
use tauri::Manager;
mod commands;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
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
        .plugin(tauri_plugin_drpc::init())
        .plugin(tauri_plugin_audio_recorder::init())
        .invoke_handler(tauri::generate_handler![
            get_all_files,
            get_all_directories,
            delete_empty_subdirectories,
            transcribe_file,
            get_install_directory,
            in_dev_env,
            get_username,
            close_app,
            randomize_file_order
        ])
        .setup(|app| {
            let window = app
                .get_webview_window("audio-replacer")
                .expect("Main window not found!");
            #[cfg(target_os = "linux")]
            // Fix WebKit2Gtk permission issues (I have no clue why this is necessary for a frontend permission fix)
            {
                use webkit2gtk::WebViewExt;
                use webkit2gtk::glib::Cast;
                use webkit2gtk::{
                    PermissionRequestExt, SettingsExt, UserMediaPermissionRequest, WebView,
                };

                window.with_webview(|webview| {
                    let inner_webview = webview.inner();
                    if let Some(wk) = inner_webview.downcast_ref::<WebView>() {
                        if let Some(settings) = wk.settings() {
                            settings.set_enable_webrtc(true);
                        }

                        wk.connect_permission_request(move |_wk, request| {
                            if let Some(_media_request) =
                                request.downcast_ref::<UserMediaPermissionRequest>()
                            {
                                request.allow();
                                return true;
                            }
                            request.deny();
                            true
                        });
                    }
                })?;
            }

            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
