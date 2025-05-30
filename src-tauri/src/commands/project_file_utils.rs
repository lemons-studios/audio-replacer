use walkdir::{WalkDir};
use std::{str, vec};
use tauri::{
    command
};

#[command]
pub fn get_all_files(path: &str, sort: bool) -> Vec<String> {
    let mut files = vec![]; 
    for e in WalkDir::new(path).into_iter().filter_map(Result::ok) {
        if e.metadata().unwrap().is_file() {
            files.push(e.path().display().to_string());
        }
    }
    if sort {
        files.sort()
    }
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

