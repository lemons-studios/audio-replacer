use std::process::exit;
use std::{env, path::PathBuf};
use tauri::{command, is_dev};

#[command]
pub fn get_install_directory() -> Result<PathBuf, String> {
    match env::current_exe() {
        Ok(exe_path) => {
            if let Some(exe_dir) = exe_path.parent() {
                Ok(exe_dir.to_path_buf())
            } else {
                Err("Failed to get the parent directory of the executable.".into())
            }
        }
        Err(e) => Err(format!("Failed to get the executable path: {}", e)),
    }
}

#[command]
pub fn in_dev_env() -> bool {
    is_dev()
}

#[command]
pub fn get_username() -> String {
    whoami::username().unwrap()
}

#[command]
pub fn close_app() {
    exit(1);
}
