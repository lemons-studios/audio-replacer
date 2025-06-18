use std::{fs, path::Path, str, vec};
use futures_lite::io;
use tauri::command;
use walkdir::WalkDir;

#[command]
pub fn get_all_files(path: &str) -> Vec<String> {
    let mut files = vec![];
    for e in WalkDir::new(path).into_iter().filter_map(Result::ok) {
        if e.metadata().unwrap().is_file() {
            files.push(e.path().display().to_string());
        }
    }
    files.sort(); // I don't think I'd ever want to not sort lists that come through this method
    files
}

#[command]
pub fn get_subdirectories(path: &str) -> Vec<String> {
    let mut dirs = vec![];
    for e in WalkDir::new(path).into_iter().filter_map(Result::ok) {
        if e.metadata().unwrap().is_dir() && e.depth() > 0 {
            dirs.push(e.path().display().to_string());
        }
    }
    dirs
}

#[command]
pub fn count_files(path: &str) -> i32 {
    WalkDir::new(path).into_iter().count() as i32
}

#[command(async)]
pub fn delete_empty_subdirectories(project_path: &str) {
    let path = Path::new(project_path);
    if !path.is_dir() {
        return;
    }
    let dirs: Vec<_> = WalkDir::new(path)
    .contents_first(true)
    .into_iter()
    .filter_map(|res| {
        match res {
            Ok(entry) => Some(entry),
            Err(e) => {
                None
            }
        }
    })
    .filter(|e| e.file_type().is_dir())
    .collect();

    for e in dirs {
        let dir_path = e.path();

        match fs::read_dir(dir_path) {
            Ok(mut read_dir) => {
                if read_dir.next().is_none() {
                    if let Err(e) = fs::remove_dir(dir_path) {
                        eprintln!("Failed to delete directory {:?}: {}", dir_path, e);
                    }                }
            }
            Err(e) => {
                eprintln!("Could not read dir {:?}: {}", dir_path, e);
            }
        }
    }
}

#[command]
pub fn calculate_completion(input_path: &str, output_path: &str) -> f32 {
    let input_files = count_files(input_path) as f32;
    let output_files = count_files(output_path) as f32;
    if input_files + output_files == 0.00 {
        return 100.00;
    }
    ((output_files) / (input_files + output_files)) * 100.00
}
