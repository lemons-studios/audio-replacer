use std::{str, vec};
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
    let count = WalkDir::new(path).into_iter().count() as i32;
    count
}

#[command]
pub fn calculate_completion(input_path: &str, output_path: &str) -> f32 {
    let input_files = count_files(input_path) as f32;
    let output_files = count_files(output_path) as f32;
    if input_files + output_files == 0.00 {
        return 100.00;
    }
    let percentage: f32 = ((output_files) / (input_files + output_files)) * 100.00;
    percentage
}