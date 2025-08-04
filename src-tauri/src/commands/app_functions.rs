use std::{env, path::PathBuf};

use tauri::command;

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
